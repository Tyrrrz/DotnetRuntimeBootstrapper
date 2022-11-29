using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public abstract class BootstrapperBase
{
    protected const string LegacyAcceptPromptEnvironmentVariable = "DOTNET_INSTALL_PREREQUISITES";
    protected const string AcceptPromptEnvironmentVariable = "DOTNET_ENABLE_BOOTSTRAPPER";

    // Installation prompt can be pre-accepted using an environment variable
    protected bool IsPromptPreAccepted { get; } =
        string.Equals(
            Environment.GetEnvironmentVariable(AcceptPromptEnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase
        ) ||
        string.Equals(
            Environment.GetEnvironmentVariable(LegacyAcceptPromptEnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

    protected virtual void ReportError(string message)
    {
        // Report to the Windows Event Log. Adapted from:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51
        try
        {
            var applicationFilePath = typeof(BootstrapperBase).Assembly.Location;
            var applicationName = Path.GetFileName(applicationFilePath);
            var bootstrapperVersion = typeof(BootstrapperBase).Assembly.GetName().Version.ToString(3);

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

    protected abstract bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    );

    protected abstract bool Install(
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
                    var isPromptAccepted = IsPromptPreAccepted || Prompt(targetAssembly, missingPrerequisites);
                    var isReadyToRun = isPromptAccepted && Install(targetAssembly, missingPrerequisites);

                    // User did not accept the installation or reboot is required
                    if (!isReadyToRun)
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