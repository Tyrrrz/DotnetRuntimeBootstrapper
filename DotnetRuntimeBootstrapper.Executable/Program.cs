using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static void DumpError(string message)
        {
            try
            {
                File.WriteAllText("BootstrapperErrorDump.txt", message);
            }
            catch
            {
                // Ignore
            }
        }

        private static IPrerequisite[] GetMissingPrerequisites(ExecutionParameters parameters)
        {
            var result = new List<IPrerequisite>
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226Prerequisite(),
                new WindowsUpdate3063858Prerequisite(),
                new WindowsUpdate3154518Prerequisite(),
                new VisualCppPrerequisite(),
                new DotnetPrerequisite(parameters.TargetRuntimeName, parameters.TargetRuntimeVersion)
            };

            // Filter out already installed prerequisites
            foreach (var prerequisite in result.ToArray())
            {
                if (prerequisite.CheckIfInstalled())
                    result.Remove(prerequisite);
            }

            return result.ToArray();
        }

        private static bool CheckPerformInstall(ExecutionParameters parameters)
        {
            var missingPrerequisites = GetMissingPrerequisites(parameters);
            if (missingPrerequisites.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show the installation prompt
            var installationPromptResult = InstallationPromptForm.Run(parameters, missingPrerequisites);

            // User chose to install
            if (installationPromptResult == InstallationPromptResult.Install)
            {
                // Start the installation process
                var installationResult = InstallationForm.Run(parameters, missingPrerequisites);

                // Reset environment variables because the installation process has most likely updated them
                EnvironmentEx.ResetEnvironmentVariables();

                return installationResult == InstallationResult.Ready;
            }

            // User chose to ignore
            if (installationPromptResult == InstallationPromptResult.Ignore)
            {
                return true;
            }

            // User chose to cancel
            return false;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => DumpError(e.ExceptionObject.ToString());

            try
            {
                // Get parameters
                var parameters = ExecutionParameters.Resolve();

                // Find target assembly
                var targetFilePath = Path.Combine(PathEx.ExecutingDirectoryPath, parameters.TargetFileName);
                if (!File.Exists(targetFilePath))
                {
                    DumpError($"Target assembly not found: '{parameters.TargetFileName}'.");
                    return 1;
                }

                // Check for missing prerequisites and install them if necessary
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
                DumpError(ex.ToString());
                return 1;
            }
        }
    }
}