using System;
using System.Collections.Generic;
using System.Management;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Platform;

// The 'Ex' suffix here is just to disambiguate from the existing 'OperatingSystem' type in the BCL
internal static class OperatingSystemEx
{
    public static Version Version { get; } = new(
        SystemVersionInfo.Instance.MajorVersion,
        SystemVersionInfo.Instance.MinorVersion
    );

    public static ProcessorArchitecture ProcessorArchitecture => SystemInfo.Instance.ProcessorArchitecture switch
    {
        0 => ProcessorArchitecture.X86,
        9 => ProcessorArchitecture.X64,
        5 => ProcessorArchitecture.Arm,
        12 => ProcessorArchitecture.Arm64,
        _ => throw new InvalidOperationException("Unknown processor architecture.")
    };

    public static IEnumerable<string> GetInstalledUpdates()
    {
        using var search = new ManagementObjectSearcher("SELECT HotFixID FROM Win32_QuickFixEngineering");
        using var results = search.Get();

        foreach (var result in results)
        {
            var id = result["HotFixID"] as string;
            if (!string.IsNullOrEmpty(id))
                yield return id;
        }
    }

    public static void Reboot() => CommandLine.Run("shutdown", new[] {"/r", "/t", "0"});
}