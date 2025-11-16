using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Dotnet;

internal partial class DotnetRuntime(string name, Version version)
{
    public string Name { get; } = name;

    public Version Version { get; } = version;

    public bool IsBase =>
        string.Equals(Name, "Microsoft.NETCore.App", StringComparison.OrdinalIgnoreCase);

    public bool IsWindowsDesktop =>
        string.Equals(Name, "Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase);

    public bool IsAspNet =>
        string.Equals(Name, "Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase);

    public bool IsSupersededBy(DotnetRuntime other) =>
        string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
        && Version.Major == other.Version.Major
        && Version <= other.Version;
}

internal partial class DotnetRuntime
{
    public static DotnetRuntime[] GetAllInstalled()
    {
        var sharedDirPath = Path.Combine(DotnetInstallation.GetDirectoryPath(), "shared");

        if (!Directory.Exists(sharedDirPath))
        {
            throw new DirectoryNotFoundException(
                "Failed to find the directory containing .NET runtime binaries."
            );
        }

        return (
            from runtimeDirPath in Directory.GetDirectories(sharedDirPath)
            let name = Path.GetFileName(runtimeDirPath)
            from runtimeVersionDirPath in Directory.GetDirectories(runtimeDirPath)
            let version = Version.TryParse(Path.GetFileName(runtimeVersionDirPath), out var v)
                ? v
                : null
            where version is not null
            select new DotnetRuntime(name, version)
        ).ToArray();
    }

    public static DotnetRuntime[] GetAllTargets(string runtimeConfigFilePath)
    {
        static DotnetRuntime ParseRuntime(JsonNode json)
        {
            var name = json.TryGetChild("name")?.TryGetString();

            var version = json.TryGetChild("version")
                ?.TryGetString()
                ?.Pipe(s => Version.TryParse(s, out var v) ? v : null);

            return !string.IsNullOrEmpty(name) && version is not null
                ? new DotnetRuntime(name, version)
                : throw new InvalidOperationException(
                    "Failed to extract runtime information from the provided runtime configuration."
                );
        }

        var json =
            Json.TryParse(File.ReadAllText(runtimeConfigFilePath))
            ?? throw new InvalidOperationException(
                $"Failed to parse runtime configuration at '{runtimeConfigFilePath}'."
            );

        return
            // Multiple targets
            json.TryGetChild("runtimeOptions")
                ?.TryGetChild("frameworks")
                ?.EnumerateChildren()
                .Select(ParseRuntime)
                .ToArray()
            ??
            // Single target
            json.TryGetChild("runtimeOptions")
                ?.TryGetChild("framework")
                ?.ToSingletonEnumerable()
                .Select(ParseRuntime)
                .ToArray()
            ?? throw new InvalidOperationException(
                "Failed to resolve the target runtime from runtime configuration."
            );
    }
}
