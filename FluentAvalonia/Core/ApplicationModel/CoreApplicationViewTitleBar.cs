using FluentAvalonia.Interop;
using FluentAvalonia.UI.Controls;

namespace FluentAvalonia.Core.ApplicationModel
{
	public sealed class CoreApplicationViewTitleBar
	{
		public CoreApplicationViewTitleBar(CoreWindow owner)
		{
			_owner = owner;
		}

		public bool ExtendViewIntoTitleBar
		{
			get => _extend;
			set
			{
				if (_extend != value)
				{
					_extend = value;
					_owner.ExtendTitleBar(value);
					LayoutMetricsChanged?.Invoke(this, null);
				}
			}
		}
		public double Height
		{
			get
			{
				if (double.IsNaN(_height))
					GetSystemTitleBarHeight();

				return _height;
			}
		}

		public bool IsVisible => true;

		// No RTL layout, so this is always zero
		public double SystemOverlayLeftInset => 0;

		// This is a constant, I've set the buttons to 46px in width * 3 = 138
		public double SystemOverlayRightInset => 138;

		public event TypedEventHandler<CoreApplicationViewTitleBar, object> LayoutMetricsChanged;

		private void GetSystemTitleBarHeight()
		{
			if (_owner == null)
			{
				_height = 32;
				return;
			}

			// WS_OVERLAPPEDWINDOW
			var style = 0x00000000L | 0x00C00000L | 0x00080000L | 0x00040000L | 0x00020000L | 0x00010000L;

			// This is causing the window to appear solid but is completely transparent. Weird...
			//Win32Interop.GetWindowLongPtr(Hwnd, -16).ToInt32();
			RECT frame = new RECT();
			Win32Interop.AdjustWindowRectExForDpi(ref frame,
				(int)style, false, 0, (int)(_owner.PlatformImpl.RenderScaling * 96));

			_height = -frame.top;
		}

		private bool _extend;
		private double _height = double.NaN;
		private CoreWindow _owner;
	}
}
