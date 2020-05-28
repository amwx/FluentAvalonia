//This is just my Win32 interop file, it's a mess
//I play around alot :)
using System;
using System.Text;
using System.Runtime.InteropServices;
using Avalonia;

namespace FluentAvalonia.Interop
{
    internal static class Win32Interop
    {
#pragma warning disable CA1401

        public const int CW_USEDEFAULT = unchecked((int)0x80000000);

        [DllImport("Shell32.dll")]
        public static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken,
        out IntPtr ppszPath);
               
        /// <summary>
        /// This is BAD practice, but may be the easiest way to do this
        /// Note that changes to *.lnk files may break this code
        /// </summary>
        /// <param name="fileLink"></param>
        /// <returns></returns>
        public static string LnkToFile(string fileLink)
        {
            string link = System.IO.File.ReadAllText(fileLink);
            int i1 = link.IndexOf("DATA\0");
            if (i1 < 0)
                return null;
            i1 += 5;
            int i2 = link.IndexOf("\0", i1);
            if (i2 < 0)
                return link.Substring(i1);
            else
                return link.Substring(i1, i2 - i1);
        }


        [DllImport("dwmapi.dll", SetLastError = true)]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        public static Avalonia.Media.Color GetThemeColor()
        {
            var colorSetEx = GetImmersiveColorFromColorSetEx(
            (uint)GetImmersiveUserColorSetPreference(false, false),
            GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni("ImmersiveSystemAccent")),//ImmersiveStartSelectionBackground")),
            false, 0);

            var colour = Avalonia.Media.Color.FromArgb((byte)((0xFF000000 & colorSetEx) >> 24), (byte)(0x000000FF & colorSetEx),
                (byte)((0x0000FF00 & colorSetEx) >> 8), (byte)((0x00FF0000 & colorSetEx) >> 16));

            return colour;
        }

        public static Avalonia.Media.Color GetThemeColorRef(string h, bool ignoreHighContrast = false)
        {
            var colorSetEx = GetImmersiveColorFromColorSetEx(
            (uint)GetImmersiveUserColorSetPreference(false, false),
            GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni(h)),
            ignoreHighContrast, 0);

            var a = 0xFFFFFF & colorSetEx >> 24;
            var r = (0xFFFFFF & colorSetEx);
            var g = (0xFFFFFF & colorSetEx) >> 8;
            var b = (0xFFFFFF & colorSetEx) >> 16;

            var colour = Avalonia.Media.Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);

            return colour;
        }

        [DllImport("uxtheme.dll", EntryPoint = "#132")]
        public static extern bool fnShouldAppsUseDarkMode();
        [DllImport("uxtheme.dll", EntryPoint = "#138")]
        public static extern bool fnShouldSystemUseDarkMode();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError =true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("dwmapi.dll", PreserveSig = true, SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int value, int attrSize);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(IntPtr hMenu, int uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MONITOR dwFlags);

        [DllImport("user32", EntryPoint = "GetMonitorInfoW", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo([In] IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32", EntryPoint = "SetWindowLongPtrA", CharSet = CharSet.Ansi)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32", EntryPoint = "SetWindowLongPtrA", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACMENT lpwndpl);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACMENT
        {
            public uint length;
            public uint flags;
            public uint showCmd;
            public PixelPoint ptMinPosition;
            public PixelPoint ptMaxPosition;
            public PixelRect rcNormalPosition;
            public PixelRect rcDevice;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(long dwExStyle, uint lpClassName, [MarshalAs(UnmanagedType.LPStr)]string lpWindowName,
            uint dwStyle, int x, int y, int cx, int cy, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        public static extern int DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [Flags]
        public enum DWM_BB
        {
            Enable = 1,
            BlurRegion = 2,
            TransitionMaximized = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;

            public DWM_BLURBEHIND(bool enabled)
            {
                fEnable = enabled;// ? 1 : 0;
                hRgnBlur = IntPtr.Zero;
                fTransitionOnMaximized = false;// 0;
                dwFlags = DWM_BB.Enable;
            }

            public System.Drawing.Region Region
            {
                get { return System.Drawing.Region.FromHrgn(hRgnBlur); }
            }

            public bool TransitionOnMaximized
            {
                get { return fTransitionOnMaximized; } //> 0
                set
                {
                    fTransitionOnMaximized = value;// ? 1 : 0;
                    dwFlags |= DWM_BB.TransitionMaximized;
                }
            }

            public void SetRegion(IntPtr hwnd, System.Drawing.Region region)
            {
                var graphics = System.Drawing.Graphics.FromHwnd(hwnd);
                hRgnBlur = region.GetHrgn(graphics);
                dwFlags |= DWM_BB.BlurRegion;
                graphics.Dispose();
            }
        }

#pragma warning enable CA1401

        public const int TPM_LEFTBUTTON = 0x0;
        public const int TPM_RIGHTBUTTON = 0x2;
        public const int TPM_RETURNCMD = 0x100;
        

    }

    [Flags]
    public enum ClassStyles : uint
    {
        /// <summary>Aligns the window's client area on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.</summary>
        ByteAlignClient = 0x1000,
        CS_VREDRAW = 0x0001,
        CS_HREDRAW = 0x0002,
        CS_DBLCLKS = 0x0008,
        CS_OWNDC = 0x0020,
        CS_CLASSDC = 0x0040,
        CS_PARENTDC = 0x0080,
        CS_NOCLOSE = 0x0200,
        CS_SAVEBITS = 0x0800,
        CS_BYTEALIGNCLIENT = 0x1000,
        CS_BYTEALIGNWINDOW = 0x2000,
        CS_GLOBALCLASS = 0x4000,
        CS_IME = 0x00010000,
        CS_DROPSHADOW = 0x00020000
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum IDC
    {
        IDC_ARROW = 32512,
        IDC_IBEAM = 32513,
        IDC_WAIT = 32514,
        IDC_CROSS = 32515,
        IDC_UPARROW = 32516,
        IDC_SIZE = 32640,
        IDC_ICON = 32641,
        IDC_SIZENWSE = 32642,
        IDC_SIZENESW = 32643,
        IDC_SIZEWE = 32644,
        IDC_SIZENS = 32645,
        IDC_SIZEALL = 32646,
        IDC_NO = 32648,
        IDC_HAND = 32649,
        IDC_APPSTARTING = 32650,
        IDC_HELP = 32651
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASSEX
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public int style;
        public IntPtr lpfnWndProc; // not WndProc
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;

        //Use this function to make a new one with cbSize already filled in.
        //For example:
        //var WndClss = WNDCLASSEX.Build()
        public static WNDCLASSEX Build()
        {
            var nw = new WNDCLASSEX();
            nw.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            return nw;
        }
    }



    public enum DWMWINDOWATTRIBUTE : uint
    {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation
    }

    public struct POINT
    {
        public int X;
        public int Y;
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int Width => right - left;
        public int Height => bottom - top;
        public RECT(Rect rect)
        {
            left = (int)rect.X;
            top = (int)rect.Y;
            right = (int)(rect.X + rect.Width);
            bottom = (int)(rect.Y + rect.Height);
        }

        public void Offset(POINT pt)
        {
            left += pt.X;
            right += pt.X;
            top += pt.Y;
            bottom += pt.Y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hWndInsertAfter;
        public IntPtr hWnd;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NCCALCSIZE_PARAMS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public RECT[] rgrc;
        public WINDOWPOS lppos;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum MONITOR
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;

        public static MONITORINFO Create()
        {
            return new MONITORINFO() { cbSize = Marshal.SizeOf<MONITORINFO>() };
        }

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }
    }



    [Flags]
    public enum WindowStyles : uint
    {
        WS_BORDER = 0x800000,
        WS_CAPTION = 0xc00000,
        WS_CHILD = 0x40000000,
        WS_CLIPCHILDREN = 0x2000000,
        WS_CLIPSIBLINGS = 0x4000000,
        WS_DISABLED = 0x8000000,
        WS_DLGFRAME = 0x400000,
        WS_GROUP = 0x20000,
        WS_HSCROLL = 0x100000,
        WS_MAXIMIZE = 0x1000000,
        WS_MAXIMIZEBOX = 0x10000,
        WS_MINIMIZE = 0x20000000,
        WS_MINIMIZEBOX = 0x20000,
        WS_OVERLAPPED = 0x0,
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000u,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEFRAME = 0x40000,
        WS_SYSMENU = 0x80000,
        WS_TABSTOP = 0x10000,
        WS_VISIBLE = 0x10000000,
        WS_VSCROLL = 0x200000
    }

    [Flags]
    public enum WindowStylesEx : uint
    {
        WS_EX_ACCEPTFILES = 0x00000010,

        WS_EX_APPWINDOW = 0x00040000,

        WS_EX_CLIENTEDGE = 0x00000200,

        WS_EX_COMPOSITED = 0x02000000,

        WS_EX_CONTEXTHELP = 0x00000400,

        WS_EX_CONTROLPARENT = 0x00010000,

        WS_EX_DLGMODALFRAME = 0x00000001,

        WS_EX_LAYERED = 0x00080000,

        WS_EX_LAYOUTRTL = 0x00400000,

        WS_EX_LEFT = 0x00000000,

        WS_EX_LEFTSCROLLBAR = 0x00004000,

        WS_EX_LTRREADING = 0x00000000,

        WS_EX_MDICHILD = 0x00000040,

        WS_EX_NOACTIVATE = 0x08000000,

        WS_EX_NOINHERITLAYOUT = 0x00100000,

        WS_EX_NOPARENTNOTIFY = 0x00000004,

        WS_EX_NOREDIRECTIONBITMAP = 0x00200000,

        WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

        WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

        WS_EX_RIGHT = 0x00001000,

        WS_EX_RIGHTSCROLLBAR = 0x00000000,

        WS_EX_RTLREADING = 0x00002000,

        WS_EX_STATICEDGE = 0x00020000,

        WS_EX_TOOLWINDOW = 0x00000080,

        WS_EX_TOPMOST = 0x00000008,

        WS_EX_TRANSPARENT = 0x00000020,

        WS_EX_WINDOWEDGE = 0x00000100
    }


    public enum SWP : uint
    {
        NOSIZE = 0x0001,
        NOMOVE = 0x0002,
        NOZORDER = 0x0004,
        NOREDRAW = 0x0008,
        NOACTIVATE = 0x0010,
        DRAWFRAME = 0x0020,
        FRAMECHANGED = 0x0020,
        SHOWWINDOW = 0x0040,
        HIDEWINDOW = 0x0080,
        NOCOPYBITS = 0x0100,
        NOOWNERZORDER = 0x0200,
        NOREPOSITION = 0x0200,
        NOSENDCHANGING = 0x0400,
        DEFERERASE = 0x2000,
        ASYNCWINDOWPOS = 0x4000
    }

    public enum ImmersiveColors
    {
        ImmersiveStartBackground,
        ImmersiveStartDesktopTilesBackground,
        ImmersiveStartDesktopTilesText,
        ImmersiveStartSystemTilesBackground,
        ImmersiveStartFocusRect,
        ImmersiveStartBackgroundDisabled,
        ImmersiveStartPrimaryText,
        ImmersiveStartSecondaryText,
        ImmersiveStartDisabledText,
        ImmersiveStartSelectionBackground,
        ImmersiveStartSelectionPrimaryText,
        ImmersiveStartHoverBackground,
        ImmersiveStartHoverPrimaryText,
        ImmersiveStartHighlight,
        ImmersiveStartInlineErrorText,
        ImmersiveStartControlLink,
        ImmersiveStartControlLinkVisited,
        ImmersiveStartControlLinkDisabled,
        ImmersiveStartControlLinkPressed,
        ImmersiveStartControlLinkMouseHover,
        ImmersiveStartControlLinkForegroundPressed,
        ImmersiveStartControlLinkBackgroundPressed,
        ImmersiveStartCommandRowRest,
        ImmersiveStartCommandRowHover,
        ImmersiveStartCommandRowPressed,
        ImmersiveStartCommandRowDisabled,
        ImmersiveStartCommandRowHighlight,
        ImmersiveStartFolderBackground,
        ImmersiveStartThumbnailPlaceholder,
        ImmersiveStartDefaultDarkFocusRect,
        ImmersiveStartDefaultLightFocusRect,
        ImmersiveSaturatedBackground,
        ImmersiveSaturatedBackgroundDisabled,
        ImmersiveSaturatedFocusRectDark,
        ImmersiveSaturatedFocusRect,
        ImmersiveSaturatedDefaultDarkFocusRect,
        ImmersiveSaturatedDefaultLightFocusRect,
        ImmersiveSaturatedPrimaryText,
        ImmersiveSaturatedSecondaryText,
        ImmersiveSaturatedSelectionBackground,
        ImmersiveSaturatedSelectionPrimaryText,
        ImmersiveSaturatedSelectionSecondaryText,
        ImmersiveSaturatedHoverBackground,
        ImmersiveSaturatedHoverPrimaryText,
        ImmersiveSaturatedHoverSecondaryText,
        ImmersiveSaturatedDivider,
        ImmersiveSaturatedHighlight,
        ImmersiveSaturatedInlineErrorText,
        ImmersiveSaturatedControlLink,
        ImmersiveSaturatedControlLinkVisited,
        ImmersiveSaturatedControlLinkDisabled,
        ImmersiveSaturatedControlLinkPressed,
        ImmersiveSaturatedControlLinkMouseHover,
        ImmersiveSaturatedControlLinkForegroundPressed,
        ImmersiveSaturatedControlLinkBackgroundPressed,
        ImmersiveSaturatedSystemToastBackground,
        ImmersiveSaturatedDesktopToastBackground,
        ImmersiveSaturatedFolderBackground,
        ImmersiveSaturatedThumbnailPlaceholder,
        ImmersiveSaturatedAltTabBackground,
        ImmersiveSaturatedAltTabHoverRect,
        ImmersiveSaturatedAltTabPressedRect,
        ImmersiveSaturatedCommandRowRest,
        ImmersiveSaturatedCommandRowHover,
        ImmersiveSaturatedCommandRowPressed,
        ImmersiveSaturatedCommandRowDisabled,
        ImmersiveSaturatedCommandRowHighlight,
        ImmersiveSaturatedSettingCharmSystemPaneButtonText,
        ImmersiveSaturatedSettingCharmSystemPaneButtonTextHover,
        ImmersiveSaturatedSettingCharmSystemPaneButtonTextPressed,
        ImmersiveSaturatedSettingCharmSystemPaneButtonTextSelected,
        ImmersiveSaturatedSettingCharmSystemPaneButtonTextDisabled,
        ImmersiveSaturatedSettingCharmSystemPaneButtonRest,
        ImmersiveSaturatedSettingCharmSystemPaneButtonHover,
        ImmersiveSaturatedSettingCharmSystemPaneButtonPressed,
        ImmersiveSaturatedSettingCharmSystemPaneButtonSelected,
        ImmersiveSaturatedSettingCharmSystemPaneButtonDisabled,
        ImmersiveSaturatedBackButtonBar,
        ImmersiveLightFocusRect,
        ImmersiveLightBackground,
        ImmersiveLightBackgroundDisabled,
        ImmersiveLightTitleText,
        ImmersiveLightPrimaryText,
        ImmersiveLightSecondaryText,
        ImmersiveLightTabText,
        ImmersiveLightSelectedTabText,
        ImmersiveLightSelectionBackground,
        ImmersiveLightSelectionPrimaryText,
        ImmersiveLightSelectionSecondaryText,
        ImmersiveLightHoverBackground,
        ImmersiveLightHoverPrimaryText,
        ImmersiveLightHoverSecondaryText,
        ImmersiveLightHighlight,
        ImmersiveLightInlineErrorText,
        ImmersiveLightWUNormal,
        ImmersiveLightWUWarning,
        ImmersiveLightWUError,
        ImmersiveLightControlLink,
        ImmersiveLightControlLinkVisited,
        ImmersiveLightControlLinkDisabled,
        ImmersiveLightControlLinkPressed,
        ImmersiveLightControlLinkMouseHover,
        ImmersiveLightControlLinkForegroundPressed,
        ImmersiveLightControlLinkBackgroundPressed,
        ImmersiveHardwarePrimaryText,
        ImmersiveHardwareClockBackground,
        ImmersiveHardwareClockText,
        ImmersiveHardwareCharmsBarBackground,
        ImmersiveHardwareCharmsBarBackgroundRest,
        ImmersiveHardwareCharmsBarBackgroundHotTrack,
        ImmersiveHardwareCharmsBarBackgroundPressed,
        ImmersiveHardwareCharmsBarText,
        ImmersiveHardwareCharmsBarTextDisabled,
        ImmersiveHardwareGutterRest,
        ImmersiveHardwareGutterDown,
        ImmersiveHardwareSettingCharmSystemPaneButtonText,
        ImmersiveHardwareSettingCharmSystemPaneButtonTextHover,
        ImmersiveHardwareSettingCharmSystemPaneButtonTextPressed,
        ImmersiveHardwareSettingCharmSystemPaneButtonTextSelected,
        ImmersiveHardwareSettingCharmSystemPaneButtonTextDisabled,
        ImmersiveHardwareSettingCharmSystemPaneButtonRest,
        ImmersiveHardwareSettingCharmSystemPaneButtonHover,
        ImmersiveHardwareSettingCharmSystemPaneButtonPressed,
        ImmersiveHardwareSettingCharmSystemPaneButtonSelected,
        ImmersiveHardwareSettingCharmSystemPaneButtonDisabled,
        ImmersiveHardwareKeyboardBackground,
        ImmersiveHardwareKeyboardKeyBackgroundDisabled,
        ImmersiveHardwareKeyboardKeyPrimaryTextDisabled,
        ImmersiveHardwareKeyboardKeySecondaryTextDisabled,
        ImmersiveHardwareKeyboardKeyBackgroundRest,
        ImmersiveHardwareKeyboardKeyPrimaryTextRest,
        ImmersiveHardwareKeyboardKeySecondaryTextRest,
        ImmersiveHardwareKeyboardKeyBackgroundPressed,
        ImmersiveHardwareKeyboardKeyPrimaryTextPressed,
        ImmersiveHardwareKeyboardKeySecondaryTextPressed,
        ImmersiveHardwareKeyboardKeyBackgroundHover,
        ImmersiveHardwareKeyboardDarkSpaceKeyBackgroundPressed,
        ImmersiveHardwareDefaultKeyboardKeyBackgroundRest,
        ImmersiveHardwareDefaultKeyboardKeyPrimaryTextRest,
        ImmersiveHardwareDefaultKeyboardKeySecondaryTextRest,
        ImmersiveHardwareDefaultKeyboardKeyBackgroundHover,
        ImmersiveHardwareKeyboardNumberKeyBackground,
        ImmersiveHardwareKeyboardNumberKeyBackgroundHover,
        ImmersiveHardwareKeyboardNumberKeyText,
        ImmersiveHardwareKeyboardFunctionKeyBackground,
        ImmersiveHardwareKeyboardFunctionKeyBackgroundHover,
        ImmersiveHardwareKeyboardFunctionKeyText,
        ImmersiveHardwareKeyboardFunctionKeyTextDisabled,
        ImmersiveHardwareKeyboardChildPanelBackground,
        ImmersiveHardwareKeyboardChildPanelKeyBackground,
        ImmersiveHardwareKeyboardChildKeyKeyText,
        ImmersiveHardwareKeyboardKeyBorder,
        ImmersiveHardwareHandwritingPanelBorder,
        ImmersiveHardwareHandwritingPanelKanjiConversionText,
        ImmersiveHardwareHandwritingPanelKanjiConversionBackground,
        ImmersiveHardwareHandwritingPanelInsertModeCharacter,
        ImmersiveHardwareHandwritingPanelSuggestedWord,
        ImmersiveHardwareHandwritingPanelCorrectionText,
        ImmersiveHardwareHandwritingPanelMatchedText,
        ImmersiveHardwareHandwritingPanelButtonRest,
        ImmersiveHardwareHandwritingPanelButtonHover,
        ImmersiveHardwareHandwritingPanelButtonPress,
        ImmersiveHardwareHandwritingPanelButtonBorder,
        ImmersiveHardwareHandwritingPanelConversionSelectedBackground,
        ImmersiveHardwareHandwritingPanelConversionUnselectedBackground,
        ImmersiveHardwareHandwritingPanelConversionSelectedText,
        ImmersiveHardwareHandwritingPanelConversionUnselectedText,
        ImmersiveHardwareTextPredictionBackgroundRest,
        ImmersiveHardwareTextPredictionBackgroundPressed,
        ImmersiveHardwareTextPredictionBorder,
        ImmersiveHardwareTextPredictionTextRest,
        ImmersiveHardwareTextPredictionTextPressed,
        ImmersiveHardwareControlLink,
        ImmersiveHardwareControlLinkVisited,
        ImmersiveHardwareControlLinkDisabled,
        ImmersiveHardwareControlLinkPressed,
        ImmersiveHardwareControlLinkMouseHover,
        ImmersiveControlTransparent,
        ImmersiveControlDarkRoundButtonOutlineDisabled,
        ImmersiveControlDarkRoundButtonOutlineLayerRest,
        ImmersiveControlDarkRoundButtonOutlineLayerHover,
        ImmersiveControlDarkRoundButtonOutlineLayerPressed,
        ImmersiveControlDarkRoundButtonGlyphDisabled,
        ImmersiveControlDarkRoundButtonGlyphLayerRest,
        ImmersiveControlDarkRoundButtonGlyphLayerHover,
        ImmersiveControlDarkRoundButtonGlyphLayerPressed,
        ImmersiveControlDarkRoundButtonFillLayerDisabled,
        ImmersiveControlDarkRoundButtonFillLayerRest,
        ImmersiveControlDarkRoundButtonFillLayerHover,
        ImmersiveControlDarkRoundButtonFillLayerPressed,
        ImmersiveControlLightRoundButtonOutlineDisabled,
        ImmersiveControlLightRoundButtonOutlineLayerRest,
        ImmersiveControlLightRoundButtonOutlineLayerHover,
        ImmersiveControlLightRoundButtonOutlineLayerPressed,
        ImmersiveControlLightRoundButtonGlyphDisabled,
        ImmersiveControlLightRoundButtonGlyphLayerRest,
        ImmersiveControlLightRoundButtonGlyphLayerHover,
        ImmersiveControlLightRoundButtonGlyphLayerPressed,
        ImmersiveControlLightRoundButtonFillLayerDisabled,
        ImmersiveControlLightRoundButtonFillLayerRest,
        ImmersiveControlLightRoundButtonFillLayerHover,
        ImmersiveControlLightRoundButtonFillLayerPressed,
        ImmersiveControlRadioButtonBackgroundSelected,
        ImmersiveControlRadioButtonBackgroundDisabledSelected,
        ImmersiveControlRadioButtonBackgroundDisabledHover,
        ImmersiveControlRadioButtonBackgroundDisabledPressed,
        ImmersiveControlRadioButtonTextDisabledSelected,
        ImmersiveControlRadioButtonTextDisabledHover,
        ImmersiveControlRadioButtonTextDisabledPressed,
        ImmersiveControlRadioButtonTextSelected,
        ImmersiveControlRadioButtonBorder,
        ImmersiveControlRadioButtonSeparator,
        ImmersiveControlDarkCheckboxLabelRest,
        ImmersiveControlDarkCheckboxBackgroundRest,
        ImmersiveControlDarkCheckboxBackgroundPressed,
        ImmersiveControlDarkCheckboxBackgroundDisabled,
        ImmersiveControlDarkCheckboxBackgroundHover,
        ImmersiveControlDarkCheckboxLabelHover,
        ImmersiveControlDarkCheckboxBorderRest,
        ImmersiveControlDarkCheckboxBorderPressed,
        ImmersiveControlDarkCheckboxBorderDisabled,
        ImmersiveControlDarkCheckboxBorderHover,
        ImmersiveControlDarkCheckboxLabelPressed,
        ImmersiveControlDarkCheckboxLabelDisabled,
        ImmersiveControlDarkCheckboxGlyphPressed,
        ImmersiveControlDarkCheckboxGlyphHover,
        ImmersiveControlDarkCheckboxGlyphRest,
        ImmersiveControlDarkCheckboxGlyphDisabled,
        ImmersiveControlLightCheckboxLabelPressed,
        ImmersiveControlLightCheckboxLabelRest,
        ImmersiveControlLightCheckboxBackgroundRest,
        ImmersiveControlLightCheckboxBackgroundPressed,
        ImmersiveControlLightCheckboxBackgroundDisabled,
        ImmersiveControlLightCheckboxBackgroundHover,
        ImmersiveControlLightCheckboxLabelHover,
        ImmersiveControlLightCheckboxBorderRest,
        ImmersiveControlLightCheckboxBorderPressed,
        ImmersiveControlLightCheckboxBorderDisabled,
        ImmersiveControlLightCheckboxBorderHover,
        ImmersiveControlLightCheckboxLabelDisabled,
        ImmersiveControlLightCheckboxGlyphPressed,
        ImmersiveControlLightCheckboxGlyphHover,
        ImmersiveControlLightCheckboxGlyphRest,
        ImmersiveControlLightCheckboxGlyphDisabled,
        ImmersiveControlDarkButtonBorderDisabled,
        ImmersiveControlDarkButtonTextDisabled,
        ImmersiveControlDarkButtonBorderPressed,
        ImmersiveControlDarkButtonTextHover,
        ImmersiveControlDarkButtonBorderHover,
        ImmersiveControlDarkButtonTextPressed,
        ImmersiveControlDarkButtonBorderRest,
        ImmersiveControlDarkButtonTextRest,
        ImmersiveControlDarkButtonBackgroundRest,
        ImmersiveControlDarkButtonBackgroundPressed,
        ImmersiveControlDarkButtonBackgroundDisabled,
        ImmersiveControlDarkButtonBackgroundHover,
        ImmersiveControlLightButtonBorderPressed,
        ImmersiveControlLightButtonBackgroundPressed,
        ImmersiveControlLightButtonBorderRest,
        ImmersiveControlLightButtonBackgroundRest,
        ImmersiveControlLightButtonBorderHover,
        ImmersiveControlLightButtonBorderDisabled,
        ImmersiveControlLightButtonBackgroundHover,
        ImmersiveControlLightButtonBackgroundDisabled,
        ImmersiveControlLightButtonTextHover,
        ImmersiveControlLightButtonTextDisabled,
        ImmersiveControlLightButtonTextPressed,
        ImmersiveControlLightButtonTextRest,
        ImmersiveControlDefaultDarkButtonTextPressed,
        ImmersiveControlDefaultDarkButtonTextHover,
        ImmersiveControlDefaultDarkButtonBorderRest,
        ImmersiveControlDefaultDarkButtonTextRest,
        ImmersiveControlDefaultDarkButtonBackgroundRest,
        ImmersiveControlDefaultDarkButtonBackgroundPressed,
        ImmersiveControlDefaultDarkButtonBorderHover,
        ImmersiveControlDefaultDarkButtonBorderPressed,
        ImmersiveControlDefaultDarkButtonBorderDisabled,
        ImmersiveControlDefaultDarkButtonTextDisabled,
        ImmersiveControlDefaultDarkButtonBackgroundDisabled,
        ImmersiveControlDefaultDarkButtonBackgroundHover,
        ImmersiveControlDefaultLightButtonBorderDisabled,
        ImmersiveControlDefaultLightButtonTextDisabled,
        ImmersiveControlDefaultLightButtonBorderPressed,
        ImmersiveControlDefaultLightButtonTextPressed,
        ImmersiveControlDefaultLightButtonTextRest,
        ImmersiveControlDefaultLightButtonTextHover,
        ImmersiveControlDefaultLightButtonBorderHover,
        ImmersiveControlDefaultLightButtonBorderRest,
        ImmersiveControlDefaultLightButtonBackgroundRest,
        ImmersiveControlDefaultLightButtonBackgroundHover,
        ImmersiveControlDefaultLightButtonBackgroundPressed,
        ImmersiveControlDefaultLightButtonBackgroundDisabled,
        ImmersiveControlDarkSelectBorderRest,
        ImmersiveControlDarkSelectBackgroundHover,
        ImmersiveControlDarkSelectBorderHover,
        ImmersiveControlDarkSelectBackgroundPressed,
        ImmersiveControlDarkSelectBorderPressed,
        ImmersiveControlDarkSelectBackgroundDisabled,
        ImmersiveControlDarkSelectTextRest,
        ImmersiveControlDarkSelectTextPressed,
        ImmersiveControlDarkSelectTextHover,
        ImmersiveControlDarkSelectGlyphDisabled,
        ImmersiveControlDarkSelectTextDisabled,
        ImmersiveControlDarkSelectBorderDisabled,
        ImmersiveControlDarkSelectGlyphRest,
        ImmersiveControlDarkSelectTextHighlighted,
        ImmersiveControlDarkSelectHighlightedTextPressed,
        ImmersiveControlDarkSelectHighlightPressed,
        ImmersiveControlDarkSelectHighlightSelected,
        ImmersiveControlDarkSelectHighlightHover,
        ImmersiveControlDarkSelectBackgroundRest,
        ImmersiveControlDarkSelectSecondaryTextPressed,
        ImmersiveControlDarkSelectSecondaryTextHighlighted,
        ImmersiveControlDarkSelectSecondaryTextHover,
        ImmersiveControlDarkSelectHighlightedSecondaryTextPressed,
        ImmersiveControlLightSelectBorderRest,
        ImmersiveControlLightSelectBackgroundRest,
        ImmersiveControlLightSelectBackgroundHover,
        ImmersiveControlLightSelectBorderHover,
        ImmersiveControlLightSelectBackgroundPressed,
        ImmersiveControlLightSelectBorderPressed,
        ImmersiveControlLightSelectBackgroundDisabled,
        ImmersiveControlLightSelectTextPressed,
        ImmersiveControlLightSelectGlyphDisabled,
        ImmersiveControlLightSelectTextDisabled,
        ImmersiveControlLightSelectBorderDisabled,
        ImmersiveControlLightSelectGlyphRest,
        ImmersiveControlLightSelectTextHighlighted,
        ImmersiveControlLightSelectHighlightedTextPressed,
        ImmersiveControlLightSelectHighlightPressed,
        ImmersiveControlLightSelectHighlightSelected,
        ImmersiveControlLightSelectTextHover,
        ImmersiveControlLightSelectTextRest,
        ImmersiveControlLightSelectHighlightHover,
        ImmersiveControlDarkRichEditBackgroundRest,
        ImmersiveControlDarkRichEditBorderRest,
        ImmersiveControlDarkRichEditBorderPressed,
        ImmersiveControlDarkRichEditBorderFocus,
        ImmersiveControlDarkRichEditBackgroundPressed,
        ImmersiveControlDarkRichEditBackgroundFocus,
        ImmersiveControlDarkRichEditBackgroundHover,
        ImmersiveControlDarkRichEditBackgroundDisabled,
        ImmersiveControlDarkRichEditBorderHover,
        ImmersiveControlDarkRichEditTextHelper,
        ImmersiveControlDarkRichEditTextRest,
        ImmersiveControlDarkRichEditTextFocus,
        ImmersiveControlDarkRichEditTextHighlighted,
        ImmersiveControlDarkRichEditTextDisabled,
        ImmersiveControlDarkRichEditBorderDisabled,
        ImmersiveControlDarkRichEditTextHover,
        ImmersiveControlDarkRichEditButtonBackgroundRest,
        ImmersiveControlDarkRichEditButtonBackgroundHover,
        ImmersiveControlDarkRichEditButtonBackgroundPressed,
        ImmersiveControlDarkRichEditButtonGlyphRest,
        ImmersiveControlDarkRichEditButtonGlyphHover,
        ImmersiveControlDarkRichEditButtonGlyphPressed,
        ImmersiveControlDarkRichEditHighlight,
        ImmersiveControlLightRichEditBackgroundRest,
        ImmersiveControlLightRichEditBorderRest,
        ImmersiveControlLightRichEditBorderPressed,
        ImmersiveControlLightRichEditBorderFocus,
        ImmersiveControlLightRichEditBackgroundPressed,
        ImmersiveControlLightRichEditBackgroundFocus,
        ImmersiveControlLightRichEditBackgroundHover,
        ImmersiveControlLightRichEditBackgroundDisabled,
        ImmersiveControlLightRichEditBorderHover,
        ImmersiveControlLightRichEditTextHelper,
        ImmersiveControlLightRichEditTextRest,
        ImmersiveControlLightRichEditTextFocus,
        ImmersiveControlLightRichEditTextDisabled,
        ImmersiveControlLightRichEditBorderDisabled,
        ImmersiveControlLightRichEditTextHover,
        ImmersiveControlLightRichEditButtonBackgroundRest,
        ImmersiveControlLightRichEditButtonBackgroundHover,
        ImmersiveControlLightRichEditButtonBackgroundPressed,
        ImmersiveControlLightRichEditButtonGlyphRest,
        ImmersiveControlLightRichEditButtonGlyphHover,
        ImmersiveControlLightRichEditButtonGlyphPressed,
        ImmersiveControlLightRichEditHighlight,
        ImmersiveControlTooltipBackground,
        ImmersiveControlTooltipDomainText,
        ImmersiveControlTooltipText,
        ImmersiveControlSliderTooltipText,
        ImmersiveControlTooltipBorder,
        ImmersiveControlDarkProgressBackground,
        ImmersiveControlDarkProgressForeground,
        ImmersiveControlLightProgressBackground,
        ImmersiveControlLightProgressForeground,
        ImmersiveControlProgressBorder,
        ImmersiveControlDarkToggleLabelDisabled,
        ImmersiveControlLightToggleLabelDisabled,
        ImmersiveControlDarkToggleOnOffTextDisabled,
        ImmersiveControlDarkToggleOnOffTextEnabled,
        ImmersiveControlLightToggleOnOffTextDisabled,
        ImmersiveControlLightToggleOnOffTextEnabled,
        ImmersiveControlDarkToggleThumbDisabled,
        ImmersiveControlLightToggleThumbDisabled,
        ImmersiveControlDarkToggleTrackBackgroundDisabled,
        ImmersiveControlLightToggleTrackBackgroundDisabled,
        ImmersiveControlDarkToggleTrackBorderDisabled,
        ImmersiveControlLightToggleTrackBorderDisabled,
        ImmersiveControlDarkToggleTrackFillDisabled,
        ImmersiveControlLightToggleTrackFillDisabled,
        ImmersiveControlDarkToggleTrackGutterDisabled,
        ImmersiveControlLightToggleTrackGutterDisabled,
        ImmersiveControlDarkSliderThumbDisabled,
        ImmersiveControlDarkSliderThumbHover,
        ImmersiveControlDarkSliderThumbPressed,
        ImmersiveControlDarkSliderThumbRest,
        ImmersiveControlLightSliderThumbDisabled,
        ImmersiveControlLightSliderThumbHover,
        ImmersiveControlLightSliderThumbPressed,
        ImmersiveControlLightSliderThumbRest,
        ImmersiveControlDarkSliderTickMark,
        ImmersiveControlLightSliderTickMark,
        ImmersiveControlDarkSliderTrackBackgroundDisabled,
        ImmersiveControlDarkSliderTrackBackgroundHover,
        ImmersiveControlDarkSliderTrackBackgroundPressed,
        ImmersiveControlDarkSliderTrackBackgroundRest,
        ImmersiveControlLightSliderTrackBackgroundDisabled,
        ImmersiveControlLightSliderTrackBackgroundHover,
        ImmersiveControlLightSliderTrackBackgroundPressed,
        ImmersiveControlLightSliderTrackBackgroundRest,
        ImmersiveControlDarkSliderTrackBufferingDisabled,
        ImmersiveControlDarkSliderTrackBufferingHover,
        ImmersiveControlDarkSliderTrackBufferingPressed,
        ImmersiveControlDarkSliderTrackBufferingRest,
        ImmersiveControlLightSliderTrackBufferingDisabled,
        ImmersiveControlLightSliderTrackBufferingHover,
        ImmersiveControlLightSliderTrackBufferingPressed,
        ImmersiveControlLightSliderTrackBufferingRest,
        ImmersiveControlDarkSliderTrackFillDisabled,
        ImmersiveControlDarkSliderTrackFillHover,
        ImmersiveControlDarkSliderTrackFillPressed,
        ImmersiveControlDarkSliderTrackFillRest,
        ImmersiveControlLightSliderTrackFillDisabled,
        ImmersiveControlLightSliderTrackFillHover,
        ImmersiveControlLightSliderTrackFillPressed,
        ImmersiveControlLightSliderTrackFillRest,
        ImmersiveControlDarkToggleLabelEnabled,
        ImmersiveControlLightToggleLabelEnabled,
        ImmersiveControlDarkToggleThumbEnabled,
        ImmersiveControlLightToggleThumbEnabled,
        ImmersiveControlDarkToggleTrackBackgroundEnabled,
        ImmersiveControlLightToggleTrackBackgroundEnabled,
        ImmersiveControlDarkToggleTrackBorderEnabled,
        ImmersiveControlLightToggleTrackBorderEnabled,
        ImmersiveControlDarkToggleTrackFillEnabled,
        ImmersiveControlLightToggleTrackFillEnabled,
        ImmersiveControlDarkToggleTrackGutterEnabled,
        ImmersiveControlLightToggleTrackGutterEnabled,
        ImmersiveControlDefaultFocusRectDark,
        ImmersiveControlDefaultFocusRectLight,
        ImmersiveControlContextMenuBackgroundRest,
        ImmersiveControlContextMenuBackgroundPressed,
        ImmersiveControlContextMenuBackgroundHover,
        ImmersiveControlContextMenuTextRest,
        ImmersiveControlContextMenuTextPressed,
        ImmersiveControlContextMenuSeparator,
        ImmersiveBootBackground,
        ImmersiveBootTitleText,
        ImmersiveBootPrimaryText,
        ImmersiveBootSecondaryText,
        ImmersiveBootConfirmationButton,
        ImmersiveBootMenuButtonGlyphBackground,
        ImmersiveBootMenuButtonMouseHover,
        ImmersiveBootMenuButtonPressedHighlight,
        ImmersiveBootMenuButtonFocusRect,
        ImmersiveBootProgressText,
        ImmersiveBootErrorText,
        ImmersiveBootEditBackground,
        ImmersiveBootTextLinkRest,
        ImmersiveBootTextLinkHover,
        ImmersiveBootTextLinkPressed
    }

    [Flags]
    public enum SystemCommands
    {
        SC_CLOSE = 0xF060,
        SC_CONTEXTHELP = 0xF180,
        SC_DEFAULT = 0xF160,
        SC_HOTKEY = 0xF150,
        SC_HSCROLL = 0xF080,
        SCF_ISSECURE = 0x00000001,
        SC_KEYMENU = 0xF100,
        SC_MAXIMIZE = 0xF030,
        SC_MINIMIZE = 0xF020,
        SC_MONITORPOWER = 0xF170,
        SC_MOUSEMENU = 0xF090,
        SC_MOVE = 0xF010,
        SC_NEXTWINDOW = 0xF040,
        SC_PREVWINDOW = 0xF050,
        SC_RESTORE = 0xF120,
        SC_SCREENSAVE = 0xF140,
        SC_SIZE = 0xF000,
        SC_TASKLIST = 0xF130,
        SC_VSCROLL = 0xF070
    }

}
