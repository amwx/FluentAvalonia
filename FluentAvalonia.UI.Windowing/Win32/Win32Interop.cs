using System.Runtime.InteropServices;
using Avalonia;
using MicroCom.Runtime;

namespace FluentAvalonia.UI.Windowing.Win32;

internal static unsafe class Win32Interop
{
    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL GetClientRect(HWND hWnd, RECT* lpRect);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL AdjustWindowRectEx(RECT* lpRect, uint dwStyle, BOOL bMenu, uint dwExStyle);

    [DllImport(s_User32, SetLastError = true)]
    public static extern int GetSystemMetrics(int smIndex);

    [DllImport(s_User32, SetLastError = true)]
    public static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL AdjustWindowRectExForDpi(RECT* lpRect, int dwStyle, BOOL bMenu, int dwExStyle, int dpi);

    [DllImport(s_User32, SetLastError = true)]
    public static extern LRESULT DefWindowProcW(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);



    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL PostMessage(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam);

    [DllImport(s_User32, SetLastError = true)]
    public static extern HMENU GetSystemMenu(HWND hWnd, BOOL bRevert);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL SetMenuItemInfo(HMENU hMenu, uint item, bool fByPosition, MENUITEMINFO* lpmii);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL SetMenuDefaultItem(HMENU hMenu, uint uItem, uint fByPos);

    [DllImport(s_User32, SetLastError = true)]
    public static extern BOOL TrackPopupMenu(HMENU hMenu, uint uFlags,
        int x, int y, int nReserved, HWND hWnd, RECT* prcRect);

    [DllImport(s_User32)]
    public static extern int GetWindowLongW(HWND hWnd, int nIndex);

    [DllImport(s_User32)]
    public static extern int SetWindowLongW(HWND hWnd, int nIndex, int dwNewLong);






    // Below is adapted from TerraFX.Interop.Windows, MIT License
    public static delegate*<HWND, int, nint> GetWindowLongPtr => &GetWindowLongPtrW;

    public static nint GetWindowLongPtrW(HWND hWnd, int nIndex)
    {
        if (sizeof(nint) == 4)
        {
            return GetWindowLongW(hWnd, nIndex);
        }
        else
        {
            [DllImport(s_User32, EntryPoint = "GetWindowLongPtrW")]
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
            [DllImport(s_User32, EntryPoint = "SetWindowLongPtrW")]
            static extern nint _SetWindowLongPtr(HWND hWnd, int nIndex, nint dwNewLong);

            return _SetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }
    }


    // DWMAPI functions
    
    [DllImport(s_dwmapi, SetLastError = true)]
    public static extern HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, MARGINS* margins);


    [DllImport(s_ole32, PreserveSig = true)]
    internal static extern int CoCreateInstance(ref Guid clsi,
        IntPtr ignore1, int ignore2, ref Guid iid, [MarshalAs(UnmanagedType.IUnknown), Out] out object pUnkOuter);

    [DllImport(s_ole32, PreserveSig = true)]
    internal static extern int CoCreateInstance(ref Guid clsid,
        IntPtr ignore1, int ignore2, ref Guid iid, [Out] out IntPtr pUnkOuter);

    internal unsafe static T CreateInstance<T>(ref Guid clsid, ref Guid iid) where T : IUnknown
    {
        var hresult = CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out IntPtr pUnk);
        if (hresult != 0)
        {
            throw new COMException("CreateInstance", hresult);
        }
        using var unk = MicroComRuntime.CreateProxyFor<IUnknown>(pUnk, true);
        return MicroComRuntime.QueryInterface<T>(unk);
    }

    // WM
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

    //SC
    public const int SC_CLOSE = 0xF060;
    public const int SC_CONTEXTHELP = 0xF180;
    public const int SC_DEFAULT = 0xF160;
    public const int SC_HOTKEY = 0xF150;
    public const int SC_HSCROLL = 0xF080;
    public const int SC_ISSECURE = 0x00000001;
    public const int SC_KEYMENU = 0xF100;
    public const int SC_MAXIMIZE = 0xF030;
    public const int SC_MINIMIZE = 0xF020;
    public const int SC_MONITORPOWER = 0xF170;
    public const int SC_MOUSEMENU = 0xF090;
    public const int SC_MOVE = 0xF010;
    public const int SC_NEXTWINDOW = 0xF040;
    public const int SC_PREVWINDOW = 0xF050;
    public const int SC_RESTORE = 0xF120;
    public const int SC_SCREENSAVE = 0xF140;
    public const int SC_SIZE = 0xF000;
    public const int SC_TASKLIST = 0xF130;
    public const int SC_VSCROLL = 0xF070;

    //MIIM
    public const int MIIM_BITMAP = 0x00000080;
    public const int MIIM_CHECKMARKS = 0x00000008;
    public const int MIIM_DATA = 0x00000020;
    public const int MIIM_FTYPE = 0x00000100;
    public const int MIIM_ID = 0x00000002;
    public const int MIIM_STATE = 0x00000001;
    public const int MIIM_STRING = 0x00000040;
    public const int MIIM_SUBMENU = 0x00000004;
    public const int MIIM_TYPE = 0x00000010;

    // SWP
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOREDRAW = 0x0008;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_SHOWWINDOW = 0x0040;
    public const int SWP_HIDEWINDOW = 0x0080;
    public const int SWP_NOCOPYBITS = 0x0100;
    public const int SWP_NOOWNERZORDER = 0x0200;
    public const int SWP_NOSENDCHANGING = 0x0400;
    public const int SWP_DRAWFRAME = 0x0020;
    public const int SWP_NOREPOSITION = 0x0200;
    public const int SWP_DEFERERASE = 0x2000;
    public const int SWP_ASYNCWINDOWPOS = 0x4000;

    // SM
    public const int SM_CXPADDEDBORDER = 92;
    public const int SM_CYSIZEFRAME = 33;

    // HT
    public const int HTNOWHERE = 0;
    public const int HTCLIENT = 1;
    public const int HTCAPTION = 2;
    public const int HTSYSMENU = 3;
    public const int HTGROWBOX = 4;
    public const int HTSIZE = 4;
    public const int HTMENU = 5;
    public const int HTHSCROLL = 6;
    public const int HTVSCROLL = 7;
    public const int HTMINBUTTON = 8;
    public const int HTMAXBUTTON = 9;
    public const int HTLEFT = 10;
    public const int HTRIGHT = 11;
    public const int HTTOP = 12;
    public const int HTTOPLEFT = 13;
    public const int HTTOPRIGHT = 14;
    public const int HTBOTTOM = 15;
    public const int HTBOTTOMLEFT = 16;
    public const int HTBOTTOMRIGHT = 17;
    public const int HTBORDER = 18;
    public const int HTREDUCE = 8;
    public const int HTZOOM = 9;
    public const int HTSIZEFIRST = 10;
    public const int HTSIZELAST = 17;
    public const int HTOBJECT = 19;
    public const int HTCLOSE = 20;
    public const int HTHELP = 21;


    // MFS
    public const int MFS_DISABLED = 0x00000003;
    public const int MFS_ENABLED = 0x00000000;

    // TPM
    public const int TPM_RETURNCMD = 0x0100;



    public static PixelPoint PointFromLParam(LPARAM lParam)
    {
        int lP = (int)lParam;
        return new PixelPoint((short)(lP & 0xffff), (short)(lP >> 16));
    }

    private const string s_User32 = "User32.dll";
    private const string s_dwmapi = "dwmapi.dll";
    private const string s_ole32 = "ole32.dll";
}
