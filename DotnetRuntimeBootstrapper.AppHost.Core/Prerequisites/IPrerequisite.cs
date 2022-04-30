using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

public interface IPrerequisite
{
    string DisplayName { get; }

    bool IsInstalled();

    IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress);
}