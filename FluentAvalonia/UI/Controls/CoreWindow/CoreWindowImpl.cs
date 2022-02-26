using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Controls
{
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

        public event EventHandler WindowOpened;

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

        public override void Show(bool activate, bool isDialog)
        {
            base.Show(activate, isDialog);

            WindowOpened?.Invoke(this, EventArgs.Empty);
        }

        internal void SetOwner(CoreWindow wnd)
		{
			 _owner = wnd;
			((IPseudoClasses)wnd.Classes).Set(":windows10", !_isWindows11);
            _owner.IsWindows11 = _isWindows11;
		}

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

                if (isOnResizeBorder)
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        return new IntPtr(2); // HT_CAPTION if maximized
                    }
                    else
                    {
                        return new IntPtr(12); // HT_TOP
                    }                    
                }

				if (_owner.HitTestTitleBarRegion(point))
                {
                    return new IntPtr(2); //HT_CAPTION                                          
                }
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
