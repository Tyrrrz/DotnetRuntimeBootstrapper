using System;
using DotnetRuntimeBootstrapper.Env;
using DotnetRuntimeBootstrapper.Utils;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Microsoft.Win32;
using OperatingSystem = DotnetRuntimeBootstrapper.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class VisualCppRuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Visual C++ Redistributable 2015-2019";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            Registry.LocalMachine.ContainsSubKey(
                $"SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\{OperatingSystem.ProcessorArchitectureMoniker}"
            );

        private string GetInstallerDownloadUrl() => OperatingSystem.ProcessorArchitecture switch
        {
            ProcessorArchitecture.X86 => "https://aka.ms/vs/16/release/vc_redist.x86.exe",
            ProcessorArchitecture.X64 => "https://aka.ms/vs/16/release/vc_redist.x64.exe",
            _ => "https://aka.ms/vs/16/release/VC_redist.arm64.exe"
        };

        public DownloadedRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName("visual_cpp_redist", "exe");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new DownloadedRuntimeComponentInstaller(this, filePath);
        }
    }
}