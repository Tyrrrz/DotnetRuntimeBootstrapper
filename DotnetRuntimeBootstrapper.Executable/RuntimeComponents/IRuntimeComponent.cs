using System;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public interface IRuntimeComponent
    {
        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        RuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress);
    }
}