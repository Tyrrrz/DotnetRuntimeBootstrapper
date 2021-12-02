using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Native;
using DotnetRuntimeBootstrapper.Executable.Native.Dotnet;
using DotnetRuntimeBootstrapper.Executable.Native.Windows;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal static class Dotnet
    {
        private static string GetRootDirectoryPath()
        {
            // .NET installation location design docs:
            // https://github.com/dotnet/designs/blob/main/accepted/2020/install-locations.md
            // 1. Try to find the .NET Core installation directory by looking in the registry
            // 2. Otherwise, try to find it by looking in the default install location

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

        private static string GetHostFrameworkResolverFilePath()
        {
            // hostfxr.dll resolution strategy:
            // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/fxr_resolver.cpp#L55-L135
            // 1. Find the root directory containing versioned subdirectories
            // 2. Get the hostfxr.dll from the subdirectory with the highest version number

            var dotnetDirPath = GetRootDirectoryPath();
            var fxrRootDirPath = Path.Combine(Path.Combine(dotnetDirPath, "host"), "fxr");
            if (!Directory.Exists(fxrRootDirPath))
                throw new DirectoryNotFoundException("Could not find hostfxr directory.");

            var highestVersion = default(Version);
            var highestVersionFilePath = default(string);
            foreach (var dirPath in Directory.GetDirectories(fxrRootDirPath))
            {
                var version = VersionEx.TryParse(Path.GetFileName(dirPath));
                if (version is null)
                    continue;

                var filePath = Path.Combine(dirPath, "hostfxr.dll");
                if (!File.Exists(filePath))
                    continue;

                if (highestVersion is null || version > highestVersion)
                {
                    highestVersion = version;
                    highestVersionFilePath = filePath;
                }
            }

            return highestVersionFilePath ?? throw new FileNotFoundException("Could not find hostfxr.dll.");
        }

        public static int Run(string filePath, string[] args)
        {
            using var resolverLib = NativeLibrary.Load(GetHostFrameworkResolverFilePath());
            var resolver = new DotnetHostFrameworkResolver(resolverLib);

            var argsFull = args.Prepend(filePath).ToArray();
            using var context = resolver.InitializeForCommandLine(argsFull);

            return context.Run();
        }
    }
}