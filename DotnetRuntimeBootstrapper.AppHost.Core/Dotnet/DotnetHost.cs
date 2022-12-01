using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Dotnet;

// Host resolver headers:
// https://github.com/dotnet/runtime/blob/57bfe47451/src/native/corehost/hostfxr.h

// Muxer implementation:
// https://github.com/dotnet/runtime/blob/57bfe47451/src/native/corehost/fxr/fx_muxer.cpp

// .NET CLI host implementation:
// https://github.com/dotnet/runtime/blob/57bfe47451/src/native/corehost/corehost.cpp

internal partial class DotnetHost : IDisposable
{
    private readonly NativeLibrary _hostResolverLibrary;

    public DotnetHost(NativeLibrary hostResolverLibrary) =>
        _hostResolverLibrary = hostResolverLibrary;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
    private delegate void ErrorWriterFn(string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
    private delegate void SetErrorWriterFn(ErrorWriterFn errorWriterFn);

    private SetErrorWriterFn GetSetErrorWriterFn() =>
        _hostResolverLibrary.GetFunction<SetErrorWriterFn>("hostfxr_set_error_writer");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
    private delegate int InitializeForCommandLineFn(
        int argc,
        string[] argv,
        IntPtr parameters,
        out IntPtr handle
    );

    private InitializeForCommandLineFn GetInitializeForCommandLineFn() =>
        _hostResolverLibrary.GetFunction<InitializeForCommandLineFn>("hostfxr_initialize_for_dotnet_command_line");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
    private delegate int RunAppFn(IntPtr handle);

    private RunAppFn GetRunAppFn() =>
        _hostResolverLibrary.GetFunction<RunAppFn>("hostfxr_run_app");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
    private delegate int CloseFn(IntPtr handle);

    private CloseFn GetCloseFn() =>
        _hostResolverLibrary.GetFunction<CloseFn>("hostfxr_close");

    private IntPtr Initialize(string targetFilePath, string[] args)
    {
        // Route errors to a buffer
        var errorBuffer = new StringBuilder();
        GetSetErrorWriterFn()(s => errorBuffer.AppendLine(s));

        // Initialize the host as if we're running the app from command line
        var status = GetInitializeForCommandLineFn()(
            args.Length + 1,
            args.Prepend(targetFilePath).ToArray(),
            IntPtr.Zero,
            out var handle
        );

        if (status != 0)
        {
            throw new ApplicationException(
                string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "Failed to initialize .NET host.",
                        "Target: " + targetFilePath,
                        "Arguments: [" + string.Join(", ", args) + ']',
                        "Status: " + status,
                        errorBuffer.Length > 0 ? errorBuffer.ToString() : "No error messages reported."
                    }
                )
            );
        }

        return handle;
    }

    private int Run(IntPtr handle)
    {
        try
        {
            return GetRunAppFn()(handle);
        }
        catch (SEHException ex)
        {
            // This is thrown when the app crashes with an unhandled exception.
            // Unfortunately, there is no way to get the original exception or its message.
            // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/23
            throw new ApplicationException(
                "Application crashed with an unhandled exception. " +
                "Unfortunately, it was not possible to retrieve the exception message or its stacktrace. " +
                "Please check the Windows Event Viewer to see if the runtime logged any additional information. " +
                "If you are the developer of the application, consider adding a global exception handler to provide a more detailed error message to the user.",
                ex
            );
        }
    }

    private void Close(IntPtr handle) =>
        // Closing the handle doesn't completely unload the host.
        // There are some native libraries loaded by the resolver
        // that are purposefully leaked to preserve state.
        // This means that we can't successfully initialize the host
        // twice, but that shouldn't matter since we'd only attempt
        // to do it again if the first attempt failed in the first place.
        GetCloseFn()(handle);

    public int Run(string targetFilePath, string[] args)
    {
        var handle = Initialize(targetFilePath, args);

        try
        {
            return Run(handle);
        }
        finally
        {
            Close(handle);
        }
    }

    public void Dispose() => _hostResolverLibrary.Dispose();
}

internal partial class DotnetHost
{
    private static string GetHostResolverFilePath()
    {
        // Host resolver (hostfxr) location strategy:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/fxr_resolver.cpp#L55-L135
        // 1. Find the hostfxr directory containing versioned subdirectories
        // 2. Get the hostfxr.dll from the subdirectory with the highest version number

        var hostResolverRootDirPath = PathEx.Combine(DotnetInstallation.GetDirectoryPath(), "host", "fxr");
        if (!Directory.Exists(hostResolverRootDirPath))
            throw new DirectoryNotFoundException("Could not find directory containing hostfxr.dll.");

        var hostResolverFilePath = (
            from dirPath in Directory.GetDirectories(hostResolverRootDirPath)
            let version = VersionEx.TryParse(Path.GetFileName(dirPath))
            let filePath = Path.Combine(dirPath, "hostfxr.dll")
            where version is not null
            where File.Exists(filePath)
            orderby version descending
            select filePath
        ).FirstOrDefault();

        return
            hostResolverFilePath ??
            throw new FileNotFoundException("Could not find hostfxr.dll.");
    }

    public static DotnetHost Load() => new(
        NativeLibrary.Load(GetHostResolverFilePath())
    );
}