using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Hosting.Windows.Internals
{
    [SupportedOSPlatform("windows")]
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsProcesses
    {
        // Marshaling of PROCESS_BASIC_INFORMATION structure.
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref WindowsProcesses processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess() => GetParentProcess(Process.GetCurrentProcess().Handle);

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Process GetParentProcess(int id) => GetParentProcess(Process.GetProcessById(id).Handle);

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            WindowsProcesses Pbi = new WindowsProcesses();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref Pbi, Marshal.SizeOf(Pbi), out returnLength);
            if (status != 0)
                throw new NotSupportedException();

            try { return Process.GetProcessById(Pbi.InheritedFromUniqueProcessId.ToInt32()); }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
