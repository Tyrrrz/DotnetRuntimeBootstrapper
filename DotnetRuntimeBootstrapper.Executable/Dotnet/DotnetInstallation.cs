using System;
using System.IO;
using DotnetRuntimeBootstrapper.Executable.Platform;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Executable.Dotnet
{
    internal static class DotnetInstallation
    {
        public static string GetDirectoryPath()
        {
            // .NET installation location design docs:
            // https://github.com/dotnet/designs/blob/main/accepted/2020/install-locations.md
            // 1. Try to find the .NET Core installation directory by looking in the registry
            // 2. Otherwise, try to find it by looking in the default install location

            // Try to resolve location from registry (covers both custom and default locations)
            {
                var archMoniker = PlatformInfo.ProcessorArchitecture.GetMoniker();

                // Installation information is only available in 32-bit view of the registry
                var dotnetRegistryKey =
                    Registry.LocalMachine.OpenSubKey(
                        "SOFTWARE\\Wow6432Node\\dotnet\\Setup\\InstalledVersions\\" + archMoniker,
                        false
                    ) ??

                    Registry.LocalMachine.OpenSubKey(
                        "SOFTWARE\\dotnet\\Setup\\InstalledVersions\\" + archMoniker,
                        false
                    );

                var dotnetDirPath = dotnetRegistryKey?.GetValue("InstallLocation", null) as string;

                if (!string.IsNullOrEmpty(dotnetDirPath) && Directory.Exists(dotnetDirPath))
                    return dotnetDirPath;
            }

            // Try to resolve location from program files (default location)
            {
                // Environment.GetFolderPath(ProgramFiles) does not return the correct path
                // if the bootstrapper is running in x86 mode on an x64 system, so we rely
                // on an environment variable instead.
                var programFilesDirPath =
                    Environment.GetEnvironmentVariable("PROGRAMFILES") ??
                    Environment.GetEnvironmentVariable("ProgramW6432") ??
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                var dotnetDirPath = Path.Combine(programFilesDirPath, "dotnet");

                if (!string.IsNullOrEmpty(dotnetDirPath) && Directory.Exists(dotnetDirPath))
                    return dotnetDirPath;
            }

            throw new DirectoryNotFoundException("Could not find .NET installation directory.");
        }
    }
}