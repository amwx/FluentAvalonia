using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using FluentAvalonia.Interop;
using FluentAvalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.Core.ApplicationModel
{
	public static class WindowImplSolver
	{
		public static IWindowImpl GetWindowImpl()
		{
			if (Design.IsDesignMode)
				return PlatformManager.CreateWindow();

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return new CoreWindowImpl();

			return PlatformManager.CreateWindow();
		}
	}

	public interface ICoreApplicationView
	{
		CoreApplicationViewTitleBar TitleBar { get; }
	}

	public class CoreWindow : Window, IStyleable, ICoreApplicationView
	{
		public CoreWindow()
			:base (WindowImplSolver.GetWindowImpl())
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				_titleBar = new CoreApplicationViewTitleBar(this);

				if (PlatformImpl is CoreWindowImpl cwi)
				{
					cwi.SetOwner(this);
				}

				ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
				ExtendClientAreaToDecorationsHint = true;
				PseudoClasses.Add(":windows");

				AvaloniaLocator.CurrentMutable.Bind<ICoreApplicationView>().ToConstant<ICoreApplicationView>(this);
				ApplicationViewTitleBar.Instance.TitleBarPropertyChanged += OnTitleBarPropertyChanged;
			}
		}

		Type IStyleable.StyleKey => typeof(Window);

		CoreApplicationViewTitleBar ICoreApplicationView.TitleBar => _titleBar;

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			_templateRoot = e.NameScope.Find<Border>("RootBorder");

			_systemCaptionButtons = e.NameScope.Find<MinMaxCloseControl>("SystemCaptionButtons");

			_defaultTitleBar = e.NameScope.Find<Panel>("DefaultTitleBar");

			if (_defaultTitleBar != null)
			{
				_defaultTitleBar.Margin = new Avalonia.Thickness(0, 0, _titleBar.SystemOverlayRightInset, 0);
				_defaultTitleBar.Height = _titleBar.Height;
			}

			if (_systemCaptionButtons != null)
			{
				_systemCaptionButtons.Height = _titleBar.Height;
			}

			SetTitleBarColors();
		}

		internal void ExtendTitleBar(bool extend)
		{
			if (Design.IsDesignMode)
				return;

			if (extend)
			{
				if (Presenter != null)
				{
					(Presenter as ContentPresenter).Margin = new Thickness();
				}
			}
			else
			{
				if (Presenter != null)
				{
					(Presenter as ContentPresenter).Margin = new Thickness(0,
						_titleBar.Height, 0, 0);
				}
			}

			// TODO:
			// 1  Per UWP TitleBar customization docs, the system still reserves a little bit of space
			//    to the left of the caption buttons, even if a custom titlebar is set
			// 2  A custom titlebar can still have elements on top of it (but not in it) and it will still work,
			//    says to use a higher z-order
			// 3  If no custom titlebar is set, the default remains, sized along the top border the height
			//    of the caption buttons [DONE]
			PseudoClasses.Set(":extended", extend);
		}

		public void SetTitleBar(IControl titleBar)
		{
#if DEBUG
			if (Design.IsDesignMode)
				return;
#endif
			if (!_titleBar.ExtendViewIntoTitleBar)
				throw new InvalidOperationException("View is not extended into titlebar. Call CoreApplicationViewTitleBar.ExtendIntoTitleBar first.");

			_customTitleBar = titleBar;
			//_customTitleBarHost.Content = titleBar;

			PseudoClasses.Set(":customtitlebar", titleBar != null);

			//_customTitleBarHost.IsVisible = titleBar != null;
			//_defaultTitleBar.IsVisible = titleBar == null;
		}

		internal bool HitTestTitleBarRegion(Point windowPoint)
		{
			if (_customTitleBar != null)
			{
				var mat = this.TransformToVisual(_customTitleBar).Value;
				var bnds = _customTitleBar.Bounds.TransformToAABB(mat);
				if (bnds.Contains(windowPoint))
				{
					var result = this.InputHitTest(windowPoint);

					return result == _customTitleBar;// _customTitleBar.HitTestCustom(windowPoint);
				}

				return false;
			}
			else
			{
				return _defaultTitleBar.HitTestCustom(windowPoint);
				//return _defaultTitleBar.InputHitTest(windowPoint) != null;
			}
		}

		internal bool HitTestCaptionButtons(Point pos)
		{
			if (pos.Y < 1 || _systemCaptionButtons == null)
				return false;

			var result = _systemCaptionButtons.HitTestCustom(pos);
			return result;
		}

		internal bool HitTestMaximizeButton(Point pos)
		{
			return _systemCaptionButtons.HitTestMaxButton(pos);
		}

		internal void FakeMaximizeHover(bool hover)
		{
			_systemCaptionButtons.FakeMaximizeHover(hover);
		}

		internal void FakeMaximizePressed(bool pressed)
		{
			_systemCaptionButtons.FakeMaximizePressed(pressed);
		}

		internal void FakeMaximizeClick()
		{
			_systemCaptionButtons.FakeMaximizeClick();
		}

		private void OnTitleBarPropertyChanged(object sender, EventArgs e)
		{
			SetTitleBarColors();			
		}

		private void SetTitleBarColors()
		{
			if (_templateRoot == null)
				return;

			var tb = ApplicationViewTitleBar.Instance;

			var flAvThm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

			bool foundAccent = _templateRoot.TryFindResource("SystemAccentColor", out var sysColor);

			var thm = flAvThm.RequestedTheme;

			string prefix = "FATitle_";
			if (_templateRoot.Resources.Count == 0)
			{
				_templateRoot.Resources.Add(prefix + "TitleBarBackground", tb.BackgroundColor ?? Colors.Transparent);
				_templateRoot.Resources.Add(prefix + "TitleBarForeground", tb.ForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White));

				_templateRoot.Resources.Add(prefix + "TitleBarBackgroundInactive", tb.InactiveBackgroundColor ?? Colors.Transparent);
				_templateRoot.Resources.Add(prefix + "TitleBarForegroundInactive", tb.InactiveForegroundColor ?? Colors.Gray);

				_templateRoot.Resources.Add(prefix + "SysCaptionBackground", tb.ButtonBackgroundColor ?? Colors.Transparent);
				_templateRoot.Resources.Add(prefix + "SysCaptionForeground", tb.ButtonForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White));

				_templateRoot.Resources.Add(prefix + "SysCaptionBackgroundHover", tb.ButtonHoverBackgroundColor ?? (foundAccent ? ((Color)sysColor) : Color.Parse("#17000000")));
				_templateRoot.Resources.Add(prefix + "SysCaptionForegroundHover", tb.ButtonHoverForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White));

				_templateRoot.Resources.Add(prefix + "SysCaptionBackgroundPressed", tb.ButtonPressedBackgroundColor ?? (foundAccent ? ((Color)sysColor) : Color.Parse("#34000000")));
				_templateRoot.Resources.Add(prefix + "SysCaptionForegroundPressed", tb.ButtonPressedForegroundColor ?? (thm == "Light" ? Color.Parse("#87000000") : Color.Parse("#87FFFFFF")));

				_templateRoot.Resources.Add(prefix + "SysCaptionBackgroundInactive", tb.ButtonInactiveBackgroundColor ?? Colors.Transparent);
				_templateRoot.Resources.Add(prefix + "SysCaptionForegroundInactive", tb.ButtonInactiveBackgroundColor ?? Colors.Gray);
			}
			else
			{
				_templateRoot.Resources[prefix + "TitleBarBackground"]= tb.BackgroundColor ?? Colors.Transparent;
				_templateRoot.Resources[prefix + "TitleBarForeground"] = tb.ForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

				_templateRoot.Resources[prefix + "TitleBarBackgroundInactive"] = tb.InactiveBackgroundColor ?? Colors.Transparent;
				_templateRoot.Resources[prefix + "TitleBarForegroundInactive"] = tb.InactiveForegroundColor ?? Colors.Gray;

				_templateRoot.Resources[prefix + "SysCaptionBackground"] = tb.ButtonBackgroundColor ?? Colors.Transparent;
				_templateRoot.Resources[prefix + "SysCaptionForeground"] = tb.ButtonForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

				_templateRoot.Resources[prefix + "SysCaptionBackgroundHover"] = tb.ButtonHoverBackgroundColor ?? (foundAccent ? ((Color)sysColor) : Color.Parse("#17000000"));
				_templateRoot.Resources[prefix + "SysCaptionForegroundHover"]= tb.ButtonHoverForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

				_templateRoot.Resources[prefix + "SysCaptionBackgroundPressed"] = tb.ButtonPressedBackgroundColor ?? (foundAccent ? ((Color)sysColor) : Color.Parse("#34000000"));
				_templateRoot.Resources[prefix + "SysCaptionForegroundPressed"] = tb.ButtonPressedForegroundColor ?? (thm == "Light" ? Color.Parse("#87000000") : Color.Parse("#87FFFFFF"));

				_templateRoot.Resources[prefix + "SysCaptionBackgroundInactive"] = tb.ButtonInactiveBackgroundColor ?? Colors.Transparent;
				_templateRoot.Resources[prefix + "SysCaptionForegroundInactive"] = tb.ButtonInactiveBackgroundColor ?? Colors.Gray;
			}
		}


		private CoreApplicationViewTitleBar _titleBar;
		private MinMaxCloseControl _systemCaptionButtons;
		private Panel _defaultTitleBar;
		private IControl _customTitleBar;
		private Border _templateRoot;
	}

	internal class CoreWindowImpl : Avalonia.Win32.WindowImpl
	{
		public CoreWindowImpl()
		{
			Win32Interop.OSVERSIONINFOEX version = new Win32Interop.OSVERSIONINFOEX
			{
				OSVersionInfoSize = Marshal.SizeOf<Win32Interop.OSVERSIONINFOEX>()
			};

			Win32Interop.RtlGetVersion(ref version);

			if (version.MajorVersion < 10)
			{
				throw new NotSupportedException("Windows versions earlier than 10 are not supported");
			}

			_isWindows11 = version.BuildNumber >= 22000;
		}

		protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			switch ((WM)msg)
			{
				case WM.NCCALCSIZE:
					// Weirdness: 
					// Windows Terminal only handles WPARAM = TRUE & only adjusts the top of the
					// rgrc[0] RECT & gets the correct result
					// Firefox, on the other hand, handles BOTH times WM_NCCALCSIZE is called,
					// and modifies the RECT.
					// HERE, I've gotten Firefox's method to work but I can resize from the Window shadows again!

					if (wParam != IntPtr.Zero) //wParam == TRUE
					{
						var ncParams = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);

						var originalTop = ncParams.rgrc[0].top;

						var ret = Win32Interop.DefWindowProc(hWnd, (uint)WM.NCCALCSIZE, wParam, lParam);
						if (ret != IntPtr.Zero)
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

						Marshal.StructureToPtr(ncParams, lParam, true);

						return IntPtr.Zero;
					}

					return IntPtr.Zero;

				case WM.NCHITTEST:
					return HandleNCHitTest(lParam);

				case WM.SIZE:
					EnsureExtended();

					if (_fakingMaximizeButton)
					{
						// Sometimes the effect can get stuck, so if we resize, clear it
						_owner.FakeMaximizePressed(false);
						_wasFakeMaximizeDown = false;
					}
					break;

				case WM.NCMOUSEMOVE:
					if (_fakingMaximizeButton)
					{
						var point = PointToClient(PointFromLParam(lParam));
						_owner.FakeMaximizeHover(_owner.HitTestMaximizeButton(point));
						return IntPtr.Zero;
					}
					break;

				case WM.NCLBUTTONDOWN:
					if (_fakingMaximizeButton)
					{
						var point = PointToClient(PointFromLParam(lParam));
						_owner.FakeMaximizePressed(_owner.HitTestMaximizeButton(point));
						_wasFakeMaximizeDown = true;

						// This is important. If we don't tell the System we've handled this, we'll get that
						// classic Win32 button showing when we mouse press, and that's not good
						return IntPtr.Zero;
					}
					break;

				case WM.NCLBUTTONUP:
					if (_fakingMaximizeButton && _wasFakeMaximizeDown)
					{
						var point = PointToClient(PointFromLParam(lParam));
						_owner.FakeMaximizePressed(false);
						_wasFakeMaximizeDown = false;
						_owner.FakeMaximizeClick();
						return IntPtr.Zero;
					}
					break;
			}

			return base.WndProc(hWnd, msg, wParam, lParam);
		}

		internal void SetOwner(CoreWindow wnd) => _owner = wnd;

		private int GetResizeHandleHeight()
		{
			// TODO GetSystemMetricsForDPI (Win 10 1607 / 10.0.14393 and later)
			return Win32Interop.GetSystemMetrics(92 /* SM_CXPADDEDBORDER */) +
				Win32Interop.GetSystemMetrics(33/* SM_CYSIZEFRAME */);
		}

		private void EnsureExtended()
		{
			// We completely ignore anything for extending client area in Avalonia Window impl b/c
			// we're doing super specialized stuff to ensure the best experience interacting with
			// the window and mimic-ing a "modern app"
			var marg = new Win32Interop.MARGINS();

			// WS_OVERLAPPEDWINDOW
			var style = 0x00000000L | 0x00C00000L | 0x00080000L | 0x00040000L | 0x00020000L | 0x00010000L;

			// This is causing the window to appear solid but is completely transparent. Weird...
			//Win32Interop.GetWindowLongPtr(Hwnd, -16).ToInt32();

			RECT frame = new RECT();
			Win32Interop.AdjustWindowRectExForDpi(ref frame,
				(int)style, false, 0, (int)(RenderScaling * 96));

			marg.topHeight = -frame.top;
			Win32Interop.DwmExtendFrameIntoClientArea(Handle.Handle, ref marg);
		}

		protected IntPtr HandleNCHitTest(IntPtr lParam)
		{
			// Because we still have the System Border (which technically extends beyond the actual window
			// into where the Drop shadows are), we can use DefWindowProc here to handle resizing, except
			// on the top. We'll handle that below
			var originalRet = Win32Interop.DefWindowProc(Hwnd, (uint)WM.NCHITTEST, IntPtr.Zero, lParam);
			if (originalRet != new IntPtr(1))
			{
				return originalRet;
			}

			// At this point, we know that the cursor is inside the client area so it
			// has to be either the little border at the top of our custom title bar,
			// the drag bar or something else in the XAML island. But the XAML Island
			// handles WM_NCHITTEST on its own so actually it cannot be the XAML
			// Island. Then it must be the drag bar or the little border at the top
			// which the user can use to move or resize the window.

			var point = PointToClient(PointFromLParam(lParam));

			RECT rcWindow;
			Win32Interop.GetWindowRect(Hwnd, out rcWindow);

			// On the Top border, the resize handle overlaps with the Titlebar area, which matches
			// a typical Win32 window or modern app window
			var resizeBorderHeight = GetResizeHandleHeight();
			bool isOnResizeBorder = point.Y < resizeBorderHeight;

			// Make sure the caption buttons still get precedence
			// This is where things get tricky too. On Win11, we still want to suppor the snap
			// layout feature when hovering over the Maximize button. Unfortunately no API exists
			// yet to call that manually if using a custom titlebar. But, if we return HT_MAXBUTTON
			// here, the pointer events no longer enter the window
			// See https://github.com/dotnet/wpf/issues/4825 for more on this...
			// To hack our way into making this work, we'll return HT_MAXBUTTON here, but manually
			// manage the state and handle stuff through the WM_NCLBUTTON... events
			// This only applies on Windows 11, Windows 10 will work normally b/c no snap layout thing

			if (_owner.HitTestCaptionButtons(point))
			{
				if (_isWindows11)
				{
					var result = _owner.HitTestMaximizeButton(point);

					if (result)
					{
						_fakingMaximizeButton = true;
						return new IntPtr(9);
					}
				}
			}
			else
			{
				if (_fakingMaximizeButton)
				{
					_fakingMaximizeButton = false;
					_owner.FakeMaximizeHover(false);
					_owner.FakeMaximizePressed(false);
				}

				if (WindowState != WindowState.Maximized && isOnResizeBorder)
					return new IntPtr(12); // HT_TOP

				if (_owner.HitTestTitleBarRegion(point))
					return new IntPtr(2); //HT_CAPTION
			}

			if (_fakingMaximizeButton)
			{
				_fakingMaximizeButton = false;
				_owner.FakeMaximizeHover(false);
				_owner.FakeMaximizePressed(false);
			}
			_fakingMaximizeButton = false;
			// return HT_CLIENT, we're in the normal window
			return new IntPtr(1);
		}

		private PixelPoint PointFromLParam(IntPtr lParam)
		{
			return new PixelPoint((short)(ToInt32(lParam) & 0xffff), (short)(ToInt32(lParam) >> 16));
		}

		private static int ToInt32(IntPtr ptr)
		{
			if (IntPtr.Size == 4)
				return ptr.ToInt32();

			return (int)(ptr.ToInt64() & 0xffffffff);
		}

		private bool _isWindows11;
		private CoreWindow _owner;
		private bool _fakingMaximizeButton;
		private bool _wasFakeMaximizeDown;
	}
}
