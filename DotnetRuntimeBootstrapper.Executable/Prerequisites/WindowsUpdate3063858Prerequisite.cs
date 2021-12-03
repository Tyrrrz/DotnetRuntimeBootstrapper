using System;
using DotnetRuntimeBootstrapper.Executable.Platform;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    // Security update
    internal class WindowsUpdate3063858Prerequisite : IPrerequisite
    {
        private const string Id = "KB3063858";

        public string DisplayName => $"Windows Update {Id}";

        public bool IsInstalled =>
            PlatformInfo.Version >= PlatformVersion.Windows8 ||
            PlatformInfo.IsUpdateInstalled(Id) ||
            // Supersession (https://github.com/Tyrrrz/LightBulb/issues/209)
            PlatformInfo.IsUpdateInstalled("KB3068708");

        public bool IsRebootRequired => true;

        private string GetInstallerDownloadUrl()
        {
            if (PlatformInfo.Version == PlatformVersion.Windows7 &&
                PlatformInfo.ProcessorArchitecture == ProcessorArchitecture.X64)
            {
                return
                    "https://download.microsoft.com/download/0/8/E/08E0386B-F6AF-4651-8D1B-C0A95D2731F0/Windows6.1-KB3063858-x64.msu";
            }

            if (PlatformInfo.Version == PlatformVersion.Windows7 &&
                PlatformInfo.ProcessorArchitecture == ProcessorArchitecture.X86)
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
}