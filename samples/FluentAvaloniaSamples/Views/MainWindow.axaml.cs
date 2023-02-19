using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.Views;

public class SampleAppSplashScreen : IApplicationSplashScreen
{
    public SampleAppSplashScreen()
    {
        var al = AvaloniaLocator.Current.GetService<IAssetLoader>();
        using (var s = al.Open(new Uri("avares://FluentAvaloniaSamples/Assets/FAIcon.ico")))
            AppIcon = new Bitmap(s);
    }

    string IApplicationSplashScreen.AppName { get; }

    public IImage AppIcon { get; }

    object IApplicationSplashScreen.SplashScreenContent { get; }

    int IApplicationSplashScreen.MinimumShowTime => 2000;

    Task IApplicationSplashScreen.RunTasks(CancellationToken token) => null;
}

public class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        //SplashScreen = new SampleAppSplashScreen();
#if DEBUG
        this.AttachDevTools();
#endif
        MinWidth = 450;
        MinHeight = 400;

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        Application.Current.ActualThemeVariantChanged += ApplicationActualThemeVariantChanged;
    }

    private void ApplicationActualThemeVariantChanged(object sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: add Windows version to CoreWindow
            if (IsWindows11 && ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                TryEnableMicaEffect();
            }
            else if (ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                // Clear the local value here, and let the normal styles take over for HighContrast theme
                SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
            }
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var thm = ActualThemeVariant;

        // Enable Mica on Windows 11
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: add Windows version to CoreWindow
            if (IsWindows11 && thm != FluentAvaloniaTheme.HighContrastTheme)
            {
                TransparencyBackgroundFallback = Brushes.Transparent;
                TransparencyLevelHint = WindowTransparencyLevel.Mica;

                TryEnableMicaEffect();
            }
        }

        var screen = Screens.ScreenFromVisual(this);
        if (screen != null)
        {
            double width = Width;
            double height = Height;

            if (screen.WorkingArea.Width > 1280)
            {
                width = 1280;
            }
            else if (screen.WorkingArea.Width > 1000)
            {
                width = 1000;
            }
            else if (screen.WorkingArea.Width > 700)
            {
                width = 700;
            }
            else if (screen.WorkingArea.Width > 500)
            {
                width = 500;
            }
            else
            {
                width = 450;
            }

            if (screen.WorkingArea.Height > 720)
            {
                width = 720;
            }
            else if (screen.WorkingArea.Height > 600)
            {
                width = 600;
            }
            else if (screen.WorkingArea.Height > 500)
            {
                width = 500;
            }
            else
            {
                width = 400;
            }
        }
    }

    private void TryEnableMicaEffect()
    {
        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Dark, out var value) ? (Color2)(Color)value : new Color2(32, 32, 32);

            color = color.LightenPercent(-0.8f);

            Background = new ImmutableSolidColorBrush(color, 0.78);
        }
        else if (ActualThemeVariant == ThemeVariant.Light)
        {
            // Similar effect here
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Light, out var value) ? (Color2)(Color)value : new Color2(243, 243, 243);

            color = color.LightenPercent(0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
