namespace Dac2Poco.Procedures;

public record ProcedureParameterInfo
{
    public string Name { get; set; } = "";
    public string SqlType { get; set; } = "";

    public int? StringLength { get; set; }
    public bool StringMax { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsOptional { get; internal set; }

    public override string ToString()
    {
        var value = SqlType switch
        {
            _ when StringMax => $"{SqlType}(MAX)",
            _ when StringLength is not null => $"{SqlType}({StringLength})",
            _ when Scale is not null && Precision is not null => $"{SqlType}({Precision},{Scale})",
            _ => SqlType.ToString()
        };
        return $"{Name} {value}";
    }
}