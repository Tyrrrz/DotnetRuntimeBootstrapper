using System;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public class Bootstrapper : BootstrapperBase
{
    protected override void ReportError(string message) =>
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    protected override bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    )
    {
        using var promptForm = new PromptForm(targetAssembly, missingPrerequisites);
        Application.Run(promptForm);

        return promptForm.IsSuccess;
    }

    protected override bool Install(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    )
    {
        using var installForm = new InstallForm(targetAssembly, missingPrerequisites);
        Application.Run(installForm);

        return installForm.IsSuccess;
    }

    [STAThread]
    public static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        return new Bootstrapper().Run(args);
    }
}
