using System;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public interface IPrerequisite
    {
        string Id { get; }

        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        IPrerequisiteInstaller DownloadInstaller(Action<double>? handleProgress);
    }
}