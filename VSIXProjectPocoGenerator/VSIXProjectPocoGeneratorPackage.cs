using EnvDTE;
using EnvDTE80;
using System.ComponentModel.Design;
using System.IO;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextTemplating.VSHost;

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace VSIXProjectPocoGenerator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]

    // what is this?
    [ProvideMenuResource("Menus.ctmenu", 1)]

    [ProvideCodeGenerator(typeof(PocoGenerator), PocoGenerator.Name, PocoGenerator.Description, true, RegisterCodeBase = true)]
    [ProvideCodeGeneratorExtension(PocoGenerator.Name, ".dacpac")]

    [Guid(PackageGuids.guidPackageString)]
    [ProvideUIContextRule(PackageGuids.guidUIContextString,
        name: "UI Context",
        expression: "dacpac",
        termNames: new[] { "dacpac" },
        termValues: new[] { "HierSingleSelectionName:.dacpac$" })]
    public sealed class VSIXProjectPocoGeneratorPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ToggleGeneratorSync.InitializeAsync(this);
        }
    }

    internal sealed class ToggleGeneratorSync
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            Assumes.Present(commandService);

            var cmdId = new CommandID(PackageGuids.guidGenPocoCmdSet, PackageIds.GenPocoSyncId);

            var cmd = new OleMenuCommand((s, e) => { OnExecute(dte); }, cmdId)
            {
                // This will defer visibility control to the VisibilityConstraints section in the .vsct file
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        private static void OnExecute(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItem item = dte.SelectedItems.Item(1).ProjectItem;
            var ext = Path.GetExtension(item?.FileNames[1] ?? "")?.ToLowerInvariant();

            item.Properties.Item("CustomTool").Value = PocoGenerator.Name;
        }
    }
}