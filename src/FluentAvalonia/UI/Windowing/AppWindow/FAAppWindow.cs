using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Custom Window that supports a modern Windows look and title bar customization,
/// with a graceful fallback for MacOS and Linux
/// </summary>
public partial class FAAppWindow : Window
{
    public FAAppWindow()
    {
        TemplateSettings = new FAAppWindowTemplateSettings();
        _titleBar = new FAAppWindowTitleBar(this);
        PseudoClasses.Add(":noFullScreen");

        if (OperatingSystem.IsWindows() && !Design.IsDesignMode)
        {
            InitializeAppWindow();
        }
    }

    static FAAppWindow()
    {
        if (OperatingSystem.IsWindows())
            ExtendClientAreaToDecorationsHintProperty.OverrideDefaultValue<FAAppWindow>(true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);      

        if (IsWindows && !Design.IsDesignMode)
        {
            _templateRoot = e.NameScope.Find<Border>("RootBorder");
            
            _defaultTitleBar = e.NameScope.Find<Panel>("DefaultTitleBar");

            // This will set all our TemplateSettings properties
            OnTitleBarHeightChanged(_titleBar.Height);

            SetTitleBarColors();
        }

        if (SplashScreen != null)
        {
            var host = e.NameScope.Find<FAAppSplashScreen>("SplashHost");
            if (host != null)
            {
                _splashContext.Host = host;
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == ExtendClientAreaToDecorationsHintProperty ||
            change.Property == ExtendClientAreaTitleBarHeightHintProperty)
        {
            if (IsWindows)
            {
                throw new InvalidOperationException("FAAppWindow cannot be customized with ExtendClientAreaToDecorationsHintProperty." +
                    "Use the TitleBar property or a regular Avalonia window");
            }
        }
        else if (change.Property == IconProperty)
        {
            base.Icon = new WindowIcon(change.NewValue as Bitmap);
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, change.NewValue != null);
        }
        else if (change.Property == ActualThemeVariantProperty)
        {
            SetTitleBarColors();
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
    }

    internal void OnExtendsContentIntoTitleBarChanged(bool isExtended)
    {
        if (isExtended)
        {
            TemplateSettings.IsTitleBarContentVisible = false;
        }
        else
        {
            TemplateSettings.IsTitleBarContentVisible = true;
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

        if (p.Y < _titleBar.Height)
        {
            if (!ComplexHitTest(p))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    internal bool ComplexHitTest(Point p)
    {
        var result = this.InputHitTest(p) as InputElement;

        // Special case for TabViewListView during drag operations where blank space 
        // is inserted and causes HitTest to fail (since nothing focusable is there)
        if (result is Visual v && v.TemplatedParent is FATabViewListView)
            return false;

        if (result == _defaultTitleBar)
            return true;

        while (result != null)
        {
            if (result.IsHitTestVisible && result.Focusable)
                return false;

            result = result.GetVisualParent() as InputElement;
        }

        return true;
    }

    private void SetTitleBarColors()
    {
        if (_titleBar == null)
            return;

        SetResource(s_TitleBarBackground, _titleBar.BackgroundColor);
        SetResource(s_TitleBarForeground, _titleBar.ForegroundColor);

        SetResource(s_TitleBarInactiveBackground, _titleBar.InactiveBackgroundColor);
        SetResource(s_TitleBarInactiveForeground, _titleBar.InactiveForegroundColor);

        SetResource(s_SysCaptionBackground, _titleBar.ButtonBackgroundColor);
        SetResource(s_SysCaptionForeground, _titleBar.ButtonForegroundColor);

        SetResource(s_SysCaptionBackgroundHover, _titleBar.ButtonHoverBackgroundColor);
        SetResource(s_SysCaptionForegroundHover, _titleBar.ButtonHoverForegroundColor);

        SetResource(s_SysCaptionBackgroundPressed, _titleBar.ButtonPressedBackgroundColor);
        SetResource(s_SysCaptionForegroundPressed, _titleBar.ButtonPressedForegroundColor);

        SetResource(s_SysCaptionBackgroundInactive, _titleBar.ButtonInactiveBackgroundColor);
        SetResource(s_SysCaptionForegroundInactive, _titleBar.ButtonInactiveForegroundColor);


        void SetResource(string name, Color? color)
        {
            if (color.HasValue)
            {
                Resources[name] = color;
            }
            else
            {
                Resources.Remove(name);
            }
        }
    }

    internal void OnShowFullScreenButtonChanged(bool value)
    {
        PseudoClasses.Set(":noFullScreen", value);
    }
    
    private async void LoadApp()
    {
        if (Presenter is not ContentPresenter cp)
            return;

        cp.IsVisible = true;

        // Taking this out, it's causing flickering of the content after the splash fade animation
        // Another regression in the animation system for 11.0...
        //using var disp = cp.SetValue(OpacityProperty, 0d, Avalonia.Data.BindingPriority.Animation);

        var aniSplash = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            FillMode = FillMode.Forward,
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

        await Task.WhenAll(aniSplash.RunAsync(_splashContext.Host),
            aniCP.RunAsync((Animatable)Presenter));

        PseudoClasses.Set(":splashOpen", false);
        _splashContext.HasShownSplashScreen = true;
    }

    private bool _hasShown;
}
