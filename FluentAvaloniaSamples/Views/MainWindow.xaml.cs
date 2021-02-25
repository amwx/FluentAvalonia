using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using IconElement = FluentAvalonia.UI.Controls.IconElement;

namespace FluentAvaloniaSamples.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            navView = this.Find<NavigationView>("NavView");
            _frame = this.Find<Frame>("FrameView");

            navView.BackRequested += NavView_BackRequested;
            navView.ItemInvoked += NavView_ItemInvoked;

            _frame.Navigated += OnFrameNavigated;

            AddNavigationViewMenuItems();

            _pages = new Dictionary<string, IControl>();

            _pages.Add("Home", new HomePage());

            //navView.Content = _pages["Home"];
            _frame.Navigate(_pages["Home"].GetType());
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var pt = e.GetCurrentPoint(this);
            if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
            {
                if (_frame.CanGoBack)
                {
                    _frame.GoBack();
                    e.Handled = true;
                }                    
            }
            else if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
            {
                if (_frame.CanGoForward)
                {
                    _frame.GoForward();
                    e.Handled = true;
                }
            }    
            base.OnPointerReleased(e);
        }


        private void OnFrameNavigated(object sender, FluentAvalonia.UI.Navigation.NavigationEventArgs e)
        {
            //Ensure the selected item matches the page in the Frame Control
            foreach(var item in _pages)
            {
                if (item.Value.GetType() == e.SourcePageType)
                {
                    foreach(var navItem in navView.MenuItems)
                    {
                        if (navItem is NavigationViewItem nvi && nvi.Content.ToString() == item.Key)
                        {
                            navView.SelectedItem = nvi;
                            return;
                        }
                    }
                }
            }
        }

        private void NavView_BackRequested(object sender, NavigationViewBackRequestedEventArgs e)
        {
            _frame?.GoBack();
        }

        private void AddNavigationViewMenuItems()
        {
            List<NavigationViewItemBase> items = new List<NavigationViewItemBase>();

            Dictionary<string, IconElement> itemInfo = new Dictionary<string, IconElement>();
            itemInfo.Add("Home", new SymbolIcon { Symbol = Symbol.Home });
            itemInfo.Add("Theme Manager", new SymbolIcon { Symbol = Symbol.DarkTheme });
            itemInfo.Add("Basic Controls", new SymbolIcon { Symbol = Symbol.RadioButton });
            itemInfo.Add("ContentDialog", new SymbolIcon { Symbol = Symbol.Alert });
            itemInfo.Add("Icons", new SymbolIcon { Symbol = Symbol.Icons });
            itemInfo.Add("NavigationView", new SymbolIcon { Symbol = Symbol.Navigation });
            itemInfo.Add("Color Picker", new SymbolIcon { Symbol = Symbol.ColorBackground });
            itemInfo.Add("Frame", new SymbolIcon { Symbol = Symbol.Document });
            itemInfo.Add("NumberBox", new SymbolIcon { Symbol = Symbol.Calculator });

            for (int i = 0; i < itemInfo.Count; i++)
            {
                var name = itemInfo.Keys.ElementAt(i);
                items.Add(new NavigationViewItem { Content = name, Icon = itemInfo[name] });
            }

            items.Insert(2, new NavigationViewItemSeparator());

            navView.MenuItems = items;
        }

        private void NavView_ItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
        {
            var nviContent = e.InvokedItem.ToString();
            if (_pages.ContainsKey(nviContent))
            {
                //navView.Content = _pages[nviContent];                
                _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
            }
            else
            {
                switch (nviContent)
                {
                    case "Theme Manager":
                        var tm = new ThemeManagerPage();
                        _pages.Add("Theme Manager", tm);
                        //navView.Content = tm;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "Basic Controls":
                        var item = new BasicControls();
                        _pages.Add("Basic Controls", item);
                        //navView.Content = item;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "ContentDialog":
                        var dia = new Dialogs();
                        _pages.Add("ContentDialog", dia);
                        //navView.Content = dia;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "Icons":
                        var ico = new IconsPage();
                        _pages.Add("Icons", ico);
                        //navView.Content = ico;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "NavigationView":
                        var nv = new NavViewPage();
                        _pages.Add("NavigationView", nv);
                        //navView.Content = nv;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "Settings":
                        var st = new SettingsPage();
                        _pages.Add("Settings", st);
                        //navView.Content = st;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "Color Picker":
                        var c = new ColorPickerPage();
                        _pages.Add("Color Picker", c);
                        //navView.Content = c;
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "Frame":
                        var f = new FramePage();
                        _pages.Add("Frame", f);
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                    case "NumberBox":
                        var nb = new NumberBoxPage();
                        _pages.Add("NumberBox", nb);
                        _frame.Navigate(_pages[nviContent].GetType(), null, e.RecommendedNavigationTransitionInfo);
                        break;

                }
            }
            navView.Header = nviContent;
        }
                
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private Dictionary<string, IControl> _pages;
        private NavigationView navView;
        private Frame _frame;
    }
}
