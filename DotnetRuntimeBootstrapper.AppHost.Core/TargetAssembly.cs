using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Dotnet;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

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

        // Desktop runtimes already include the base runtimes, so we can filter out unnecessary targets
        // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/30
        if (runtimes.Count > 1)
        {
            foreach (var desktopRuntime in runtimes.Where(r => r.IsWindowsDesktop).ToArray())
            {
                // Only filter out compatible base runtimes!
                // If the app targets .NET 5 desktop and .NET 6 base,
                // we still need to keep the base.
                // Very unlikely that such a situation will happen though.
                runtimes.RemoveAll(r =>
                    r.IsBase &&
                    r.Version.Major == desktopRuntime.Version.Major &&
                    r.Version <= desktopRuntime.Version
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
        prerequisites.RemoveAll(p => p.IsInstalled());

        return prerequisites.ToArray();
    }

    public int Run(string[] args)
    {
        using var host = DotnetHost.Initialize();
        return host.Run(FilePath, args);
    }
}

public partial class TargetAssembly
{
    public static TargetAssembly Resolve()
    {
        // Target assembly file name is provided to the bootstrapper in an embedded
        // resource. It's injected during the build process by a special MSBuild task.
        var fileName = typeof(TargetAssembly).Assembly.GetManifestResourceString(nameof(TargetAssembly));

        var filePath = Path.Combine(
            Path.GetDirectoryName(EnvironmentEx.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory,
            fileName
        );

        // Ensure that the target assembly is present in the executing directory
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Could not find target assembly '{fileName}'.");

        var title =
            FileVersionInfo.GetVersionInfo(filePath).ProductName?.NullIfEmptyOrWhiteSpace() ??
            Path.GetFileNameWithoutExtension(filePath);

        return new TargetAssembly(filePath, title);
    }
}