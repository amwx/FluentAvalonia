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

    // Only values actually used are included below

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

    private const string s_User32 = "User32.dll";
    private const string s_dwmapi = "dwmapi.dll";
    private const string s_ole32 = "ole32.dll";
}
