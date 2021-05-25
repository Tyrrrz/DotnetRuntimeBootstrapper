using System;
using System.Diagnostics;
using DotnetRuntimeBootstrapper.Env;
using Microsoft.Win32;
using OperatingSystem = DotnetRuntimeBootstrapper.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class VisualCppRuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Visual C++ Redistributable 2015-2019";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled()
        {
            var architectureMoniker = OperatingSystem.ProcessorArchitectureMoniker;

            var registryKey = Registry.LocalMachine.OpenSubKey(
                $"SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\{architectureMoniker}",
                false
            );

            return registryKey != null;
        }

        public string GetInstallerDownloadUrl()
        {
            switch (OperatingSystem.ProcessorArchitecture)
            {
                case ProcessorArchitecture.X86:
                    return "http://aka.ms/vs/16/release/vc_redist.x86.exe";
                case ProcessorArchitecture.X64:
                    return "http://aka.ms/vs/16/release/vc_redist.x64.exe";
                case ProcessorArchitecture.Arm:
                case ProcessorArchitecture.Arm64:
                    return "http://aka.ms/vs/16/release/VC_redist.arm64.exe";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RunInstaller(string installerFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = installerFilePath,
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