using Dac2Poco.Abstractions;

namespace Dac2Poco.Views;

public record ColumnInfo
{
    public string Name { get; set; } = "";
    public string SqlType { get; set; } = "string";

    public bool IsComputed { get; set; }

    public override string ToString()
    {
        return $"{Name} COMPUTED";
    }
}