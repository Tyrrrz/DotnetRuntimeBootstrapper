using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Dotnet;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class DotnetRuntimePrerequisite(DotnetRuntime runtime) : IPrerequisite
{
    private string ShortName =>
        runtime switch
        {
            { IsWindowsDesktop: true } => "Desktop",
            { IsAspNet: true } => "ASP.NET",
            { IsBase: true } => "Base",
            _ => runtime.Name
        };

    public string DisplayName => $".NET Runtime ({ShortName}) v{runtime.Version}";

    // We are looking for a runtime with the same name and the same major version.
    // Installed runtime may have higher minor version than the target runtime, but not lower.
    public bool IsInstalled()
    {
        try
        {
            return DotnetRuntime.GetAllInstalled().Any(runtime.IsSupersededBy);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
        {
            // .NET is likely not installed altogether
            return false;
        }
    }

    private string GetInstallerDownloadUrl()
    {
        var manifest = Http.GetContentString(
            "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/"
                + $"{runtime.Version.ToString(2)}/releases.json"
        );

        // Find the installer download URL applicable for the current system
        return Json
            // Find the list of files for the latest release
            .TryParse(manifest)
                ?.TryGetChild("releases")
                ?.TryGetChild(0)
                ?.TryGetChild(
                    runtime switch
                    {
                        { IsWindowsDesktop: true } => "windowsdesktop",
                        { IsAspNet: true } => "aspnetcore-runtime",
                        _ => "runtime"
                    }
                )
                ?.TryGetChild("files")
                ?.EnumerateChildren()
                // Filter by processor architecture
                .Where(
                    f =>
                        string.Equals(
                            f.TryGetChild("rid")?.TryGetString(),
                            "win-" + OperatingSystemEx.ProcessorArchitecture.GetMoniker(),
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                // Filter by file type
                .Where(
                    f =>
                        string.Equals(
                            f.TryGetChild("name")?.TryGetString()?.Pipe(Path.GetExtension),
                            ".exe",
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                .Select(f => f.TryGetChild("url")?.TryGetString())
                .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Failed to resolve the download URL for the required .NET runtime. "
                    + $"Please try to download ${DisplayName} manually "
                    + $"from https://dotnet.microsoft.com/download/dotnet/{runtime.Version.ToString(2)} or "
                    + "from https://get.dot.net."
            );
    }

    public IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress)
    {
        var downloadUrl = GetInstallerDownloadUrl();
        var filePath = FileEx.GenerateTempFilePath(
            Url.TryExtractFileName(downloadUrl) ?? "installer.exe"
        );

        Http.DownloadFile(downloadUrl, filePath, handleProgress);

        return new ExecutablePrerequisiteInstaller(this, filePath);
    }
}
