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
        public string DisplayName => "Windows Update KB3154518";

        public bool IsRebootRequired => true;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows8 ||
            OperatingSystem.IsUpdateInstalled("KB3154518");

        private string GetInstallerDownloadUrl() =>
            OperatingSystem.IsProcessor64Bit
                ? "https://download.microsoft.com/download/6/8/0/680ee424-358c-4fdf-a0de-b45dee07b711/windows6.1-kb3154518-x64.msu"
                : "https://download.microsoft.com/download/6/8/0/680ee424-358c-4fdf-a0de-b45dee07b711/windows6.1-kb3154518-x86.msu";

        public IRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName("KB3154518", "msu");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new WindowsUpdateRuntimeComponentInstaller(this, filePath);
        }
    }
}