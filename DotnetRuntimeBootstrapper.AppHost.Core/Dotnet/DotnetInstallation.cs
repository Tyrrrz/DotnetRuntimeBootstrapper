using System;
using System.IO;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Dotnet;

internal static class DotnetInstallation
{
    private static string? TryGetDirectoryPathFromRegistry()
    {
        var dotnetRegistryKey = Registry.LocalMachine.OpenSubKey(
            (
                OperatingSystemEx.ProcessorArchitecture.Is64Bit()
                    ? "SOFTWARE\\Wow6432Node\\"
                    : "SOFTWARE\\"
            )
                + "dotnet\\Setup\\InstalledVersions\\"
                + OperatingSystemEx.ProcessorArchitecture.GetMoniker(),
            false
        );

        var dotnetDirPath = dotnetRegistryKey?.GetValue("InstallLocation", null) as string;

        return !string.IsNullOrEmpty(dotnetDirPath) && Directory.Exists(dotnetDirPath)
            ? dotnetDirPath
            : null;
    }

    private static string? TryGetDirectoryPathFromEnvironment()
    {
        // Environment.GetFolderPath(ProgramFiles) does not return the correct path
        // if the apphost is running in x86 mode on an x64 system, so we rely
        // on an environment variable instead.
        var programFilesDirPath =
            Environment.GetEnvironmentVariable("PROGRAMFILES")
            ?? Environment.GetEnvironmentVariable("ProgramW6432")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        var dotnetDirPath = Path.Combine(programFilesDirPath, "dotnet");

        return !string.IsNullOrEmpty(dotnetDirPath) && Directory.Exists(dotnetDirPath)
            ? dotnetDirPath
            : null;
    }

    // .NET installation location design docs:
    // https://github.com/dotnet/designs/blob/main/accepted/2020/install-locations.md
    public static string GetDirectoryPath() =>
        // Try to resolve location from registry (covers both custom and default locations)
        TryGetDirectoryPathFromRegistry()
        ??
        // Try to resolve location from program files (default location)
        TryGetDirectoryPathFromEnvironment()
        ?? throw new DirectoryNotFoundException("Could not find .NET installation directory.");
}
