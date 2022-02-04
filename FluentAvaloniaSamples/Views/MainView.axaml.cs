using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.Core.ApplicationModel;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvaloniaSamples.Pages;
using FluentAvaloniaSamples.Services;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewViewModel();

            this.Find<AutoCompleteBox>("SearchBox").KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    var acb = (s as AutoCompleteBox);
                    if (acb.SelectedItem != null)
                    {
                        var item = acb.SelectedItem as MainAppSearchItem;
                        NavigationService.Instance.Navigate(item.PageType);
                    }
                    else
                    {
                        var items = (DataContext as MainViewViewModel).MainSearchItems;
                        foreach(var item in items)
                        {
                            if (string.Equals(item.Header, acb.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                NavigationService.Instance.Navigate(item.PageType);
                                break;
                            }
                        }
                    }
                    e.Handled = true;
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (e.Root is Window b)
            {
                b.Opened += OnParentWindowOpened;
            }

            _windowIconControl = this.FindControl<IControl>("WindowIcon");
            _frameView = this.FindControl<Frame>("FrameView");
            _navView = this.FindControl<NavigationView>("NavView");
            _navView.MenuItems = GetNavigationViewItems();
            _navView.FooterMenuItems = GetFooterNavigationViewItems();

            _frameView.Navigated += OnFrameViewNavigated;
            _navView.ItemInvoked += OnNavigationViewItemInvoked;
            _navView.BackRequested += OnNavigationViewBackRequested;

            _frameView.Navigate(typeof(HomePage));

            NavigationService.Instance.SetFrame(_frameView);
            NavigationService.Instance.SetOverlayHost(this.FindControl<Panel>("OverlayHost"));
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var pt = e.GetCurrentPoint(this);

            if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
            {
                if (_frameView.CanGoBack)
                {
                    _frameView.GoBack();
                    e.Handled = true;
                }
            }
            else if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
            {
                if (_frameView.CanGoForward)
                {
                    _frameView.GoForward();
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        private List<NavigationViewItem> GetNavigationViewItems()
        {
            return new List<NavigationViewItem>
            {
                new NavigationViewItem
                {
                    Content = "Home",
                    Tag = typeof(HomePage),
                    Icon = new IconSourceElement { IconSource = (IconSource)this.FindResource("HomeIcon") },
                    Classes =
                    {
                        "SampleAppNav"
                    }
                },
                 new NavigationViewItem
                {
                    Content = "Core Controls",
                    Tag = typeof(CoreControlsPage),
                    Icon = new IconSourceElement { IconSource = (IconSource)this.FindResource("CoreCtrlsIcon") },
                    Classes =
                    {
                        "SampleAppNav"
                    }
                },
                new NavigationViewItem
                {
                    Content = "New Controls",
                    Tag = typeof(NewControlsPage),
                    Icon = new IconSourceElement { IconSource = (IconSource)this.FindResource("CtrlsIcon") },
                    Classes =
                    {
                        "SampleAppNav"
                    }
                },
                new NavigationViewItem
                {
                    Content = "Resources",
                    Tag = typeof(ResourcesPage),
                    Icon = new IconSourceElement { IconSource = (IconSource)this.FindResource("ResourcesIcon") },
                    Classes =
                    {
                        "SampleAppNav"
                    }
                },
            };
        }

        private List<NavigationViewItem> GetFooterNavigationViewItems()
        {
            return new List<NavigationViewItem>
            {
                new NavigationViewItem
                {
                    Content = "Settings",
                    Tag = typeof(SettingsPage),
                    Icon = new IconSourceElement { IconSource = (IconSource)this.FindResource("SettingsIcon") },
                    Classes =
                    {
                        "SampleAppNav"
                    }
                }
            };
        }

        private void OnParentWindowOpened(object sender, EventArgs e)
        {
            (sender as Window).Opened -= OnParentWindowOpened;

            if (sender is CoreWindow cw)
            {
                var titleBar = cw.TitleBar;
                if (titleBar != null)
                {
                    titleBar.ExtendViewIntoTitleBar = true;

                    titleBar.LayoutMetricsChanged += OnApplicationTitleBarLayoutMetricsChanged;

                    if (this.FindControl<Grid>("TitleBarHost") is Grid g)
                    {
                        cw.SetTitleBar(g);
                        g.Margin = new Thickness(0, 0, titleBar.SystemOverlayRightInset, 0);
                    }
                }                
            }
        }

        private void OnApplicationTitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (this.FindControl<Grid>("TitleBarHost") is Grid g)
            {
                g.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
            }
        }
        
        private void OnNavigationViewBackRequested(object sender, NavigationViewBackRequestedEventArgs e)
        {
            _frameView.GoBack();
        }

        private void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
        {
            // Change the current selected item back to normal
            SetNVIIcon(_navView.SelectedItem as NavigationViewItem, false);

            if (e.InvokedItemContainer is NavigationViewItem nvi && nvi.Tag is Type typ)
            {
                _frameView.Navigate(typ, null, e.RecommendedNavigationTransitionInfo);
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
                (item.Icon as IconSourceElement).IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value) ? 
                    (IconSource)value : null;
            }
            else if (t == typeof(CoreControlsPage))
            {
                (item.Icon as IconSourceElement).IconSource = this.TryFindResource(selected ? "CoreCtrlsIconFilled" : "CoreCtrlsIcon", out var value) ?
                    (IconSource)value : null;
            }
            // Skip NewControlsPage as its icon is the same for both
            else if (t == typeof(ResourcesPage))
            {
                (item.Icon as IconSourceElement).IconSource = this.TryFindResource(selected ? "ResourcesIconFilled" : "ResourcesIcon", out var value) ?
                    (IconSource)value : null;
            }
            else if (t == typeof(SettingsPage))
            {
                (item.Icon as IconSourceElement).IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value) ?
                   (IconSource)value : null;
            }
        }

        private void OnFrameViewNavigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType.IsAssignableTo(typeof(FAControlsPageBase)))
            {
                // Keep new Control tab selected if we're within a new controls page
                _navView.SelectedItem = _navView.MenuItems.ElementAt(2);
            }
            else
            {
                bool found = false;
                foreach (NavigationViewItem nvi in _navView.MenuItems)
                {
                    if (nvi.Tag is Type tag && tag == e.SourcePageType)
                    {
                        found = true;
                        _navView.SelectedItem = nvi;
                        SetNVIIcon(nvi, true);
                        break;
                    }
                }

                if (!found)
                {
                    if (e.SourcePageType == typeof(SettingsPage))
                    {
                        _navView.SelectedItem = _navView.FooterMenuItems.ElementAt(0);
                    }
                    else
                    {
                        // only remaining page type is core controls pages
                        _navView.SelectedItem = _navView.MenuItems.ElementAt(1);
                    }
                }
            }           

            if (_frameView.BackStackDepth > 0 && !_navView.IsBackButtonVisible)
            {
                AnimateContentForBackButton(true);
            }
            else if (_frameView.BackStackDepth == 0 && _navView.IsBackButtonVisible)
            {
                AnimateContentForBackButton(false);
            }
        }

        private async void AnimateContentForBackButton(bool show)
        {
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

                await ani.RunAsync((Animatable)_windowIconControl, null);

                _navView.IsBackButtonVisible = true;
            }
            else
            {
                _navView.IsBackButtonVisible = false;

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

                await ani.RunAsync((Animatable)_windowIconControl, null);              
            }
        }

        private IControl _windowIconControl;
        private Frame _frameView;
        private NavigationView _navView;
    }
}
