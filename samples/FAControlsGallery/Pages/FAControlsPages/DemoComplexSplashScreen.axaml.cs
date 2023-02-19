using Avalonia.Controls;
using Avalonia.Threading;

namespace FAControlsGallery.Pages;

public partial class DemoComplexSplashScreen : UserControl
{
    public DemoComplexSplashScreen()
    {
        InitializeComponent();
    }

    public async Task InitApp()
    {
        var start = DateTime.Now.Ticks;
        var time = start;
        var progressValue = 0;

        while ((time - start) < TimeSpan.TicksPerSecond)
        {
            progressValue++;
            Dispatcher.UIThread.Post(() => ProgressBar1.Value = progressValue);
            await Task.Delay(100);
            time = DateTime.Now.Ticks;
        }

        start = time;
        Dispatcher.UIThread.Post(() => LoadingText.Text = "Initializing settings");
        var limit = TimeSpan.TicksPerSecond * 2.5;
        while ((time - start) < limit)
        {
            progressValue += 1;
            Dispatcher.UIThread.Post(() => ProgressBar1.Value = progressValue);
            await Task.Delay(150);
            time = DateTime.Now.Ticks;
        }

        Dispatcher.UIThread.Post(() => LoadingText.Text = "Preparing app...");

        while (progressValue < 100)
        {
            progressValue += 1;
            Dispatcher.UIThread.Post(() => ProgressBar1.Value = progressValue);
            await Task.Delay(10);
        }
    }
}
