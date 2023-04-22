using Dac2Poco.Abstractions;

using System.Xml.Linq;
using System.Xml.XPath;

namespace Dac2Poco.Procedures;

public class Reader : ReaderBase
{
    public Reader(string path) : base(path) { }

    public IEnumerable<ProcedureInfo> GetProcedures()
    {
        var procedures = xml.XPathSelectElements("//ns:Element[@Type='SqlProcedure']", nsMgr);

        foreach (var xProcedure in procedures)
        {
            yield return GetProcedure(xProcedure);
        }
    }

    private ProcedureInfo GetProcedure(XElement xProcedure)
    {
        var proceduresInfo = new ProcedureInfo();

        var tableName = xProcedure.Attribute("Name")?.Value ?? "";
        proceduresInfo.Schema = tableName.Split('.')[0].Trim('[', ']');
        proceduresInfo.Name = tableName.Split('.')[1].Trim('[', ']');

        proceduresInfo.Parameters = GetParam(xProcedure, proceduresInfo).ToArray();

        return proceduresInfo;
    }

    private IEnumerable<ProcedureParameterInfo> GetParam(XElement xProcedure, ProcedureInfo procInfo)
    {
        var columns = xProcedure.XPathSelectElements(".//ns:Relationship[@Name='Parameters']//ns:Element[@Type='SqlSubroutineParameter']", nsMgr);

        foreach (var column in columns)
        {
            yield return GetParameter(xProcedure, column);
        }
    }

    private ProcedureParameterInfo GetParameter(XElement xParent, XElement column)
    {
        var paramInfo = new ProcedureParameterInfo();

        paramInfo.Name = column.Attribute("Name")?.Value.Split('.')[2]?.Trim('[', ']').Trim('@') ?? "Invalid";

        paramInfo.SqlType = column.XPathSelectElement(".//ns:Element[@Type='SqlTypeSpecifier']//ns:References", nsMgr)?.Attribute("Name")?.Value.Trim('[', ']') ?? "Invalid";

        paramInfo.IsOptional = column.XPathSelectElement(".//ns:Property[@Name='DefaultExpressionScript']", nsMgr) is not null;

        paramInfo.StringLength = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Length']", nsMgr)?.Attribute("Value")?.Value, out int len) ? len : null;

        paramInfo.Precision = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Precision']", nsMgr)?.Attribute("Value")?.Value, out int prec) ? prec : null;

        paramInfo.Scale = int.TryParse(column.XPathSelectElement(".//ns:Property[@Name='Scale']", nsMgr)?.Attribute("Value")?.Value, out int scale) ? scale : null;

        paramInfo.StringMax = column.XPathSelectElement(".//ns:Property[@Name='IsMax'][@Value='True']", nsMgr) is not null;

        return paramInfo;
    }
}
