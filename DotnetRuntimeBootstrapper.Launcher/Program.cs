using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Launcher.Env;
using DotnetRuntimeBootstrapper.Launcher.RuntimeComponents;

namespace DotnetRuntimeBootstrapper.Launcher
{
    public static class Program
    {
        private static void Init()
        {
            // Rudimentary error logging
            AppDomain.CurrentDomain.UnhandledException +=
                (_, args) => Console.Error.WriteLine(args.ExceptionObject.ToString());

            // Disable certificate validation (valid certificate may fail on old operating systems).
            // Try to enable TLS1.2 if it's supported (not a requirement, at least yet).
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType) 0x00000C00 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Ssl3;
        }

        private static IRuntimeComponent[] GetMissingRuntimeComponents(BootstrapperInputs inputs)
        {
            var result = new List<IRuntimeComponent>
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226RuntimeComponent(),
                new WindowsUpdate3063858RuntimeComponent(),
                new WindowsUpdate3154518RuntimeComponent(),
                new VisualCppRuntimeComponent(),
                new DotnetRuntimeComponent(inputs.TargetRuntimeName, inputs.TargetRuntimeVersion)
            };

            // Remove already installed components
            foreach (var component in result.ToArray())
            {
                if (component.CheckIfInstalled())
                    result.Remove(component);
            }

            return result.ToArray();
        }

        private static bool PerformInstall(BootstrapperInputs inputs, IRuntimeComponent[] missingRuntimeComponents)
        {
            if (missingRuntimeComponents.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new InstallationForm(inputs, missingRuntimeComponents);
            Application.Run(form);

            return form.Result == DialogResult.OK;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                Init();

                // Get inputs
                var inputs = BootstrapperInputs.Resolve();
                if (!File.Exists(inputs.TargetExecutableFilePath))
                {
                    Console.Error.WriteLine($"Target executable not found: '{inputs.TargetExecutableFilePath}'.");
                    return 1;
                }

                // Install missing components
                var missingComponents = GetMissingRuntimeComponents(inputs);
                if (!PerformInstall(inputs, missingComponents))
                {
                    // Either the installation failed or requires reboot
                    return 1;
                }

                // Run the target
                return Dotnet.Run(inputs.TargetExecutableFilePath, args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}