using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Env;
using DotnetRuntimeBootstrapper.RuntimeComponents;

namespace DotnetRuntimeBootstrapper
{
    public static class Program
    {
        private static void Init()
        {
            // Rudimentary error logging
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                // Dump unhandled exceptions to a file
                try
                {
                    File.WriteAllText("bootstrapper-error.txt", args.ExceptionObject.ToString());
                }
                catch
                {
                    // Ignore errors
                }
            };

            // Disable certificate validation (valid certificate may fail on old operating systems).
            // Try to enable TLS1.2 if it's supported (not a requirement, at least yet).
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType) 0x00000C00 |
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Ssl3;
            }
            catch
            {
                // Ignore errors
            }
        }

        private static IRuntimeComponent[] GetMissingRuntimeComponents()
        {
            var result = new List<IRuntimeComponent>
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226RuntimeComponent(),
                new WindowsUpdate3063858RuntimeComponent(),
                new WindowsUpdate3154518RuntimeComponent(),
                new VisualCppRuntimeComponent(),
                new DotnetRuntimeComponent(Inputs.TargetRuntimeName, Inputs.TargetRuntimeVersion)
            };

            // Remove already installed components
            foreach (var component in result.ToArray())
            {
                if (component.CheckIfInstalled())
                    result.Remove(component);
            }

            return result.ToArray();
        }

        private static bool PerformInstallation(IRuntimeComponent[] missingRuntimeComponents)
        {
            if (missingRuntimeComponents.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new InstallationForm(missingRuntimeComponents);
            Application.Run(form);

            return form.Result == DialogResult.OK;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            Init();

            // Install missing components
            var missingComponents = GetMissingRuntimeComponents();
            if (!PerformInstallation(missingComponents))
            {
                // Either the installation failed or requires reboot
                return 1;
            }

            // Run the target
            return Dotnet.Run(Inputs.TargetExecutableFilePath, args);
        }
    }
}