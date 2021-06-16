using System;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    // Enables TLS1.2 protocol, which is not strictly required by .NET Runtime,
    // but is very likely going to be needed by the actual application if it
    // sends any HTTP requests whatsoever, as most servers currently reject
    // clients that only support older certificate protocols.
    public class WindowsUpdate3154518RuntimeComponent : IRuntimeComponent
    {
        public string Id => "KB3154518";

        public string DisplayName => $"Windows Update {Id}";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows8 ||
            OperatingSystem.IsUpdateInstalled(Id) ||
            // Supersession (https://github.com/Tyrrrz/LightBulb/issues/209)
            OperatingSystem.IsUpdateInstalled("KB3125574") ||
            // Avoid trying to install updates that we've already tried to install before
            InstallationHistory.Contains(Id);

        private string GetInstallerDownloadUrl()
        {
            if (OperatingSystem.Version == OperatingSystemVersion.Windows7 &&
                OperatingSystem.ProcessorArchitecture == ProcessorArchitecture.X64)
            {
                return
                    "https://download.microsoft.com/download/6/8/0/680ee424-358c-4fdf-a0de-b45dee07b711/windows6.1-kb3154518-x64.msu";
            }

            if (OperatingSystem.Version == OperatingSystemVersion.Windows7 &&
                OperatingSystem.ProcessorArchitecture == ProcessorArchitecture.X86)
            {
                return
                    "https://download.microsoft.com/download/6/8/0/680ee424-358c-4fdf-a0de-b45dee07b711/windows6.1-kb3154518-x86.msu";
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