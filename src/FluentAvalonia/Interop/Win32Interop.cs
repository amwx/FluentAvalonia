using System;
using System.Runtime.InteropServices;
using System.Security;
using Avalonia;
using FluentAvalonia.Interop;
using FluentAvalonia.Interop.Win32;
using MicroCom.Runtime;

namespace FluentAvalonia.Interop;

internal static unsafe partial class Win32Interop
{
    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL GetClientRect(HWND hWnd, RECT* lpRect);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL AdjustWindowRectEx(RECT* lpRect, uint dwStyle, BOOL bMenu, uint dwExStyle);

    [DllImport(s_user32, SetLastError = true)]
    private static extern int GetSystemMetrics(int smIndex);

    [DllImport(s_user32, SetLastError = true)]
    private static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport(s_user32, SetLastError = true)]
    private static extern BOOL AdjustWindowRectExForDpi(RECT* lpRect, int dwStyle, BOOL bMenu, int dwExStyle, int dpi);
    
    [DllImport(s_user32, SetLastError = true)]
    private static extern bool AdjustWindowRectEx(RECT* lpRect, int dwStyle, bool bMenu, int dwExStyle);
    
    [DllImport(s_user32, SetLastError = true)]
    public static extern LRESULT DefWindowProcW(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);

    [DllImport(s_user32, SetLastError = true)]
    public static extern LRESULT CallWindowProcW(nint lpPrevWndProc, HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);


    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL PostMessage(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);

    [DllImport(s_user32, SetLastError = true)]
    public static extern HMENU GetSystemMenu(HWND hWnd, BOOL bRevert);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL SetMenuItemInfo(HMENU hMenu, uint item, bool fByPosition, MENUITEMINFO* lpmii);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL SetMenuDefaultItem(HMENU hMenu, uint uItem, uint fByPos);

    [DllImport(s_user32, SetLastError = true)]
    public static extern BOOL TrackPopupMenu(HMENU hMenu, uint uFlags,
        int x, int y, int nReserved, HWND hWnd, RECT* prcRect);

    [DllImport(s_user32)]
    public static extern int GetWindowLongW(HWND hWnd, int nIndex);

    [DllImport(s_user32)]
    public static extern int SetWindowLongW(HWND hWnd, int nIndex, int dwNewLong);

    // Below is adapted from TerraFX.Interop.Windows, MIT License
    public static delegate*<HWND, int, nint> GetWindowLongPtr => &GetWindowLongPtrW;

    public static int GetSystemMetricsWithFallback(int nIndex, uint dpi)
    {
        if (OSVersionHelper.IsWindowsAtLeast(10, 0, 14393)) // 1607
            return GetSystemMetricsForDpi(nIndex, dpi);
        return GetSystemMetrics(nIndex);
    }
    
    public static void AdjustWindowRectExWithFallback(RECT* lpRect, int dwStyle, BOOL bMenu, int dwExStyle, int dpi)
    {
        if (OSVersionHelper.IsWindowsAtLeast(10, 0, 14393)) // 1607
        {
            AdjustWindowRectExForDpi(lpRect, dwStyle, bMenu, dwExStyle, dpi);
            return;
        }
        AdjustWindowRectEx(lpRect, dwStyle, bMenu, dwExStyle);
    }
    
    public static nint GetWindowLongPtrW(HWND hWnd, int nIndex)
    {
        if (sizeof(nint) == 4)
        {
            return GetWindowLongW(hWnd, nIndex);
        }
        else
        {
            [DllImport(s_user32, EntryPoint = "GetWindowLongPtrW")]
            static extern nint _GetWindowLongPtrW(HWND hWnd, int nIndex);

            return _GetWindowLongPtrW(hWnd, nIndex);
        }
    }

    public static delegate*<HWND, int, nint, nint> SetWindowLongPtr => &SetWindowLongPtrW;

    public static nint SetWindowLongPtrW(HWND hWnd, int nIndex, nint dwNewLong)
    {
        if (sizeof(nint) == 4)
        {
            return SetWindowLongW(hWnd, nIndex, (int)dwNewLong);
        }
        else
        {
            [DllImport(s_user32, EntryPoint = "SetWindowLongPtrW")]
            static extern nint _SetWindowLongPtr(HWND hWnd, int nIndex, nint dwNewLong);

            return _SetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }
    }

    [DllImport(s_dwmapi, SetLastError = true)]
    public static extern HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, MARGINS* margins);

    [DllImport(s_ole32, ExactSpelling = true)]
    public static extern HRESULT CoCreateInstance(Guid* rclsid, void* pUnkOuter,
        int dwClsContext, Guid* riid, void** ppv);

    internal unsafe static T CreateInstance<T>(Guid clsid, Guid iid) where T : IUnknown
    {
        void* pUnk;
        var hresult = CoCreateInstance(&clsid, null, 1, &iid, &pUnk);
        if (hresult != 0)
        {
            throw new COMException("CreateInstance", hresult);
        }
        using var unk = MicroComRuntime.CreateProxyFor<IUnknown>(pUnk, true);
        return MicroComRuntime.QueryInterface<T>(unk);
    }


    [DllImport(s_dwmapi, SetLastError = true)]
    public static extern int DwmSetWindowAttribute(nint hWnd, DWMWINDOWATTRIBUTE attr, void* value, int attrSize);

    

    [DllImport(s_user32, SetLastError = true)]
    public static unsafe extern int SetWindowCompositionAttribute(IntPtr hwnd, WINDOWCOMPOSITIONATTRIBDATA* data);

    [DllImport(s_uxtheme, EntryPoint = "#104", SetLastError = true)]
    public static extern void fnRefreshImmersiveColorPolicyState();

    [DllImport(s_uxtheme, EntryPoint = "#135", SetLastError = true)]
    public static extern PreferredAppMode fnSetPreferredAppMode(IntPtr hwnd, PreferredAppMode appMode);

    [DllImport(s_uxtheme, EntryPoint = "#135")]
    public static extern bool fnAllowDarkModeForApp(IntPtr hwnd, bool allow);


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
        SetWindowPos((HWND)hwnd, HWND.NULL, 0, 0, 0, 0,
            SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        return true;
    }


    public const int GWLP_WNDPROC = -4;
    public const int GWL_STYLE = -16;

    public const uint WS_MAXIMIZE = 0x01000000;

    public const int WM_CREATE = 0x0001;
    public const int WM_SIZE = 0x0005;
    public const int WM_NCMOUSEMOVE = 0x00A0;
    public const int WM_NCLBUTTONDOWN = 0x00A1;
    public const int WM_NCLBUTTONUP = 0x00A2;
    public const int WM_NCHITTEST = 0x0084;
    public const int WM_NCCALCSIZE = 0x0083;
    public const int WM_ACTIVATE = 0x0006;
    public const int WM_NCRBUTTONDOWN = 0x00A4;
    public const int WM_NCRBUTTONDBLCLK = 0x00A6;
    public const int WM_NCRBUTTONUP = 0x00A5;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_SETTINGCHANGE = 0x001A; // Also WM_WININICHANGE
    public const int WM_SYSCOLORCHANGE = 0x0015;
    public const int WM_DESTROY = 0x0002;

    //SC
    public const int SC_CLOSE = 0xF060;
    public const int SC_KEYMENU = 0xF100;
    public const int SC_MAXIMIZE = 0xF030;
    public const int SC_MINIMIZE = 0xF020;
    public const int SC_MOVE = 0xF010;
    public const int SC_RESTORE = 0xF120;
    public const int SC_SIZE = 0xF000;

    //MIIM
    public const int MIIM_STATE = 0x00000001;

    // SWP
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOREDRAW = 0x0008;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_SHOWWINDOW = 0x0040;
    public const int SWP_NOOWNERZORDER = 0x0200;
    public const int SWP_DRAWFRAME = 0x0020;
    public const int SWP_NOREPOSITION = 0x0200;

    // SM
    public const int SM_CXPADDEDBORDER = 92;
    public const int SM_CYSIZEFRAME = 33;

    // HT
    public const int HTCLIENT = 1;
    public const int HTCAPTION = 2;
    public const int HTMAXBUTTON = 9;
    public const int HTTOP = 12;


    // MFS
    public const int MFS_DISABLED = 0x00000003;
    public const int MFS_ENABLED = 0x00000000;

    // TPM
    public const int TPM_RETURNCMD = 0x0100;

    public static readonly Guid ITaskBarList3CLSID = Guid.Parse("56FDF344-FD6D-11D0-958A-006097C9A090");
    public static readonly Guid ITaskBarList3IID = Guid.Parse("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf");

    public static PixelPoint PointFromLParam(LPARAM lParam)
    {
        int lP = (int)lParam;
        return new PixelPoint((short)(lP & 0xffff), (short)(lP >> 16));
    }


    private const string s_ole32 = "ole32.dll";
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
