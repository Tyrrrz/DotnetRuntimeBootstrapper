using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Dotnet;
using DotnetRuntimeBootstrapper.Executable.Platform;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    internal class DotnetPrerequisite : IPrerequisite
    {
        private readonly DotnetRuntimeInfo _runtimeInfo;

        public string Id => $"{_runtimeInfo.Name}_{_runtimeInfo.Version}";

        private string ShortName =>
            _runtimeInfo.Name
                .TrimStart("Microsoft.", StringComparison.OrdinalIgnoreCase)
                .TrimEnd(".App", StringComparison.OrdinalIgnoreCase);

        public string DisplayName => $".NET Runtime ({ShortName}) v{_runtimeInfo.Version}";

        public bool IsRebootRequired => false;

        public DotnetPrerequisite(DotnetRuntimeInfo runtimeInfo) =>
            _runtimeInfo = runtimeInfo;

        // We are looking for a runtime with the same name and the same major version.
        // Installed runtime may have higher minor version than the target runtime, but not lower.
        public bool CheckIfInstalled() => DotnetRuntimeInfo
            .GetInstalled()
            .Any(r =>
                string.Equals(r.Name, _runtimeInfo.Name, StringComparison.OrdinalIgnoreCase) &&
                r.Version.Major == _runtimeInfo.Version.Major &&
                r.Version >= _runtimeInfo.Version
            );

        private string GetInstallerDownloadUrl()
        {
            var manifest = Http.GetContentString(
                "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/" +
                $"{_runtimeInfo.Version}/releases.json"
            );

            // Find the list of files for the latest release
            var latestRuntimeFilesJson = Json
                .TryParse(manifest)?
                .TryGetChild("releases")?
                .TryGetChild(0)?
                .TryGetChild(_runtimeInfo switch
                {
                    { IsWindowsDesktop: true } => "windowsdesktop",
                    { IsAspNet: true } => "aspnetcore-runtime",
                    _ => "runtime"
                })?
                .TryGetChild("files")?
                .EnumerateChildren() ?? Enumerable.Empty<JsonNode>();

            // Find the installer download URL applicable for the current system
            foreach (var latestRuntimeFileJson in latestRuntimeFilesJson)
            {
                var runtimeIdentifier = latestRuntimeFileJson.TryGetChild("rid")?.TryGetString();

                // Filter by processor architecture
                if (!string.Equals(
                    runtimeIdentifier,
                    "win-" + PlatformInfo.ProcessorArchitecture.GetMoniker(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = latestRuntimeFileJson.TryGetChild("url")?.TryGetString();
                if (string.IsNullOrEmpty(downloadUrl))
                    continue;

                // Filter out non-installer downloads
                if (!string.Equals(Path.GetExtension(downloadUrl), ".exe", StringComparison.OrdinalIgnoreCase))
                    continue;

                return downloadUrl;
            }

            throw new InvalidOperationException(
                "Failed to resolve download URL for the required .NET runtime. " +
                $"Please try to download ${DisplayName} manually from " +
                $"https://dotnet.microsoft.com/download/dotnet/{_runtimeInfo.Version} or from https://get.dot.net, " +
                "then run the application again."
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