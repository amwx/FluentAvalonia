﻿using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core.ApplicationModel;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls;

public static class WindowImplSolver
{
    public static IWindowImpl GetWindowImpl()
    {
        bool useCoreWindowImpl = false;

#if NET6_0_OR_GREATER
        useCoreWindowImpl = !Design.IsDesignMode && OperatingSystem.IsWindows();
#else
        useCoreWindowImpl = !Design.IsDesignMode && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

        if (useCoreWindowImpl)
        {
            return GetCoreWindowImpl();
        }

        return PlatformManager.CreateWindow();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IWindowImpl GetCoreWindowImpl() => 
        new CoreWindowImpl();
}

public class CoreWindow : Window, IStyleable, ICoreApplicationView
{
    public CoreWindow()
        : base(WindowImplSolver.GetWindowImpl())
    {
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows() && !Design.IsDesignMode)
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Design.IsDesignMode)
#endif
        {
            InitializeCoreWindow();
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

    public IApplicationSplashScreen SplashScreen
    {
        get => _splashContext?.SplashScreen;
        set
        {
            if (value == null)
            {
                if (_splashContext != null)
                {
                    _splashContext.Host.SplashScreen = null;
                }

                _splashContext = null;
                PseudoClasses.Set(":splashScreen", false);
            }
            else
            {
                _splashContext = new SplashScreenContext(value);
                PseudoClasses.Set(":splashScreen", true);
            }
        }
    }

    protected internal bool IsWindows11 { get; internal set; }

    protected internal bool IsWindows { get; private set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        var sz = base.MeasureOverride(availableSize);

        // UGLY HACK: Seems with CanResize=False, the window shrinks exactly the amount
        // we modify the window in WM_NCCALCSIZE so we need to fix that here
        // But the content measures to the normal size - so in constrained environments
        // like the TaskDialog, stuff gets cut off
        if (!CanResize && IsWindows)
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
                _defaultTitleBar.Margin = new Thickness(0, 0, _titleBar.SystemOverlayRightInset, 0);
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

        if (SplashScreen != null)
        {
            var host = e.NameScope.Find<CoreSplashScreen>("SplashHost");
            if (host != null)
            {
                _splashContext.Host = host;
            }
        }
    }

    protected override async void OnOpened(EventArgs e)
    {
        if (_splashContext != null && !_splashContext.HasShownSplashScreen && !Design.IsDesignMode)
        {
            PseudoClasses.Set(":splashOpen", true);
            var time = DateTime.Now;

            await _splashContext.RunJobs();

            var delta = DateTime.Now - time;
            if (delta.TotalMilliseconds < _splashContext.SplashScreen.MinimumShowTime)
            {
                await Task.Delay(Math.Max(1, _splashContext.SplashScreen.MinimumShowTime - (int)delta.TotalMilliseconds));
            }

            LoadApp();
        }

        base.OnOpened(e);
    }

    private async void LoadApp()
    {
        Presenter.IsVisible = true;

        var aniSplash = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                    },
                    KeySpline = new KeySpline(0,0,0,1)
                }
            }
        };

        var aniCP = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(167),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                    },
                    KeySpline = new KeySpline(0,0,0,1)
                }
            }
        };

        await Task.WhenAll(aniSplash.RunAsync(_splashContext.Host, null),
            aniCP.RunAsync((Animatable)Presenter, null));

        PseudoClasses.Set(":splashOpen", false);
        _splashContext.HasShownSplashScreen = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _splashContext?.TryCancel();
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
        if (IsWindows)
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
            // v2 - see below
            return DefaultHitTest(windowPoint);
        }
        else
        {
            // v2 - New bug with compositing renderer
            // Using .HitTestCustom fails since TransformedBounds are never set
            // So use new logic to avoid HitTestCustom
            return DefaultHitTest(windowPoint);
        }

        bool DefaultHitTest(Point pt)
        {
            var mat = _defaultTitleBar.TransformToVisual(this);
            var bnds = _defaultTitleBar.Bounds.TransformToAABB(mat.Value);
            if (bnds.Contains(windowPoint))
            {
                var result = this.InputHitTest(windowPoint);

                if (result == _defaultTitleBar)
                {
                    return true;
                }
                else
                {
                    // We may have hit the TextBlock title, work upwards and see
                    var vis = result as IVisual;
                    while (vis != null)
                    {
                        if (vis == _defaultTitleBar)
                            return true;

                        vis = vis.VisualParent;
                    }
                }
            }

            return false;
        }
    }

    internal bool HitTestCaptionButtons(Point pos)
    {
        if (_systemCaptionButtons == null)
            return false;

        if (WindowState != WindowState.Maximized && pos.Y <= 1)
            return false;

        // v2 - Bug in Compositing Renderer prevent usage of HitTestCustom
        // var result = _systemCaptionButtons.HitTestCustom(pos);
        return HitTest(pos);

        bool HitTest(Point p)
        {
            var result = this.InputHitTest(p);

            if (result == _systemCaptionButtons)
            {
                return true;
            }
            else
            {
                // We may have hit the TextBlock title, work upwards and see
                var vis = result as IVisual;
                while (vis != null)
                {
                    if (vis == _systemCaptionButtons)
                        return true;

                    vis = vis.VisualParent;
                }
            }

            return false;
        }
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

        if (thm == FluentAvaloniaTheme.LightModeString)
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
            _templateRoot.Resources[prefix + "TitleBarBackground"] = tb.BackgroundColor ?? Colors.Transparent;
            _templateRoot.Resources[prefix + "TitleBarForeground"] = tb.ForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

            _templateRoot.Resources[prefix + "TitleBarBackgroundInactive"] = tb.InactiveBackgroundColor ?? Colors.Transparent;
            _templateRoot.Resources[prefix + "TitleBarForegroundInactive"] = tb.InactiveForegroundColor ?? Colors.Gray;

            _templateRoot.Resources[prefix + "SysCaptionBackground"] = tb.ButtonBackgroundColor ?? Colors.Transparent;
            _templateRoot.Resources[prefix + "SysCaptionForeground"] = tb.ButtonForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

            _templateRoot.Resources[prefix + "SysCaptionBackgroundHover"] = tb.ButtonHoverBackgroundColor ?? (foundAccent ? ((Color)sysColor) : Color.Parse("#17000000"));
            _templateRoot.Resources[prefix + "SysCaptionForegroundHover"] = tb.ButtonHoverForegroundColor ?? (thm == "Light" ? Colors.Black : Colors.White);

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeCoreWindow()
    {
        IsWindows = true;
        _titleBar = new CoreApplicationViewTitleBar(this);

        if (PlatformImpl is CoreWindowImpl cwi)
        {
            cwi.SetOwner(this);
            cwi.WindowOpened += WindowOpened_Windows;
        }

        // NOTE FOR FUTURE: 
        // Do NOT enable these properties, doing so causes a clash of logic between here and
        // the actual window logic within avalonia leading to the window shrinking when restoring
        // from maximized or minimized state. 
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaToDecorationsHint = true;
        PseudoClasses.Add(":windows");

        PlatformImpl.Closed += WindowClosed_Windows;
    }

    private SplashScreenContext _splashContext;
    private CoreApplicationViewTitleBar _titleBar;
    private MinMaxCloseControl _systemCaptionButtons;
    private Panel _defaultTitleBar;
    private IControl _customTitleBar;
    private Border _templateRoot;
    private bool _hideSizeButtons;

    // Special class to manage SplashScreen. We specifically do this so we only add 1 pointer
    // to CoreWindow if no splash screen is used, instead of all of what's used here
    private class SplashScreenContext
    {
        public SplashScreenContext(IApplicationSplashScreen splash)
        {
            _splashCTS = new CancellationTokenSource();
            _splashScreen = splash;
        }

        public IApplicationSplashScreen SplashScreen => _splashScreen;

        public bool HasShownSplashScreen { get; set; }

        public CoreSplashScreen Host
        {
            get => _splashHost;
            set
            {
                _splashHost = value;
                _splashHost.SplashScreen = SplashScreen;
            }
        }

        public async Task RunJobs()
        {
            _splashCTS = new CancellationTokenSource();
            await Task.Run(() =>
            {
                SplashScreen.RunTasks();
            }, _splashCTS.Token);
            _splashCTS.Dispose();
            _splashCTS = null;
        }

        public void TryCancel()
        {
            if (_splashCTS != null)
            {
                _splashCTS.Cancel();
                _splashCTS.Dispose();
                _splashCTS = null;
            }
        }

        private CancellationTokenSource _splashCTS;
        private IApplicationSplashScreen _splashScreen;
        private CoreSplashScreen _splashHost;
    }
}
