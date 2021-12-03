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
        public DotnetRuntime[] Runtimes { get; }

        public DotnetRuntimeConfig(DotnetRuntime[] runtimes) =>
            Runtimes = runtimes;
    }

    internal partial class DotnetRuntimeConfig
    {
        private static DotnetRuntime ParseRuntime(JsonNode json)
        {
            var name = json.TryGetChild("name")?.TryGetString();
            var version = json.TryGetChild("version")?.TryGetString()?.Pipe(VersionEx.TryParse);

            if (string.IsNullOrEmpty(name) || version is null)
                throw new ApplicationException("Could not parse runtime info from runtime config.");

            return new DotnetRuntime(name, version);
        }

        public static DotnetRuntimeConfig Parse(string json)
        {
            var runtimeConfigJson =
                Json.TryParse(json) ??
                throw new ApplicationException("Could not parse runtime config.");

            // .NET 6 and higher
            var runtimes = runtimeConfigJson
                .TryGetChild("runtimeOptions")?
                .TryGetChild("frameworks")?
                .EnumerateChildren()
                .Select(ParseRuntime)
                .ToArray();

            if (runtimes is not null)
                return new DotnetRuntimeConfig(runtimes);

            // .NET 5 and lower
            var runtime = runtimeConfigJson
                .TryGetChild("runtimeOptions")?
                .TryGetChild("framework")?
                .Pipe(ParseRuntime);

            if (runtime is not null)
                return new DotnetRuntimeConfig(new[] {runtime});

            throw new ApplicationException("Could not parse runtime infos from runtime config.");
        }

        public static DotnetRuntimeConfig Load(string filePath) => Parse(File.ReadAllText(filePath));
    }
}