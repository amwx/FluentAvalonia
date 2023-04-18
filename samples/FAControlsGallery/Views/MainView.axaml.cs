using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Styling;
using FAControlsGallery.Pages;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;

namespace FAControlsGallery.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        SearchBox.KeyUp += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                var acb = (s as AutoCompleteBox);
                if (acb.SelectedItem != null)
                {
                    var item = acb.SelectedItem as MainAppSearchItem;
                    NavigationService.Instance.NavigateFromContext(item.ViewModel);
                }
                else
                {
                    var items = (DataContext as MainViewViewModel).SearchTerms;
                    foreach (var item in items)
                    {
                        if (string.Equals(item.Header, acb.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            NavigationService.Instance.NavigateFromContext(item.ViewModel);
                            break;
                        }
                    }
                }
                e.Handled = true;
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Simple check - all desktop versions of this app will have a window as the TopLevel
        // Mobile and WASM will have something else
        _isDesktop = TopLevel.GetTopLevel(this) is Window;
        var vm = new MainViewViewModel();
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.SetFrame(FrameView);
        NavigationService.Instance.SetOverlayHost(OverlayHost);

        // On desktop, the window will call this during the splashscreen
        //if (e.Root is not Window)
        //{
            InitializeNavigationPages();
        //}

        FrameView.Navigated += OnFrameViewNavigated;
        NavView.ItemInvoked += OnNavigationViewItemInvoked;
        NavView.BackRequested += OnNavigationViewBackRequested;

        //FrameView.NavigateFromObject((NavView.MenuItemsSource.ElementAt(0) as Control).Tag);        
        FrameView.NavigateFromObject((NavView.FooterMenuItemsSource.ElementAt(0) as Control).Tag);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    public void InitializeNavigationPages()
    {
        string GetControlsList(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var stream = assets.Open(new Uri(name)))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        var coreControls = GetControlsList("avares://FAControlsGallery/Assets/CoreControlsGroups.json");
        var faControls = GetControlsList("avares://FAControlsGallery/Assets/FAControlsGroups.json");

        var mainPages = new MainPageViewModelBase[]
        {
            new HomePageViewModel
            {
                NavHeader = "Home",
                IconKey = "HomeIcon"
            },
            new CoreControlsPageViewModel(coreControls)
            {
                NavHeader = "Core Controls",
                IconKey = "CoreCtrlsIcon"
            },
            new FAControlsOverviewPageViewModel(faControls)
            {
                NavHeader = "FA Controls",
                IconKey = "CtrlsIcon"
            },
            new DesignPageViewModel
            {
                NavHeader = "Design",
                IconKey = "DesignIcon",
                ShowsInFooter = true
            },
            new SettingsPageViewModel
            {
                NavHeader = "Settings",
                IconKey = "SettingsIcon",
                ShowsInFooter = true
            }
        };

        var menuItems = new List<NavigationViewItemBase>(4);
        var footerItems = new List<NavigationViewItemBase>(2);

        bool inDesign = Design.IsDesignMode;
        
        for (int i = 0; i < mainPages.Length; i++)
        {
            var pg = mainPages[i];
            var nvi = new NavigationViewItem
            {
                Content = pg.NavHeader,
                Tag = pg,
                IconSource = (IconSource)this.FindResource(pg.IconKey)
            };

            if (_isDesktop)
            {
                nvi.Classes.Add("SampleAppNav");
            }

            if (pg.ShowsInFooter)
                footerItems.Add(nvi);
            else
                menuItems.Add(nvi);

            if (!inDesign)
            {
                (DataContext as MainViewViewModel).BuildSearchTerms(pg);
            }
        }

        NavView.MenuItemsSource = menuItems;
        NavView.FooterMenuItemsSource = footerItems;

        if (!_isDesktop)
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            NavView.Classes.Remove("SampleAppNav");
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);

        // TODO: Use BackRequested from TopLevel
        // This enables the X1/X2 buttons of my mouse to handle back & forward navigation
        if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
        {
            if (FrameView.CanGoBack)
            {
                FrameView.GoBack();
                e.Handled = true;
            }
        }
        else if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
        {
            if (FrameView.CanGoForward)
            {
                FrameView.GoForward();
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }

    private void OnNavigationViewBackRequested(object sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
    {
        // Change the current selected item back to normal
        SetNVIIcon(NavView.SelectedItem as NavigationViewItem, false);

        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            FrameView.NavigateFromObject(nvi.Tag, new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = e.RecommendedNavigationTransitionInfo
            });
        }
    }

    private void OnFrameViewNavigated(object sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        var dc = page.DataContext;

        MainPageViewModelBase mainPage = null;

        if (dc is MainPageViewModelBase mpvmb)
        {
            mainPage = mpvmb;
        }
        else if (dc is PageBaseViewModel pbvm)
        {
            mainPage = pbvm.Parent;
        }

        bool found = false;
        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag == mainPage)
            {
                found = true;
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
                break;
            }
        }

        if (!found)
        {
            foreach (NavigationViewItem nvi in NavView.FooterMenuItemsSource)
            {
                if (nvi.Tag == mainPage)
                {
                    found = true;
                    NavView.SelectedItem = nvi;
                    SetNVIIcon(nvi, true);
                    break;
                }
            }            
        }
           
        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
        }
    }

    private void SetNVIIcon(NavigationViewItem item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        Type t = item.Tag as Type;

        if (t == typeof(HomePage))
        {
            item.IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value) ?
                (IconSource)value : null;
        }
        //else if (t == typeof(CoreControlsPage))
        //{
        //    item.IconSource = this.TryFindResource(selected ? "CoreCtrlsIconFilled" : "CoreCtrlsIcon", out var value) ?
        //        (IconSource)value : null;
        //}
        //// Skip NewControlsPage as its icon is the same for both
        //else if (t == typeof(ResourcesPage))
        //{
        //    item.IconSource = this.TryFindResource(selected ? "ResourcesIconFilled" : "ResourcesIcon", out var value) ?
        //        (IconSource)value : null;
        //}
        else if (t == typeof(SettingsPage))
        {
            item.IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value) ?
               (IconSource)value : null;
        }
    }

    private async void AnimateContentForBackButton(bool show)
    {
        if (!WindowIcon.IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0,0,0,1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48,4,12,4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon, null);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0,0,0,1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12,4,12,4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon, null);
        }
    }

    private bool _isDesktop;
}
