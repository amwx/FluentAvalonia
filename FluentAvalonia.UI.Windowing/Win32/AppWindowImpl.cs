using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Platform;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Windowing.Win32;
using static FluentAvalonia.UI.Windowing.Win32.Win32Interop;

namespace FluentAvalonia.UI.Windowing;

internal unsafe class AppWindowImpl : Avalonia.Win32.WindowImpl, IWindowImpl
{
    public AppWindowImpl()
        : base()
    {
        _isWindows11 = Interop.OSVersionHelper.IsWindows11();

        // AppWindow is forced into DarkMode ...ALWAYS... regardless of the true app theme
        // This matches Win10/11 apps and prevents that dreaded white border from showing
        // around the window; see GH #142
        Interop.Win32Interop.ApplyTheme(Hwnd, true);
    }

    public event EventHandler WindowOpened;

    protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
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

                return IntPtr.Zero;

            case WM_NCHITTEST:
                return HandleNCHITTEST(lParam);

            case WM_SIZE:
                if (_fakingMaximizeButton)
                {
                    _wasFakeMaximizeDown = false;
                    _owner.SystemCaptionControl.ClearMaximizedState();
                }
                break;

            case WM_NCLBUTTONDOWN:
                if (_fakingMaximizeButton)
                {
                    var point = PointToClient(PointFromLParam(lParam));
                    _owner.SystemCaptionControl.FakeMaximizePressed(_owner.SystemCaptionControl.HitTestMaxButton(point));
                    _wasFakeMaximizeDown = true;

                    // This is important. If we don't tell the System we've handled this, we'll get that
                    // classic Win32 button showing when we mouse press, and that's not good
                    return IntPtr.Zero;
                }
                break;

            case WM_NCLBUTTONUP:
                if (_fakingMaximizeButton && _wasFakeMaximizeDown)
                {
                    _owner.SystemCaptionControl.FakeMaximizePressed(false);
                    _wasFakeMaximizeDown = false;
                    _owner.SystemCaptionControl.FakeMaximizeClick();

                    return IntPtr.Zero;
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

            case WM_SETTINGCHANGE:
                {
                    HandleSETTINGCHANGED((WPARAM)wParam, lParam);
                }
                break;

                // TODO: System Params changed (Theme, accent, etc.)
        }

        return base.WndProc(hWnd, msg, wParam, lParam);
    }

    void IWindowImpl.Resize(Size value, PlatformResizeReason reason)
    {
        Resize(value, reason);
        unsafe
        {
            RECT rc;
            GetClientRect((HWND)Hwnd, &rc);

            // TODO: how does this work with DPI >96??
            if (rc.Width < value.Width || rc.Height < value.Height)
            {
                value = new Size(value.Width + 16, value.Height + 8);
                SetWindowPos((HWND)Hwnd, HWND.NULL,
                    0, 0, (int)value.Width, (int)value.Height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
            }
        }
    }

    public override void Show(bool activate, bool isDialog)
    {
        base.Show(activate, isDialog);

        WindowOpened?.Invoke(this, EventArgs.Empty);
    }

    public void SetOwner(AppWindow wnd)
    {
        _owner = wnd;
    }

    private int GetResizeHandleHeight() =>
        GetSystemMetricsForDpi(SM_CXPADDEDBORDER, (uint)(96 * RenderScaling)) +
        GetSystemMetricsForDpi(SM_CYSIZEFRAME, (uint)(96 * RenderScaling));

    private void EnsureExtended()
    {
        var marg = new MARGINS();

        var style = (int)GetWindowLongPtr((HWND)Hwnd, -16);
        var exStyle = (int)GetWindowLongPtr((HWND)Hwnd, -20);

        RECT frame;
        AdjustWindowRectExForDpi(&frame, style, false, exStyle, (int)(RenderScaling * 96));

        marg.topHeight = -frame.top;

        var hr = DwmExtendFrameIntoClientArea((HWND)Hwnd, &marg);

        if (!hr.SUCCEEDED)
        {
            Logger.TryGet(LogEventLevel.Error, "AppWindow")?.Log("AppWindow.EnsureExtended", "DwmExtendFrameIntoClientArea failed with HR {hr}", hr);
        }

        SetWindowPos((HWND)Hwnd, HWND.NULL, 0, 0, 0, 0,
            SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOREPOSITION | SWP_NOSIZE | SWP_NOZORDER);
    }

    private LRESULT HandleNCCALCSIZE(WPARAM wParam, LPARAM lParam)
    {
        var ncParams = Unsafe.Read<NCCALCSIZE_PARAMS>((void*)lParam.Value);

        var originalTop = ncParams.rgrc[0].top;
        var ret = DefWindowProcW((HWND)Hwnd, WM_NCCALCSIZE, wParam, lParam);
        if (ret != 0)
            return ret;

        var newSize = ncParams.rgrc[0];
        newSize.top = originalTop;

        if (WindowState == WindowState.Maximized)
        {
            newSize.top += GetResizeHandleHeight();
        }

        newSize.left += 8;
        newSize.right -= 8;
        newSize.bottom -= 8;

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
        var point = PointToClient(PointFromLParam(lParam));
        RECT rcWindow;
        GetWindowRect((HWND)Hwnd, &rcWindow);

        // On the Top border, the resize handle overlaps with the Titlebar area, which matches
        // a typical Win32 window or modern app window
        var resizeBorderHeight = GetResizeHandleHeight();
        var isOnResizeBorder = point.Y < resizeBorderHeight;

        // Make sure the caption buttons still get precedence
        // This is where things get tricky too. On Win11, we still want to support the snap
        // layout feature when hovering over the Maximize button. Unfortunately no API exists
        // yet to call that manually if using a custom titlebar. But, if we return HT_MAXBUTTON
        // here, the pointer events no longer enter the window
        // See https://github.com/dotnet/wpf/issues/4825 for more on this...
        // To hack our way into making this work, we'll return HT_MAXBUTTON here, but manually
        // manage the state and handle stuff through the WM_NCLBUTTON... events
        // This only applies on Windows 11, Windows 10 will work normally b/c no snap layout thing
                
        if (_owner.SystemCaptionControl?.HitTest(point, out var isMaximize) == true)
        {
            if (_isWindows11 && isMaximize)
            {
                _fakingMaximizeButton = true;
                _owner.SystemCaptionControl.FakeMaximizeHover(true);
                return HTMAXBUTTON;
            }
        }
        else
        {
            if (_fakingMaximizeButton)
            {
                _fakingMaximizeButton = false;
                _owner.SystemCaptionControl?.ClearMaximizedState();
            }

            if (isOnResizeBorder)
            {
                if (WindowState == WindowState.Maximized)
                {
                    return HTCAPTION;
                }
                else
                {
                    return HTTOP;
                }
            }

            // Hit Test titlebar region
            if (_owner.HitTestTitleBar(point))
            {
                return HTCAPTION;
            }
        }

        if (_fakingMaximizeButton)
        {
            _fakingMaximizeButton = false;
            _owner.SystemCaptionControl?.ClearMaximizedState();
        }

        return HTCLIENT;
    }

    private unsafe void HandleRBUTTONUP(LPARAM lParam)
    {
        var pt = PointFromLParam(lParam);
        if (_owner.HitTestTitleBar(pt.ToPoint(RenderScaling)))
        {
            var sysMenu = GetSystemMenu((HWND)Hwnd, false);
            bool isMax = WindowState == WindowState.Maximized;

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

            var scPt = PointToScreen(pt.ToPoint(RenderScaling));

            var ret = TrackPopupMenu(sysMenu, TPM_RETURNCMD, scPt.X, scPt.Y, 0, (HWND)Hwnd, null);
            if (ret)
            {
                PostMessage((HWND)Hwnd, WM_SYSCOMMAND, (WPARAM)ret, 0);
            }
        }
    }

    private unsafe void HandleSETTINGCHANGED(WPARAM wParam, LPARAM lParam)
    {
        if (_owner == null)
            return;

        // In my testing, this message is sent 4 times for every system change...YIKES
        try
        {
            var str = new string((char*)(nint)lParam);
            Debug.WriteLine($"SETTING {wParam} {str}");
            if (str.Equals("ImmersiveColorSet", StringComparison.OrdinalIgnoreCase))
            {
                // Theme change was done - this is anything in the Windows Setting area,
                // So it could be Light/Dark mode, or Accent Color
                // Tell FATheme to requery the system theme
                var faTheme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
                faTheme.ForceWin32WindowToTheme(_owner);
                faTheme.InvalidateThemingFromSystemThemeChanged();
            }
            else if (str.Equals("WindowsThemeElement"))
            {
                // When a constrast theme is activated, we get an ImmersiveColorSet message
                // one with wParam = 67, one with wParam = 4131, and one with lParam
                // set to "WindowsThemeElement" (the last one)
                // So we'll use this to trigger an invalidation of the HighContrast theme
                var faTheme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
                faTheme.ForceWin32WindowToTheme(_owner);
                faTheme.InvalidateThemingFromSystemThemeChanged();
            }
        }
        catch { }
        
    }

    private bool _isWindows11;
    private AppWindow _owner;
    private bool _fakingMaximizeButton;
    private bool _wasFakeMaximizeDown;
}
