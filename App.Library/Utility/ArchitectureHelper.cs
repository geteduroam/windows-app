using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace App.Library.Utility;

public static class ArchitectureHelper
{
    public enum MachineType : ushort
    {
        // https://learn.microsoft.com/en-us/windows/win32/sysinfo/image-file-machine-constants
        // https://learn.microsoft.com/en-us/windows/win32/debug/pe-format#machine-types
        // Architectures not in wProcessorArchitecture are commented out
        // https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-system_info#members
        UNKNOWN = 0,
        I386 = 0x014c, // Intel 386
        //R3000 = 0x0162, // MIPS little-endian, 0x160 big-endian
        //R4000 = 0x0166, // MIPS little-endian
        //R10000 = 0x0168, // MIPS little-endian
        //WCEMIPSV2 = 0x0169, // MIPS little-endian WCE v2
        //ALPHA = 0x0184, // Alpha_AXP
        //SH3 = 0x01a2, // SH3 little-endian
        //SH3DSP = 0x01a3, // SH3DSP
        //SH3E = 0x01a4, // SH3E little-endian
        //SH4 = 0x01a6, // SH4 little-endian
        //SH5 = 0x01a8, // SH5
        ARM = 0x01c0, // ARM Little-Endian
        //THUMB = 0x01c2, // ARM Thumb/Thumb-2 Little-Endian
        //ARMNT = 0x01c4, // ARM Thumb-2 Little-Endian
        //AM33 = 0x01d3, // TAM33BD
        //POWERPC = 0x01F0, // IBM PowerPC Little-Endian
        //POWERPCFP = 0x01f1, // POWERPCFP
        IA64 = 0x0200, // Intel 64
        //MIPS16 = 0x0266, // MIPS
        //ALPHA64 = 0x0284, // ALPHA64
        //MIPSFPU = 0x0366, // MIPS
        //MIPSFPU16 = 0x0466, // MIPS
        //AXP64 = 0x0284, // AXP64
        //TRICORE = 0x0520, // Infineon
        //CEF = 0x0CEF, // CEF
        //EBC = 0x0EBC, // EFI Byte Code
        AMD64 = 0x8664, // AMD64 (K8)
        //M32R = 0x9041, // M32R little-endian
        ARM64 = 0xAA64, // ARM64 Little-Endian
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process2(
        IntPtr process,
        out ushort processMachine,
        out ushort nativeMachine
    );

    public static MachineType GetFileArch(string path)
    {
        // https://learn.microsoft.com/en-us/windows/win32/debug/pe-format
        // Offset 0 contains 0x5A4D (MZ)
        // Offset &0x3c contains 0x00004550 (EP)
        // Machine Type is stored in LE in &0x40 (&(0x3c+4))
        if (!BitConverter.IsLittleEndian) return 0;
        using var file = File.OpenRead(path);
        var buffer = new byte[8];
        file.Seek(0, SeekOrigin.Begin);
        file.Read(buffer, 0, 2);
        if (BitConverter.ToUInt16(buffer, 0) != 0x5A4D) return 0;
        file.Seek(0x3c, SeekOrigin.Begin);
        file.Read(buffer, 0, 4);
        var pePointer = BitConverter.ToInt32(buffer, 0);
        file.Seek(pePointer, SeekOrigin.Begin);
        file.Read(buffer, 0, 8);
        var signature = BitConverter.ToUInt32(buffer, 0);
        var machineType = BitConverter.ToUInt16(buffer, 4);

        return signature == 0x00004550 ? (MachineType)machineType : 0;
    }

    public static MachineType GetNativeArch()
    {
        var handle = Process.GetCurrentProcess().Handle;
        IsWow64Process2(handle, out var processMachineNull, out var nativeMachine);
        // processMachineNull is NULL because it only is set for WOW64, a kind of "universal" binary

        return (MachineType)nativeMachine;
    }
    public static MachineType GetProcessArch()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86: return MachineType.I386;
            case Architecture.X64: return MachineType.AMD64;
            case Architecture.Arm: return MachineType.ARM;
            case Architecture.Arm64: return MachineType.ARM64;
        }
        return 0;
    }

    public static bool ProcessIsNative()
    {
        return GetNativeArch() == GetProcessArch();
    }
}
