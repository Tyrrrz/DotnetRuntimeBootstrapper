using System;

namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public interface IRuntimeComponent
    {
        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        IRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress);
    }
}