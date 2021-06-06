using System;
using System.Diagnostics;
using System.IO;
using DotnetRuntimeBootstrapper.Executable.Utils;

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

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            // Windows update
            if (string.Equals(extension, "msu", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.FileName = "wusa";
                startInfo.Arguments = $"{CommandLine.EscapeArgument(FilePath)} /quiet /norestart";
            }
            // Regular installer
            else
            {
                startInfo.FileName = FilePath;
                startInfo.Arguments = "/quiet";
            }

            return startInfo;
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