using System.Data;
using System.Xml.XPath;

namespace Dac2Poco;

public static class SqlUtilities
{
    public static bool HasRangeString(this string sqlDataType, out string range)
    {
        if (!Enum.TryParse<SqlDataTypeOption>(sqlDataType, true, out var sqlType))
        {
            range = string.Empty;
            return false;
        }

        range = sqlType switch
        {
            SqlDataTypeOption.Int => $"[Range({int.MinValue}, {int.MaxValue})]",
            SqlDataTypeOption.SmallInt => $"[Range({short.MinValue}, {short.MaxValue})]",
            SqlDataTypeOption.TinyInt => $"[Range(0, {byte.MaxValue})]",
            SqlDataTypeOption.BigInt => $"[Range({long.MinValue}, {long.MaxValue})]",
            SqlDataTypeOption.Float => $"[Range({double.MinValue}, {double.MaxValue})]",
            SqlDataTypeOption.Real => $"[Range({float.MinValue}, {float.MaxValue})]",
            SqlDataTypeOption.Decimal => $"[Range({-Math.Pow(10, 38) + 1}, {Math.Pow(10, 38) - 1})]",
            SqlDataTypeOption.Money => $"[Range({decimal.MinValue}, {decimal.MaxValue})]",
            SqlDataTypeOption.SmallMoney => $"[Range({-214748.3648M}, {214748.3647M})]",
            SqlDataTypeOption.Date => $"[Range(\"{DateOnly.MinValue}\", \"{DateOnly.MaxValue}\")]",
            SqlDataTypeOption.DateTime => $"[Range(\"{DateOnly.MinValue}\", \"{DateOnly.MaxValue}\")]",
            SqlDataTypeOption.DateTime2 => $"[Range(\"{DateOnly.MinValue}\", \"{DateOnly.MaxValue}\")]",
            _ => string.Empty
        };
        return !string.IsNullOrEmpty(range);
    }

    public static bool IsUnicodeSqlType(this string sqlDataType)
    {
        if (!Enum.TryParse<SqlDataTypeOption>(sqlDataType, true, out var result))
        {
            return false;
        }
        return result switch
        {
            SqlDataTypeOption.NChar => true,
            SqlDataTypeOption.NText => true,
            SqlDataTypeOption.NVarChar => true,
            _ => false
        };
    }

    public static string GetDotnetType(this SqlDataTypeOption sqlDataType, bool isNullable = false)
    {
        if (IsUnsupportedType())
        {
            return string.Empty;
        }

        var dotnetType = typeof(string);
        switch (sqlDataType)
        {
            case SqlDataTypeOption.BigInt: dotnetType = typeof(long); break;
            case SqlDataTypeOption.Binary:
            case SqlDataTypeOption.Image:
            case SqlDataTypeOption.VarBinary: dotnetType = typeof(byte[]); break;
            case SqlDataTypeOption.Bit: dotnetType = typeof(bool); break;
            case SqlDataTypeOption.Char: dotnetType = typeof(char); break;
            case SqlDataTypeOption.Time: dotnetType = typeof(TimeOnly); break;
            case SqlDataTypeOption.Date: dotnetType = typeof(DateOnly); break;
            case SqlDataTypeOption.DateTime:
            case SqlDataTypeOption.SmallDateTime: dotnetType = typeof(DateTime); break;
            case SqlDataTypeOption.DateTime2:
            case SqlDataTypeOption.DateTimeOffset: dotnetType = typeof(DateTimeOffset); break;
            case SqlDataTypeOption.Decimal:
            case SqlDataTypeOption.Money:
            case SqlDataTypeOption.Numeric: dotnetType = typeof(decimal); break;
            case SqlDataTypeOption.Float: dotnetType = typeof(double); break;
            case SqlDataTypeOption.Int: dotnetType = typeof(int); break;
            case SqlDataTypeOption.NChar:
            case SqlDataTypeOption.NVarChar:
            case SqlDataTypeOption.Text:
            case SqlDataTypeOption.VarChar: dotnetType = typeof(string); break;
            case SqlDataTypeOption.Real: dotnetType = typeof(float); break;
            case SqlDataTypeOption.SmallInt: dotnetType = typeof(short); break;
            case SqlDataTypeOption.TinyInt: dotnetType = typeof(byte); break;
            case SqlDataTypeOption.UniqueIdentifier: dotnetType = typeof(Guid); break;
        }

        return dotnetType.Name + (isNullable ? "?" : string.Empty);

        bool IsUnsupportedType()
        {
            var types = new[]
            {
                SqlDataTypeOption.Sql_Variant,
                SqlDataTypeOption.Timestamp,
                SqlDataTypeOption.Rowversion,
            };
            return types.Contains(sqlDataType);
        }
    }

    public static string GetDotnetType(this string sqlDataType, bool isNullable = false)
    {
        RemoveAnyPercision();

        if (!Enum.TryParse(sqlDataType, true, out SqlDataTypeOption dataTypeOption))
        {
            return string.Empty;
        }

        return dataTypeOption.GetDotnetType(isNullable);

        void RemoveAnyPercision()
        {
            int index = sqlDataType.IndexOf("(");
            if (index != -1)
            {
                sqlDataType = sqlDataType.Substring(0, index).Trim();
            }
        }
    }

    public static string GetSqlSyntax(this SqlDataTypeOption SqlType, bool IsMax = false, int? Length = null, int? Scale = null, int? Precision = null)
    {
        return SqlType switch
        {
            _ when IsMax => $"{SqlType}(MAX)",
            _ when Length is not null => $"{SqlType}({Length})",
            _ when Scale is not null && Precision is not null => $"{SqlType}({Precision},{Scale})",
            _ => SqlType.ToString()
        };
    }
}
