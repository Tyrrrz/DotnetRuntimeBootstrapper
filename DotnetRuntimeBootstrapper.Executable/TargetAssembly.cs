﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Dotnet;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

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

        public IPrerequisite[] GetMissingPrerequisites()
        {
            var runtimeConfig = DotnetRuntimeConfig.Load(Path.ChangeExtension(FilePath, "runtimeconfig.json"));

            var targetRuntimeInfo =
                runtimeConfig.Runtimes.OrderBy(r => !r.IsBase).LastOrDefault() ??
                throw new InvalidOperationException("Could not resolve target runtime from runtime config.");

            return new IPrerequisite[]
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226Prerequisite(),
                new WindowsUpdate3063858Prerequisite(),
                new VisualCppPrerequisite(),
                new DotnetPrerequisite(targetRuntimeInfo)
            }.Where(p => !p.CheckIfInstalled()).ToArray();
        }

        public int Run(string[] args) => DotnetHost.Initialize().Run(FilePath, args);
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
}