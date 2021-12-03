using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Executable.Native;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Platform
{
    internal static class PlatformInfo
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

        // TODO: refactor (find a way to get this info without CLI?)
        private static HashSet<string>? _installedUpdates;
        private static HashSet<string> GetInstalledUpdates()
        {
            if (_installedUpdates is not null)
                return _installedUpdates;

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var wmicOutput = CommandLine.TryRunWithOutput("wmic", new[] {"qfe", "list", "full"});

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

            return _installedUpdates = result;
        }

        // TODO: refactor
        public static bool IsUpdateInstalled(string updateId) => GetInstalledUpdates().Contains(updateId);

        // TODO: refactor
        public static void InitiateReboot() => CommandLine.Run("shutdown", new[] {"/r", "/t", "0"});
    }
}