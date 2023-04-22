using Dac2Poco.Abstractions;

namespace Dac2Poco.Tables;

public class TableInfo
{
    public string Schema { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullName => $"[{Schema}].[{Name}]";

    public ColumnInfo[] Columns { get; set; } = Array.Empty<ColumnInfo>();

    public bool IsEdge { get; set; }
    public bool IsNode { get; set; }
    public bool IsGraph => IsEdge || IsNode;

    public bool HasKey => Columns.Any(c => c.IsPrimaryKey);
    public string KeyName => Columns.FirstOrDefault(c => c.IsPrimaryKey)?.Name ?? "";

    public override string ToString()
    {
        return $"{FullName}" + (IsGraph ? " [Graph]" : "");
    }
}