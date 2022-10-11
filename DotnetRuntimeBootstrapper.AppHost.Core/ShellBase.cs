using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public abstract class ShellBase
{
    protected virtual void ReportError(string message)
    {
        // Report to Windows Event Log
        // Inspired by:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51
        try
        {
            var applicationFilePath = typeof(ShellBase).Assembly.Location;
            var applicationName = Path.GetFileName(applicationFilePath);
            var bootstrapperVersion = typeof(ShellBase).Assembly.GetName().Version.ToString(3);

            var content = string.Join(
                Environment.NewLine,
                new[]
                {
                    "Description: " + "Bootstrapper for a .NET application has failed.",
                    "Application: " + applicationName,
                    "Path: " + applicationFilePath,
                    "AppHost: " + $".NET Runtime Bootstrapper v{bootstrapperVersion}",
                    "Message: " + message
                }
            );

            EventLog.WriteEntry(".NET Runtime", content, EventLogEntryType.Error, 1023);
        }
        catch
        {
            // Ignore
        }
    }

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
                // Hot path: attempt to run the target first without any checks
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