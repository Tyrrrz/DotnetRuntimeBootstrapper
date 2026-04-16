using System;
using System.IO;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using Microsoft.Win32;
using PowerKit;
using PowerKit.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class VisualCppPrerequisite : IPrerequisite
{
    public string DisplayName => "Visual C++ Redistributable 2015-2019";

    public bool IsInstalled() =>
        Registry.LocalMachine.ContainsSubKey(
            (
                OperatingSystemEx.ProcessorArchitecture.Is64Bit()
                    ? "SOFTWARE\\Wow6432Node\\"
                    : "SOFTWARE\\"
            )
                + "Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\"
                + OperatingSystemEx.ProcessorArchitecture.GetMoniker()
        );

    public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
    {
        var fileName = $"VC_redist.{OperatingSystemEx.ProcessorArchitecture.GetMoniker()}.exe";
        var tempFile = new TempFile(
            Path.Combine(
                Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(fileName)
                    + "."
                    + Guid.NewGuid().ToString("N")
                    + Path.GetExtension(fileName)
            )
        );

        Http.DownloadFile(
            $"https://aka.ms/vs/16/release/{fileName}",
            tempFile.Path,
            handleProgress
        );

        return new ExecutablePrerequisiteInstaller(this, tempFile);
    }
}
