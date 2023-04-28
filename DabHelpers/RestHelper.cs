﻿// learn more at https://aka.ms/dab

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

    public async Task<(IEnumerable<T> Items, string ContinuationUrl)> GetManyAsync(
        string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null)
    {
        var url = CombineQuerystring();
        Trace.WriteLine($"{nameof(GetManyAsync)} URL:{url}");

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
        return (result?.Values!, result?.ContinuationUrl!);

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

    public async Task<T?> GetOneAsync(T model)
    {
        var url = ModelToValidUrl(model);
        Trace.WriteLine($"{nameof(GetOneAsync)} URL:{url}");

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
        return result!.Values.SingleOrDefault();
    }

    public async IAsyncEnumerable<(T Input, T? Result, Exception? Error)> InsertAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return await RunAsync(model);
        }

        async Task<(T model, T? Result, Exception? Error)> RunAsync(T model)
        {
            try
            {
                return (model, await InsertAsync(model), default);
            }
            catch (Exception ex)
            {
                return (model, default, ex);
            }
        }
    }

    public async Task<T> InsertAsync(T model)
    {
        ModelValid(model);
        Trace.WriteLine($"{nameof(InsertAsync)} URL:{baseUri}");

        var clone = Clone(model, removeKeys: false);
        var response = await httpClient.PostAsJsonAsync(baseUri, clone);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
        return result!.Values.Single();
    }

    public async IAsyncEnumerable<(T Input, T? Result, Exception? Error)> UpsertAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return await RunAsync(model);
        }

        async Task<(T model, T? Result, Exception? Error)> RunAsync(T model)
        {
            try
            {
                return (model, await UpsertAsync(model), default);
            }
            catch (Exception ex)
            {
                return (model, default, ex);
            }
        }
    }

    public async Task<T> UpsertAsync(T model)
    {
        var url = ModelToValidUrl(model);
        Trace.WriteLine($"{nameof(UpsertAsync)} URL:{url}");

        var clone = Clone(model);
        var response = await httpClient.PutAsJsonAsync(url, clone);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
        return result!.Values.Single();
    }

    public async IAsyncEnumerable<(T Input, T? Result, Exception? Error)> UpdateAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return await RunAsync(model);
        }

        async Task<(T model, T? Result, Exception? Error)> RunAsync(T model)
        {
            try
            {
                return (model, await UpdateAsync(model), default);
            }
            catch (Exception ex)
            {
                return (model, default, ex);
            }
        }
    }

    public async Task<T> UpdateAsync(T model)
    {
        var url = ModelToValidUrl(model);
        Trace.WriteLine($"{nameof(UpdateAsync)} URL:{url}");

        var clone = Clone(model);
        var response = await httpClient.PatchAsJsonAsync(url, clone);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RestRoot<T>>();
        return result!.Values.Single();
    }

    public async IAsyncEnumerable<(T Input, Exception? Error)> DeleteAsync(IEnumerable<T> models)
    {
        foreach (var model in models)
        {
            yield return await RunAsync(model);
        }

        async Task<(T model, Exception? Error)> RunAsync(T model)
        {
            try
            {
                await DeleteAsync(model);
                return (model, default);
            }
            catch (Exception ex)
            {
                return (model, ex);
            }
        }
    }

    public async Task DeleteAsync(T model)
    {
        var url = ModelToValidUrl(model);
        Trace.WriteLine($"{nameof(DeleteAsync)} URL:{url}");

        var response = await httpClient.DeleteAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
    }

    private IEnumerable<(string Name, string Value)> GetKeyPropertiesWithValues(T model)
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)))
            .Select(x => (x.Name, x.GetValue(model)?.ToString() ?? string.Empty));
    }

    private object Clone(T model, bool removeKeys = true, bool removeComputedColumns = true)
    {
        ArgumentNullException.ThrowIfNull(model);

        var props = model.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Except(KeyProperties())
            .Except(ReadonlyProperties());

        var clone = new ExpandoObject() as IDictionary<string, Object>;

        foreach (var prop in props)
        {
            clone.Add(prop.Name, prop.GetValue(model)!);
        }

        return clone;

        IEnumerable<PropertyInfo> ReadonlyProperties()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(DatabaseGeneratedAttribute))
                    && (p.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), true).FirstOrDefault()! as DatabaseGeneratedAttribute)?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed);
        }

        IEnumerable<PropertyInfo> KeyProperties()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
        }
    }

    private IEnumerable<(string Name, string Value)> ModelValid(T model)
    {
        ArgumentNullException.ThrowIfNull(nameof(model));

        var keys = GetKeyPropertiesWithValues(model);

        if (!keys.Any())
        {
            throw new ArgumentException("At least one key is required.");
        }

        if (keys.Any(x => string.IsNullOrEmpty(x.Value)))
        {
            throw new ArgumentException($"All keys must have values in: {typeof(T)}", nameof(model));
        }

        return keys;
    }

    private string ModelToValidUrl(T model)
    {
        var keys = ModelValid(model);

        var builder = new StringBuilder(baseUri);

        foreach (var key in keys)
        {
            var name = HttpUtility.UrlEncode(key.Name);
            var value = HttpUtility.UrlEncode(key.Value.ToString());
            builder.Append($"/{name}/{value}");
        }

        return builder.ToString();
    }
}