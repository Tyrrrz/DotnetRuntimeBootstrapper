using System;
using DotnetRuntimeBootstrapper.Executable.Native.Windows;

namespace DotnetRuntimeBootstrapper.Executable.Native.Dotnet
{
    internal partial class DotnetHostFrameworkResolver
    {
        private readonly NativeLibrary _hostfxrLib;

        public DotnetHostFrameworkResolver(NativeLibrary hostfxrLib) =>
            _hostfxrLib = hostfxrLib;

        public DotnetHostContext InitializeForCommandLine(string[] args)
        {
            var initialize = _hostfxrLib.GetFunction<HostfxrInitializeForCommandLineFn>(
                "hostfxr_initialize_for_dotnet_command_line"
            );

            return initialize(args.Length, args, IntPtr.Zero, out var contextHandle) == 0
                ? new DotnetHostContext(_hostfxrLib, contextHandle)
                : throw new InvalidOperationException("Failed to initialize .NET runtime context for command line.");
        }
    }

    internal partial class DotnetHostFrameworkResolver
    {
        private delegate int HostfxrInitializeForCommandLineFn(
            int argc,
            string[] argv,
            IntPtr parameters,
            out IntPtr handle
        );
    }
}