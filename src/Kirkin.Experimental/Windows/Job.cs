#if !__MOBILE__

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Kirkin.Windows
{
    /// <summary>
    /// Managed wrapper over Win32 job functionality.
    /// </summary>
    public sealed class Job : IDisposable
    {
        private readonly IntPtr _jobHandle;
        private bool _disposed;

        #region Public API

        /// <summary>
        /// Creates a new <see cref="Job"/> instance.
        /// </summary>
        public Job()
        {
            _jobHandle = Kernel32.CreateJobObject(IntPtr.Zero, null);

            // Prepare job.
            IntPtr extendedInfoPtr = IntPtr.Zero;
 
            try
            {
                JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
                    BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION {
                        LimitFlags = JOBOBJECTLIMIT.KillOnJobClose
                    }
                };

                int extendedInfoSize = Marshal.SizeOf(extendedInfo);

                extendedInfoPtr = Marshal.AllocHGlobal(extendedInfoSize);

                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!Kernel32.SetInformationJobObject(_jobHandle, JOBOBJECTINFOCLASS.ExtendedLimitInformation, extendedInfoPtr, (uint)extendedInfoSize)) {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (extendedInfoPtr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(extendedInfoPtr);
                }
            }
        }

        /// <summary>
        /// Ties the lifetime of the given process to the lifetime of the current process.
        /// If the current process terminates, the operating system will kill the associated process too.
        /// </summary>
        public void EnlistProcess(Process process)
        {
            if (!Kernel32.AssignProcessToJobObject(_jobHandle, process.Handle)) {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Releases the resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_jobHandle != IntPtr.Zero) {
                    Kernel32.CloseHandle(_jobHandle);
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Releases the resources held by this object.
        /// </summary>
        ~Job()
        {
            Dispose(false);
        }

        #endregion

        #region Platform Invoke

        static class Kernel32
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetInformationJobObject(IntPtr hJob, JOBOBJECTINFOCLASS infoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

            [DllImport("kernel32.dll", SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr handle);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }
    
        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public JOBOBJECTLIMIT LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }
 
        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        enum JOBOBJECTLIMIT : uint
        {
            KillOnJobClose = 0x00002000
        }

        enum JOBOBJECTINFOCLASS
        {
            ExtendedLimitInformation = 9
        }

        #endregion
    }
}

#endif