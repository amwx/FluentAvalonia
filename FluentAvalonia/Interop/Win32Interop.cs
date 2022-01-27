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

            // Try to get the window to redraw to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, (uint)(0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020 | 0x0200));

			return true;
		}

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

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
