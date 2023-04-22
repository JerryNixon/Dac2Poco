using Dac2Poco.Abstractions;

using System.Xml.Linq;
using System.Xml.XPath;

namespace Dac2Poco.Tables;

public class Reader : ReaderBase
{
    public Reader(string path) : base(path) { }

    public IEnumerable<TableInfo> GetTables()
    {
        var tables = xml.XPathSelectElements("//ns:Element[@Type='SqlTable']", nsMgr);

        foreach (var xTable in tables)
        {
            yield return GetTable(xTable);
        }
    }

    protected TableInfo GetTable(XElement xTable)
    {
        var tableInfo = new TableInfo();

        var tableName = xTable.Attribute("Name")?.Value ?? "";
        tableInfo.Schema = tableName.Split('.')[0].Trim('[', ']');
        tableInfo.Name = tableName.Split('.')[1].Trim('[', ']');

        tableInfo.IsEdge = xTable.XPathSelectElement(".//ns:Property[@Name='IsEdge'][@Value='True']", nsMgr) is not null;
        tableInfo.IsNode = xTable.XPathSelectElement(".//ns:Property[@Name='IsNode'][@Value='True']", nsMgr) is not null;

        tableInfo.Columns = GetColumns(xTable, tableInfo).ToArray();
        return tableInfo;
    }

    protected IEnumerable<ColumnInfo> GetColumns(XElement xTable, TableInfo tableInfo)
    {
        var columns = xTable.XPathSelectElements(".//ns:Relationship[@Name='Columns']//ns:Element[@Type='SqlSimpleColumn' or @Type='SqlComputedColumn']", nsMgr);

        foreach (var column in columns)
        {
            yield return GetColumn(xTable, column, tableInfo);
        }
    }

    protected virtual ColumnInfo GetColumn(XElement xParent, XElement column, TableInfo tableInfo)
    {
        var columnInfo = new ColumnInfo();

        columnInfo.Name = column.Attribute("Name")?.Value.Split('.')[2]?.Trim('[', ']') ?? "Invalid";

        columnInfo.IsIdentity = column.XPathSelectElement(".//ns:Property[@Name='IsIdentity'][@Value='True']", nsMgr) is not null;

        columnInfo.IsNullable = column.XPathSelectElement(".//ns:Property[@Name='IsNullable'][@Value='False']", nsMgr) is null;

        columnInfo.SqlType = column.XPathSelectElement(".//ns:Element[@Type='SqlTypeSpecifier']//ns:References", nsMgr)?.Attribute("Name")?.Value.Trim('[', ']') ?? "Invalid";

        columnInfo.StringLength = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Length']", nsMgr)?.Attribute("Value")?.Value, out int len) ? len : null;

        columnInfo.Precision = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Precision']", nsMgr)?.Attribute("Value")?.Value, out int prec) ? prec : null;

        columnInfo.Scale = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Scale']", nsMgr)?.Attribute("Value")?.Value, out int scale) ? scale : null;

        columnInfo.IsComputed = column.XPathSelectElement(".//ns:Property[@Name='ExpressionScript']", nsMgr) is not null;

        columnInfo.Expression = column.XPathSelectElement(".//ns:Property[@Name='ExpressionScript']", nsMgr)?.Value ?? string.Empty;

        columnInfo.IsGraph = column.XPathSelectElement(".//ns:Property[@Name='GraphType']", nsMgr) is not null;

        columnInfo.StringMax = column.XPathSelectElement(".//ns:Property[@Name='IsMax'][@Value='True']", nsMgr) is not null;

        // TODO: unique to table only
        string xpath = $"//ns:Element[@Type=\"SqlPrimaryKeyConstraint\"]/ns:Relationship[@Name=\"ColumnSpecifications\"]/ns:Entry/ns:Element/ns:Relationship/ns:Entry/ns:References[contains(@Name, concat('[', '{tableInfo.Schema}', '].', '[', '{tableInfo.Name}', '].', '[', '{columnInfo.Name}', ']'))]";
        columnInfo.IsPrimaryKey = xParent.XPathSelectElement(xpath, nsMgr) is not null;

        return columnInfo;
    }
}