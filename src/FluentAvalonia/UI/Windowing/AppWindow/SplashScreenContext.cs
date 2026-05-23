using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Windowing;

internal class SplashScreenContext
{
    public SplashScreenContext(IFAApplicationSplashScreen splash)
    {
        SplashScreen = splash;
    }

    public IFAApplicationSplashScreen SplashScreen { get; }

    public bool HasShownSplashScreen { get; set; }

    public FAAppSplashScreen Host
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
        await SplashScreen.RunTasks(_splashCTS.Token);
        _splashCTS?.Dispose();
        _splashCTS = null;
    }

    public void TryCancel()
    {
        _splashCTS?.Cancel();
        _splashCTS?.Dispose();
        _splashCTS = null;
    }

    private FAAppSplashScreen _splashHost;
    private CancellationTokenSource _splashCTS;
}

public class FAAppSplashScreen : TemplatedControl
{
    public IFAApplicationSplashScreen SplashScreen { get; set; }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (SplashScreen != null)
        {
            // User set content has priority
            if (SplashScreen.SplashScreenContent != null)
            {
                var cp = e.NameScope.Find<ContentPresenter>("ContentHost");
                cp.Content = SplashScreen.SplashScreenContent;
            }
            else if (SplashScreen.AppIcon != null) // Followed by the icon
            {
                var img = e.NameScope.Find<Image>("AppImageHost");
                img.Source = SplashScreen.AppIcon;
            }
            else if (!string.IsNullOrEmpty(SplashScreen.AppName)) // Followed by just the app name
            {
                var tb = e.NameScope.Find<TextBlock>("AppNameText");
                tb.Text = SplashScreen.AppName;
            }
        }
    }
}
