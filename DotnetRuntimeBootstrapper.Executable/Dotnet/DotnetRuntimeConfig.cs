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
        public DotnetRuntime Runtime { get; }

        public DotnetRuntimeConfig(DotnetRuntime runtime) =>
            Runtime = runtime;
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
            {
                // Config lists multiple frameworks, usually the base and the app-specific one,
                // for example: Microsoft.NETCore.App and Microsoft.WindowsDesktop.App.
                // We only care about the app-specific one.
                var runtime = runtimeConfigJson
                    .TryGetChild("runtimeOptions")?
                    .TryGetChild("frameworks")?
                    .EnumerateChildren()
                    .Select(ParseRuntime)
                    .OrderBy(r => !r.IsBase)
                    .FirstOrDefault();

                if (runtime is not null)
                    return new DotnetRuntimeConfig(runtime);
            }

            // .NET 5 and lower
            {
                var runtime = runtimeConfigJson
                    .TryGetChild("runtimeOptions")?
                    .TryGetChild("framework")?
                    .Pipe(ParseRuntime);

                if (runtime is not null)
                    return new DotnetRuntimeConfig(runtime);
            }

            throw new ApplicationException("Could not parse runtime infos from runtime config.");
        }

        public static DotnetRuntimeConfig Load(string filePath) => Parse(File.ReadAllText(filePath));
    }
}