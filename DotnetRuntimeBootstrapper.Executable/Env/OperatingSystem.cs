using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Executable.Env.Native;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal static class OperatingSystem
    {
        private static SystemInfo? _systemInfo;

        private static SystemInfo GetSystemInfo()
        {
            if (_systemInfo is not null)
                return _systemInfo.Value;

            var systemInfo = default(SystemInfo);
            NativeMethods.GetNativeSystemInfo(ref systemInfo);

            _systemInfo = systemInfo;
            return systemInfo;
        }

        private static SystemVersionInfo? _systemVersionInfo;

        private static SystemVersionInfo GetSystemVersionInfo()
        {
            if (_systemVersionInfo is not null)
                return _systemVersionInfo.Value;

            var systemVersionInfo = default(SystemVersionInfo);
            NativeMethods.RtlGetVersion(ref systemVersionInfo);

            _systemVersionInfo = systemVersionInfo;
            return systemVersionInfo;
        }

        private static HashSet<string>? _installedUpdates;

        private static HashSet<string> GetInstalledUpdates()
        {
            if (_installedUpdates is not null)
                return _installedUpdates;

            var result = new List<string>();

            var wmicOutput = CommandLine.TryRun("wmic", "qfe list full");

            if (!string.IsNullOrEmpty(wmicOutput))
            {
                foreach (var line in wmicOutput.Split('\n'))
                {
                    var match = Regex.Match(
                        line,
                        @"^HotFixID=(KB\d+)",
                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
                    );

                    var updateId = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(updateId))
                        result.Add(updateId);
                }
            }

            return _installedUpdates = new HashSet<string>(result, StringComparer.OrdinalIgnoreCase);
        }

        public static OperatingSystemVersion Version
        {
            get
            {
                if (GetSystemVersionInfo().MajorVersion >= 10)
                    return OperatingSystemVersion.Windows10;

                if (GetSystemVersionInfo().MajorVersion >= 6)
                {
                    if (GetSystemVersionInfo().MinorVersion >= 3)
                        return OperatingSystemVersion.Windows81;

                    if (GetSystemVersionInfo().MinorVersion >= 2)
                        return OperatingSystemVersion.Windows8;

                    if (GetSystemVersionInfo().MinorVersion >= 1)
                        return OperatingSystemVersion.Windows7;
                }

                return OperatingSystemVersion.Unknown;
            }
        }

        public static ProcessorArchitecture ProcessorArchitecture => GetSystemInfo().ProcessorArchitecture switch
        {
            0 => ProcessorArchitecture.X86,
            9 => ProcessorArchitecture.X64,
            5 => ProcessorArchitecture.Arm,
            12 => ProcessorArchitecture.Arm64,
            _ => ProcessorArchitecture.X64
        };

        public static string ProcessorArchitectureMoniker => ProcessorArchitecture switch
        {
            ProcessorArchitecture.X86 => "x86",
            ProcessorArchitecture.X64 => "x64",
            ProcessorArchitecture.Arm => "arm",
            ProcessorArchitecture.Arm64 => "arm64",
            _ => "x64"
        };

        public static bool IsProcessor64Bit =>
            ProcessorArchitecture is ProcessorArchitecture.X64 or ProcessorArchitecture.Arm64;

        public static bool IsUpdateInstalled(string updateId) => GetInstalledUpdates().Contains(updateId);

        public static void InitiateReboot()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 0"
            };

            using var process = new Process{StartInfo = startInfo};
            process.Start();
            process.WaitForExit();
        }
    }
}