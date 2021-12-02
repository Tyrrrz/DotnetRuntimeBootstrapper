using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static TargetAssembly TargetAssembly { get; } = TargetAssembly.Resolve();

        private static bool EnsurePrerequisitesInstalled()
        {
            var missingPrerequisites = TargetAssembly.GetMissingPrerequisites();

            // Nothing to install
            if (missingPrerequisites.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Form to show the missing prerequisites and ask if the user wants to install them
            InstallationPromptResult ShowInstallationPrompt()
            {
                using var form = new InstallationPromptForm(TargetAssembly, missingPrerequisites);
                Application.Run(form);
                return form.Result;
            }

            // Form to show installation progress
            InstallationResult ShowInstallation()
            {
                using var form = new InstallationForm(TargetAssembly, missingPrerequisites);
                Application.Run(form);
                return form.Result;
            }

            return ShowInstallationPrompt() switch
            {
                InstallationPromptResult.Confirmed => ShowInstallation() switch
                {
                    // Ready to run the target application
                    InstallationResult.Succeeded => true,

                    // Failed or requires reboot
                    _ => false
                },

                // Canceled
                _ => false
            };
        }

        private static int Run(string[] args)
        {
            var canRunTarget = EnsurePrerequisitesInstalled();
            if (canRunTarget)
            {
                // Reset environment variables after the installation process to update PATH
                // and other variables that may have been changed by the installation process.
                EnvironmentEx.ResetEnvironmentVariables();

                return TargetAssembly.Run(args);
            }

            // Installation failed, was canceled, or still requires reboot
            return 1;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            static void HandleError(string message)
            {
                // Report the error by writing it to a file and showing in a dialog.
                // Either one of the two can fail for various reasons, which
                // is why we use two approaches for redundancy.

                try
                {
                    var timestamp = DateTimeOffset.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                    File.WriteAllText($"Bootstrapper_Error_{timestamp}.txt", message);
                }
                catch
                {
                    // Ignore
                }

                try
                {
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    // Ignore
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleError(e.ExceptionObject.ToString());

            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                HandleError(ex.ToString());
                return 1;
            }
        }
    }
}