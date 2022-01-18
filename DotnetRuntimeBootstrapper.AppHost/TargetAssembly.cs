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

    public string RuntimeConfigFilePath => Path.ChangeExtension(FilePath, "runtimeconfig.json");

    public string Title { get; }

    public TargetAssembly(string filePath, string title)
    {
        FilePath = filePath;
        Title = title;
    }

    public IPrerequisite[] GetMissingPrerequisites()
    {
        var prerequisites = new List<IPrerequisite>
        {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226Prerequisite(),
                new WindowsUpdate3063858Prerequisite(),
                new VisualCppPrerequisite()
        }.ToList();

        var runtimes = DotnetRuntime.FromRuntimeConfig(RuntimeConfigFilePath);

        foreach (var runtime in runtimes)
        {
            prerequisites.Add(new DotnetPrerequisite(runtime));
        }

        return prerequisites.Where(p => !p.IsInstalled).ToArray();
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