using System;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public abstract class BootstrapperBase
{
    protected abstract void ReportError(string message);

    protected abstract bool InstallPrerequisites(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    );

    public int Run(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportError(e.ExceptionObject.ToString());

        try
        {
            var targetAssembly = TargetAssembly.Resolve();

            try
            {
                // Attempt to run the target first without any checks (hot path)
                return targetAssembly.Run(args);
            }
            // Possible exception causes:
            // - .NET host not found (DirectoryNotFoundException)
            // - .NET host failed to initialize (ApplicationException)
            catch
            {
                // Check for and install missing prerequisites
                var missingPrerequisites = targetAssembly.GetMissingPrerequisites();
                if (missingPrerequisites.Any())
                {
                    var isInstallationSuccessful = InstallPrerequisites(targetAssembly, missingPrerequisites);

                    // User canceled installation or reboot is required
                    if (!isInstallationSuccessful)
                        return 0xB007;

                    // Reset environment to update PATH and other variables
                    // that may have been changed by the installation process.
                    EnvironmentEx.RefreshEnvironmentVariables();
                }

                // Attempt to run the target again, this time without ignoring exceptions
                return targetAssembly.Run(args);
            }
        }
        catch (Exception ex)
        {
            ReportError(ex.ToString());
            return 0xDEAD;
        }
    }
}