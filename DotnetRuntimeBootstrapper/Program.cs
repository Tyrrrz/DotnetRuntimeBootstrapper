using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.RuntimeComponents;
using DotnetRuntimeBootstrapper.Utils;

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