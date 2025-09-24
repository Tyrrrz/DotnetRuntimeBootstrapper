using System;
using System.Runtime.InteropServices;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class VisualCppPrerequisite : IPrerequisite
{
    public string DisplayName => "Visual C++ Redistributable 2015-2022";

    public bool IsInstalled()
    {
        var registryKey = Registry.LocalMachine.OpenSubKey(
            (
                OperatingSystemEx.ProcessorArchitecture.Is64Bit()
                    ? "SOFTWARE\\Wow6432Node\\"
                    : "SOFTWARE\\"
            )
                + "Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\"
                + OperatingSystemEx.ProcessorArchitecture.GetMoniker(),
            false
        );

        if (registryKey == null)
            return false;

        // than check if the minor version is ok for 2022
        var minorVersion = Convert.ToInt32(registryKey.GetValue("Minor", 0));
        return minorVersion >= 44;
    }

    public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
    {
        var fileName = $"VC_redist.{OperatingSystemEx.ProcessorArchitecture.GetMoniker()}.exe";
        var filePath = FileEx.GenerateTempFilePath(fileName);

        Http.DownloadFile($"https://aka.ms/vs/17/release/{fileName}", filePath, handleProgress);

        return new ExecutablePrerequisiteInstaller(this, filePath);
    }
}
