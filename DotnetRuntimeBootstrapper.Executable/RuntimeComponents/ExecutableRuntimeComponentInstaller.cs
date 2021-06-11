using System.Diagnostics;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public class ExecutableRuntimeComponentInstaller : IRuntimeComponentInstaller
    {
        public IRuntimeComponent Component { get; }

        public string FilePath { get; }

        public ExecutableRuntimeComponentInstaller(IRuntimeComponent component, string filePath)
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
                    FileName = FilePath,
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}