using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using FluentAvalonia.Core.ApplicationModel;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Controls
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

	public class CoreWindow : Window, IStyleable, ICoreApplicationView
	{
		public CoreWindow()
			:base (WindowImplSolver.GetWindowImpl())
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Design.IsDesignMode)
			{
				_titleBar = new CoreApplicationViewTitleBar(this);

				if (PlatformImpl is CoreWindowImpl cwi)
				{
					cwi.SetOwner(this);
                    cwi.WindowOpened += WindowOpened_Windows;
				}

				ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
				ExtendClientAreaToDecorationsHint = true;
				PseudoClasses.Add(":windows");

                PlatformImpl.Closed += WindowClosed_Windows;
			}
		}

        Type IStyleable.StyleKey => typeof(Window);

        /// <summary>
        /// Gets the <see cref="CoreApplicationViewTitleBar"/> associated with this Window
        /// </summary>
		public CoreApplicationViewTitleBar TitleBar => _titleBar;

        /// <summary>
        /// Gets or sets whether the window should show in a dialog state with the 
        /// maximize and minimize buttons hidden.
        /// </summary>
        /// <remarks>
        ///  WINDOWS only, only respected on window launch
        /// </remarks>
        public bool ShowAsDialog
        {
            get => _hideSizeButtons;
            set
            {
                _hideSizeButtons = value;
                PseudoClasses.Set(":dialog", value);
            }
        }

        protected internal bool IsWindows11 { get; internal set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            var sz = base.MeasureOverride(availableSize);

            // UGLY HACK: Seems with CanResize=False, the window shrinks exactly the amount
            // we modify the window in WM_NCCALCSIZE so we need to fix that here
            // But the content measures to the normal size - so in constrained environments
            // like the TaskDialog, stuff gets cut off
            if (!CanResize && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sz = sz.WithWidth(sz.Width + 16)
                    .WithHeight(sz.Height + 8);
            }

            if (_systemCaptionButtons != null)
            {
                var wid = _systemCaptionButtons.DesiredSize.Width;
                if (_customTitleBar != null)
                {
                    wid += _defaultTitleBar.Width;
                }

                if (_titleBar.SystemOverlayRightInset != wid)
                {
                    _titleBar.SystemOverlayRightInset = wid;
                    _defaultTitleBar.Margin = new Avalonia.Thickness(0, 0, _titleBar.SystemOverlayRightInset, 0);
                }
            }

            return sz;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			_templateRoot = e.NameScope.Find<Border>("RootBorder");

			_systemCaptionButtons = e.NameScope.Find<MinMaxCloseControl>("SystemCaptionButtons");

			_defaultTitleBar = e.NameScope.Find<Panel>("DefaultTitleBar");

			if (_defaultTitleBar != null)
			{
				//_defaultTitleBar.Margin = new Avalonia.Thickness(0, 0, _titleBar.SystemOverlayRightInset, 0);
				_defaultTitleBar.Height = _titleBar.Height;
			}

			if (_systemCaptionButtons != null)
			{
                //_systemCaptionButtons.ApplyTemplate();

				_systemCaptionButtons.Height = _titleBar.Height;
			}

			if (_titleBar != null && Presenter != null)
			{
				if (_titleBar.ExtendViewIntoTitleBar)
				{
					(Presenter as ContentPresenter).Margin = new Thickness();
				}
				else
				{
					(Presenter as ContentPresenter).Margin = new Thickness(0,
						_titleBar.Height, 0, 0);
				}
			}
               
			SetTitleBarColors();
		}

        private void WindowOpened_Windows(object sender, EventArgs e)
        {
            ApplicationViewTitleBar.Instance.TitleBarPropertyChanged += OnTitleBarPropertyChanged;

            var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
            if (faTheme != null)
            {
                faTheme.RequestedThemeChanged += OnRequestedThemeChanged;
            }
        }

        private void WindowClosed_Windows()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ApplicationViewTitleBar.Instance.TitleBarPropertyChanged -= OnTitleBarPropertyChanged;

                var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                if (faTheme != null)
                {
                    faTheme.RequestedThemeChanged -= OnRequestedThemeChanged;
                }
            }
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
			if (Design.IsDesignMode)
				return;

			if (!_titleBar.ExtendViewIntoTitleBar)
				throw new InvalidOperationException("View is not extended into titlebar. Call CoreApplicationViewTitleBar.ExtendIntoTitleBar first.");

			_customTitleBar = titleBar;

            _titleBar.SetCustomTitleBar(titleBar);

			PseudoClasses.Set(":customtitlebar", titleBar != null);
		}

		internal bool HitTestTitleBarRegion(Point windowPoint)
		{
			if (_customTitleBar != null)
			{
                var mat = _customTitleBar.TransformToVisual(this);
                if (mat.HasValue)
                {
                    var bnds = _customTitleBar.Bounds.TransformToAABB(mat.Value);
                    if (bnds.Contains(windowPoint))
                    {
                        var result = this.InputHitTest(windowPoint);

                        return result == _customTitleBar;
                    }
                }

                // Default TitleBar is still *slightly* visible to the left of the caption buttons even with
                // a custom titlebar set, so make sure we test it
                return _defaultTitleBar.HitTestCustom(windowPoint);
			}
			else
			{
				return _defaultTitleBar.HitTestCustom(windowPoint);
			}
		}

		internal bool HitTestCaptionButtons(Point pos)
		{
			if (_systemCaptionButtons == null)
				return false;

            if (WindowState != WindowState.Maximized && pos.Y <= 1)
                return false;

			var result = _systemCaptionButtons.HitTestCustom(pos);
			return result;
		}

		internal bool HitTestMaximizeButton(Point pos)
		{
            if (ShowAsDialog || !CanResize)
                return false;

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
            bool foundAccentLight2 = false;

            var thm = flAvThm.RequestedTheme;
            object sysColorLight2 = null;

            if (thm==FluentAvaloniaTheme.LightModeString)
            {
                _templateRoot.TryFindResource("SystemAccentColorDark1", out sysColorLight2);
            }
            else 
            {
                _templateRoot.TryFindResource("SystemAccentColorLight2", out sysColorLight2);
            }
            

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
				_templateRoot.Resources.Add(prefix + "SysCaptionForegroundInactive", tb.ButtonInactiveBackgroundColor ?? (foundAccentLight2 ? (Color)sysColorLight2 : Colors.Gray));
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
				_templateRoot.Resources[prefix + "SysCaptionForegroundInactive"] = tb.ButtonInactiveBackgroundColor ?? (foundAccentLight2 ? (Color)sysColorLight2 : Colors.Gray);
			}
		}

        private void OnRequestedThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
        {
            // We need to monitor for theme changes, because we need to update the titlebar colors appropriately
            SetTitleBarColors();

            sender.ForceWin32WindowToTheme(this);
        }


        private CoreApplicationViewTitleBar _titleBar;
		private MinMaxCloseControl _systemCaptionButtons;
		private Panel _defaultTitleBar;
		private IControl _customTitleBar;
		private Border _templateRoot;
        private bool _hideSizeButtons;
	}
}
