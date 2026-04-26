using static FluentAvalonia.Interop.Win32Interop;
using Avalonia.Controls;
using FluentAvalonia.Interop.Win32;
using Avalonia;
using Avalonia.Logging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using Avalonia.Platform;
using FluentAvalonia.Interop;

namespace FluentAvalonia.UI.Windowing;

internal unsafe class Win32WindowManager
{
    public Win32WindowManager(AppWindow window)
    {
        _window = window;

        Hwnd = (HWND)_window.TryGetPlatformHandle().Handle;
        
        _oldWndProc = GetWindowLongPtrW(Hwnd, GWLP_WNDPROC);

#if NET5_0_OR_GREATER
        _appWindowRegistry.Add(Hwnd, this);

        // Apparently...nint and void* aren't blittable types to the mono-wasm compiler
        // so the function pointer here needs to use IntPtr
        _wndProc = (nint)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr>)&WndProcStatic;
#else
        _wndProc = Marshal.GetFunctionPointerForDelegate(WndProc);
#endif

        SetWindowLongPtrW(Hwnd, GWLP_WNDPROC, _wndProc);

        var ps = Application.Current.PlatformSettings;
        ps.ColorValuesChanged += OnPlatformColorValuesChanged;
        _window.Closed += WindowOnClosed;
    }

    public HWND Hwnd { get; }

    public Size LastWMSizeSize { get; set; } = Size.Infinity;

    public double LastUserWidth { get; set; } = double.NaN;

    public double LastUserHeight { get; set; } = double.NaN;

    public bool IsFullscreen { get; set; }

    public void GoToFullScreen()
    {
        RECT lpRect;
        GetWindowRect(Hwnd, &lpRect);

        _restoreRect = lpRect;
        IsFullscreen = true;
    }

    public void RestoreFromFullScreen()
    {
        Dispatcher.UIThread.Post(() =>
        {
            SetWindowPos(Hwnd, HWND.NULL,
                _restoreRect.left, _restoreRect.top,
                _restoreRect.right - _restoreRect.left,
                _restoreRect.bottom - _restoreRect.top,
                SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE);

            EnsureExtended();
        });

        IsFullscreen = false;
    }

    private LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case WM_ACTIVATE:
                EnsureExtended();
                break;

            case WM_NCCALCSIZE:
                if ((WPARAM)wParam != 0)
                {
                    return HandleNCCALCSIZE((WPARAM)wParam, (LPARAM)lParam);
                }

                return 0;

            case WM_NCHITTEST:
                return HandleNCHITTEST(lParam);

            case WM_SIZE:
                var pt = PointFromLParam(lParam);
                LastWMSizeSize = new Size(pt.X, pt.Y);

                if (_fakingMaximizeButton)
                {
                    _wasFakeMaximizeDown = false;
                    _window.SystemCaptionControl.ClearMaximizedState();
                }

                EnsureExtended();
                return CallWindowProcW(_oldWndProc, hWnd, msg, wParam, lParam);

            case WM_NCLBUTTONDOWN:
                if (_fakingMaximizeButton)
                {
                    var point = _window.PointToClient(PointFromLParam(lParam));
                    _window.SystemCaptionControl.FakeMaximizePressed(_window.SystemCaptionControl.HitTestMaxButton(point));
                    _wasFakeMaximizeDown = true;

                    // This is important. If we don't tell the System we've handled this, we'll get that
                    // classic Win32 button showing when we mouse press, and that's not good
                    return 0;
                }
                break;

            case WM_NCLBUTTONUP:
                if (_fakingMaximizeButton && _wasFakeMaximizeDown)
                {
                    _window.SystemCaptionControl.FakeMaximizePressed(false);
                    _wasFakeMaximizeDown = false;
                    _window.SystemCaptionControl.FakeMaximizeClick();

                    return 0;
                }
                break;

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

#if NET5_0_OR_GREATER
            case WM_DESTROY:
                _appWindowRegistry.Remove(hWnd);
                break;
#endif
        }

        return CallWindowProcW(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double GetScaling() =>
        _window.RenderScaling;

    private int GetResizeHandleHeight() =>
        GetSystemMetricsWithFallback(SM_CXPADDEDBORDER, (uint)(96 * GetScaling())) +
        GetSystemMetricsWithFallback(SM_CYSIZEFRAME, (uint)(96 * GetScaling()));

    private int GetTopBorderHeight()
    {
        if (_isMaximized || _window.WindowState == WindowState.FullScreen)
        {
            return 0;
        }

        // This is always 1 regardless of DPI
        return 1;
    }

    private void EnsureExtended()
    {
        var marg = new MARGINS();

        var style = (int)GetWindowLongPtr((HWND)Hwnd, -16);
        var exStyle = (int)GetWindowLongPtr((HWND)Hwnd, -20);

        RECT frame;
        AdjustWindowRectExWithFallback(&frame, style, false, exStyle, (int)(GetScaling() * 96));

        marg.topHeight = -frame.top;

        var hr = DwmExtendFrameIntoClientArea((HWND)Hwnd, &marg);
        
        if (!hr.SUCCEEDED)
        {
            Logger.TryGet(LogEventLevel.Error, "AppWindow")?.Log("AppWindow.EnsureExtended", "DwmExtendFrameIntoClientArea failed with HR {hr}", hr);
        }

        SetWindowPos((HWND)Hwnd, HWND.NULL, 0, 0, 0, 0,
            SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOREPOSITION | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    private LRESULT HandleNCCALCSIZE(WPARAM wParam, LPARAM lParam)
    {
        var ncParams = Unsafe.Read<NCCALCSIZE_PARAMS>((void*)lParam.Value);

        var originalTop = ncParams.rgrc[0].top;
        var ret = DefWindowProcW((HWND)Hwnd, WM_NCCALCSIZE, wParam, lParam);
        if (ret != 0)
            return ret;

        UpdateContentPosition();

        var newSize = ncParams.rgrc[0];
        newSize.top = originalTop;
        var windowState = _window.WindowState;

        UpdateMaximizeState();
                
        if (_isMaximized)
        {
            newSize.top += GetResizeHandleHeight();
        }

        // Fullscreen is handled for us upstream in Win32 WindowImpl - all we need to do here
        // is remove the normal frame adjustments we do
        // This will prevent that weird effect some apps have where the top/left is offset when the 
        // window enters full screen 

        if (windowState != WindowState.FullScreen)
        {
            // Use AdjustWindowRectExForDpi here to get the frame adjustments on the remaining sides
            // Using 8 fails in high DPI scenarios, this tells us the exact difference of the NC frame
            // and we can get accurate values that work regardless of DPI without "hacking" it
            var style = (int)GetWindowLongPtr((HWND)Hwnd, -16);
            var exStyle = (int)GetWindowLongPtr((HWND)Hwnd, -20);

            RECT frame;
            AdjustWindowRectExWithFallback(&frame, style, false, exStyle, (int)(GetScaling() * 96));

            newSize.left -= frame.left; // left frame is negative, subtract to add it back
            newSize.right -= frame.right; // right frame is positive, subtract to pull it back
            newSize.bottom -= frame.bottom; // bottom frame is positive, subtract to pull it back
        }

        ncParams.rgrc[0] = newSize;

        Unsafe.Write((void*)lParam.Value, ncParams);

        return 0;
    }

    private LRESULT HandleNCHITTEST(LPARAM lParam)
    {
        // Because we still have the System Border (which technically extends beyond the actual window
        // into where the Drop shadows are), we can use DefWindowProc here to handle resizing, except
        // on the top. We'll handle that below
        var originalRet = DefWindowProcW((HWND)Hwnd, WM_NCHITTEST, 0, lParam);
        if (originalRet != HTCLIENT)
            return originalRet;

        // At this point, we know that the cursor is inside the client area so it
        // has to be either the little border at the top of our custom title bar,
        // the drag bar or something else in the XAML island. But the XAML Island
        // handles WM_NCHITTEST on its own so actually it cannot be the XAML
        // Island. Then it must be the drag bar or the little border at the top
        // which the user can use to move or resize the window.
        var point = _window.PointToClient(PointFromLParam(lParam));
        RECT rcWindow;
        GetWindowRect((HWND)Hwnd, &rcWindow);

        // On the Top border, the resize handle overlaps with the Titlebar area, which matches
        // a typical Win32 window or modern app window
        var resizeBorderHeight = GetResizeHandleHeight();
        var isOnResizeBorder = point.Y < resizeBorderHeight && !IsFullscreen;

        // Make sure the caption buttons still get precedence
        // This is where things get tricky too. On Win11, we still want to support the snap
        // layout feature when hovering over the Maximize button. Unfortunately no API exists
        // yet to call that manually if using a custom titlebar. But, if we return HT_MAXBUTTON
        // here, the pointer events no longer enter the window
        // See https://github.com/dotnet/wpf/issues/4825 for more on this...
        // To hack our way into making this work, we'll return HT_MAXBUTTON here, but manually
        // manage the state and handle stuff through the WM_NCLBUTTON... events
        // This only applies on Windows 11, Windows 10 will work normally b/c no snap layout thing

        if (_window.SystemCaptionControl?.HitTest(point, out var isMaximize) == true)
        {
            if (true && isMaximize)//_isWindows11
            {
                _fakingMaximizeButton = true;
                _window.SystemCaptionControl.FakeMaximizeHover(true);
                return HTMAXBUTTON;
            }
        }
        else
        {
            if (_fakingMaximizeButton)
            {
                _fakingMaximizeButton = false;
                _window.SystemCaptionControl?.ClearMaximizedState();
            }

            if (isOnResizeBorder)
            {
                if (_window.WindowState == WindowState.Maximized)
                {
                    return HTCAPTION;
                }
                else
                {
                    return HTTOP;
                }
            }

            // Hit Test titlebar region, except in full screen mode
            if (!IsFullscreen && _window.HitTestTitleBar(point))
            {
                return HTCAPTION;
            }
        }

        if (_fakingMaximizeButton)
        {
            _fakingMaximizeButton = false;
            _window.SystemCaptionControl?.ClearMaximizedState();
        }

        return HTCLIENT;
    }

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

    private void UpdateContentPosition()
    {
        var topHeight = GetTopBorderHeight() / GetScaling();

        // Why do we do this? We remove the entire top part of the frame between
        // DwmExtendFrameIntoClientArea and WM_NCCALCSIZE so we need to offset the 
        // window content by 1px to return the top frame border. Because we're doing
        // this in the xaml part of the window, we divide by scaling to undo it so we
        // get correct results - since scaling is automatically done for us
        // We also need to make the top border 0 when maximized otherwise the top pixel row
        // won't allow interactions
        _window?.UpdateContentPosition(new Thickness(0, (topHeight == 0 && !IsFullscreen) ? (-1 / GetScaling()) : topHeight, 0, 0));
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

    private void UpdateMaximizeState()
    {
        // Using the WindowState property is unreliable - seems to fail if titlebar
        // is double clicked, but works with the maximize button - this will work
        // in all cases
        var sty = GetWindowLongPtrW(Hwnd, GWL_STYLE);
        _isMaximized = (sty & WS_MAXIMIZE) == WS_MAXIMIZE;
    }

#if NET5_0_OR_GREATER
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
#endif

    private readonly AppWindow _window;
    private bool _fakingMaximizeButton;
    private bool _wasFakeMaximizeDown;
    private bool _isMaximized;

    private readonly nint _oldWndProc;
    private readonly nint _wndProc;
    private RECT _restoreRect;
}
