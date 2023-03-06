using Avalonia;
using Avalonia.Browser;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

namespace FAControlsGallery.Web;

public class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp()
        .SetupBrowserApp("out");

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>();
}
