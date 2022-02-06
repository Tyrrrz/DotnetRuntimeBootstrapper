using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites;

internal class WindowsUpdatePrerequisiteInstaller : IPrerequisiteInstaller
{
    public IPrerequisite Prerequisite { get; }

    public string FilePath { get; }

    public WindowsUpdatePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
    {
        Prerequisite = prerequisite;
        FilePath = filePath;
    }

    public PrerequisiteInstallerResult Run()
    {
        var exitCode = CommandLine.Run("wusa", new[] { FilePath, "/quiet", "/norestart" }, true);

        // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/24#issuecomment-1021447102
        return exitCode is 3010 or 3011 or 1641
            ? PrerequisiteInstallerResult.RebootRequired
            : PrerequisiteInstallerResult.Success;
    }
}