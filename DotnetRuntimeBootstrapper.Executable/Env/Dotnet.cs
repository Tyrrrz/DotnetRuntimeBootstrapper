using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal static class Dotnet
    {
        // Don't cache the result because the CLI path may change during execution if a new runtime is installed
        private static string GetCliFilePath()
        {
            // .NET installation location design docs:
            // https://github.com/dotnet/designs/blob/main/accepted/2020/install-locations.md

            // Try to resolve location from registry (covers both custom and default locations)
            {
                // Installation information is only available in 32-bit view of the registry
                var dotnetRegistryKey =
                    Registry.LocalMachine.OpenSubKey(
                        "SOFTWARE\\Wow6432Node\\dotnet\\Setup\\InstalledVersions\\" +
                        OperatingSystem.ProcessorArchitectureMoniker,
                        false
                    ) ??

                    Registry.LocalMachine.OpenSubKey(
                        "SOFTWARE\\dotnet\\Setup\\InstalledVersions\\" +
                        OperatingSystem.ProcessorArchitectureMoniker,
                        false
                    );

                var dotnetDirPath = dotnetRegistryKey?.GetValue("InstallLocation", null) as string;

                if (!string.IsNullOrEmpty(dotnetDirPath) && Directory.Exists(dotnetDirPath))
                {
                    var dotnetFilePath = Path.Combine(dotnetDirPath, "dotnet.exe");
                    if (File.Exists(dotnetFilePath))
                        return dotnetFilePath;
                }
            }

            // Try to resolve location from program files (default location)
            {
                // Environment.GetFolderPath does not return the correct path
                // if the bootstrapper is running in x86 mode on an x64 system,
                // so we rely on an environment variable instead.
                var programFilesDirPath =
                    Environment.GetEnvironmentVariable("PROGRAMFILES") ??
                    Environment.GetEnvironmentVariable("ProgramW6432") ??
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                if (!string.IsNullOrEmpty(programFilesDirPath) && Directory.Exists(programFilesDirPath))
                {
                    var dotnetFilePath = Path.Combine(programFilesDirPath, "dotnet\\dotnet.exe");
                    if (File.Exists(dotnetFilePath))
                        return dotnetFilePath;
                }
            }

            // Fall back to PATH as the last resort
            return "dotnet";
        }

        public static IEnumerable<string> ListRuntimes()
        {
            var commandOutput = CommandLine.TryRunWithOutput(GetCliFilePath(), new[] {"--list-runtimes"});
            if (string.IsNullOrEmpty(commandOutput))
                yield break;

            foreach (var runtimeLine in commandOutput.Split('\n'))
            {
                yield return runtimeLine.Trim();
            }
        }

        public static int Run(string targetFilePath, string[] arguments) =>
            CommandLine.Run(GetCliFilePath(), arguments.Prepend(targetFilePath).ToArray());
    }
}