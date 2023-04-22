using Dac2Poco.Abstractions;

namespace Dac2Poco.Procedures;

public record ProcedureInfo
{
    public string Schema { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullName => $"[{Schema}].[{Name}]";

    public ProcedureParameterInfo[] Parameters { get; set; } = Array.Empty<ProcedureParameterInfo>();

    public override string ToString()
    {
        return $"{FullName} " + string.Join(", ", Parameters.Select(p => "@" + p.Name.ToString()));
    }
}
