using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Interop;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.UI.Windowing;

public partial class AppWindow : Window, IStyleable
{
    public AppWindow()
        : base(AppWindowImplSolver.GetWindowImpl())
    {
        TemplateSettings = new AppWindowTemplateSettings();
        _titleBar = new AppWindowTitleBar(this);

        if (OSVersionHelper.IsWindows() && !Design.IsDesignMode)
        {
            InitializeAppWindow();
        }
    }

    static AppWindow()
    {
        AllowInteractionInTitleBarProperty.Changed.AddClassHandler<Control>(OnAllowInteractionInTitleBarChanged);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var sz = base.MeasureOverride(availableSize);

        // UGLY HACK: Seems with CanResize=False, the window shrinks exactly the amount
        // we modify the window in WM_NCCALCSIZE so we need to fix that here
        // But the content measures to the normal size - so in constrained environments
        // like the TaskDialog, stuff gets cut off
        if (IsWindows)
        {
            // TODO: This doesn't appear necessary for TaskDialog anymore, but just in case, I'll
            //       keep this here as a reminder
            //if (!CanResize)
            //{
                //var wid = (16 * PlatformImpl.RenderScaling);
                //var hgt = (8 * PlatformImpl.RenderScaling);
                //sz = new Size(sz.Width + wid, sz.Height + hgt);
            //}

            if (SystemCaptionControl != null)
                _titleBar.SetInset(SystemCaptionControl.DesiredSize.Width, FlowDirection);
        }        

        return sz;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);      

        if (IsWindows && !Design.IsDesignMode)
        {
            _templateRoot = e.NameScope.Find<Border>("RootBorder");
                        
            _captionButtons = e.NameScope.Find<MinMaxCloseControl>("SystemCaptionButtons");
            _defaultTitleBar = e.NameScope.Find<Panel>("DefaultTitleBar");

            // This will set all our TemplateSettings properties
            OnTitleBarHeightChanged(_titleBar.Height);

            var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
            _currentAppTheme = faTheme.RequestedTheme;
            faTheme.RequestedThemeChanged += OnRequestedThemeChanged;

            SetTitleBarColors();
        }

        if (SplashScreen != null)
        {
            var host = e.NameScope.Find<AppSplashScreen>("SplashHost");
            if (host != null)
            {
                _splashContext.Host = host;
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty)
        {
            base.Icon = new WindowIcon(change.NewValue as IBitmap);
            PseudoClasses.Set(":icon", change.NewValue != null);
        }
    }

    protected override async void OnOpened(EventArgs e)
    {
        if (_splashContext != null && !_splashContext.HasShownSplashScreen && !Design.IsDesignMode)
        {
            PseudoClasses.Set(":splashOpen", true);
            var time = DateTime.Now;

            // n00b async/await mistake - need to await here, thansk to GH taj-ny for finding and fixing this
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

    protected override void OnClosed(EventArgs e)
    {
        _splashContext?.TryCancel();

        base.OnClosed(e);

        if (IsWindows && !Design.IsDesignMode)
            AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedThemeChanged -= OnRequestedThemeChanged;       
    }

    internal void OnExtendsContentIntoTitleBarChanged(bool isExtended)
    {
        if (isExtended)
        {
            TemplateSettings.IsTitleBarContentVisible = false;
            TemplateSettings.ContentMargin = new Thickness();
        }
        else
        {
            TemplateSettings.IsTitleBarContentVisible = true;
            
            if (WindowState != WindowState.FullScreen)
                TemplateSettings.ContentMargin = new Thickness(0, _titleBar.Height, 0, 0);
        }
    }

    internal void OnTitleBarHeightChanged(double height)
    {
        TemplateSettings.TitleBarHeight = height;
        OnExtendsContentIntoTitleBarChanged(_titleBar.ExtendsContentIntoTitleBar);
    }

    internal void TitleBarColorsChanged()
    {
        SetTitleBarColors();
    }

    internal bool HitTestTitleBar(Point p)
    {
        if (_defaultTitleBar == null)
            return false;

        // Check the specified drag rectangles first - these override the default titlebar behavior
        // If that returns null, no drag rects were specified and we use the default logic
        var result = _titleBar.HitTestDragRects(p);

        if (result.HasValue)
        {
            if (result.Value)
                result = CheckExclusionList(p);

            return result.Value;
        }
        else
        {
            // DEFAULT LOGIC

            // We do a simple bounds check here first for the default title bar
            // which is always present, even w/ content extended into titlebar

            // What we know is that the default title bar will always be [0,0,WindowWidth,TitleBar.Height]
            // Therefore, we only need to do check the Y coordinate
            var hgt = _titleBar.Height * PlatformImpl.RenderScaling;
            if (p.Y < hgt)
            {
                if (TitleBar.TitleBarHitTestType == TitleBarHitTestType.Complex &&
                    !ComplexHitTest(p))
                {
                    return false;
                }
                // Now we know we're in the top part of the window designated as the titlebar
                // We need to see if we can actually drag
                // We need to check our exclusion list to see if we we should let the pointer event pass into
                // the client area or say we've grabbed the title bar

                return CheckExclusionList(p);
            }

            return false;
        }


        bool CheckExclusionList(Point p)
        {
            if (_excludeHitTestList != null && _excludeHitTestList.Count > 0)
            {
                // We don't have to read the AttachedProperty value here because we only add them to the list
                // IF the value is true, and remove them if it's set to false later - saving us that overhead

                // We iterate backwards as this is where we'll purge the list if any controls 
                // have been GC'd and the WeakReference no longer exists
                for (int i = _excludeHitTestList.Count - 1; i >= 0; i--)
                {
                    if (_excludeHitTestList[i].TryGetTarget(out var target))
                    {
                        // Skip invisible or disconnected controls
                        if (!target.IsVisible || !((IVisual)target).IsAttachedToVisualTree)
                            continue;

                        // If control was reparented into new window, matrix may be null, catch that case
                        var mat = target.TransformToVisual(this);
                        if (mat.HasValue)
                        {
                            if (new Rect(target.Bounds.Size).TransformToAABB(mat.Value).Contains(p))
                            {
                                // We've hit a control that's asked to be in the titlebar but allow interaction
                                // return false so NCHITTEST returns HTCLIENT
                                return false;
                            }
                        }
                    }
                    else
                    {
                        _excludeHitTestList.RemoveAt(i);
                    }
                }
            }

            return true;
        }
    }

    internal bool ComplexHitTest(Point p)
    {
        var result = this.InputHitTest(p);

        // Special case for TabViewListView during drag operations where blank space 
        // is inserted and causes HitTest to fail (since nothing focusable is there)
        if (result is Visual v && v.TemplatedParent is TabViewListView)
            return false;

        if (result == _defaultTitleBar)
            return true;

        while (result != null)
        {
            if (result.IsHitTestVisible && result.Focusable)
                return false;

            result = result.VisualParent as IInputElement;
        }

        return true;
    }

    internal void UpdateContentPosition(Thickness t)
    {        
        if (_templateRoot != null)
            _templateRoot.Margin = t;
    }

    internal void UpdateFullScreenState(bool isFullScreen)
    {
        if (isFullScreen)
        {
            TemplateSettings.ContentMargin = new Thickness();
        }
        else
        {
            OnExtendsContentIntoTitleBarChanged(_titleBar.ExtendsContentIntoTitleBar);
        }
    }

    internal void OnWin32WindowStateChanged(WindowState state)
    {
        if (state == WindowState.FullScreen)
        {
            TemplateSettings.ContentMargin = new Thickness();
        }
        else
        {
            OnExtendsContentIntoTitleBarChanged(_titleBar.ExtendsContentIntoTitleBar);
        }
    }

    protected virtual void OnRequestedThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
    {
        if (IsWindows)
        {
            _currentAppTheme = args.NewTheme;
            SetTitleBarColors();
        }
    }

    private void SetTitleBarColors()
    {
        if (_templateRoot == null)
            return;

        bool foundAccent = _templateRoot.TryFindResource(s_SystemAccentColor, out var sysColor);
        Color? accentVariant = null;

        if (_currentAppTheme == FluentAvaloniaTheme.LightModeString)
        {
            if (_templateRoot.TryFindResource(s_SystemAccentColorDark1, out var v))
            {
                accentVariant = Unsafe.Unbox<Color>(v);
            }
        }
        else
        {
            if (_templateRoot.TryFindResource(s_SystemAccentColorLight1, out var v))
            {
                accentVariant = Unsafe.Unbox<Color>(v);
            }
        }

        Color textColor;
        if (_templateRoot.TryFindResource(s_TextFillColorPrimary, out var value))
        {
            textColor = Unsafe.Unbox<Color>(value);
        }
        else
        {
            if (_currentAppTheme == FluentAvaloniaTheme.DarkModeString)
            {
                textColor = Colors.White;
            }
            else
            {
                textColor = Colors.Black;
            }
        }

        SetResource(s_TitleBarBackground, _titleBar.BackgroundColor ?? Colors.Transparent);
        SetResource(s_TitleBarForeground, _titleBar.ForegroundColor ?? textColor);

        SetResource(s_TitleBarInactiveBackground, _titleBar.InactiveBackgroundColor ?? Colors.Transparent);
        SetResource(s_TitleBarInactiveForeground, _titleBar.InactiveForegroundColor ?? Colors.Gray);

        SetResource(s_SysCaptionBackground, _titleBar.ButtonBackgroundColor ?? Colors.Transparent);
        SetResource(s_SysCaptionForeground, _titleBar.ButtonForegroundColor ?? textColor);

        SetResource(s_SysCaptionBackgroundHover, _titleBar.ButtonHoverBackgroundColor ??
            (foundAccent ? Unsafe.Unbox<Color>(sysColor) : Color.FromArgb(23, 0, 0, 0)));
        SetResource(s_SysCaptionForegroundHover, _titleBar.ButtonHoverForegroundColor ?? textColor);

        SetResource(s_SysCaptionBackgroundPressed, _titleBar.ButtonPressedBackgroundColor ??
            (foundAccent ? Unsafe.Unbox<Color>(sysColor) : Color.FromArgb(52, 0, 0, 0)));
        SetResource(s_SysCaptionForegroundPressed, _titleBar.ButtonPressedForegroundColor ?? GetPressedColor(textColor));

        SetResource(s_SysCaptionBackgroundInactive, _titleBar.ButtonInactiveBackgroundColor ?? Colors.Transparent);
        SetResource(s_SysCaptionForegroundInactive, _titleBar.ButtonInactiveForegroundColor ??
            (accentVariant ?? Colors.Gray));


        void SetResource(string name, Color color)
        {
            _templateRoot.Resources[name] = color;
        }

        Color GetPressedColor(Color c)
        {
            Color2 c2 = (Color2)c;
            c2.GetHSLf(out _, out _, out var l, out _);

            if (l < 0.5f)
            {
                return c2.LightenPercent(0.15f);
            }
            else
            {
                return c2.LightenPercent(-0.15f);
            }
        }
    }

    private static void OnAllowInteractionInTitleBarChanged(Control control, AvaloniaPropertyChangedEventArgs propChangeArgs)
    {
        if (control is TopLevel tl || control is Popup)
            return; //throw new InvalidOperationException("AllowTitleBarHitTest cannot be set on TopLevels or Popups");

        
        if (propChangeArgs.GetNewValue<bool>())
        {
            // Control likely isn't attached to the visual tree yet so we have no way of attaching this to the 
            // AppWindow hosting it, defer now, but we'll check first just in case
            if (control.GetVisualRoot() is AppWindow aw)
            {
                aw.AddExcludeHitTestItem(control);
            }
            else if (!Design.IsDesignMode) // Don't attach in Design mode
            {
                // Defer until attached to visual tree
                control.AttachedToVisualTree += AwaitControlAttachedToVisualTree;
            }
        }
        else
        {
            // If we change the value to false while still connected, we'll remove it from the list
            // Otherwise, we'll have to wait for the ref to be GC'd then we'll remove it later
            if (control.GetVisualRoot() is AppWindow aw)
            {
                aw.RemoveExcludeHitTestItem(control);
            }
        }
        
        void AwaitControlAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs args)
        {
            var control = sender as Control;
            control.AttachedToVisualTree -= AwaitControlAttachedToVisualTree;

            if (control.GetVisualRoot() is AppWindow aw)
            {
                aw.AddExcludeHitTestItem(control);
            }
        }
    }

    private void AddExcludeHitTestItem(Control c)
    {
        if (_excludeHitTestList == null)
            _excludeHitTestList = new List<WeakReference<Control>>();

        for (int i = 0; i < _excludeHitTestList.Count; i++)
        {
            // Control was already added - can happen if removed and re-added to the visual tree and
            // the control ref was never GC'd (like page navigation w/ cache)
            if (_excludeHitTestList[i].TryGetTarget(out var target) && target == c)
                return;
        }

        _excludeHitTestList.Add(new WeakReference<Control>(c));
    }

    private void RemoveExcludeHitTestItem(Control c)
    {
        if (_excludeHitTestList == null)
            return;
            
        for (int i = _excludeHitTestList.Count - 1; i >= 0; i--)
        {
            if (_excludeHitTestList[i].TryGetTarget(out var target) && target == c)
            {
                _excludeHitTestList.RemoveAt(i);
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeAppWindow()
    {
        IsWindows = true;
        IsWindows11 = OSVersionHelper.IsWindows11();

        (PlatformImpl as AppWindowImpl).SetOwner(this);

        // NOTE FOR FUTURE: 
        // Do NOT enable these properties, doing so causes a clash of logic between here and
        // the actual window logic within avalonia leading to the window shrinking when restoring
        // from maximized or minimized state. 
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaToDecorationsHint = true;
        PseudoClasses.Add(":windows");

        PlatformFeatures = new Win32AppWindowFeatures(this);
    }

    private async void LoadApp()
    {
        Presenter.IsVisible = true;

        using var disp = Presenter.SetValue(OpacityProperty, 0d, Avalonia.Data.BindingPriority.Animation);

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
}
