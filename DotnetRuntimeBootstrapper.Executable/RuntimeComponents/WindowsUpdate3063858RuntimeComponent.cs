using System;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    // Security update
    public class WindowsUpdate3063858RuntimeComponent : IRuntimeComponent
    {
        public string Id => "KB3063858";

        public string DisplayName => $"Windows Update {Id}";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows8 ||
            OperatingSystem.IsUpdateInstalled(Id) ||
            // Supersession (https://github.com/Tyrrrz/LightBulb/issues/209)
            OperatingSystem.IsUpdateInstalled("KB3068708") ||
            // Avoid trying to install updates that we've already tried to install before
            InstallationHistory.Contains(Id);

        private string GetInstallerDownloadUrl()
        {
            if (OperatingSystem.Version == OperatingSystemVersion.Windows7 &&
                OperatingSystem.ProcessorArchitecture == ProcessorArchitecture.X64)
            {
                return
                    "https://download.microsoft.com/download/0/8/E/08E0386B-F6AF-4651-8D1B-C0A95D2731F0/Windows6.1-KB3063858-x64.msu";
            }

            if (OperatingSystem.Version == OperatingSystemVersion.Windows7 &&
                OperatingSystem.ProcessorArchitecture == ProcessorArchitecture.X86)
            {
                return
                    "https://download.microsoft.com/download/C/9/6/C96CD606-3E05-4E1C-B201-51211AE80B1E/Windows6.1-KB3063858-x86.msu";
            }

            throw new InvalidOperationException("Unsupported operating system version.");
        }

        public IRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName(Id, "msu");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new WindowsUpdateRuntimeComponentInstaller(this, filePath);
        }
    }
}