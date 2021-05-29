using System;

namespace DotnetRuntimeBootstrapper.Launcher.RuntimeComponents
{
    public interface IRuntimeComponent
    {
        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        DownloadedRuntimeComponentInstaller DownloadInstaller(Action<double>? handleProgress);
    }
}