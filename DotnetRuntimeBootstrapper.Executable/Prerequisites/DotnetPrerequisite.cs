using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using QuickJson;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public class DotnetPrerequisite : IPrerequisite
    {
        private readonly string _name;
        private readonly Version _version;

        public string Id => $"{_name}_{_version}";

        private string ShortName =>
            _name
                .TrimStart("Microsoft.", StringComparison.OrdinalIgnoreCase)
                .TrimEnd(".App", StringComparison.OrdinalIgnoreCase);

        public string DisplayName => $".NET Runtime ({ShortName}) v{_version}";

        public bool IsRebootRequired => false;

        public DotnetPrerequisite(string name, Version version)
        {
            _name = name;
            _version = version;
        }

        public bool CheckIfInstalled()
        {
            var expectedRuntimeVersion = new Version(_version);

            foreach (var runtimeLine in Dotnet.ListRuntimes())
            {
                var match = Regex.Match(runtimeLine, @"^(.*?)\s+(.*?)\s+");

                var runtimeName = match.Groups[1].Value;
                var runtimeVersion = new Version(match.Groups[2].Value);

                // Names should match directly
                var isNameMatch = string.Equals(runtimeName, _name, StringComparison.OrdinalIgnoreCase);

                if (!isNameMatch)
                    continue;

                // Versions should match or there should be a higher version within the same major
                var isVersionMatch =
                    runtimeVersion.Major == expectedRuntimeVersion.Major &&
                    runtimeVersion.Minor >= expectedRuntimeVersion.Minor &&
                    runtimeVersion.Build >= expectedRuntimeVersion.Build;

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
                return "windowsdesktop";
            }

            if (string.Equals(_name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase))
            {
                return "aspnetcore-runtime";
            }

            return "runtime";
        }

        private string GetInstallerDownloadUrl()
        {
            var manifest = Http.GetContentString(
                "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/" +
                $"{_version}/releases.json"
            );

            // Find the list of files for the latest release
            var latestRuntimeFilesJson = Json
                .TryParse(manifest)?
                .TryGetChild("releases")?
                .TryGetChild(0)?
                .TryGetChild(GetRuntimeMoniker())?
                .TryGetChild("files")?
                .EnumerateChildren() ?? Enumerable.Empty<JsonNode>();

            // Find the installer download URL applicable for the current system
            foreach (var latestRuntimeFileJson in latestRuntimeFilesJson)
            {
                var runtimeIdentifier = latestRuntimeFileJson.TryGetChild("rid")?.TryGetString();

                // Filter by processor architecture
                if (!string.Equals(
                    runtimeIdentifier,
                    "win-" + OperatingSystem.ProcessorArchitectureMoniker,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = latestRuntimeFileJson.TryGetChild("url")?.TryGetString();

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    continue;
                }

                // Filter out non-installer downloads
                if (!string.Equals(
                    Path.GetExtension(downloadUrl),
                    ".exe",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return downloadUrl;
            }

            throw new InvalidOperationException(
                "Failed to resolve download URL for the required .NET runtime. " +
                $"Please try to download ${DisplayName} manually from https://dotnet.microsoft.com/download/dotnet/{_version} or from https://get.dot.net. " +
                $"After that, try running the application again."
            );
        }

        public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var filePath = FileEx.GetTempFileName(Id, "exe");
            Http.DownloadFile(GetInstallerDownloadUrl(), filePath, handleProgress);

            return new ExecutablePrerequisiteInstaller(this, filePath);
        }
    }
}