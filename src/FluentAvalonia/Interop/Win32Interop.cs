using System;
using System.Runtime.InteropServices;
using Avalonia;
using System.Security;

namespace FluentAvalonia.Interop;

internal static unsafe class Win32Interop
{
    [DllImport(s_dwmapi, SetLastError = true)]
    public static extern int DwmSetWindowAttribute(nint hWnd, DWMWINDOWATTRIBUTE attr, void* value, int attrSize);

    // TODO: TabViewListView
    [DllImport(s_user32)]
    public static extern int GetSystemMetrics(int smIndex);
    [DllImport(s_user32)]
    public static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);


    [DllImport(s_user32, SetLastError = true)]
    public static unsafe extern int SetWindowCompositionAttribute(IntPtr hwnd, WINDOWCOMPOSITIONATTRIBDATA* data);

    [DllImport(s_uxtheme, EntryPoint = "#104", SetLastError = true)]
    public static extern void fnRefreshImmersiveColorPolicyState();

    [DllImport(s_uxtheme, EntryPoint = "#135", SetLastError = true)]
    public static extern PreferredAppMode fnSetPreferredAppMode(IntPtr hwnd, PreferredAppMode appMode);

    [DllImport(s_uxtheme, EntryPoint = "#135")]
    public static extern bool fnAllowDarkModeForApp(IntPtr hwnd, bool allow);



    [DllImport(s_user32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [SecurityCritical]
    [DllImport(s_ntdll, SetLastError = true, CharSet = CharSet.Unicode)]
    internal static unsafe extern int RtlGetVersion(OSVERSIONINFOEX* versionInfo);

    public static bool ApplyTheme(IntPtr hwnd, bool useDark)
    {
        if (!OSVersionHelper.IsWindowsAtLeast(10, 0, 17763)) // 1809
            return false;

        if (!OSVersionHelper.IsWindowsAtLeast(10, 0, 18362)) //1903
        {
            var res = fnAllowDarkModeForApp(hwnd, useDark);
            if (res == false)
                return res;

            unsafe
            {
                int dark = useDark ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, &dark, sizeof(int));
            }
            
        }
        else
        {
            //Not sure what a successful return value is on this one
            fnSetPreferredAppMode(hwnd, useDark ? PreferredAppMode.AllowDark : PreferredAppMode.Default);
            fnRefreshImmersiveColorPolicyState();

            int success = 0;
            unsafe
            {
                WINDOWCOMPOSITIONATTRIBDATA data = new WINDOWCOMPOSITIONATTRIBDATA
                {
                    attrib = WINDOWCOMPOSITIONATTRIB.WCA_USEDARKMODECOLORS,
                    data = &useDark,
                    sizeOfData = sizeof(int)
                };

                success = SetWindowCompositionAttribute(hwnd, &data);
            }
            if (success == 0)
                return false;
        }

        // Try to get the window to redraw to reflect the changes
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, (uint)(0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020 | 0x0200));

        return true;
    }

    private const string s_dwmapi = "dwmapi";
    private const string s_user32 = "user32.dll";
    private const string s_ntdll = "ntdll.dll";
    private const string s_uxtheme = "uxtheme.dll";
}

public unsafe struct WINDOWCOMPOSITIONATTRIBDATA
{
    public WINDOWCOMPOSITIONATTRIB attrib;
    public void* data;
    public int sizeOfData;
}

public enum PreferredAppMode
{
    Default,
    AllowDark,
    ForceDark,
    ForceLight,
    Max
}

public enum WINDOWCOMPOSITIONATTRIB
{
    WCA_UNDEFINED = 0,
    WCA_NCRENDERING_ENABLED = 1,
    WCA_NCRENDERING_POLICY = 2,
    WCA_TRANSITIONS_FORCEDISABLED = 3,
    WCA_ALLOW_NCPAINT = 4,
    WCA_CAPTION_BUTTON_BOUNDS = 5,
    WCA_NONCLIENT_RTL_LAYOUT = 6,
    WCA_FORCE_ICONIC_REPRESENTATION = 7,
    WCA_EXTENDED_FRAME_BOUNDS = 8,
    WCA_HAS_ICONIC_BITMAP = 9,
    WCA_THEME_ATTRIBUTES = 10,
    WCA_NCRENDERING_EXILED = 11,
    WCA_NCADORNMENTINFO = 12,
    WCA_EXCLUDED_FROM_LIVEPREVIEW = 13,
    WCA_VIDEO_OVERLAY_ACTIVE = 14,
    WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 15,
    WCA_DISALLOW_PEEK = 16,
    WCA_CLOAK = 17,
    WCA_CLOAKED = 18,
    WCA_ACCENT_POLICY = 19,
    WCA_FREEZE_REPRESENTATION = 20,
    WCA_EVER_UNCLOAKED = 21,
    WCA_VISUAL_OWNER = 22,
    WCA_HOLOGRAPHIC = 23,
    WCA_EXCLUDED_FROM_DDA = 24,
    WCA_PASSIVEUPDATEMODE = 25,
    WCA_USEDARKMODECOLORS = 26,
    WCA_LAST = 27
};

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct OSVERSIONINFOEX
{
    // The OSVersionInfoSize field must be set
    public uint OSVersionInfoSize;
    public uint MajorVersion;
    public uint MinorVersion;
    public uint BuildNumber;
    public uint PlatformId;
    public fixed ushort CSDVersion[128];
    public ushort ServicePackMajor;
    public ushort ServicePackMinor;
    public ushort SuiteMask;
    public byte ProductType;
    public byte Reserved;
}
