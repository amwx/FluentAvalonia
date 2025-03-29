using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using FAControlsGallery.Pages;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public MainViewViewModel()
    {
        NavigationFactory = new NavigationFactory(this);
    }

    public NavigationFactory NavigationFactory { get; }

    public AvaloniaList<MainAppSearchItem> SearchTerms { get; } = new AvaloniaList<MainAppSearchItem>();

    public void BuildSearchTerms(MainPageViewModelBase pageItem)
    {
        if (pageItem is HomePageViewModel || pageItem is SettingsPageViewModel)
            return;

        if (pageItem is CoreControlsPageViewModel ccpg)
        {
            for (int i = 0; i < ccpg.CoreControlGroups.Count; i++)
            {
                var item = ccpg.CoreControlGroups[i];

                Add(item);
            }
        }
        else if (pageItem is FAControlsOverviewPageViewModel fapg)
        {
            for (int i = 0; i < fapg.ControlGroups.Count; i++)
            {
                for (int j = 0; j < fapg.ControlGroups[i].Controls.Count; j++)
                {
                    var item = fapg.ControlGroups[i].Controls[j];

                    Add(item);
                }
            }
        }

        void Add(PageBaseViewModel item)
        {
            if (item.SearchKeywords is null)
                return;

            string ctrlNamespace = "Avalonia.UI.Controls";
            if (item is FAControlsPageItem fa)
                ctrlNamespace = fa.Namespace;

            for (int i = 0; i < item.SearchKeywords.Length; i++)
            {
                SearchTerms.Add(new MainAppSearchItem
                {
                    Header = item.SearchKeywords[i],
                    ViewModel = item,
                    Namespace = ctrlNamespace
                });
            }            
        }
    }
}

public class NavigationFactory : INavigationPageFactory
{
    public NavigationFactory(MainViewViewModel owner)
    {
        Owner = owner;
    }

    public MainViewViewModel Owner { get; }

    public Control GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        if (target is HomePageViewModel)
        {
            return new HomePage
            {
                DataContext = target
            };
        }
        else if (target is CoreControlsPageViewModel)
        {
            return new CoreControlsPage
            {
                DataContext = target
            };
        }
        else if (target is FAControlsOverviewPageViewModel)
        {
            return new FAControlsOverviewPage
            {
                DataContext = target
            };
        }
        else if (target is DesignPageViewModel)
        {
            return new DesignPage
            {
                DataContext = target
            };
        }
        else if (target is SettingsPageViewModel)
        {
            return new SettingsPage
            {
                DataContext = target
            };
        }
        else
        {
            return ResolvePage(target as PageBaseViewModel);
        }
    }

    private Control ResolvePage(PageBaseViewModel pbvm)
    {
        if (pbvm is null)
            return null;

        Control page = null;
        var key = pbvm.PageKey;

        if (CorePages.TryGetValue(key, out var func))
        {
            page = func();

            const string faPageGithub =
               "https://github.com/amwx/FluentAvalonia/tree/master/samples/FAControlsGallery/Pages/CoreControlPages";

            (page as ControlsPageBase).GithubPrefixString = faPageGithub;
            (page as ControlsPageBase).CreationContext = pbvm;
        }
        else if (FAPages.TryGetValue(key, out func))
        {
            var pg = (ControlsPageBase)func();
            var dc = (FAControlsPageItem)pbvm;
            const string faPageGithub =
               "https://github.com/amwx/FluentAvalonia/tree/master/samples/FAControlsGallery/Pages/FAControlsPages";

            pg.GithubPrefixString = faPageGithub;
            pg.PreviewImage = Application.Current.FindResource(dc.IconResourceKey) as IconSource;
            pg.ControlName = dc.Header;
            pg.ControlNamespace = dc.Namespace;
            pg.Description = dc.Description;
            pg.CreationContext = pbvm;

            if (dc.WinUIDocsLink is not null)
                pg.WinUIDocsLink = new Uri(dc.WinUIDocsLink);

            pg.WinUINamespace = dc.WinUINamespace;
            
            if (dc.WinUIGuidelinesLink is not null)
                pg.WinUIGuidelinesLink = new Uri(dc.WinUIGuidelinesLink);

            page = pg;
        }

        return page;
    }

    // Do this to avoid needing Activator.CreateInstance to create from type info
    // and to avoid a ridiculous amount of 'ifs'
    private readonly Dictionary<string, Func<Control>> CorePages = new Dictionary<string, Func<Control>>
    {
        { "BasicInput", () => new BasicInputControlsPage() },
        { "Text", () => new TextControlsPage() },
        { "List", () => new ListControlsPage() },
        { "DateTime", () => new DateTimeControlsPage() },
        { "Range", () => new RangeControlsPage() },
        { "Menu", () => new MenuControlsPage() },
        { "View", () => new ViewControlsPage() },
        { "Data", () => new DataControlsPage() },
        { "Misc", () => new MiscControlsPage() },
    };

    private readonly Dictionary<string, Func<Control>> FAPages = new Dictionary<string, Func<Control>>
    {
        { "AppWindow", () => new AppWindowPage() },
        { "NumberBox", () => new NumberBoxPage() },
        { "FAComboBox", () => new FAComboBoxPage() },
        { "TaskDialog", () => new TaskDialogPage() },
        { "ContentDialog", () => new ContentDialogPage() {  } },
        { "PickerFlyoutBase", () => new PickerFlyoutBasePage() },
        { "TeachingTip", () => new TeachingTipPage() },
        { "Icons", () => new IconsPage() },
        { "NavigationView", () => new NavigationViewPage() },
        { "Frame", () => new FramePage() },
        { "TabView", () => new TabViewPage() },
        { "FAMenuFlyout", () => new MenuFlyoutPage() },
        { "CommandBar", () => new CommandBarPage() },
        { "CommandBarFlyout", () => new CommandBarFlyoutPage() },
        { "XamlUICommand", () => new XamlUICommandPage() },
        { "FAColorPicker", () => new ColorPickerPage() },
        { "ColorPickerButton", () => new ColorPickerButtonPage() },
        { "InfoBar", () => new InfoBarPage() },
        { "InfoBadge", () => new InfoBadgePage() },
        { "SettingsExpander", () => new SettingsExpanderPage() },
        { "RangeSlider", () => new RangeSliderPage() },
        { "ProgressRing", () => new ProgressRingPage() },
        { "BreadcrumbBar", () => new BreadcrumbBarPage() }
    };
}

public class MainAppSearchItem
{
    public MainAppSearchItem() { }

    public MainAppSearchItem(string pageHeader, Type pageType)
    {
        Header = pageHeader;
        PageType = pageType;
    }

    public string Header { get; set; }

    public PageBaseViewModel ViewModel { get; set; }

    public string Namespace { get; set; }

    public Type PageType { get; set; }
}
