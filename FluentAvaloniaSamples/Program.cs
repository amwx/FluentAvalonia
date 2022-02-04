using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace FluentAvaloniaSamples
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Information, "FluentAvaloniaTheme")
                .With(new Win32PlatformOptions()
                {
                    UseWindowsUIComposition = true,
                    EnableMultitouch = true, 
                    CompositionBackdropCornerRadius = 8f
                });
    }
}
