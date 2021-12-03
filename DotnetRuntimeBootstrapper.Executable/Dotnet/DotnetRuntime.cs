using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Dotnet
{
    internal partial class DotnetRuntime
    {
        public string Name { get; }

        public Version Version { get; }

        public bool IsBase =>
            string.Equals(Name, "Microsoft.NETCore.App", StringComparison.OrdinalIgnoreCase);

        public bool IsWindowsDesktop =>
            string.Equals(Name, "Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase);

        public bool IsAspNet =>
            string.Equals(Name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase);

        public DotnetRuntime(string name, Version version)
        {
            Name = name;
            Version = version;
        }
    }

    internal partial class DotnetRuntime
    {
        public static DotnetRuntime[] GetAllInstalled()
        {
            var sharedDirPath = Path.Combine(DotnetInstallation.GetDirectoryPath(), "shared");
            if (!Directory.Exists(sharedDirPath))
                throw new DirectoryNotFoundException("Could not find directory containing .NET runtime binaries.");

            var result = new List<DotnetRuntime>();
            foreach (var runtimeDirPath in Directory.GetDirectories(sharedDirPath))
            {
                var name = Path.GetFileName(runtimeDirPath);

                foreach (var runtimeVersionDirPath in Directory.GetDirectories(runtimeDirPath))
                {
                    var version = VersionEx.TryParse(Path.GetFileName(runtimeVersionDirPath));
                    if (version is not null)
                        result.Add(new DotnetRuntime(name, version));
                }
            }

            return result.ToArray();
        }
    }
}