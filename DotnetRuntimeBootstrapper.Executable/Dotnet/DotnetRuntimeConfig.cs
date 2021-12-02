using System;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.Executable.Dotnet
{
    internal partial class DotnetRuntimeConfig
    {
        public DotnetRuntimeInfo[] Runtimes { get; }

        public DotnetRuntimeConfig(DotnetRuntimeInfo[] runtimes) =>
            Runtimes = runtimes;
    }

    internal partial class DotnetRuntimeConfig
    {
        private static DotnetRuntimeInfo ParseRuntimeInfo(JsonNode json)
        {
            var name = json.TryGetChild("name")?.TryGetString();
            var version = json.TryGetChild("version")?.TryGetString()?.Pipe(VersionEx.TryParse);

            if (string.IsNullOrEmpty(name) || version is null)
                throw new InvalidOperationException("Could not parse runtime info.");

            return new DotnetRuntimeInfo(name, version);
        }

        public static DotnetRuntimeConfig Parse(string json)
        {
            var runtimeConfigJson =
                Json.TryParse(json) ??
                throw new InvalidOperationException("Could not parse runtime config.");

            // .NET 6 and higher
            var runtimeInfos = runtimeConfigJson
                .TryGetChild("runtimeOptions")?
                .TryGetChild("frameworks")?
                .EnumerateChildren()
                .Select(ParseRuntimeInfo)
                .ToArray();

            if (runtimeInfos is not null)
                return new DotnetRuntimeConfig(runtimeInfos);

            // .NET 5 and lower
            var runtimeInfo = runtimeConfigJson
                .TryGetChild("runtimeOptions")?
                .TryGetChild("framework")?
                .Pipe(ParseRuntimeInfo);

            if (runtimeInfo is not null)
                return new DotnetRuntimeConfig(new[] {runtimeInfo});

            throw new InvalidOperationException("Could not parse runtime config.");
        }

        public static DotnetRuntimeConfig Load(string filePath) => Parse(File.ReadAllText(filePath));
    }
}