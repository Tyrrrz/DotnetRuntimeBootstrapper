using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;
using QuickJson;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class TargetAssembly
    {
        public string FilePath { get; }

        public string Title { get; }

        public TargetAssembly(string filePath, string title)
        {
            FilePath = filePath;
            Title = title;
        }

        private TargetRuntimeInfo GetRuntimeInfo()
        {
            var runtimeConfigFilePath = Path.ChangeExtension(FilePath, "runtimeconfig.json");

            var runtimeConfigJson = Json.TryParse(File.ReadAllText(runtimeConfigFilePath));
            if (runtimeConfigJson is null)
            {
                throw new InvalidOperationException(
                    $"Could not parse runtime config file '{runtimeConfigFilePath}'."
                );
            }

            var framework =
                runtimeConfigJson
                    .TryGetChild("runtimeOptions")?
                    .TryGetChild("framework") ??

                // When there are multiple frameworks, take the one that
                // isn't the base framework (i.e. not "Microsoft.NETCore.App").
                runtimeConfigJson
                    .TryGetChild("runtimeOptions")?
                    .TryGetChild("frameworks")?
                    .EnumerateChildren()
                    .FirstOrDefault(j =>
                        !string.Equals(
                            j.TryGetChild("name")?.TryGetString(),
                            "Microsoft.NETCore.App",
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

            var name = framework?.TryGetChild("name")?.TryGetString();
            var version = framework?.TryGetChild("version")?.TryGetString()?.Pipe(VersionEx.TryParse);

            if (string.IsNullOrEmpty(name) || version is null)
            {
                throw new InvalidOperationException(
                    $"Could not resolve target runtime from runtime config file '{runtimeConfigFilePath}'."
                );
            }

            return new TargetRuntimeInfo(name, version);
        }

        public IPrerequisite[] GetMissingPrerequisites()
        {
            var runtime = GetRuntimeInfo();

            return new IPrerequisite[]
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226Prerequisite(),
                new WindowsUpdate3063858Prerequisite(),
                new VisualCppPrerequisite(),
                new DotnetPrerequisite(runtime.Name, runtime.Version)
            }.Where(p => !p.CheckIfInstalled()).ToArray();
        }
    }

    public partial class TargetAssembly
    {
        public static TargetAssembly Resolve()
        {
            // Target assembly file name is provided to the bootstrapper in an embedded
            // resource. It's injected during the build process by a special MSBuild task.
            var filePath = Path.Combine(
                PathEx.ExecutingDirectoryPath,
                typeof(TargetAssembly).Assembly.GetManifestResourceString(nameof(TargetAssembly))
            );

            // Ensure that the target assembly is present in the executing directory
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Could not find target assembly '{filePath}'.");

            var title =
                FileVersionInfo.GetVersionInfo(filePath).ProductName?.NullIfEmptyOrWhiteSpace() ??
                Path.GetFileNameWithoutExtension(filePath);

            return new TargetAssembly(filePath, title);
        }
    }

    public partial class TargetAssembly
    {
        private class TargetRuntimeInfo
        {
            public string Name { get; }

            public Version Version { get; }

            public TargetRuntimeInfo(string name, Version version)
            {
                Name = name;
                Version = version;
            }
        }
    }
}