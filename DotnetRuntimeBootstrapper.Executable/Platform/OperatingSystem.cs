using System;
using System.Management;
using DotnetRuntimeBootstrapper.Executable.Native;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Platform
{
    internal static class OperatingSystem
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

        public static bool IsUpdateInstalled(string updateId)
        {
            using var search = new ManagementObjectSearcher("SELECT HotFixID FROM Win32_QuickFixEngineering");
            using var results = search.Get();

            foreach (var result in results)
            {
                var id = result["HotFixID"] as string;
                if (string.Equals(id, updateId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static void Reboot() => CommandLine.Run("shutdown", new[] {"/r", "/t", "0"});
    }
}