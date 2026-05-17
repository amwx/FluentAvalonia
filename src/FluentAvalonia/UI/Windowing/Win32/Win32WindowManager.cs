using static FluentAvalonia.Interop.Win32Interop;
using Avalonia.Controls;
using FluentAvalonia.Interop.Win32;
using Avalonia;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using FluentAvalonia.Interop;

namespace FluentAvalonia.UI.Windowing;

internal unsafe class Win32WindowManager
{
    public Win32WindowManager(FAAppWindow window)
    {
        _window = window;

        Hwnd = (HWND)_window.TryGetPlatformHandle().Handle;
        
        _oldWndProc = GetWindowLongPtrW(Hwnd, GWLP_WNDPROC);

        _appWindowRegistry.Add(Hwnd, this);

        // Apparently...nint and void* aren't blittable types to the mono-wasm compiler
        // so the function pointer here needs to use IntPtr
        _wndProc = (nint)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr>)&WndProcStatic;

        SetWindowLongPtrW(Hwnd, GWLP_WNDPROC, _wndProc);

        var ps = Application.Current.PlatformSettings;
        ps.ColorValuesChanged += OnPlatformColorValuesChanged;
        _window.Closed += WindowOnClosed;
    }

    public HWND Hwnd { get; }

    private LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case WM_RBUTTONUP:
                HandleRBUTTONUP(lParam);
                break;

            case WM_SYSCOMMAND:
                // Enables ALT+SPACE to open the system menu
                if ((WPARAM)wParam == SC_KEYMENU)
                {
                    return DefWindowProcW((HWND)hWnd, msg, (WPARAM)wParam, lParam);
                }
                break;

            case WM_DESTROY:
                _appWindowRegistry.Remove(hWnd);
                break;
        }

        return CallWindowProcW(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double GetScaling() =>
        _window.RenderScaling;

    private unsafe void HandleRBUTTONUP(LPARAM lParam)
    {
        var pt = PointFromLParam(lParam);

        if (_window.HitTestTitleBar(pt.ToPoint(GetScaling())))
        {
            var sysMenu = GetSystemMenu((HWND)Hwnd, false);
            bool isMax = _window.WindowState == WindowState.Maximized;

            var mii = new MENUITEMINFO
            {
                cbSize = (uint)sizeof(MENUITEMINFO),
                fMask = MIIM_STATE,
                fState = MFS_ENABLED
            };
            // Always enabled
            SetMenuItemInfo(sysMenu, (uint)SC_MINIMIZE, false, &mii);
            SetMenuItemInfo(sysMenu, (uint)SC_CLOSE, false, &mii);

            // Restore only enabled if maximized
            mii.fState = (uint)(isMax ? MFS_ENABLED : MFS_DISABLED);
            SetMenuItemInfo(sysMenu, (uint)SC_RESTORE, false, &mii);

            // Only available if normal state
            mii.fState = (uint)(isMax ? MFS_DISABLED : MFS_ENABLED);
            SetMenuItemInfo(sysMenu, (uint)SC_MOVE, false, &mii);
            SetMenuItemInfo(sysMenu, (uint)SC_SIZE, false, &mii);
            SetMenuItemInfo(sysMenu, (uint)SC_MAXIMIZE, false, &mii);

            SetMenuDefaultItem(sysMenu, uint.MaxValue, 0);

            var scPt = _window.PointToScreen(pt.ToPoint(GetScaling()));

            var ret = TrackPopupMenu(sysMenu, TPM_RETURNCMD, scPt.X, scPt.Y, 0, (HWND)Hwnd, null);
            if (ret)
            {
                PostMessage((HWND)Hwnd, WM_SYSCOMMAND, (WPARAM)ret, 0);
            }
        }
    }

    private void OnPlatformColorValuesChanged(object sender, PlatformColorValues e)
    {
        // We need to override Avalonia's default setting of this to always keep AppWindow
        // in dark mode, which matches what windows do on Win 10/11, regardless of the actual
        // app or system theme.
        Win32Interop.ApplyTheme(Hwnd, true);
    }
    
    private void WindowOnClosed(object sender, EventArgs e)
    {
        var ps = Application.Current.PlatformSettings;
        ps.ColorValuesChanged -= OnPlatformColorValuesChanged;
        _window.Closed -= WindowOnClosed;
    }

    [UnmanagedCallersOnly]
    private static IntPtr WndProcStatic(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (_appWindowRegistry.TryGetValue((HWND)hwnd, out var wnd))
        {
            return wnd.WndProc((HWND)hwnd, msg, (WPARAM)wParam, (LPARAM)lParam);
        }

        return (IntPtr)0;
    }

    private static Dictionary<HWND, Win32WindowManager> _appWindowRegistry =
        new Dictionary<HWND, Win32WindowManager>();


    private readonly FAAppWindow _window;

    private readonly nint _oldWndProc;
    private readonly nint _wndProc;
}
