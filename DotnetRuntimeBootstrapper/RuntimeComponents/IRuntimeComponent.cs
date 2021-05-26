using System;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public interface IRuntimeComponent
    {
        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        DownloadedRuntimeComponentInstaller DownloadInstaller(Action<double> handleProgress);
    }
}