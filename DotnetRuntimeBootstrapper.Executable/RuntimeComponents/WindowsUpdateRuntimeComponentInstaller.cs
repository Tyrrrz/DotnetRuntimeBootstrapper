using System.Diagnostics;
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
        }
    }
}