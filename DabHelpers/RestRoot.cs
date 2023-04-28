// learn more at https://aka.ms/dab

using System.Text.Json.Serialization;

namespace DabHelpers;

public class RestRoot<T>
{
    [JsonPropertyName("value")]
    public IEnumerable<T> Values { get; set; } = default!;
    [JsonPropertyName("nextLink")]
    public string? ContinuationUrl { get; set; }
}
