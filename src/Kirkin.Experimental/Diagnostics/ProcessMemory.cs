#if !__MOBILE__

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Process memory reader/writer.
    /// </summary>
    public sealed class ProcessMemory
    {
        /// <summary>
        /// Process specified when this instance was created.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Creates a new <see cref="ProcessMemory"/> instance targeting
        /// the given <see cref="System.Diagnostics.Process"/>.
        /// </summary>
        /// <param name="process">Process whose memory will be read or written.</param>
        public ProcessMemory(Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            Process = process;
        }

        /// <summary>
        /// Reads the entire memory contents of the process' main module.
        /// </summary>
        public byte[] Read()
        {
            ProcessModule module = Process.MainModule;

            return Read(module.BaseAddress, module.ModuleMemorySize);
        }

        /// <summary>
        /// Reads process memory at the given address.
        /// </summary>
        public byte[] Read(IntPtr address, int length)
        {
            IntPtr handle = OpenProcess(PROCESS_VM_READ, false, Process.Id);

            if (handle == IntPtr.Zero) {
                Win32Throw();
            }

            byte[] bytes = new byte[length];
            int bytesRead = 0;

            if (!ReadProcessMemory(handle, address, bytes, length, ref bytesRead)) {
                Win32Throw();
            }

            if (bytes.Length != bytesRead) {
                Array.Resize(ref bytes, bytesRead);
            }

            return bytes;
        }

        /// <summary>
        /// Writes process memory at the given address.
        /// </summary>
        public void Write(IntPtr address, byte[] bytes)
        {
            IntPtr handle = OpenProcess(PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, Process.Id);

            if (handle == IntPtr.Zero) {
                Win32Throw();
            }

            int bytesWritten = 0;

            if (!WriteProcessMemory(handle, address, bytes, bytes.Length, ref bytesWritten)) {
                Win32Throw();
            }

            if (bytesWritten != bytes.Length) {
                throw new InvalidOperationException("Incomplete process memory write.");
            }
        }

        private static void Win32Throw()
        {
            int errorCode = Marshal.GetLastWin32Error();

            throw new Win32Exception(errorCode);
        }

        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
    }
}

#endif