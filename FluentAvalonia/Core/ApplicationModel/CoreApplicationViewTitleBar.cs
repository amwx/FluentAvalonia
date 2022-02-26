using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
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

        /// <summary>
        /// Gets or sets whether the current view is extended into the titlebar space
        /// </summary>
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

        /// <summary>
        /// Gets the height of the titlebar
        /// </summary>
		public double Height
		{
			get
			{
				if (double.IsNaN(_height))
					GetSystemTitleBarHeight();

				return _customTitleBar?.Bounds.Height ?? _height;
			}
		}

        /// <summary>
        /// Gets whether the 
        /// </summary>
		public bool IsVisible => true;

        /// <summary>
        /// Gets the width of the system-reserved region of the upper-left corner of the app window. This region is reserved when the current language is a right-to-left language.
        /// </summary>
        /// <remarks>
        /// No RTL layout in Avalonia, this value is always 0
        /// </remarks>
        public double SystemOverlayLeftInset => 0;

        /// <summary>
        /// Gets the width of the system-reserved region of the upper-right corner of the app window. This region is reserved when the current language is a left-to-right language.
        /// </summary>
        public double SystemOverlayRightInset
        {
            get => _insetWidthRight;
            internal set
            {
                if (_insetWidthRight != value)
                {
                    _insetWidthRight = value;
                    LayoutMetricsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

		public event TypedEventHandler<CoreApplicationViewTitleBar, EventArgs> LayoutMetricsChanged;

        public event TypedEventHandler<CoreApplicationViewTitleBar, EventArgs> IsVisibleChanged;

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
				(int)style, false, 0, 96);

			_height = -frame.top;
		}

        internal void SetCustomTitleBar(IControl ctrl)
        {
            _customTitleBarDisp?.Dispose();

            _customTitleBar = ctrl;

            if (ctrl != null)
            {
                _customTitleBarDisp = new CompositeDisposable(
                    _customTitleBar.GetPropertyChangedObservable(Visual.BoundsProperty).Subscribe(OnCustomTitleBarBoundsChanged),
                    _customTitleBar.GetPropertyChangedObservable(Visual.IsVisibleProperty).Subscribe(OnCustomTitleBarVisibleChanged));
            }

            LayoutMetricsChanged?.Invoke(this, EventArgs.Empty);
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnCustomTitleBarBoundsChanged(AvaloniaPropertyChangedEventArgs obj)
        {
            LayoutMetricsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnCustomTitleBarVisibleChanged(AvaloniaPropertyChangedEventArgs obj)
        {
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _extend;
		private double _height = double.NaN;
		private CoreWindow _owner;
        private IControl _customTitleBar;
        private IDisposable _customTitleBarDisp;
        private double _insetWidthRight = double.NaN;
	}
}
