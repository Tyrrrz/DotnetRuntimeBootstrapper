namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

public interface IPrerequisiteInstaller
{
    IPrerequisite Prerequisite { get; }

    string FilePath { get; }

    PrerequisiteInstallerResult Run();
}