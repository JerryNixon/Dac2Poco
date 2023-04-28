// learn more at https://aka.ms/dab

using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace DabHelpers;

public partial class RestHelper<T> : IRestHelper<T> where T : new()
{
    private readonly string baseUri;
    private readonly HttpClient httpClient;

    public RestHelper(string baseUri, HttpClient? httpClient = null)
    {
        this.baseUri = Uri.TryCreate(baseUri, UriKind.Absolute, out _) ? baseUri : throw new ArgumentException("Uri invalid.", nameof(baseUri));
        this.httpClient = httpClient ?? new();
    }

    public async Task<(IEnumerable<T> Items, Exception Error, string ContinuationUrl)> GetManyAsync(
        string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null)
    {
        try
        {
            var url = CombineQuerystring();
            Trace.WriteLine($"{nameof(GetManyAsync)} URL:{url}");

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
            return (result?.Values!, default!, result?.ContinuationToken!);
        }
        catch (Exception ex)
        {
            return (default!, ex, default!);
        }

        string CombineQuerystring()
        {
            var builder = new UriBuilder(baseUri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrEmpty(select)) query["$select"] = select;
            if (!string.IsNullOrEmpty(filter)) query["$filter"] = filter;
            if (!string.IsNullOrEmpty(orderby)) query["$orderby"] = orderby;
            if (first.HasValue) query["$first"] = first.Value.ToString();
            if (after.HasValue) query["$after"] = after.Value.ToString();
            builder.Query = query.ToString();
            return builder.Uri.ToString();
        }
    }


    public async Task<RestResult<T?>> GetOneAsync(T model)
    {
        try
        {
            if (!ModelValid<T?>(model, out var error))
            {
                return error;
            }

            var url = ConstructUrl(model);
            Trace.WriteLine($"{nameof(GetOneAsync)} URL:{url}");

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
            return result!.Values.SingleOrDefault();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public async IAsyncEnumerable<(T model, RestResult<T> Result)> InsertAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return (model, await InsertAsync(model));
        }
    }

    public async Task<RestResult<T>> InsertAsync(T model)
    {
        try
        {
            if (!ModelValid<T>(model, out var error))
            {
                return error;
            }

            Trace.WriteLine($"{nameof(InsertAsync)} URL:{baseUri}");

            var clone = Clone(model, removeKeys: false);

            var response = await httpClient.PostAsJsonAsync(baseUri, clone);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
            return result!.Values.Single();
        }
        catch (Exception ex)
        {
            return ex;
        }

    }

    public async IAsyncEnumerable<(T Model, RestResult<T> Result)> UpsertAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return (model, await UpsertAsync(model));
        }
    }

    public async Task<RestResult<T>> UpsertAsync(T model)
    {
        try
        {
            if (!ModelValid<T>(model, out var error))
            {
                return error;
            }

            var url = ConstructUrl(model);
            Trace.WriteLine($"{nameof(UpsertAsync)} URL:{url}");

            var clone = Clone(model);

            var response = await httpClient.PutAsJsonAsync(url, clone);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
            return result!.Values.Single();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public async IAsyncEnumerable<(T Model, RestResult<T> Result)> UpdateAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return (model, await UpdateAsync(model));
        }
    }

    public async Task<RestResult<T>> UpdateAsync(T model)
    {
        try
        {
            if (!ModelValid<T>(model, out var error))
            {
                return error;
            }

            var url = ConstructUrl(model);
            Trace.WriteLine($"{nameof(UpdateAsync)} URL:{url}");

            var clone = Clone(model);

            var response = await httpClient.PatchAsJsonAsync(url, clone);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
            return result!.Values.Single();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public async Task DeleteAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            await DeleteAsync(model);
        }
    }

    public async Task<RestResult<bool>> DeleteAsync(T model)
    {
        try
        {
            if (!ModelValid<bool>(model, out var error))
            {
                return error;
            }

            var url = ConstructUrl(model);
            Trace.WriteLine($"{nameof(DeleteAsync)} URL:{url}");

            var response = await httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private IEnumerable<(string Name, string Value)> GetKeyPropertiesWithValues(T model)
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)))
            .Select(x => (x.Name, x.GetValue(model)?.ToString() ?? string.Empty));
    }

    private string ConstructUrl(T model)
    {
        var keys = GetKeyPropertiesWithValues(model);

        if (!keys.Any())
        {
            throw new ArgumentException($"No keys defined in {typeof(T)}.");
        }

        foreach (var error in keys.Where(x => string.IsNullOrEmpty(x.Value)))
        {
            throw new ArgumentException($"Key property {error.Value} has no value.");
        }

        return ConstructUrl(keys.Select(x => (x.Name, x.Value)).ToArray());
    }

    private string ConstructUrl(params (string Name, string Value)[] keys)
    {
        var builder = new StringBuilder(baseUri);

        foreach (var key in keys)
        {
            var name = HttpUtility.UrlEncode(key.Name);
            var value = HttpUtility.UrlEncode(key.Value.ToString());
            builder.Append($"/{name}/{value}");
        }
        
        return builder.ToString();
    }

    private object Clone(T model, bool removeKeys = true, bool removeComputedColumns = true)
    {
        ArgumentNullException.ThrowIfNull(model);

        var props = model.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Except(GetKeyProperties())
            .Except(GetReadonlyProperties());

        var clone = new ExpandoObject() as IDictionary<string, Object>;

        foreach (var prop in props)
        {
            clone.Add(prop.Name, prop.GetValue(model)!);
        }

        return clone;

        IEnumerable<PropertyInfo> GetReadonlyProperties()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(DatabaseGeneratedAttribute))
                    && (p.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), true).FirstOrDefault()! as DatabaseGeneratedAttribute)?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed);
        }

        IEnumerable<PropertyInfo> GetKeyProperties()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
        }
    }

    private bool ModelValid<TResult>(T model, out Exception error)
    {
        error = null!;

        if (model is null)
        {
            error = new ArgumentNullException(nameof(model));
        }

        var keys = GetKeyPropertiesWithValues(model);

        if (!keys.Any())
        {
            error = new ArgumentException("At least one key is required.");
        }

        if (keys.Any(x => string.IsNullOrEmpty(x.Value)))
        {
            error = new ArgumentException($"All keys must have values in: {typeof(T)}", nameof(model));
        }

        return error is null;
    }
}