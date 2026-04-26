using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Windowing;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace FAControlsGallery.Pages;

public partial class AppWindowPage : ControlsPageBase
{
    public AppWindowPage()
    {
        InitializeComponent();

        TargetType = typeof(AppWindow);
        //Description = "A special window style designed to mock the modern UWP/WinUI window style. " +
        //    "The window is a DWM extended frame window but made to ensure the resize handles are " +
        //    "still outside the window frame in the shadow area and the titlebar retains its size " +
        //    "when the window is maximized, and the snap layout flyout still works on Windows 11.\n\n" +
        //    "Not on Windows? AppWindow gracefully falls back to a normal window style.";

        SplashButton1.Click += ShowSplashClick;
        SplashButton2.Click += ShowSplashClick;
        SplashButton3.Click += ShowSplashClick;

        ColorPicker1.ColorChanged += HandleColorPicker1ColorChanged;
        SetTaskBarProgressBar.Click += SetTaskBarProgressBar_Click;
    }

    private void SetTaskBarProgressBar_Click(object sender, RoutedEventArgs e)
    {
        var state = ProgressStateCB.SelectedIndex;

        var tbState = state switch
        {
            0 => TaskBarProgressBarState.None,
            1 => TaskBarProgressBarState.Normal,
            2 => TaskBarProgressBarState.Paused,
            3 => TaskBarProgressBarState.Error,
            4 => TaskBarProgressBarState.Indeterminate,
            _ => throw new ArgumentOutOfRangeException()
        };

        var value = (ulong)ProgressValue.Value;

        var tl = TopLevel.GetTopLevel(this);

        if (tl is AppWindow aw)
        {
            aw.PlatformFeatures.SetTaskBarProgressBarState(tbState);

            // MS Docs:
            //Note that a call to SetProgressValue will switch a progress indicator currently in an indeterminate mode
            //(TBPF_INDETERMINATE) to a normal (determinate) display and clear the TBPF_INDETERMINATE flag.
            if (tbState != TaskBarProgressBarState.Indeterminate)
            {
                if (tbState == TaskBarProgressBarState.None)
                {
                    aw.PlatformFeatures.SetTaskBarProgressBarValue(0, 0);
                }
                else
                {
                    aw.PlatformFeatures.SetTaskBarProgressBarValue(value, 100);
                }
            }                      
        }
    }

    private void HandleColorPicker1ColorChanged(object sender, ColorChangedEventArgs args)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is AppWindow aw)
        {
            aw.PlatformFeatures.SetWindowBorderColor(args.NewColor);
        }
    }

    private Stream GetResource(string loc)
    {
        return AssetLoader.Open(new Uri(loc));
    }

    private void ShowSplashClick(object sender, RoutedEventArgs e)
    {
        var aw = new AppWindow
        {
            Content = "AppWindow content!!"
        };

        if (sender == SplashButton1)
        {
            aw.SplashScreen = new DemoSplashScreen
            {
                AppName = "FAControlsGallery",
                MinimumShowTime = 1200
            };
        }
        else if (sender == SplashButton2)
        {
            using var stream = GetResource("avares://FAControlsGallery/Assets/FAIcon.ico");
            var bmp = new Bitmap(stream);
            aw.SplashScreen = new DemoSplashScreen
            {
                AppIcon = bmp,
                MinimumShowTime = 1200
            };
        }
        else if (sender == SplashButton3)
        {
            aw.SplashScreen = new ComplexSplashScreen();
        }

        aw.Show();
    }
}

internal class DemoSplashScreen : IApplicationSplashScreen
{
    public string AppName { get; init; }
    public IImage AppIcon { get; init; }
    public object SplashScreenContent { get; init; }

    // To avoid too quickly transitioning away from the splash screen, you can set a minimum
    // time to hold before loading the content, value is in Milliseconds
    public int MinimumShowTime { get; set; }

    // Place your loading tasks here. NOTE, this is already called on a background thread, so
    // if any UI thread work needs to be done, use Dispatcher.UIThread.Post or .InvokeAsync
    public Task RunTasks(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}

internal class ComplexSplashScreen : IApplicationSplashScreen
{
    public ComplexSplashScreen()
    {
        SplashScreenContent = new DemoComplexSplashScreen();
    }

    public string AppName { get; }
    public IImage AppIcon { get; }
    public object SplashScreenContent { get; }

    // To avoid too quickly transitioning away from the splash screen, you can set a minimum
    // time to hold before loading the content, value is in Milliseconds
    public int MinimumShowTime { get; set; }


    // Place your loading tasks here. NOTE, this is already called on a background thread, so
    // if any UI thread work needs to be done, use Dispatcher.UIThread.Post or .InvokeAsync
    public async Task RunTasks(CancellationToken token)
    {
        await ((DemoComplexSplashScreen)SplashScreenContent).InitApp();
    }
}

