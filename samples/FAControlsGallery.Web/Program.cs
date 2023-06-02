using Avalonia;
using Avalonia.Browser;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

namespace FAControlsGallery.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        await BuildAvaloniaApp().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>();
}
