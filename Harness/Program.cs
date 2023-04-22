using Dac2Poco;

using System.Diagnostics;

internal partial class Program
{
    private static void Main(string[] args)
    {
        var tablesReader = new Dac2Poco.Tables.Reader("sample.dacpac");
        var tables = tablesReader.GetTables().ToArray();

        var viewsReader= new Dac2Poco.Views.Reader("sample.dacpac");
        var views = viewsReader.GetViews().ToArray();

        var procesReader = new Dac2Poco.Procedures.Reader("sample.dacpac");
        var procs = procesReader.GetProcedures().ToArray();

        var writer = new Writer(tables, views);
        var code = writer.Generate("Poco", true);

        var path = "output.cs";
        File.WriteAllText(path, code);
        try { OpenVsCode(path); } catch { Process.Start("notepad.exe", path); }

        void OpenVsCode(string path)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var vscode = Path.Combine(userProfile, @"AppData\Local\Programs\Microsoft VS Code\Code.exe");
            vscode = Environment.ExpandEnvironmentVariables(vscode);
            System.Diagnostics.Process.Start(vscode, path);
        }
    }
}