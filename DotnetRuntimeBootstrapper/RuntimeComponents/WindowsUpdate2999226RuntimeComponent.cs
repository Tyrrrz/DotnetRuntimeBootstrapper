using System;
using System.Diagnostics;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Utils.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class WindowsUpdate2999226RuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Windows Update KB2999226";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows10 ||
            OperatingSystem.IsUpdateInstalled("KB2999226");

        public string GetInstallerDownloadUrl()
        {
            switch (OperatingSystem.ProcessorArchitecture)
            {
                case ProcessorArchitecture.X86:
                case ProcessorArchitecture.Arm:
                    return
                        "http://download.microsoft.com/download/4/F/E/4FE73868-5EDD-4B47-8B33-CE1BB7B2B16A/Windows6.1-KB2999226-x86.msu";
                case ProcessorArchitecture.X64:
                case ProcessorArchitecture.Arm64:
                    return
                        "http://download.microsoft.com/download/1/1/5/11565A9A-EA09-4F0A-A57E-520D5D138140/Windows6.1-KB2999226-x64.msu";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RunInstaller(string installerFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "wusa.exe",
                Arguments = installerFilePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            using (var process = new Process{StartInfo = startInfo})
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}