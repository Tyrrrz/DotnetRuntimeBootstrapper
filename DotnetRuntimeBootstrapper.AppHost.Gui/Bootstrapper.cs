using System;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

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

    protected override bool InstallPrerequisites(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (var promptForm = new InstallationPromptForm(targetAssembly, missingPrerequisites))
        {
            Application.Run(promptForm);
            if (!promptForm.Result)
                return false;
        }

        using (var installationForm = new InstallationForm(targetAssembly, missingPrerequisites))
        {
            Application.Run(installationForm);
            return installationForm.Result;
        }
    }

    [STAThread]
    public static int Main(string[] args) => new Bootstrapper().Run(args);
}