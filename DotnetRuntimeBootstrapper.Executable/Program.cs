using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class Program
    {
        private readonly TargetAssembly _targetAssembly;

        public Program(TargetAssembly targetAssembly) =>
            _targetAssembly = targetAssembly;

        private bool EnsurePrerequisitesInstalled()
        {
            var missingPrerequisites = _targetAssembly.GetMissingPrerequisites();

            // Nothing to install
            if (missingPrerequisites.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Form to show the missing prerequisites and ask if the user wants to install them
            InstallationPromptResult ShowInstallationPrompt()
            {
                using var form = new InstallationPromptForm(_targetAssembly, missingPrerequisites);
                Application.Run(form);
                return form.Result;
            }

            // Form to show installation progress
            InstallationResult ShowInstallation()
            {
                using var form = new InstallationForm(_targetAssembly, missingPrerequisites);
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

        private int Run(string[] args)
        {
            try
            {
                // Attempt to run the target first without any checks (hot path)
                return _targetAssembly.Run(args);
            }
            catch
            {
                // Resolve missing prerequisites and try again
                var canRunTarget = EnsurePrerequisitesInstalled();
                if (canRunTarget)
                {
                    // Reset environment variables after the installation process to update PATH
                    // and other variables that may have been changed by the installation process.
                    EnvironmentEx.ResetEnvironmentVariables();

                    return _targetAssembly.Run(args);
                }

                // Installation failed, was canceled, or still requires reboot
                return 1;
            }
        }
    }

    public partial class Program
    {
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
                    var filePath = Path.Combine(PathEx.ExecutingDirectoryPath, $"Bootstrapper_Error_{timestamp}.txt");
                    File.WriteAllText(filePath, message);
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
                return new Program(TargetAssembly.Resolve()).Run(args);
            }
            catch (Exception ex)
            {
                HandleError(ex.ToString());
                return 1;
            }
        }
    }
}