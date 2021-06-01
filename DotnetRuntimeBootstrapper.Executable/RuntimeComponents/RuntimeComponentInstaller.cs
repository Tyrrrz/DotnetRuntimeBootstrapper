using System;
using System.Diagnostics;
using System.IO;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public class RuntimeComponentInstaller
    {
        public IRuntimeComponent Component { get; }

        public string FilePath { get; }

        public RuntimeComponentInstaller(IRuntimeComponent component, string filePath)
        {
            Component = component;
            FilePath = filePath;
        }

        private ProcessStartInfo GetStartInfo()
        {
            var extension = Path.GetExtension(FilePath).Trim('.');

            // Windows update
            if (string.Equals(extension, "msu", StringComparison.OrdinalIgnoreCase))
            {
                return new ProcessStartInfo
                {
                    FileName = "wusa",
                    Arguments = $"{FilePath} /quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                };
            }

            // Regular executable
            return new ProcessStartInfo
            {
                FileName = FilePath,
                Arguments = "/quiet",
                UseShellExecute = true,
                Verb = "runas"
            };
        }

        public void Run()
        {
            using var process = new Process{StartInfo = GetStartInfo()};

            process.Start();
            process.WaitForExit();

            // Below seems to have false negatives
            //if (process.ExitCode != 0)
            //throw new InvalidOperationException($"Failed to run the installer. Exit code {process.ExitCode}.");
        }
    }
}