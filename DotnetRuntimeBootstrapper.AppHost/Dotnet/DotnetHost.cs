using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DotnetRuntimeBootstrapper.AppHost.Native;
using DotnetRuntimeBootstrapper.AppHost.Utils;
using DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Dotnet
{
    internal partial class DotnetHost : IDisposable
    {
        private readonly NativeLibrary _hostfxrLib;

        public DotnetHost(NativeLibrary hostfxrLib) =>
            _hostfxrLib = hostfxrLib;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate int HostfxrInitializeForCommandLineFn(
            int argc,
            string[] argv,
            IntPtr parameters,
            out IntPtr handle
        );

        private HostfxrInitializeForCommandLineFn GetInitializeForCommandLineFn() =>
            _hostfxrLib.GetFunction<HostfxrInitializeForCommandLineFn>("hostfxr_initialize_for_dotnet_command_line");

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int HostfxrRunAppFn(IntPtr handle);

        private HostfxrRunAppFn GetRunAppFn() =>
            _hostfxrLib.GetFunction<HostfxrRunAppFn>("hostfxr_run_app");

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int HostfxrCloseFn(IntPtr handle);

        private HostfxrCloseFn GetCloseFn() =>
            _hostfxrLib.GetFunction<HostfxrCloseFn>("hostfxr_close");

        public int Run(string targetFilePath, string[] args)
        {
            var argsCombined = args.Prepend(targetFilePath).ToArray();

            if (GetInitializeForCommandLineFn()(argsCombined.Length, argsCombined, IntPtr.Zero, out var handle) != 0)
            {
                throw new ApplicationException(
                    $"Failed to initialize .NET host with arguments [{string.Join(", ", argsCombined)}]."
                );
            }

            try
            {
                // This returns program's exit code, but may also return codes corresponding to
                // .NET host errors. We can't discern between them, but we probably don't need
                // to worry about it since the majority of errors are already filtered out by
                // the previous step.
                return GetRunAppFn()(handle);
            }
            finally
            {
                // Ignore errors
                GetCloseFn()(handle);
            }
        }

        public void Dispose() => _hostfxrLib.Dispose();
    }

    internal partial class DotnetHost
    {
        private static string GetHostFrameworkResolverFilePath()
        {
            // hostfxr.dll resolution strategy:
            // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/fxr_resolver.cpp#L55-L135
            // 1. Find the hostfxr directory containing versioned subdirectories
            // 2. Get the hostfxr.dll from the subdirectory with the highest version number

            var hostfxrRootDirPath = Path.Combine(Path.Combine(DotnetInstallation.GetDirectoryPath(), "host"), "fxr");
            if (!Directory.Exists(hostfxrRootDirPath))
                throw new DirectoryNotFoundException("Could not find directory containing hostfxr.dll.");

            var highestVersion = default(Version);
            var highestVersionFilePath = default(string);
            foreach (var dirPath in Directory.GetDirectories(hostfxrRootDirPath))
            {
                var version = VersionEx.TryParse(Path.GetFileName(dirPath));
                if (version is null)
                    continue;

                var filePath = Path.Combine(dirPath, "hostfxr.dll");
                if (!File.Exists(filePath))
                    continue;

                if (highestVersion is null || version > highestVersion)
                {
                    highestVersion = version;
                    highestVersionFilePath = filePath;
                }
            }

            return highestVersionFilePath ?? throw new FileNotFoundException("Could not find hostfxr.dll.");
        }

        public static DotnetHost Initialize() => new(NativeLibrary.Load(GetHostFrameworkResolverFilePath()));
    }
}