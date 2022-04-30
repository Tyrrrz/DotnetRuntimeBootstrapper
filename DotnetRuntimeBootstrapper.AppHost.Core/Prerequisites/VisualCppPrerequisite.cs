using System;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;
using Microsoft.Win32;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Core.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class VisualCppPrerequisite : IPrerequisite
{
    public string DisplayName => "Visual C++ Redistributable 2015-2019";

    public bool IsInstalled() =>
        Registry.LocalMachine.ContainsSubKey(
            (OperatingSystem.ProcessorArchitecture.Is64Bit()
                ? "SOFTWARE\\Wow6432Node\\"
                : "SOFTWARE\\") +
            "Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\" +
            OperatingSystem.ProcessorArchitecture.GetMoniker()
        );

    public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
    {
        var fileName = $"VC_redist.{OperatingSystem.ProcessorArchitecture.GetMoniker()}.exe";
        var filePath = FileEx.GenerateTempFilePath(fileName);

        Http.DownloadFile($"https://aka.ms/vs/16/release/{fileName}", filePath, handleProgress);

        return new ExecutablePrerequisiteInstaller(this, filePath);
    }
}