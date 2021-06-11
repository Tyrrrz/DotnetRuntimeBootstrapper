using System;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    // Security update
    public class WindowsUpdate3063858RuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Windows Update KB3063858";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows8 ||
            OperatingSystem.IsUpdateInstalled("KB3063858") ||
            // Supersession (https://github.com/Tyrrrz/LightBulb/issues/209)
            OperatingSystem.IsUpdateInstalled("KB3068708");

        private string GetInstallerDownloadUrl() =>
            OperatingSystem.IsProcessor64Bit
                ? "https://download.microsoft.com/download/0/8/E/08E0386B-F6AF-4651-8D1B-C0A95D2731F0/Windows6.1-KB3063858-x64.msu"
                : "https://download.microsoft.com/download/C/9/6/C96CD606-3E05-4E1C-B201-51211AE80B1E/Windows6.1-KB3063858-x86.msu";

        public IRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName("KB3063858", "msu");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new WindowsUpdateRuntimeComponentInstaller(this, filePath);
        }
    }
}