using System;
using System.Runtime.InteropServices;

namespace IWshRuntimeLibrary
{
    [ComImport]
    [Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")]
    [CoClass(typeof(WshShellClass))]
    public interface WshShell : IWshShell
    {
    }

    [ComImport]
    [Guid("F935DC21-1CF0-11D0-ADB9-00C04FD58A0B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IWshShell
    {
        [DispId(1002)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object CreateShortcut([In, MarshalAs(UnmanagedType.BStr)] string PathLink);
    }

    [ComImport]
    [Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IWshShortcut
    {
        [DispId(0)]
        string FullName { [return: MarshalAs(UnmanagedType.BStr)] get; }
        [DispId(1000)]
        string Arguments { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1001)]
        string Description { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1002)]
        string Hotkey { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1003)]
        string IconLocation { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1004)]
        string RelativePath { [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1005)]
        string TargetPath { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(1006)]
        int WindowStyle { get; [param: In] set; }
        [DispId(1007)]
        string WorkingDirectory { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        [DispId(2000)]
        void Load([In, MarshalAs(UnmanagedType.BStr)] string PathLink);
        [DispId(2001)]
        void Save();
    }

    [ComImport]
    [Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")]
    [ClassInterface(ClassInterfaceType.None)]
    public class WshShellClass
    {
    }
}

namespace NETWORKLIST
{
    [ComImport]
    [Guid("DCB00000-570F-4A9B-8D69-199FDBA5723B")]
    [CoClass(typeof(NetworkListManagerClass))]
    public interface NetworkListManager : INetworkListManager
    {
    }

    [ComImport]
    [Guid("DCB00000-570F-4A9B-8D69-199FDBA5723B")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface INetworkListManager
    {
        [DispId(1)]
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetNetworks([In] int Flags);
        [DispId(2)]
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetNetwork([In] Guid gdNetworkId);
        [DispId(3)]
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetNetworkConnections();
        [DispId(4)]
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetNetworkConnection([In] Guid gdNetworkConnectionId);
        [DispId(5)]
        bool IsConnectedToInternet { [DispId(5)] get; }
        [DispId(6)]
        bool IsConnected { [DispId(6)] get; }
    }

    [ComImport]
    [Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B")]
    [ClassInterface(ClassInterfaceType.None)]
    public class NetworkListManagerClass
    {
    }
}
