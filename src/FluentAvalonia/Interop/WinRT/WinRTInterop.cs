using System;
using System.Runtime.InteropServices;
using System.Threading;
using MicroCom.Runtime;

namespace FluentAvalonia.Interop.WinRT;

internal static class WinRTInterop
{
    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall,
        PreserveSig = false)]
    internal static extern unsafe IntPtr WindowsCreateString(
        [MarshalAs(UnmanagedType.LPWStr)] string sourceString,
        int length);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern unsafe char* WindowsGetStringRawBuffer(IntPtr hstring, uint* length);

    internal static unsafe string GetString(IntPtr hString)
    {
        uint length;
        var buffer = WindowsGetStringRawBuffer(hString, &length);
        return new string(buffer, 0, (int)length);
    }

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall,
        PreserveSig = false)]
    internal static extern unsafe bool WindowsIsStringEmpty(IntPtr @string);

    internal static IntPtr WindowsCreateString(string sourceString)
        => WindowsCreateString(sourceString, sourceString.Length);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll",
        CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
    internal static extern unsafe IntPtr WindowsDeleteString(IntPtr hString);

    internal static T CreateInstance<T>(string fullName) where T : IUnknown
    {
        var s = WindowsCreateString(fullName);
        EnsureRoInitialized();
        var pUnk = RoActivateInstance(s);
        using var unk = MicroComRuntime.CreateProxyFor<IUnknown>(pUnk, true);
        WindowsDeleteString(s);
        return MicroComRuntime.QueryInterface<T>(unk);
    }

    private static bool _initialized;
    private static void EnsureRoInitialized()
    {
        if (_initialized)
            return;
        RoInitialize(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA ?
            RO_INIT_TYPE.RO_INIT_SINGLETHREADED :
            RO_INIT_TYPE.RO_INIT_MULTITHREADED);
        _initialized = true;
    }

    internal enum RO_INIT_TYPE
    {
        RO_INIT_SINGLETHREADED = 0, // Single-threaded application
        RO_INIT_MULTITHREADED = 1, // COM calls objects on any thread.
    }

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern void RoInitialize(RO_INIT_TYPE initType);

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern IntPtr RoActivateInstance(IntPtr activatableClassId);

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern IntPtr RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid);
}
