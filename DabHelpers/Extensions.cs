// learn more at https://aka.ms/dab

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DabHelpers
{
    public static class Extensions
    {

        public static IEnumerable<(string Name, string Value)> GetComputedProperties(this object model)
        {
            ArgumentNullException.ThrowIfNull(nameof(model));
           
            var props = model!.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(DatabaseGeneratedAttribute))
                    && ((DatabaseGeneratedAttribute)p.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), true).FirstOrDefault()!)?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed);

            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                yield return (name, value);
            }
        }

        public static IEnumerable<(string Name, string Value)> GetKeyProperties(this object model)
        {
            ArgumentNullException.ThrowIfNull(nameof(model));

            var props = model!.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                yield return (name, value);
            }
        }

        public static (object Clone, string Json, StringContent Content) CloneWithoutProperties(this object model, params string[] propsToOmit)
        {
            ArgumentNullException.ThrowIfNull(model);

            var clone = new ExpandoObject() as IDictionary<string, Object>;
            var props = model.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props.Where(x => !propsToOmit.Contains(x.Name)))
            {
                clone.Add(prop.Name, prop.GetValue(model)!);
            }

            var json = JsonSerializer.Serialize(clone);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return (clone, json, content);
        }

        public static IDictionary<string, Object> DeserializeUnknownType(this string json)
        {
            if (string.IsNullOrEmpty(json)) throw new Exception();
            
            ArgumentNullException.ThrowIfNullOrEmpty(json);
            var result = JsonSerializer.Deserialize<ExpandoObject>(json);
            return result!;
        }

        public static async Task<IEnumerable<T>> GetJsonValuePropertyAsync<T>(this HttpResponseMessage response)
        {
            ArgumentNullException.ThrowIfNull(response);

            return await GetJsonPropertyAsync<IEnumerable<T>>(response, "value") ?? Array.Empty<T>();
        }

        public static async Task<T?> GetJsonPropertyAsync<T>(this HttpResponseMessage response, string propName)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNullOrEmpty(propName);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var value = document.RootElement.GetProperty(propName).GetRawText();
            return JsonSerializer.Deserialize<T>(value) ?? default;
        }
    }
}
