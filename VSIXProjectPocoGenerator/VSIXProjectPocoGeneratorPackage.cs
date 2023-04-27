using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Task = System.Threading.Tasks.Task;

namespace VSIXProjectPocoGenerator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VSIXProjectPocoGeneratorPackage.PackageGuidString)]
    [ProvideCodeGenerator(typeof(PocoGenerator), PocoGenerator.Name, PocoGenerator.Description, true, RegisterCodeBase = true)]
    [ProvideCodeGeneratorExtension(PocoGenerator.Name, ".dacpac")]
    public sealed class VSIXProjectPocoGeneratorPackage : AsyncPackage
    {
        public const string PackageGuidString = "5ba6b949-6b8c-4284-a30f-7a48c5a53ca2";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
    }
}
