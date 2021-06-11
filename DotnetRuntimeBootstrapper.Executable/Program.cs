using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.RuntimeComponents;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static string ExecutingDirectoryPath { get; } =
            Path.GetDirectoryName(typeof(Program).Assembly.Location) ??
            AppDomain.CurrentDomain.BaseDirectory;

        private static void ShowError(string message) => MessageBox.Show(
            message,
            @"Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );

        private static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) => ShowError(args.ExceptionObject.ToString());

            try
            {
                // Disable certificate validation (valid certificate may fail on old operating systems).
                // Try to enable TLS1.2 if it's supported (not a requirement, at least yet).
                ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType) 0x00000C00 |
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Ssl3;
            }
            catch
            {
                // This can fail if the protocol is not available
            }
        }

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

        private static bool PerformInstall(
            ExecutionParameters parameters,
            IRuntimeComponent[] missingRuntimeComponents)
        {
            if (missingRuntimeComponents.Length <= 0)
                return true;

            // Show the installation form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new InstallationForm(parameters, missingRuntimeComponents);
            Application.Run(form);

            // Refresh the PATH variable so that .NET CLI can be located immediately after it's installed
            Environment.SetEnvironmentVariable(
                "PATH",
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
            );

            // Proceed to running the application only if completed successfully or ignored
            return form.Result is InstallationFormResult.CompletedAndReady or InstallationFormResult.Ignored;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                Init();

                // Get parameters
                var parameters = ExecutionParameters.Resolve();

                // Find target assembly
                var targetFilePath = Path.Combine(ExecutingDirectoryPath, parameters.TargetFileName);
                if (!File.Exists(targetFilePath))
                {
                    ShowError($"Target assembly not found: '{parameters.TargetFileName}'.");
                    return 1;
                }

                // Install missing components
                var missingComponents = GetMissingRuntimeComponents(parameters);
                if (!PerformInstall(parameters, missingComponents))
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