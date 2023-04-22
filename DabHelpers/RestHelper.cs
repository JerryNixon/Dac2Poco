// learn more at https://aka.ms/dab

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Web;

namespace DabHelpers;

public class RestHelper<T>
{
    private readonly string baseUri;
    private readonly HttpClient httpClient;

    public RestHelper(string baseUri, HttpClient? httpClient = null)
    {
        this.baseUri = Uri.TryCreate(baseUri, UriKind.Absolute, out _) ? baseUri : throw new ArgumentException("Uri invalid.", nameof(baseUri));
        this.httpClient = httpClient ?? new();
    }

    public async Task<(IEnumerable<T> List, string ContinuationUrl)> GetManyAsync(string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null)
    {
        var url = CombineQuerystring();
        
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var value = await response.GetJsonValuePropertyAsync<T>();
        var nextLink = await response.GetJsonPropertyAsync<string>("nextLink");

        return (value ?? Array.Empty<T>(), nextLink ?? string.Empty);

        string CombineQuerystring()
        {
            var uriBuilder = new UriBuilder(baseUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (!string.IsNullOrEmpty(select)) query["$select"] = select;
            if (!string.IsNullOrEmpty(filter)) query["$filter"] = filter;
            if (!string.IsNullOrEmpty(orderby)) query["$orderby"] = orderby;
            if (first.HasValue) query["$first"] = first.Value.ToString();
            if (after.HasValue) query["$after"] = after.Value.ToString();
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri.AbsoluteUri;
        }
    }

    public async Task<T?> GetOneAsync(object Id)
    {
        ArgumentNullException.ThrowIfNull(Id);

        var key = KeyName();
        var url = $"{baseUri}/{key}/{Id}";
        
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
        
        var value = await response.GetJsonValuePropertyAsync<T>();
        return value.FirstOrDefault();

        string KeyName() => typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .First(p => Attribute.IsDefined(p, typeof(KeyAttribute))).Name;
    }

    public async Task<T> InsertAsync(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (kname, kvalue) = model.GetKeyProperties().First();
        var url = $"{baseUri}";
        var cnames = model.GetComputedProperties();
        var (_, json, content) = model.CloneWithoutProperties(new[] { kname }.Union(cnames.Select(x => x.Name)).ToArray());
        
        var response = await httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
        
        var value = await response.GetJsonValuePropertyAsync<T>();
        return value.First();
    }

    public async Task<T> UpsertAsync(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (kname, kvalue) = model.GetKeyProperties().First();
        var url = $"{baseUri}/{kname}/{kvalue}";
        var (_, json, content) = model.CloneWithoutProperties(kname);
        
        var response = await httpClient.PutAsync(url, content);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
        
        var result = await response.GetJsonValuePropertyAsync<T>();
        return result.First();
    }

    public async Task<T> UpdateAsync(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (kname, kvalue) = model.GetKeyProperties().First();
        var url = $"{baseUri}/{kname}/{kvalue}";
        var cnames = model.GetComputedProperties();
        var (_, json, content) = model.CloneWithoutProperties(new[] { kname }.Union(cnames.Select(x => x.Name)).ToArray());

        var response = await httpClient.PatchAsync(url, content);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
        
        var value = await response.GetJsonValuePropertyAsync<T>();
        return value.First();
    }

    public async Task DeleteAsync(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var (kname, kvalue) = model.GetKeyProperties().First();
        var url = $"{baseUri}/{kname}/{kvalue}";
        
        var response = await httpClient.DeleteAsync(url);
        if (!response.IsSuccessStatusCode) Debugger.Break();
        response.EnsureSuccessStatusCode();
    }
}
