using Avalonia;
using FluentAvaloniaTests.Helpers;
using Avalonia.Headless;
using FluentAvalonia.Styling;
using Avalonia.Controls.ApplicationLifetimes;

[assembly: AvaloniaTestApplication(typeof(UnitTestApplication))]

namespace FluentAvaloniaTests.Helpers;

public class UnitTestApplication : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<UnitTestApplication>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        })
        .AfterSetup(InitStyles);

    private static void InitStyles(AppBuilder ab)
    {
        // FATheme requires Application.Current to be set, so we run this after Application setup has finished
        Current.Styles.Add(new FluentAvaloniaTheme());
    }
}
