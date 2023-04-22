using Dac2Poco.Tables;

namespace Dac2Poco.Views;

public class ViewInfo
{
    public string Schema { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullName => $"[{Schema}].[{Name}]";

    public ColumnInfo[] Columns { get; set; } = Array.Empty<ColumnInfo>();

    public override string ToString()
    {
        return $"{FullName}";
    }
}
