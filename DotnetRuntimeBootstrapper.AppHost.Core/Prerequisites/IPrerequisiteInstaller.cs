using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

public interface IPrerequisiteInstaller : IDisposable
{
    IPrerequisite Prerequisite { get; }

    PrerequisiteInstallerResult Run();
}
