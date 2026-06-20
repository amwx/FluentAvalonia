using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using FAControlsGallery.Pages;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;

namespace FAControlsGallery.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        SearchBox.KeyUp += HandleSearchBoxKeyUp;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        StorageService.Create(TopLevel.GetTopLevel(this));
        ClipboardService.Owner = TopLevel.GetTopLevel(this);
        // Simple check - all desktop versions of this app will have a window as the TopLevel
        // Mobile and WASM will have something else
        _isDesktop = TopLevel.GetTopLevel(this) is Window;
        var vm = new MainViewViewModel();
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.SetFrame(FrameView);
        NavigationService.Instance.SetOverlayHost(OverlayHost);

        // On desktop, the window will call this during the splashscreen
        if (TopLevel.GetTopLevel(this) is FAAppWindow aw)
        {
            (aw.SplashScreen as MainAppSplashScreen).InitApp += () =>
            {
                InitializeNavigationPages();                
            };
        }
        else
        {
            InitializeNavigationPages();
        }

        FrameView.Navigated += OnFrameViewNavigated;
        NavView.ItemInvoked += OnNavigationViewItemInvoked;
        NavView.BackRequested += OnNavigationViewBackRequested;        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (TopLevel.GetTopLevel(this) is FAAppWindow aw)
        {
            // Prior to v3 when AppWindow took care of all the caption button drawing we reported the width
            // of the caption buttons via TitleBar.RightInset. Now we use Avalonia's WindowDrawnDecorations
            // which doesn't report that. However, the default size used in the template I put was
            // to make the buttons 46 dip, so x3 = 138. We'll use 140 for 2 extra padding
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(140, GridUnitType.Pixel);
        }
    }

    public void InitializeNavigationPages()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var mainPages = new MainPageViewModelBase[]
            {
                new HomePageViewModel
                {
                    NavHeader = "Home",
                    IconKey = "HomeIcon",
                    Parent = DataContext as MainViewViewModel
                },
                new CoreControlsPageViewModel()
                {
                    NavHeader = "Core Controls",
                    IconKey = "CoreControlsIcon",
                    Parent = DataContext as MainViewViewModel
                },
                new FAControlsOverviewPageViewModel()
                {
                    NavHeader = "FA Controls",
                    IconKey = "FAControlsIcon",
                    Parent = DataContext as MainViewViewModel
                },
                new PlaygroundPageViewModel()
                {
                    NavHeader = "Playground",
                    IconKey = "PlaygroundIcon",
                    Parent = DataContext as MainViewViewModel
                },
                new DesignPageViewModel
                {
                    NavHeader = "Design",
                    IconKey = "DesignIcon",
                    ShowsInFooter = true,
                    Parent = DataContext as MainViewViewModel
                },
                new SettingsPageViewModel
                {
                    NavHeader = "Settings",
                    IconKey = "SettingsIcon",
                    ShowsInFooter = true,
                    Parent = DataContext as MainViewViewModel
                }
            };

            var menuItems = new List<FANavigationViewItemBase>(4);
            var footerItems = new List<FANavigationViewItemBase>(2);

            bool inDesign = Design.IsDesignMode;

            var dc = DataContext as MainViewViewModel;
            for (int i = 0; i < mainPages.Length; i++)
            {
                var pg = mainPages[i];
                var nvi = new FANavigationViewItem
                {
                    Content = pg.NavHeader,
                    Tag = pg,
                    IconSource = this.FindResource(pg.IconKey) as FAIconSource
                };

                //ToolTip.SetTip(nvi, pg.NavHeader);

                if (_isDesktop || OperatingSystem.IsBrowser())
                {
                    nvi.Classes.Add("SampleAppNav");
                }

                if (pg.ShowsInFooter)
                    footerItems.Add(nvi);
                else
                    menuItems.Add(nvi);

                if (!inDesign)
                {
                    dc.BuildSearchTerms(pg);
                }
            }

            dc.MainNavPages = mainPages;
            dc.BuildSearchTerms2();

            NavView.MenuItemsSource = menuItems;
            NavView.FooterMenuItemsSource = footerItems;
            
            if (_isDesktop || OperatingSystem.IsBrowser())
            {
                NavView.Classes.Add("SampleAppNav");
            }
            else
            {
                NavView.PaneDisplayMode = FANavigationViewPaneDisplayMode.LeftMinimal;
            }

            FrameView.NavigateFromObject((NavView.MenuItemsSource.ElementAt(0) as Control).Tag);
        });
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);

        // Frame handles X1 -> BackRequested automatically, we can handle X2
        // here to enable forward navigation
        if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
        {
            if (FrameView.CanGoForward)
            {
                FrameView.GoForward();
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }

    private void HandleSearchBoxKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var acb = SearchBox;
            MainAppSearchItem source = null;
            ViewModelBase viewModel = null;
            if (acb.SelectedItem != null)
            {
                var item = acb.SelectedItem as MainAppSearchItem;
                viewModel = item.ViewModel;
                source = item;
            }
            else
            {
                var items = (DataContext as MainViewViewModel).SearchTerms;
                foreach (var item in items)
                {
                    if (string.Equals(item.Header, acb.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        source = item;
                        viewModel = item.ViewModel;
                        break;
                    }
                }
            }

            if (viewModel != null)
            {
                if (viewModel is DesignPageViewModel dpvm)
                {
                    if (source.Header == DesignPageViewModel.TypographyKey)
                    {
                        dpvm.CurrentIndex = 0;
                    }
                    else if (source.Header == DesignPageViewModel.IconsKey)
                    {
                        dpvm.CurrentIndex = 1;
                    }
                    else if (source.Header == DesignPageViewModel.ColorsKey)
                    {
                        dpvm.CurrentIndex = 2;
                    }
                }

                NavigationService.Instance.NavigateFromContext(viewModel,
                    new FAEntranceNavigationTransitionInfo());
            }

            e.Handled = true;
        }
    }

    private void OnNavigationViewBackRequested(object sender, FANavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object sender, FANavigationViewItemInvokedEventArgs e)
    {
        // Change the current selected item back to normal
        // SetNVIIcon(sender as NavigationViewItem, false);

        if (e.InvokedItemContainer is FANavigationViewItem nvi)
        {
            FANavigationTransitionInfo info;

            // Keep the frame navigation when not using connected animation but suppress it
            // if we have a connected animation binding two pages
            if (FrameView.Content is ControlsPageBase cpb &&
                ((cpb.TargetType == null && nvi.Tag is CoreControlsPageViewModel) ||
                (cpb.TargetType != null && nvi.Tag is FAControlsOverviewPageViewModel)))
            {
                info = new FASuppressNavigationTransitionInfo();
            }
            else
            {
                info = e.RecommendedNavigationTransitionInfo;
            }

            NavigationService.Instance.NavigateFromContext(nvi.Tag, info);
        }
    }

    private void OnFrameViewNavigated(object sender, FANavigationEventArgs e)
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
        else if (page is ControlsPageBase cpb)
        {
            mainPage = cpb.CreationContext.Parent;
        }

        foreach (FANavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag == mainPage)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (FANavigationViewItem nvi in NavView.FooterMenuItemsSource)
        {
            if (nvi.Tag == mainPage)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
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

    private void SetNVIIcon(FANavigationViewItem item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        var t = item.Tag;

        if (t is HomePageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value) ?
                (FAIconSource)value : null;
        }
        else if (t is CoreControlsPageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "CoreControlsIconFilled" : "CoreControlsIcon", out var value) ?
                (FAIconSource)value : null;
        }
        else if (t is FAControlsOverviewPageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "FAControlsIconFilled" : "FAControlsIcon", out var value) ?
                (FAIconSource)value : null;
        }
        else if (t is DesignPageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "DesignIconFilled" : "DesignIcon", out var value) ?
                (FAIconSource)value : null;
        }
        else if (t is PlaygroundPageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "PlaygroundIconFilled" : "PlaygroundIcon", out var value) ?
                (FAIconSource)value : null;
        }
        else if (t is SettingsPageViewModel)
        {
            item.IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value) ?
               (FAIconSource)value : null;
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

            await ani.RunAsync(WindowIcon);

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

            await ani.RunAsync(WindowIcon);
        }
    }

    private bool _isDesktop;
}
