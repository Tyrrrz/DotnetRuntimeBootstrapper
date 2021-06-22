using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.RuntimeComponents;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static void ShowError(string message) =>
            MessageBox.Show(message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static IRuntimeComponent[] GetMissingRuntimeComponents(ExecutionParameters parameters)
        {
            var result = new List<IRuntimeComponent>
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226RuntimeComponent(),
                new WindowsUpdate3063858RuntimeComponent(),
                new WindowsUpdate3154518RuntimeComponent(),
                new VisualCppRuntimeComponent(),
                new DotnetRuntimeComponent(parameters.TargetRuntimeName, parameters.TargetRuntimeVersion)
            };

            // Filter out already installed components
            foreach (var component in result.ToArray())
            {
                if (component.CheckIfInstalled())
                    result.Remove(component);
            }

            return result.ToArray();
        }

        private static bool CheckPerformInstall(ExecutionParameters parameters)
        {
            var missingRuntimeComponents = GetMissingRuntimeComponents(parameters);
            if (missingRuntimeComponents.Length <= 0)
                return true;

            // Show the installation form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new InstallationForm(parameters, missingRuntimeComponents);
            Application.Run(form);

            // Reset environment variables because the installation process has most likely updated them
            EnvironmentEx.ResetEnvironmentVariables();

            // Proceed to running the application only if completed successfully or ignored
            return form.Result is InstallationFormResult.Completed or InstallationFormResult.Ignored;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowError(e.ExceptionObject.ToString());

            try
            {
                // Get parameters
                var parameters = ExecutionParameters.Resolve();

                // Find target assembly
                var targetFilePath = Path.Combine(PathEx.ExecutingDirectoryPath, parameters.TargetFileName);
                if (!File.Exists(targetFilePath))
                {
                    ShowError($"Target assembly not found: '{parameters.TargetFileName}'.");
                    return 1;
                }

                // Check for missing components and install them if necessary
                var canProceed = CheckPerformInstall(parameters);
                if (!canProceed)
                {
                    // Either the installation failed, was canceled, or still requires reboot
                    return 1;
                }

                // Run the target
                return Dotnet.Run(targetFilePath, args);
            }
            catch (Exception ex)
            {
                ShowError(ex.ToString());
                return 1;
            }
        }
    }
}