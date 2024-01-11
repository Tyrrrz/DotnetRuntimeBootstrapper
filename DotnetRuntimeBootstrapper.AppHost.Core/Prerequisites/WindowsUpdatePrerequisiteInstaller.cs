using System.ComponentModel;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

internal class WindowsUpdatePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
    : IPrerequisiteInstaller
{
    public IPrerequisite Prerequisite { get; } = prerequisite;

    public string FilePath { get; } = filePath;

    public PrerequisiteInstallerResult Run()
    {
        try
        {
            var exitCode = CommandLine.Run("wusa", [FilePath, "/quiet", "/norestart"], true);

            // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/24#issuecomment-1021447102
            if (exitCode is 3010 or 3011 or 1641)
                return PrerequisiteInstallerResult.RebootRequired;

            if (exitCode != 0)
            {
                throw new BootstrapperException(
                    $"Failed to install '{Prerequisite.DisplayName}'. "
                        + $"Exit code: {exitCode}. "
                        + $"Restart the application to try again, or install this component manually."
                );
            }

            return PrerequisiteInstallerResult.Success;
        }
        // Installation was canceled before the process could start
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            throw new BootstrapperException(
                $"Failed to install '{Prerequisite.DisplayName}'. "
                    + $"The operation was canceled. "
                    + $"Restart the application to try again, or install this component manually.",
                ex
            );
        }
    }
}
