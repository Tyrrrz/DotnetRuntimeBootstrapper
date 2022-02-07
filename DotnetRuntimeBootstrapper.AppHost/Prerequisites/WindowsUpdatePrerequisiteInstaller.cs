﻿using System;
using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites;

internal class WindowsUpdatePrerequisiteInstaller : IPrerequisiteInstaller
{
    public IPrerequisite Prerequisite { get; }

    public string FilePath { get; }

    public WindowsUpdatePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
    {
        Prerequisite = prerequisite;
        FilePath = filePath;
    }

    public PrerequisiteInstallerResult Run()
    {
        var exitCode = CommandLine.Run("wusa", new[] { FilePath, "/quiet", "/norestart" }, true);

        // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/24#issuecomment-1021447102
        if (exitCode is 3010 or 3011 or 1641)
            return PrerequisiteInstallerResult.RebootRequired;

        if (exitCode != 0)
        {
            throw new ApplicationException(
                $"Failed to install {Prerequisite.DisplayName}. " +
                $"Exit code: {exitCode}. " +
                "Please try to install this component manually."
            );
        }

        return PrerequisiteInstallerResult.Success;
    }
}