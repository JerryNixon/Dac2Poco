using Dac2Poco.Abstractions;
using Dac2Poco.Tables;

using System.Xml.Linq;
using System.Xml.XPath;

namespace Dac2Poco.Views;

public class Reader : ReaderBase
{
    public Reader(string path) : base(path) { }

    public IEnumerable<ViewInfo> GetViews()
    {
        var views = xml.XPathSelectElements("//ns:Element[@Type='SqlView']", nsMgr);

        foreach (var xView in views)
        {
            yield return GetView(xView);
        }
    }

    protected ViewInfo GetView(XElement xTable)
    {
        var viewInfo = new ViewInfo();

        var tableName = xTable.Attribute("Name")?.Value ?? "";
        viewInfo.Schema = tableName.Split('.')[0].Trim('[', ']');
        viewInfo.Name = tableName.Split('.')[1].Trim('[', ']');

        viewInfo.Columns = GetColumns(xTable, viewInfo).ToArray();
        return viewInfo;
    }

    protected IEnumerable<ColumnInfo> GetColumns(XElement xTable, ViewInfo tableInfo)
    {
        var columns = xTable.XPathSelectElements(".//ns:Relationship[@Name='Columns']//ns:Element[@Type='SqlComputedColumn']", nsMgr);

        foreach (var column in columns)
        {
            yield return GetColumn(xTable, column);
        }
    }

    protected virtual ColumnInfo GetColumn(XElement xParent, XElement column)
    {
        var columnInfo = new ColumnInfo();

        columnInfo.IsComputed = true;

        columnInfo.Name = column.Attribute("Name")?.Value.Split('.')[2]?.Trim('[', ']') ?? "Invalid";

        return columnInfo;
    }
}