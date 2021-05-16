using System;
using System.Diagnostics;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Utils.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class WindowsUpdate3063858RuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Windows Update KB3063858";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows8 ||
            OperatingSystem.IsUpdateInstalled("KB3063858");

        public string GetInstallerDownloadUrl()
        {
            switch (OperatingSystem.ProcessorArchitecture)
            {
                case ProcessorArchitecture.X86:
                case ProcessorArchitecture.Arm:
                    return
                        "http://download.microsoft.com/download/C/9/6/C96CD606-3E05-4E1C-B201-51211AE80B1E/Windows6.1-KB3063858-x86.msu";
                case ProcessorArchitecture.X64:
                case ProcessorArchitecture.Arm64:
                    return
                        "http://download.microsoft.com/download/0/8/E/08E0386B-F6AF-4651-8D1B-C0A95D2731F0/Windows6.1-KB3063858-x64.msu";
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