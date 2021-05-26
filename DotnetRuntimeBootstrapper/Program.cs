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

        [STAThread]
        public static void Main(string[] args)
        {
            // Rudimentary error logging
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                // Try to dump exception to file
                try
                {
                    File.WriteAllText("bootstrapper-error.txt", e.ExceptionObject.ToString());
                }
                catch
                {
                    // Ignore errors
                }
            };

            // Disable certificate validation (old operating systems may not properly support newer protocols).
            // Try to switch to TLS1.2 if possible.
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType) 0x00000C00;
            }
            catch
            {
                // This can fail on Windows 7 without KB3154518 installed.
                // If that's the case we will attempt to install it and exit early.
            }

            // Install missing components (if any)
            var missingComponents = GetMissingRuntimeComponents();
            if (missingComponents.Length > 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var form = new MainForm(missingComponents);

                Application.Run(form);

                if (form.Result != DialogResult.OK)
                {
                    Environment.ExitCode = 1;
                    return;
                }
            }

            // At this point the missing components have either been installed or
            // were already installed previously, so just run the target executable.
            Dotnet.Run(Inputs.TargetExecutableFilePath, args);
        }
    }
}