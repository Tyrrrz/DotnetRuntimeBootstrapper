using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites;

internal class ExecutablePrerequisiteInstaller : IPrerequisiteInstaller
{
    public IPrerequisite Prerequisite { get; }

    public string FilePath { get; }

    public ExecutablePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
    {
        Prerequisite = prerequisite;
        FilePath = filePath;
    }

    public void Run() => CommandLine.Run(FilePath, new[] {"/install", "/quiet", "/norestart"}, true);
}