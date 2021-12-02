using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Dotnet
{
    internal partial class DotnetRuntimeInfo
    {
        public string Name { get; }

        public Version Version { get; }

        public bool IsBase =>
            string.Equals(Name, "Microsoft.NETCore.App", StringComparison.OrdinalIgnoreCase);

        public bool IsWindowsDesktop =>
            string.Equals(Name, "Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase);

        public bool IsAspNet =>
            string.Equals(Name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase);

        public DotnetRuntimeInfo(string name, Version version)
        {
            Name = name;
            Version = version;
        }
    }

    internal partial class DotnetRuntimeInfo
    {
        public static DotnetRuntimeInfo[] GetInstalled() =>
        (
            from runtimeDirPath in Directory.GetDirectories(Path.Combine(DotnetInstallation.GetDirectoryPath(), "shared"))
            let name = Path.GetFileName(runtimeDirPath)
            from runtimeVersionDirPath in Directory.GetDirectories(runtimeDirPath)
            let version = VersionEx.TryParse(Path.GetFileName(runtimeVersionDirPath))
            where version is not null
            select new DotnetRuntimeInfo(name, version)
        ).ToArray();
    }
}