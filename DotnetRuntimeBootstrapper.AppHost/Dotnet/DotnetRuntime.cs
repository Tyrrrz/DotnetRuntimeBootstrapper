using System;
using System.Collections.Generic;
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

    public static DotnetRuntime FromRuntimeConfig(string filePath)
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
            Json.TryParse(File.ReadAllText(filePath)) ??
            throw new ApplicationException($"Failed to parse runtime config '{filePath}'.");

        return
            // .NET 6 and higher
            // Config lists multiple frameworks, usually the base and the app-specific one,
            // for example: Microsoft.NETCore.App and Microsoft.WindowsDesktop.App.
            // We only care about the app-specific one.
            json
                .TryGetChild("runtimeOptions")?
                .TryGetChild("frameworks")?
                .EnumerateChildren()
                .Select(ParseRuntime)
                .OrderBy(r => r.IsBase) // base comes last
                .FirstOrDefault() ??

            // .NET 5 and lower
            json
                .TryGetChild("runtimeOptions")?
                .TryGetChild("framework")?
                .Pipe(ParseRuntime) ??

            throw new ApplicationException("Could not resolve target runtime from runtime config.");
    }
}