using System;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites
{
    public interface IPrerequisite
    {
        string DisplayName { get; }

        bool IsInstalled { get; }

        bool IsRebootRequired { get; }

        IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress);
    }
}