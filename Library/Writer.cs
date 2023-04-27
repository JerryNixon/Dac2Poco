using Dac2Poco;
using Dac2Poco.Procedures;
using Dac2Poco.Tables;
using Dac2Poco.Views;

using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;

public class Writer
{
    private readonly TableInfo[] tables;
    private readonly ViewInfo[] views;

    public Writer(TableInfo[] tables, ViewInfo[] views)
    {
        this.tables = tables;
        this.views = views;
    }

    public string Generate(string? baseName = null, bool attributes = true, bool methods = true)
    {
        var schemas = tables.Select(x => x.Schema).Union(views.Select(x => x.Schema)).Distinct();
        var code = new StringBuilder();

        code.AppendLine("// learn more at https://aka.ms/dab");
        code.AppendLine();

        code.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        code.AppendLine("using System.ComponentModel.DataAnnotations;");
        code.AppendLine("using System.ComponentModel;");

        if (!string.IsNullOrEmpty(baseName))
        {
            code.AppendLine("using Models;");
            code.AppendLine();
            code.AppendLine($"namespace Models");
            code.AppendLine("{");
            code.AppendLine($"    public abstract class {baseName} {{ /* TODO */ }}");
            code.AppendLine("}");
        }

        code.AppendLine();

        foreach (var schema in schemas)
        {
            if (schema != schemas.First()) code.AppendLine();
            code.AppendLine($"namespace Models.{schema}");
            code.AppendLine("{");

            var schemaTables = tables.Where(x => x.Schema == schema).Where(x => !x.IsGraph);
            var schemaViews = views.Where(x => x.Schema == schema);

            foreach (var table in schemaTables)
            {
                if (table != schemaTables.First()) code.AppendLine();
                GenerateTable(table, code, attributes, methods, baseName);
            }

            if (schemaTables.Any() && schemaViews.Any())
            {
                code.AppendLine();
            }

            foreach (var view in schemaViews)
            {
                if (view != schemaViews.First()) code.AppendLine();
                GenerateView(view, code, attributes, methods, baseName);
            }
        }
        code.AppendLine("}");

        return code.ToString();
    }

    private void GenerateTable(TableInfo table, StringBuilder code, bool attributes, bool methods, string? baseName)
    {
        if (attributes)
        {
            if (table.Columns.Any(x => x.IsPrimaryKey))
            {
                var key = table.Columns.First(x => x.IsPrimaryKey);
                code.AppendLine($"    [DebuggerDisplay(\"{table.Name}.{key.Name} = {{{key.Name}}}\")]");
            }

            code.AppendLine($"    [Table(\"{table.Name}\", Schema = \"{table.Schema}\")]");
        }

        var baseText = (baseName is not null) ? $" : {baseName}" : string.Empty;
        code.AppendLine($"    public partial class {table.Name}{baseText}");
        code.AppendLine("    {");

        foreach (var column in table.Columns.Where(x => !x.IsGraph))
        {
            var netTypeText = SqlUtilities.GetDotnetType(column.SqlType, column.IsNullable);
            if (string.IsNullOrEmpty(netTypeText) && !column.IsComputed) continue;
            var netType = Type.GetType("System." + netTypeText.Trim('?'));

            if (attributes && column != table.Columns.First()) code.AppendLine();

            if (attributes && column.IsPrimaryKey)
            {
                code.AppendLine("        [Key]");
            }
            if (attributes && !column.IsNullable)
            {
                code.AppendLine($"        [Required{(netType == typeof(System.String) ? "(AllowEmptyStrings = true)" : string.Empty)}]");
            }
            if (attributes && column.IsComputed)
            {
                code.AppendLine($"        [ReadOnly(true)]");
            }
            if (attributes && netType == typeof(System.String) && column.StringMax)
            {
                code.AppendLine($"        [StringLength(5000)]");
            }
            else if (attributes && netType == typeof(System.String) && !column.StringMax)
            {
                code.AppendLine($"        [StringLength({column.StringLength})]");
            }
            if (attributes && column.IsComputed)
            {
                code.AppendLine($"        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]");
            }
            if (attributes && column.IsIdentity)
            {
                code.AppendLine("        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            }
            if (attributes)
            {
                var typeName = column.IsComputed ? "Computed" : column.SqlType;
                code.AppendLine($"        [Column(\"{column.Name}\", TypeName = \"{typeName}\")]");
            }

            var set = column.IsComputed ? string.Empty : " set;";
            var inlineType = column.IsComputed ? "string" : netTypeText;
            var def = column.IsNullable ? "default!" : netType == typeof(System.String) ? "string.Empty" : "default";
            code.AppendLine($"        public {inlineType} @{column.Name} {{ get;{set} }} = {def};");
        }

        code.AppendLine("    }");
    }

    private void GenerateView(ViewInfo view, StringBuilder code, bool attributes, bool methods, string? baseName)
    {
        if (attributes)
        {
            code.AppendLine($"    [Table(\"{view.Name}\", Schema = \"{view.Schema}\")] // View");
        }

        var baseText = (baseName is not null) ? $" : {baseName}" : string.Empty;
        code.AppendLine($"    public partial class {view.Name}{baseText}");
        code.AppendLine("    {");

        foreach (var column in view.Columns)
        {
            // the dacpac does not know the data inlineType of view columns
            var netType = column.SqlType;

            if (attributes && column != view.Columns.First()) code.AppendLine();

            if (attributes)
            {
                code.AppendLine($"        [Column(\"{column.Name}\", TypeName = \"{column.SqlType}\")]");
            }

            code.AppendLine($"        public {netType} @{column.Name} {{ get; set; }} = default!;");
        }
        code.AppendLine("    }");
    }
}