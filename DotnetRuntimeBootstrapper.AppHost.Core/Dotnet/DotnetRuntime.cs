using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Utils;
using DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.AppHost.Dotnet;

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

        return (
            from runtimeDirPath in Directory.GetDirectories(sharedDirPath)
            let name = Path.GetFileName(runtimeDirPath)
            from runtimeVersionDirPath in Directory.GetDirectories(runtimeDirPath)
            let version = VersionEx.TryParse(Path.GetFileName(runtimeVersionDirPath))
            where version is not null
            select new DotnetRuntime(name, version)
        ).ToArray();
    }

    public static DotnetRuntime[] GetAllTargets(string runtimeConfigFilePath)
    {
        static DotnetRuntime ParseRuntime(JsonNode json)
        {
            var name = json.TryGetChild("name")?.TryGetString();
            var version = json.TryGetChild("version")?.TryGetString()?.Pipe(VersionEx.TryParse);

            if (string.IsNullOrEmpty(name) || version is null)
                throw new ApplicationException("Could not parse runtime info from runtime config.");

            return new DotnetRuntime(name, version);
        }

        var json =
            Json.TryParse(File.ReadAllText(runtimeConfigFilePath)) ??
            throw new ApplicationException($"Failed to parse runtime config '{runtimeConfigFilePath}'.");

        return
            // Multiple targets
            json
                .TryGetChild("runtimeOptions")?
                .TryGetChild("frameworks")?
                .EnumerateChildren()
                .Select(ParseRuntime)
                .ToArray() ??

            // Single target
            json
                .TryGetChild("runtimeOptions")?
                .TryGetChild("framework")?
                .Pipe(ParseRuntime)
                .Enumerate()
                .ToArray() ??

            throw new ApplicationException("Could not resolve target runtime from runtime config.");
    }
}