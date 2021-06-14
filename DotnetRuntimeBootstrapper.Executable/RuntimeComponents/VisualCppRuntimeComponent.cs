﻿using System;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using Microsoft.Win32;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public class VisualCppRuntimeComponent : IRuntimeComponent
    {
        public string Id => "VisualCppRedist_2015_2019";

        public string DisplayName => "Visual C++ Redistributable 2015-2019";

        public bool IsRebootRequired => false;

        public bool CheckIfInstalled() =>
            Registry.LocalMachine.ContainsSubKey(
                $"SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\{OperatingSystem.ProcessorArchitectureMoniker}"
            );

        public IRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName(Id, "exe");

            Http.DownloadFile(
                $"https://aka.ms/vs/16/release/VC_redist.{OperatingSystem.ProcessorArchitectureMoniker}.exe",
                filePath,
                handleProgress
            );

            return new ExecutableRuntimeComponentInstaller(this, filePath);
        }
    }
}