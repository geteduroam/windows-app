using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace App.Library.Utility;
public static class ArchitectureHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    // P/Invoke for GetNativeSystemInfo
    [DllImport("kernel32.dll")]
    static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

    public static bool IsArm64()
    {
        SYSTEM_INFO sysInfo = new SYSTEM_INFO();
        GetNativeSystemInfo(ref sysInfo);
        return sysInfo.wProcessorArchitecture is 5 or 12; // ARM of ARM64
    }
}
