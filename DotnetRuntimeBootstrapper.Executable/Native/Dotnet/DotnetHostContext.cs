using System;
using DotnetRuntimeBootstrapper.Executable.Native.Windows;

namespace DotnetRuntimeBootstrapper.Executable.Native.Dotnet
{
    internal partial class DotnetHostContext : IDisposable
    {
        private readonly NativeLibrary _hostfxrLib;
        private readonly IntPtr _handle;

        public DotnetHostContext(NativeLibrary hostfxrLib, IntPtr handle)
        {
            _hostfxrLib = hostfxrLib;
            _handle = handle;
        }

        public int Run() => _hostfxrLib.GetFunction<HostfxrRunAppFn>("hostfxr_run_app")(_handle);

        public void Dispose() => _hostfxrLib.GetFunction<HostfxrCloseFn>("hostfxr_close")(_handle);
    }

    internal partial class DotnetHostContext
    {
        private delegate int HostfxrCloseFn(IntPtr handle);

        private delegate int HostfxrRunAppFn(IntPtr handle);
    }
}