using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace Dac2Poco.Abstractions;

public class ReaderBase : IDisposable
{
    protected readonly string modelPath;
    protected readonly XmlNamespaceManager nsMgr;
    protected readonly XDocument xml;

    public ReaderBase(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }

        var extension = Path.GetExtension(path).ToLower().Trim('.');
        if (extension != "dacpac")
        {
            throw new ArgumentException("Expected dacpac.", nameof(path));
        }

        modelPath = ExtractModelXmlFromZip(path);

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException(modelPath);
        }

        nsMgr = new XmlNamespaceManager(new NameTable());
        nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02");
        xml = XDocument.Load(modelPath);

    }

    public void Dispose()
    {
        if (File.Exists(modelPath))
        {
            File.Delete(modelPath);
        }
    }

    private static string ExtractModelXmlFromZip(string path)
    {
        var modelPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // Create the temporary directory
        Directory.CreateDirectory(modelPath);

        // Extract the model.xml file to the temporary directory
        using (var archive = ZipFile.OpenRead(path))
        {
            var entry = archive.GetEntry("model.xml");
            if (entry != null)
            {
                modelPath = Path.Combine(modelPath, "model.xml");
                entry.ExtractToFile(modelPath, true);
            }
        }

        return modelPath;
    }
}
