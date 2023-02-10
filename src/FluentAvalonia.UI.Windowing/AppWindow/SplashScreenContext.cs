using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Windowing;

internal class SplashScreenContext
{
    public SplashScreenContext(IApplicationSplashScreen splash)
    {
        SplashScreen = splash;
    }

    public IApplicationSplashScreen SplashScreen { get; }

    public bool HasShownSplashScreen { get; set; }

    public AppSplashScreen Host
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
        });
        _splashCTS.Dispose();
        _splashCTS = null;
    }

    public void TryCancel()
    {
        _splashCTS?.Cancel();
        _splashCTS?.Dispose();
        _splashCTS = null;
    }

    private AppSplashScreen _splashHost;
    private CancellationTokenSource _splashCTS;
}

public class AppSplashScreen : TemplatedControl
{
    public IApplicationSplashScreen SplashScreen { get; set; }

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
