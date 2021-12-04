using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Dotnet;
using DotnetRuntimeBootstrapper.AppHost.Platform;
using DotnetRuntimeBootstrapper.AppHost.Utils;
using DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;
using QuickJson;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites
{
    internal class DotnetPrerequisite : IPrerequisite
    {
        private readonly DotnetRuntime _runtime;

        private string ShortName =>
            _runtime.Name
                .TrimStart("Microsoft.", StringComparison.OrdinalIgnoreCase)
                .TrimEnd(".App", StringComparison.OrdinalIgnoreCase);

        public string DisplayName => $".NET Runtime ({ShortName}) v{_runtime.Version}";

        // We are looking for a runtime with the same name and the same major version.
        // Installed runtime may have higher minor version than the target runtime, but not lower.
        public bool IsInstalled
        {
            get
            {
                try
                {
                    return DotnetRuntime
                        .GetAllInstalled()
                        .Any(r =>
                            string.Equals(r.Name, _runtime.Name, StringComparison.OrdinalIgnoreCase) &&
                            r.Version.Major == _runtime.Version.Major &&
                            r.Version >= _runtime.Version
                        );
                }
                catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
                {
                    // .NET is likely not installed altogether
                    return false;
                }
            }
        }

        public bool IsRebootRequired => false;

        public DotnetPrerequisite(DotnetRuntime runtime) =>
            _runtime = runtime;

        private string GetInstallerDownloadUrl()
        {
            var manifest = Http.GetContentString(
                "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/" +
                $"{_runtime.Version.ToString(2)}/releases.json"
            );

            // Find the list of files for the latest release
            var latestRuntimeFilesJson = Json
                .TryParse(manifest)?
                .TryGetChild("releases")?
                .TryGetChild(0)?
                .TryGetChild(_runtime switch
                {
                    { IsWindowsDesktop: true } => "windowsdesktop",
                    { IsAspNet: true } => "aspnetcore-runtime",
                    _ => "runtime"
                })?
                .TryGetChild("files")?
                .EnumerateChildren() ?? Enumerable.Empty<JsonNode>();

            // Find the installer download URL applicable for the current system
            foreach (var fileJson in latestRuntimeFilesJson)
            {
                var runtimeIdentifier = fileJson.TryGetChild("rid")?.TryGetString();

                // Filter by processor architecture
                if (!string.Equals(
                    runtimeIdentifier,
                    "win-" + OperatingSystem.ProcessorArchitecture.GetMoniker(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = fileJson.TryGetChild("url")?.TryGetString();
                if (string.IsNullOrEmpty(downloadUrl))
                    continue;

                // Filter out non-installer downloads
                if (!string.Equals(Path.GetExtension(downloadUrl), ".exe", StringComparison.OrdinalIgnoreCase))
                    continue;

                return downloadUrl;
            }

            throw new ApplicationException(
                "Failed to resolve download URL for the required .NET runtime. " +
                $"Please try to download ${DisplayName} manually " +
                $"from https://dotnet.microsoft.com/download/dotnet/{_runtime.Version.ToString(2)} or " +
                "from https://get.dot.net."
            );
        }

        public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
        {
            var downloadUrl = GetInstallerDownloadUrl();
            var filePath = FileEx.GenerateTempFilePath(Url.TryExtractFileName(downloadUrl) ?? "installer.exe");

            Http.DownloadFile(downloadUrl, filePath, handleProgress);

            return new ExecutablePrerequisiteInstaller(this, filePath);
        }
    }
}