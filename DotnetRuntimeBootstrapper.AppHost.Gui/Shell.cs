using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public class Shell : ShellBase
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

        // Prompt the user
        using (var promptForm = new InstallationPromptForm(targetAssembly, missingPrerequisites))
        {
            Application.Run(promptForm);
            if (!promptForm.Result)
                return false;
        }

        // Perform the installation
        using (var installationForm = new InstallationForm(targetAssembly, missingPrerequisites))
        {
            Application.Run(installationForm);
            return installationForm.Result;
        }
    }
}