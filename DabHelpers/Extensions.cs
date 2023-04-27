// learn more at https://aka.ms/dab

using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace DabHelpers;

public static class Extensions
{
    public static object CloneWithoutProperties(this object model, params string[] propsToOmit)
    {
        ArgumentNullException.ThrowIfNull(model);

        var clone = new ExpandoObject() as IDictionary<string, Object>;
        var props = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props.Where(x => !propsToOmit.Contains(x.Name)))
        {
            clone.Add(prop.Name, prop.GetValue(model)!);
        }

        return clone;
    }

    public static async Task<T?> GetJsonPropertyAsync<T>(this HttpResponseMessage response, string propName)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNullOrEmpty(propName);

        var json = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty(propName, out var prop)) return default;
        
        var value = prop.GetRawText();
        return JsonSerializer.Deserialize<T>(value) ?? default;
    }
}
