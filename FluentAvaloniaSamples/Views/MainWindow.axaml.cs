using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Navigation;
using FluentAvaloniaSamples.Pages;
using FluentAvaloniaSamples.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using IconElement = FluentAvalonia.UI.Controls.IconElement;

namespace FluentAvaloniaSamples.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
			InitializeComponent();

			DataContext = new MainWindowViewModel();
#if DEBUG
            this.AttachDevTools();
#endif

			navView = this.Find<NavigationView>("NavView");
            _frame = this.Find<Frame>("FrameView");

            navView.BackRequested += NavView_BackRequested;
            navView.ItemInvoked += NavView_ItemInvoked;

            _frame.Navigated += OnFrameNavigated;

            AddNavigationViewMenuItems();

			_frame.Navigate(typeof(HomePage));
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


        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            //Ensure the selected item matches the page in the Frame Control
			if (e.SourcePageType == typeof(SettingsPage))
			{
				navView.SettingsItem.IsSelected = true;
				navView.Header = "Settings";
			}
			else
			{
				foreach (var navItem in navView.MenuItems)
				{
					if (navItem is NavigationViewItem nvi && nvi.Tag is Type t && t == e.SourcePageType)
					{
						navView.SelectedItem = nvi;

						navView.Header = nvi.Content;
						return;
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
			List<NavigationViewItemBase> items = new List<NavigationViewItemBase>
			{
				new NavigationViewItem { Content = "Home", Icon = new SymbolIcon{ Symbol=Symbol.Home }, Tag = typeof(HomePage)},
				new NavigationViewItem { Content = "Themes", Icon = new SymbolIcon{ Symbol=Symbol.DarkTheme }, Tag = typeof(ThemeManagerPage)},
				
				new NavigationViewItemHeader { Content = "Restyled Core Controls" },
				new NavigationViewItem { Content = "Core Controls", Icon = new SymbolIcon{ Symbol=Symbol.Checkmark }, Tag = typeof(BasicControls)},
				
				new NavigationViewItemHeader { Content = "New Controls" },
				new NavigationViewItem { Content = "Basic Controls", Icon = new SymbolIcon{ Symbol=Symbol.Checkmark }, Tag = typeof(NewBasicControlsPage)},
				new NavigationViewItem { Content = "Dialogs & Flyouts", Icon = new SymbolIcon{ Symbol=Symbol.Alert }, Tag = typeof(Dialogs)},
				new NavigationViewItem 
				{ 
					Content = "Icons",
					Icon = new SymbolIcon{ Symbol=Symbol.Icons },
					SelectsOnInvoked = false, 
					MenuItems = new List<NavigationViewItem>
					{
						new NavigationViewItem { Content = "Symbol", Icon = new SymbolIcon{ Symbol=Symbol.Icons }, Tag = typeof(SymbolIconPage)},
						new NavigationViewItem { Content = "Font", Icon = new SymbolIcon{ Symbol=Symbol.Font }, Tag = typeof(FontIconPage)},
						new NavigationViewItem { Content = "Path", Icon = new SymbolIcon{ Symbol=Symbol.ColorLine }, Tag = typeof(PathIconPage)},
						new NavigationViewItem { Content = "Bitmap", Icon = new SymbolIcon{ Symbol=Symbol.Image }, Tag = typeof(BitmapIconPage)},
						new NavigationViewItem { Content = "Image", Icon = new SymbolIcon{ Symbol=Symbol.ImageAltText }, Tag = typeof(ImageIconPage)},
					}
				},
				new NavigationViewItem { Content = "NavigationView", Icon = new SymbolIcon{ Symbol=Symbol.Navigation }, Tag = typeof(NavViewPage)},
				new NavigationViewItem { Content = "Color Picker", Icon = new SymbolIcon{ Symbol=Symbol.ColorBackground }, Tag = typeof(ColorPickerPage)},
				new NavigationViewItem { Content = "Frame", Icon = new SymbolIcon{ Symbol=Symbol.Document }, Tag = typeof(FramePage)},
				new NavigationViewItem { Content = "NumberBox", Icon = new SymbolIcon{ Symbol=Symbol.Calculator }, Tag = typeof(NumberBoxPage)},
				new NavigationViewItem { Content = "InfoBar", Icon = new SymbolIcon{ Symbol=Symbol.Help }, Tag = typeof(InfoBarPage)},
			};

			navView.MenuItems = items;
        }

        private void NavView_ItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
        {
			if (e.InvokedItemContainer is NavigationViewItem nvi && nvi.Tag is Type typ)
			{
				_frame.Navigate(typ, null, e.RecommendedNavigationTransitionInfo);
			}
			else if (e.IsSettingsInvoked)
			{
				_frame.Navigate(typeof(SettingsPage), null, e.RecommendedNavigationTransitionInfo);				
			}
        }
                
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private NavigationView navView;
        private Frame _frame;
    }
}
