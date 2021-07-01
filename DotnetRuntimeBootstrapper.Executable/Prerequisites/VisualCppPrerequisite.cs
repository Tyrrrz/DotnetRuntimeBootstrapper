using System;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using Microsoft.Win32;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public class VisualCppPrerequisite : IPrerequisite
    {
        public string Id => "VisualCppRedist_2015_2019";

        public string DisplayName => "Visual C++ Redistributable 2015-2019";

        public bool IsRebootRequired => false;

        public bool CheckIfInstalled()
        {
            if (OperatingSystem.ProcessorArchitecture == ProcessorArchitecture.X64)
            {
                return Registry.LocalMachine.ContainsSubKey(
                    $"SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\{OperatingSystem.ProcessorArchitectureMoniker}"
                );
            }

            return Registry.LocalMachine.ContainsSubKey(
                $"SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\{OperatingSystem.ProcessorArchitectureMoniker}"
            );
        }

        public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName(Id, "exe");

            Http.DownloadFile(
                $"https://aka.ms/vs/16/release/VC_redist.{OperatingSystem.ProcessorArchitectureMoniker}.exe",
                filePath,
                handleProgress
            );

            return new ExecutablePrerequisiteInstaller(this, filePath);
        }
    }
}