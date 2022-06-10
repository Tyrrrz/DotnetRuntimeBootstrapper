using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public class ApplicationShell : ApplicationShellBase
{
    protected override void ReportError(string message)
    {
        // Report to Windows Event Log
        // Inspired by:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51
        try
        {
            var applicationFilePath = typeof(ApplicationShell).Assembly.Location;
            var applicationName = Path.GetFileName(applicationFilePath);
            var bootstrapperVersion = typeof(ApplicationShell).Assembly.GetName().Version.ToString(3);

            var content = string.Join(
                Environment.NewLine,
                new[]
                {
                    "Description: " + "Bootstrapper for a .NET application has failed.",
                    "Application: " + applicationName,
                    "Path: " + applicationFilePath,
                    "AppHost: " + $".NET Runtime Bootstrapper v{bootstrapperVersion} (GUI)",
                    "Message: " + message
                }
            );

            EventLog.WriteEntry(".NET Runtime", content, EventLogEntryType.Error, 1023);
        }
        catch
        {
            // Ignore
        }

        // Report to the GUI
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