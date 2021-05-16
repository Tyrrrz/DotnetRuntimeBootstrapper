using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Utils.Native;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal static class OperatingSystem
    {
        private static SystemInfo? _systemInfo;

        private static SystemInfo GetSystemInfo()
        {
            if (_systemInfo != null)
                return _systemInfo.Value;

            var systemInfo = default(SystemInfo);
            NativeMethods.GetNativeSystemInfo(ref systemInfo);

            _systemInfo = systemInfo;
            return systemInfo;
        }

        private static SystemVersionInfo? _systemVersionInfo;

        private static SystemVersionInfo GetSystemVersionInfo()
        {
            if (_systemVersionInfo != null)
                return _systemVersionInfo.Value;

            var systemVersionInfo = default(SystemVersionInfo);
            NativeMethods.RtlGetVersion(ref systemVersionInfo);

            _systemVersionInfo = systemVersionInfo;
            return systemVersionInfo;
        }

        private static HashSet<string> _installedUpdates;

        private static HashSet<string> GetInstalledUpdates()
        {
            if (_installedUpdates != null)
                return _installedUpdates;

            var result = new List<string>();

            var wmicOutput = CommandLine.TryExecuteProcess("wmic", "qfe list full");

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

        public static ProcessorArchitecture ProcessorArchitecture
        {
            get
            {
                if (GetSystemInfo().ProcessorArchitecture == 0)
                    return ProcessorArchitecture.X86;

                if (GetSystemInfo().ProcessorArchitecture == 9)
                    return ProcessorArchitecture.X64;

                if (GetSystemInfo().ProcessorArchitecture == 5)
                    return ProcessorArchitecture.Arm;

                if (GetSystemInfo().ProcessorArchitecture == 12)
                    return ProcessorArchitecture.Arm64;

                return ProcessorArchitecture.X64;
            }
        }

        public static string ProcessorArchitectureMoniker
        {
            get
            {
                if (ProcessorArchitecture == ProcessorArchitecture.X86)
                    return "x86";

                if (ProcessorArchitecture == ProcessorArchitecture.X64)
                    return "x64";

                if (ProcessorArchitecture == ProcessorArchitecture.Arm)
                    return "arm";

                if (ProcessorArchitecture == ProcessorArchitecture.Arm64)
                    return "arm64";

                return "x64";
            }
        }

        public static bool IsUpdateInstalled(string updateId) => GetInstalledUpdates().Contains(updateId);

        public static void InitiateReboot()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t:0"
            };

            using (var process = new Process{StartInfo = startInfo})
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}