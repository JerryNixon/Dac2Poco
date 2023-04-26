using Microsoft.VisualStudio.TextTemplating.VSHost;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProjectPocoGenerator
{
    public class PocoGenerator : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(PocoGenerator);
        
        public const string Description = "Description";

        public override string GetDefaultExtension() => ".cs";

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            return Encoding.UTF8.GetBytes("Hello!");
        }
    }
}
