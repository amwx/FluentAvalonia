using System;
using System.Runtime.InteropServices;
using Avalonia;
using System.Security;

namespace FluentAvalonia.Interop
{
	public static unsafe class Win32Interop
	{
#pragma warning disable CA1401

		[DllImport("dwmapi.dll", SetLastError = true)]
		public static extern int DwmIsCompositionEnabled(out bool enabled);

		[DllImport("uxtheme.dll", EntryPoint = "#95")]
		public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
		
		[DllImport("uxtheme.dll", EntryPoint = "#96")]
		public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
		
		[DllImport("uxtheme.dll", EntryPoint = "#98")]
		public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);


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

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref HIGHCONTRAST pvParam, uint fWinIni);

		[DllImport("user32.dll")]
		public static extern uint GetSysColor(int nIndex);

        [DllImport("user32.dll")]
        public static extern uint GetSysColor(SystemColors nIndex);

        [DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll")]
		public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle,
					bool bMenu, uint dwExStyle);

		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int smIndex);

		[DllImport("user32.dll")]
		public static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

		[DllImport("user32.dll", SetLastError = true)]
		public static unsafe extern int SetWindowCompositionAttribute(IntPtr hwnd, WINDOWCOMPOSITIONATTRIBDATA* data);

		[DllImport("uxtheme.dll", EntryPoint = "#104", SetLastError = true)]
		public static extern void fnRefreshImmersiveColorPolicyState();

		[DllImport("uxtheme.dll", EntryPoint = "#137")]
		public static extern bool fnIsDarkModeAllowedForWindow(IntPtr hWnd);

		[DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
		public static extern PreferredAppMode fnSetPreferredAppMode(IntPtr hwnd, PreferredAppMode appMode);

		[DllImport("uxtheme.dll", EntryPoint = "#135")]
		public static extern bool fnAllowDarkModeForApp(IntPtr hwnd, bool allow);

		[DllImport("uxtheme.dll", EntryPoint = "#132")]
		public static extern bool fnShouldAppsUseDarkMode(); //1809

		[DllImport("uxtheme.dll", EntryPoint = "#138")]
		public static extern bool fnShouldSystemUseDarkMode(); //Use on 1903+

		[DllImport("dwmapi.dll", PreserveSig = true, SetLastError = true)]
		public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int value, int attrSize);

		public static bool GetSystemTheme(OSVERSIONINFOEX osInfo)
		{
			if (osInfo.MajorVersion < 10 || osInfo.BuildNumber < 17763) //1809
				return false;

			if (osInfo.BuildNumber < 18362) //1903
				return fnShouldAppsUseDarkMode();

			return fnShouldSystemUseDarkMode();
		}

		public static bool ApplyTheme(IntPtr hwnd, bool useDark, OSVERSIONINFOEX osInfo)
		{
			if (osInfo.MajorVersion < 10 || osInfo.BuildNumber < 17763) //1809
				return false;

			if (osInfo.BuildNumber < 18362) //1903
			{
				var res = fnAllowDarkModeForApp(hwnd, useDark);
				if (res == false)
					return res;

				int dark = useDark ? 1 : 0;
				DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.UseImmersiveDarkMode, ref dark, Marshal.SizeOf<int>());
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

			return true;
		}

		[SecurityCritical]
		[DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

		[StructLayout(LayoutKind.Sequential)]
		public struct OSVERSIONINFOEX
		{
			// The OSVersionInfoSize field must be set
			public int OSVersionInfoSize;
			public int MajorVersion;
			public int MinorVersion;
			public int BuildNumber;
			public int PlatformId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string CSDVersion;
			public ushort ServicePackMajor;
			public ushort ServicePackMinor;
			public short SuiteMask;
			public byte ProductType;
			public byte Reserved;
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern unsafe bool AdjustWindowRectExForDpi(
			ref RECT lpRect,
			int dwStyle,
			[MarshalAs(UnmanagedType.Bool)] bool bMenu,
			int dwExStyle,
			int dpi);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct HIGHCONTRAST
		{
			public uint cbSize;
			public HCF dwFlags;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszDefaultScheme;
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
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}

	[Flags]
	public enum HCF : uint
	{
		HCF_HIGHCONTRASTON = 0x00000001,
		HCF_AVAILABLE = 0x00000002,
		HCF_HOTKEYACTIVE = 0x00000004,
		HCF_CONFIRMHOTKEY = 0x00000008,
		HCF_HOTKEYSOUND = 0x00000010,
		HCF_INDICATOR = 0x00000020,
		HCF_HOTKEYAVAILABLE = 0x00000040,
		HCF_OPTION_NOTHEMECHANGE = 0x00001000
	}
	
	public enum SystemColors
	{
		COLOR_3DDKSHADOW = 21,
        COLOR_3DFACE = 15,
        COLOR_3DHIGHLIGHT = 20,
        COLOR_3DLIGHT = 22,
        COLOR_3DSHADOW = 16,
        COLOR_ACTIVEBORDER = 10,
        COLOR_ACTIVECAPTION = 2,
        COLOR_APPWORKSPACE = 12,
        COLOR_BACKGROUND = 1,
        COLOR_BTNFACE = 15,
        COLOR_BTNHIGHLIGHT = 20,
        COLOR_BTNSHADOW = 16,
        COLOR_BTNTEXT = 18,
        COLOR_CAPTIONTEXT = 9,
        COLOR_DESKTOP = 1,
        COLOR_GRADIENTACTIVECAPTION = 27,
        COLOR_GRADIENTINACTIVECAPTION = 28,
        COLOR_GRAYTEXT = 17,
        COLOR_HIGHLIGHT = 13,
        COLOR_HIGHLIGHTTEXT = 14,
        COLOR_HOTLIGHT = 26,
        COLOR_INACTIVEBORDER = 11,
        COLOR_INACTIVECAPTION = 3,
        COLOR_INACTIVECAPTIONTEXT = 19,
        COLOR_INFOBK = 24,
        COLOR_INFOTEXT = 23,
        COLOR_MENU = 4,
        COLOR_MENUHILIGHT = 29,
        COLOR_MENUBAR = 30,
        COLOR_MENUTEXT = 7,
        COLOR_SCROLLBAR = 0,
        COLOR_WINDOW = 5,
        COLOR_WINDOWFRAME = 6,
        COLOR_WINDOWTEXT = 8
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
		FreezeRepresentation,
		UseImmersiveDarkMode = 20
	}

	public enum WM : uint
	{
		SIZE = 0x0005,
		NCMOUSEMOVE = 0x00A0,
		NCLBUTTONDOWN = 0x00A1,
		NCLBUTTONUP = 0x00A2,
		NCHITTEST = 0x0084,
		NCCALCSIZE = 0x0083,
	}
}
