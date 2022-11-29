using System;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

// Security update
internal class WindowsUpdate3063858Prerequisite : IPrerequisite
{
    private const string Id = "KB3063858";

    public string DisplayName => $"Windows Update {Id}";

    public bool IsInstalled() =>
        OperatingSystemEx.Version >= OperatingSystemVersion.Windows8 ||
        OperatingSystemEx.GetInstalledUpdates().Any(u =>
            string.Equals(u, Id, StringComparison.OrdinalIgnoreCase) ||
            // Supersession (https://github.com/Tyrrrz/LightBulb/issues/209)
            string.Equals(u, "KB3068708", StringComparison.OrdinalIgnoreCase)
        );

    private string GetInstallerDownloadUrl()
    {
        if (OperatingSystemEx.Version == OperatingSystemVersion.Windows7 &&
            OperatingSystemEx.ProcessorArchitecture == ProcessorArchitecture.X64)
        {
            return
                "https://download.microsoft.com/download/0/8/E/08E0386B-F6AF-4651-8D1B-C0A95D2731F0/Windows6.1-KB3063858-x64.msu";
        }

        if (OperatingSystemEx.Version == OperatingSystemVersion.Windows7 &&
            OperatingSystemEx.ProcessorArchitecture == ProcessorArchitecture.X86)
        {
            return
                "https://download.microsoft.com/download/C/9/6/C96CD606-3E05-4E1C-B201-51211AE80B1E/Windows6.1-KB3063858-x86.msu";
        }

        throw new ApplicationException("Unsupported operating system version.");
    }

    public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
    {
        var filePath = FileEx.GenerateTempFilePath($"{Id}.msu");

        Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

        return new WindowsUpdatePrerequisiteInstaller(this, filePath);
    }
}