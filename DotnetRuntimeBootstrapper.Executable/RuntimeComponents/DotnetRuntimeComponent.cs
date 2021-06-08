using System;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public class DotnetRuntimeComponent : IRuntimeComponent
    {
        private readonly string _name;
        private readonly SemanticVersion _version;

        private string ShortName =>
            _name
                .TrimStart("Microsoft.", StringComparison.OrdinalIgnoreCase)
                .TrimEnd(".App", StringComparison.OrdinalIgnoreCase);

        public string DisplayName => $".NET Runtime ({ShortName}) v{_version}";

        public bool IsRebootRequired => false;

        public DotnetRuntimeComponent(string name, SemanticVersion version)
        {
            _name = name;
            _version = version;
        }

        public bool CheckIfInstalled()
        {
            foreach (var runtimeLine in Dotnet.ListRuntimes())
            {
                var match = Regex.Match(runtimeLine, @"^(.*?)\s+(.*?)\s+");

                var runtimeName = match.Groups[1].Value;
                var runtimeVersion = SemanticVersion.TryParse(match.Groups[2].Value);

                // Names should match directly
                var isNameMatch = string.Equals(runtimeName, _name, StringComparison.OrdinalIgnoreCase);

                if (!isNameMatch)
                    continue;

                // Versions should match or there should be a higher version within the same major
                var isVersionMatch =
                    runtimeVersion is not null &&
                    runtimeVersion.Major == _version.Major &&
                    runtimeVersion.Minor >= _version.Minor &&
                    runtimeVersion.Patch >= _version.Patch;

                if (!isVersionMatch)
                    continue;

                return true;
            }

            return false;
        }

        private string GetRuntimeMoniker()
        {
            if (string.Equals(_name, "Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase))
            {
                return "runtime-desktop";
            }

            if (string.Equals(_name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase))
            {
                return "runtime-aspnetcore";
            }

            return "runtime";
        }

        private string GetInstallerDownloadUrl()
        {
            var downloadPageUrl =
                "https://dotnet.microsoft.com/download/dotnet/thank-you/" +
                $"{GetRuntimeMoniker()}-{_version}-windows-{OperatingSystem.ProcessorArchitectureMoniker}-installer";

            var downloadPageContent = Http.GetContentString(downloadPageUrl);

            var installerUrl = Regex.Match(downloadPageContent, "href=\"(.*?\\.exe)\"").Groups[1].Value;
            if (string.IsNullOrEmpty(installerUrl))
                throw new InvalidOperationException("Failed to extract .NET runtime installer URL.");

            return installerUrl;
        }

        public RuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName("dotnet_runtime_installer", "exe");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new RuntimeComponentInstaller(this, filePath);
        }
    }
}