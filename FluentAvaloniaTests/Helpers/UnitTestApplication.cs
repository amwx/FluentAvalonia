using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Styling;

namespace FluentAvaloniaTests.Helpers
{
    public class UnitTestApplication : Application
    {
        public UnitTestApplication(bool loadCoreStyles = true)
        {
            LoadCoreStyles = loadCoreStyles;
            AvaloniaLocator.CurrentMutable.BindToSelf<Application>(this);
            RegisterServices();            
        }

        static UnitTestApplication()
        {
            AssetLoader.RegisterResUriParsers();
        }

        public bool LoadCoreStyles { get; set; }

        public static IDisposable Start(bool loadCoreStyles = true)
        {
            var scope = AvaloniaLocator.EnterScope();
            var app = new UnitTestApplication()
            {
                LoadCoreStyles = loadCoreStyles
            };

            return Disposable.Create(() =>
            {
                scope.Dispose();
            });
        }

        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IAssetLoader>().ToConstant(new AssetLoader())
                .Bind<IStyler>().ToConstant(new Styler())
                .BindToSelf<IGlobalStyles>(this)
                .Bind<IFontManagerImpl>().ToConstant(new MockFontManager())
                .Bind<ITextShaperImpl>().ToConstant(new MockTextShaper())
                .Bind<IGlobalClock>().ToConstant(new MockGlobalClock())
                .Bind<IPlatformRenderInterface>().ToConstant(new MockPlatformRenderInterface())
                .Bind<IFocusManager>().ToConstant(new FocusManager());

            if (LoadCoreStyles)
            {
                // This loads just the core base resources that enable StaticResources to work (since they throw if not found)
                // Controls are not loaded here since that's a big ask - individual tests can load what they need
                Styles.Add(new FluentAvaloniaTheme(new Uri("avares://FluentAvalonia/Styling"))
                {
                    // ALL will instruct FluentAvaloniaTheme to not load any templates - we'll let individual control unit tests
                    // load what they need so we don't waste CPU/memory here
                    SkipControls = "ALL"
                });
            }
        }
    }
}
