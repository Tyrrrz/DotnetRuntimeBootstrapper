using System;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Gui.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public class Bootstrapper : BootstrapperBase
{
    protected override void ReportError(string message)
    {
        base.ReportError(message);

        try
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // Ignore
        }
    }

    protected override bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        ApplicationEx.EnsureInitialized();

        using var promptForm = new PromptForm(targetAssembly, missingPrerequisites);
        Application.Run(promptForm);

        return promptForm.IsSuccess;
    }

    protected override bool Install(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        ApplicationEx.EnsureInitialized();

        using var installForm = new InstallForm(targetAssembly, missingPrerequisites);
        Application.Run(installForm);

        return installForm.IsSuccess;
    }

    [STAThread]
    public static int Main(string[] args) => new Bootstrapper().Run(args);
}