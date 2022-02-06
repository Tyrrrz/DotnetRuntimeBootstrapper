using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Dotnet;
using DotnetRuntimeBootstrapper.AppHost.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Utils;
using DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost;

public partial class TargetAssembly
{
    public string FilePath { get; }

    public string Title { get; }

    public TargetAssembly(string filePath, string title)
    {
        FilePath = filePath;
        Title = title;
    }

    private DotnetRuntime[] GetRuntimes()
    {
        var configFilePath = Path.ChangeExtension(FilePath, "runtimeconfig.json");
        var runtimes = DotnetRuntime.GetAllTargets(configFilePath).ToList();

        // Non-base runtimes already include the base runtime, so
        // filter out unnecessary targets.
        if (runtimes.Count > 1)
        {
            foreach (var nonBaseRuntime in runtimes.Where(r => !r.IsBase).ToArray())
            {
                // Only filter out compatible base runtimes!
                // If the app targets .NET 5 desktop and .NET 6 base,
                // we still need to keep the base.
                // This is very unlikely to happen, but it's nice to
                // be able to handle those edge cases.
                runtimes.RemoveAll(r =>
                    r.IsBase &&
                    r.Version.Major == nonBaseRuntime.Version.Major &&
                    r.Version <= nonBaseRuntime.Version
                );
            }
        }

        return runtimes.ToArray();
    }

    public IPrerequisite[] GetMissingPrerequisites()
    {
        var prerequisites = new List<IPrerequisite>
        {
            new WindowsUpdate2999226Prerequisite(),
            new WindowsUpdate3063858Prerequisite(),
            new VisualCppPrerequisite()
        };

        foreach (var runtime in GetRuntimes())
            prerequisites.Add(new DotnetPrerequisite(runtime));

        // Filter out prerequisites that are already installed
        prerequisites.RemoveAll(p => p.IsInstalled);

        return prerequisites.ToArray();
    }

    public int Run(string[] args) => DotnetHost.Initialize().Run(FilePath, args);
}

public partial class TargetAssembly
{
    public static TargetAssembly Resolve()
    {
        // Target assembly file name is provided to the bootstrapper in an embedded
        // resource. It's injected during the build process by a special MSBuild task.
        var fileName = typeof(TargetAssembly).Assembly.GetManifestResourceString(nameof(TargetAssembly));
        var filePath = Path.Combine(PathEx.ExecutingDirectoryPath, fileName);

        // Ensure that the target assembly is present in the executing directory
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Could not find target assembly '{fileName}'.");

        var title =
            FileVersionInfo.GetVersionInfo(filePath).ProductName?.NullIfEmptyOrWhiteSpace() ??
            Path.GetFileNameWithoutExtension(filePath);

        return new TargetAssembly(filePath, title);
    }
}