using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Utils.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class DotnetRuntimeComponent : IRuntimeComponent
    {
        private readonly string _name;
        private readonly SemanticVersion _version;

        public string DisplayName => $"{_name} v{_version}";

        public bool IsRebootRequired => false;

        public DotnetRuntimeComponent(string name, SemanticVersion version)
        {
            _name = name;
            _version = version;
        }

        public bool CheckIfInstalled()
        {
            foreach (var runtimeIdentifier in Dotnet.ListRuntimes())
            {
                var match = Regex.Match(runtimeIdentifier, @"^(.*?)\s+(.*?)\s+");

                var runtimeName = match.Groups[1].Value;
                var runtimeVersion = SemanticVersion.TryParse(match.Groups[2].Value);

                // Names should match directly
                var isNameMatch = string.Equals(runtimeName, _name, StringComparison.OrdinalIgnoreCase);

                if (!isNameMatch)
                    continue;

                // Versions should match or there should be a higher version within the same major
                var isVersionMatch =
                    runtimeVersion != null &&
                    runtimeVersion.Major == _version.Major &&
                    runtimeVersion.Minor >= _version.Minor &&
                    runtimeVersion.Patch >= _version.Patch;

                if (!isVersionMatch)
                    continue;

                return true;
            }

            return false;
        }

        public string GetInstallerDownloadUrl()
        {
            // Don't use 'https' because it requires TLS1.2 which Windows 7 doesn't support out of the box
            const string host = "http://dotnetcli.azureedge.net/dotnet";

            var architectureMoniker = OperatingSystem.ProcessorArchitectureMoniker;

            // Desktop
            if (string.Equals(_name, "Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase))
            {
                return _version.Major >= 5
                    ? $"{host}/WindowsDesktop/{_version}/windowsdesktop-runtime-{_version}-win-{architectureMoniker}.exe"
                    : $"{host}/Runtime/{_version}/windowsdesktop-runtime-{_version}-win-{architectureMoniker}.exe";
            }

            // ASP.NET Core
            if (string.Equals(_name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase))
            {
                return
                    $"{host}/aspnetcore/Runtime/{_version}/aspnetcore-runtime-{_version}-win-{architectureMoniker}.exe";
            }

            // Base
            return
                $"{host}/Runtime/{_version}/dotnet-runtime-{_version}-win-{architectureMoniker}.exe";
        }

        public void RunInstaller(string installerFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = installerFilePath,
                Arguments = "/install", // can also use /quiet but probably best not to
                UseShellExecute = true,
                Verb = "runas"
            };

            using (var process = new Process{StartInfo = startInfo})
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}