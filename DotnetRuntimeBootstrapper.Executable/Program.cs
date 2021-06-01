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

        private static IRuntimeComponent[] GetMissingRuntimeComponents(BootstrapperConfig config)
        {
            var result = new List<IRuntimeComponent>
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226RuntimeComponent(),
                new WindowsUpdate3063858RuntimeComponent(),
                new WindowsUpdate3154518RuntimeComponent(),
                new VisualCppRuntimeComponent(),
                new DotnetRuntimeComponent(config.TargetRuntimeName, config.TargetRuntimeVersion)
            };

            // Remove already installed components
            foreach (var component in result.ToArray())
            {
                if (component.CheckIfInstalled())
                    result.Remove(component);
            }

            return result.ToArray();
        }

        private static bool PerformInstall(BootstrapperConfig config, IRuntimeComponent[] missingRuntimeComponents)
        {
            if (missingRuntimeComponents.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new InstallationForm(config, missingRuntimeComponents);
            Application.Run(form);

            return form.Result == DialogResult.OK;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                Init();

                // Get config
                var config = BootstrapperConfig.Resolve();
                if (!File.Exists(config.TargetExecutableFilePath))
                {
                    Console.Error.WriteLine($"Target executable not found: '{config.TargetExecutableFilePath}'.");
                    return 1;
                }

                // Install missing components
                var missingComponents = GetMissingRuntimeComponents(config);
                if (!PerformInstall(config, missingComponents))
                {
                    // Either the installation failed or requires reboot
                    return 1;
                }

                // Run the target
                return Dotnet.Run(config.TargetExecutableFilePath, args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}