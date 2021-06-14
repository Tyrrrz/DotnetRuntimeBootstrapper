using System.Diagnostics;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public class WindowsUpdateRuntimeComponentInstaller : IRuntimeComponentInstaller
    {
        public IRuntimeComponent Component { get; }

        public string FilePath { get; }

        public WindowsUpdateRuntimeComponentInstaller(IRuntimeComponent component, string filePath)
        {
            Component = component;
            FilePath = filePath;
        }

        public void Run()
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wusa",
                    Arguments = $"{CommandLine.EscapeArgument(FilePath)} /quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                }
            };

            process.Start();
            process.WaitForExit();

            // Regardless of the outcome, record the fact that this update has been installed,
            // so we don't attempt to install it again in the future.
            // We need to do this because an update may fail to install if it was superseded
            // by some other already installed update which we are not aware of.
            // https://github.com/Tyrrrz/LightBulb/issues/209
            InstallationHistory.Record(Component.Id);
        }
    }
}