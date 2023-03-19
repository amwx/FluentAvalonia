using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using FAControlsGallery.Pages;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public MainViewViewModel()
    {
        //GetPredefColors();
        //GetSearchTerms();

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

    private void GetSearchTerms()
    {
        //MainSearchItems = new List<MainAppSearchItem>();

        //Type TryResolveAvaloniaType(string type)
        //{
        //    return Type.GetType($"Avalonia.Controls.{type}") ??
        //        Type.GetType($"Avalonia.Controls.Primitives.{type}");
        //}

        //var file = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/CoreControlsGroups.json");
        //var coreControls = JsonSerializer.Deserialize<List<CoreControlsGroupItem>>(file);
        //for (int i = 1; i < coreControls.Count; i++)
        //{
        //    var desc = coreControls[i].Description.Split(',');

        //    foreach (var item in desc)
        //    {
        //        MainSearchItems.Add(new MainAppSearchItem(item.Trim(), Type.GetType($"FluentAvaloniaSamples.Pages.{coreControls[i].PageType}")));
        //    }
        //}

        //// Get all FluentAvalonia pages
        //file = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/FAControlsGroups.json");
        //var controls = JsonSerializer.Deserialize<List<FAControlsGroupItem>>(file);
        //foreach (var group in controls)
        //{
        //    foreach (var ctrl in group.Controls)
        //    {
        //        // Differentiate between the Avalonia MenuFlyout and my own
        //        if (ctrl.Header == "MenuFlyout")
        //        {
        //            MainSearchItems.Add(new MainAppSearchItem($"{ctrl.Header} (FluentAvalonia)", Type.GetType($"FluentAvaloniaSamples.Pages.{ctrl.PageType}")));
        //        }
        //        else
        //        {
        //            MainSearchItems.Add(new MainAppSearchItem(ctrl.Header, Type.GetType($"FluentAvaloniaSamples.Pages.{ctrl.PageType}")));
        //        }

        //    }
        //}

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
        }
        else if (FAPages.TryGetValue(key, out func))
        {
            var pg = (ControlsPageBase)func();
            var dc = (FAControlsPageItem)pbvm;

            pg.PreviewImage = Application.Current.FindResource(dc.IconResourceKey) as IconSource;
            pg.ControlName = dc.Header;
            pg.ControlNamespace = dc.Namespace;
            pg.Description = dc.Description;

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
        { "HyperlinkButton", () => new HyperlinkButtonPage() },
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
        { "SettingsExpander", () => new SettingsExpanderPage() }
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
