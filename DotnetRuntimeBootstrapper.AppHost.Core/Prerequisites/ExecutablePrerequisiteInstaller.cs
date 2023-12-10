using System;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class ExecutablePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
    : IPrerequisiteInstaller
{
    public IPrerequisite Prerequisite { get; } = prerequisite;

    public string FilePath { get; } = filePath;

    public PrerequisiteInstallerResult Run()
    {
        var exitCode = CommandLine.Run(
            FilePath,
            new[] { "/install", "/quiet", "/norestart" },
            true
        );

        // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/24#issuecomment-1021447102
        if (exitCode is 3010 or 3011 or 1641)
            return PrerequisiteInstallerResult.RebootRequired;

        if (exitCode != 0)
        {
            throw new ApplicationException(
                $"Failed to install {Prerequisite.DisplayName}. "
                    + $"Exit code: {exitCode}. "
                    + "Please try to install this component manually."
            );
        }

        return PrerequisiteInstallerResult.Success;
    }
}
