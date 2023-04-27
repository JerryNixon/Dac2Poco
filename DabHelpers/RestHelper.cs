// learn more at https://aka.ms/dab

using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

    public async Task<(RestResult<IEnumerable<T>> result, string ContinuationUrl)> GetManyAsync(string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null)
    {
        try
        {
            var url = CombineQuerystring();
            Trace.WriteLine($"{nameof(GetManyAsync)} URL:{url}");

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var value = await response.GetJsonPropertyAsync<IEnumerable<T>>("value");
            var nextLink = await response.GetJsonPropertyAsync<string>("nextLink");

            return (value?.ToArray() ?? Array.Empty<T>(), nextLink ?? string.Empty);
        }
        catch (Exception ex)
        {
            return (ex, default!);
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

    public async Task<RestResult<T?>> GetOneAsync(params (string Name, object Value)[] keys)
    {
        try
        {
            if (!KeysValid<T?>(keys, out var error))
            {
                return error;
            }

            var url = ConstructUrl(keys);
            Trace.WriteLine($"{nameof(GetOneAsync)} URL:{url}");

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) Debugger.Break();
            response.EnsureSuccessStatusCode();

            return (await ReturnSingleValueAsync(response, required: false))!;
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

            return await ReturnSingleValueAsync(response, true);
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

            return await ReturnSingleValueAsync(response, true);
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

            return await ReturnSingleValueAsync(response, true);
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

    private IEnumerable<string> GetReadonlyProperties()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(DatabaseGeneratedAttribute))
                && ((DatabaseGeneratedAttribute)p.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), true).FirstOrDefault()!)?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed)
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetKeyProperties()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)))
            .Select(x => x.Name);
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

        return ConstructUrl(keys.Select(x => (x.Name, (object)x.Value)).ToArray());
    }

    private string ConstructUrl(params (string Name, object Value)[] keys)
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
        var knames = removeKeys ? GetKeyProperties() : Array.Empty<string>();
        var cnames = removeComputedColumns ? GetReadonlyProperties() : Array.Empty<string>();
        return model!.CloneWithoutProperties(knames.Union(cnames).ToArray());
    }

    private async Task<RestResult<T>> ReturnSingleValueAsync(HttpResponseMessage response, bool required)
    {
        var result = await response.GetJsonPropertyAsync<IEnumerable<T>>("value");
        if (result is null)
        {
            return new Exception("Invalid value property in Json response.");
        }
        if (required && !result.Any())
        {
            return new Exception("Response returned no items.");
        }
        return result.FirstOrDefault()!;
    }

    private bool ModelValid<TResult>(T model, out RestResult<TResult> result)
    {
        result = null!;

        if (model is null)
        {
            result = new ArgumentNullException(nameof(model));
        }

        if (GetKeyPropertiesWithValues(model).Any(x => string.IsNullOrEmpty(x.Value)))
        {
            result = new ArgumentException($"All keys must have values in: {typeof(T)}", nameof(model));
        }

        return result is null;
    }

    private bool KeysValid<TResult>((string Name, object Value)[] keys, out RestResult<TResult> result)
    {
        result = null!;

        if (!keys.Any())
        {
            result = new ArgumentException("At least one key is required.");
        }

        if (keys.Any(x => string.IsNullOrEmpty(x.Name)))
        {
            result = new ArgumentException("Every key.Name is required.");
        }

        if (keys.Any(x => string.IsNullOrEmpty(x.Value?.ToString())))
        {
            result = new ArgumentException("Every key.Value is required.");
        }

        var names = GetKeyProperties();

        if (keys.Length != names.Count() || !names.All(n => keys.Any(k => k.Name == n)))
        {
            result = new ArgumentException($"Supplied keys must match keys in {typeof(T)}.");
        }

        return result is null;
    }
}