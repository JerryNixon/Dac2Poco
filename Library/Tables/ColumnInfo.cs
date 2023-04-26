using Dac2Poco.Abstractions;

namespace Dac2Poco.Tables;

public record ColumnInfo
{
    public string Name { get; set; } = "";
    public string SqlType { get; set; } = "";

    public int? StringLength { get; set; }
    public bool StringMax { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }

    public bool IsUnicode => SqlType.IsUnicodeSqlType();
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsComputed { get; set; }
    public string? Expression { get; set; }
    public bool IsGraph { get; set; }

    public override string ToString()
    {
        var value = SqlType switch
        {
            _ when StringMax => $"{SqlType}(MAX)",
            _ when StringLength is not null => $"{SqlType}({StringLength})",
            _ when Scale is not null && Precision is not null => $"{SqlType}({Precision},{Scale})",
            _ => SqlType.ToString()
        };
        return $"{Name} {value}" + (IsPrimaryKey ? " [Key]" : "") + (IsGraph ? " [Graph]" : "");
    }
}
