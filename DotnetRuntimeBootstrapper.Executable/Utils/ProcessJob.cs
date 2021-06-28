using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DotnetRuntimeBootstrapper.Executable.Native;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal partial class ProcessJob : IDisposable
    {
        private readonly IntPtr _handle;
        private readonly IntPtr _limitInfoHandle;

        public ProcessJob(IntPtr handle, IntPtr limitInfoHandle)
        {
            _handle = handle;
            _limitInfoHandle = limitInfoHandle;
        }

        ~ProcessJob() => Dispose();

        public void Dispose()
        {
            NativeMethods.CloseHandle(_handle);
            Marshal.FreeHGlobal(_limitInfoHandle);
        }

        public void AddProcess(IntPtr processHandle) =>
            NativeMethods.AssignProcessToJobObject(_handle, processHandle);

        public void AddProcess(Process process) =>
            AddProcess(process.Handle);
    }

    internal partial class ProcessJob
    {
        public static ProcessJob? TryCreate()
        {
            var handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
            if (handle == IntPtr.Zero)
                return null;

            var limitInfo = new JobObjectExtendedLimitInformation
            {
                BasicLimitInformation = new JobObjectBasicLimitInformation
                {
                    // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                    LimitFlags = 0x2000
                }
            };

            var limitInfoSize = Marshal.SizeOf(typeof(JobObjectExtendedLimitInformation));
            var limitInfoHandle = Marshal.AllocHGlobal(limitInfoSize);
            Marshal.StructureToPtr(limitInfo, limitInfoHandle, false);

            if (!NativeMethods.SetInformationJobObject(
                handle,
                JobObjectInfoType.ExtendedLimitInformation,
                limitInfoHandle,
                (uint) limitInfoSize))
            {
                NativeMethods.CloseHandle(handle);
                Marshal.FreeHGlobal(limitInfoHandle);
                return null;
            }

            return new ProcessJob(handle, limitInfoHandle);
        }
    }
}