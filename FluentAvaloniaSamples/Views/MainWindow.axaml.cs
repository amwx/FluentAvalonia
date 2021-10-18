using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Diagnostics;
using Avalonia.Dialogs;
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

			if (navView != null)
			{
				navView.BackRequested += NavView_BackRequested;
				navView.ItemInvoked += NavView_ItemInvoked;

				AddNavigationViewMenuItems();
			}

			if (_frame != null)
			{
				_frame.Navigated += OnFrameNavigated;

				_frame.Navigate(typeof(HomePage));
			}

		}

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);		
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
				var nvi = GetNVIFromPageSourceType(navView.MenuItems, e.SourcePageType);
				if (nvi != null)
				{
					navView.SelectedItem = nvi;

					if (e.SourcePageType == typeof(HomePage))
					{
						navView.AlwaysShowHeader = false;
					}
					else
					{
						navView.AlwaysShowHeader = true;
					}

					navView.Header = nvi.Content;
				}
			}
        }

		private NavigationViewItem GetNVIFromPageSourceType(IEnumerable items, Type t)
		{
			foreach(var item in items)
			{
				if (item is NavigationViewItem nvi)
				{
					if (nvi.MenuItems != null && nvi.MenuItems.Count() > 0)
					{
						var inner = GetNVIFromPageSourceType(nvi.MenuItems, t);
						if (inner == null)
							continue;

						return inner;
					}
					else
					{
						if (nvi.Tag is Type tag && tag == t)
						{
							return nvi;
						}
					}
				}
			}

			return null;
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
				new NavigationViewItem { Content = "How to Use", Icon = new SymbolIcon{ Symbol=Symbol.DarkTheme }, Tag = typeof(ThemeManagerPage)},
				
				new NavigationViewItemHeader { Content = "Restyled Core Controls" },
				new NavigationViewItem { Content = "Core Controls", Icon = new SymbolIcon{ Symbol=Symbol.Checkmark }, Tag = typeof(BasicControls)},
				
				new NavigationViewItemHeader { Content = "New Controls" },
				new NavigationViewItem { Content = "Basic Controls", Icon = new SymbolIcon{ Symbol=Symbol.Checkmark }, Tag = typeof(NewBasicControlsPage)},
				new NavigationViewItem 
				{ 
					Content = "Dialogs and Flyouts", 
					Icon = new SymbolIcon{ Symbol=Symbol.Alert }, 
					SelectsOnInvoked = false,
					MenuItems = new List<NavigationViewItem>
					{
						new NavigationViewItem { Content = "ContentDialog", Icon = new SymbolIcon { Symbol = Symbol.Alert }, Tag = typeof(ContentDialogPage) },
						new NavigationViewItem { Content = "PickerFlyoutBase", Icon = new SymbolIcon { Symbol = Symbol.Comment }, Tag = typeof(PickerFlyoutBasePage) },
					}
				},
				new NavigationViewItem 
				{ 
					Content = "Icons",
					Icon = new SymbolIcon{ Symbol=Symbol.Icons },
					SelectsOnInvoked = false, 
					MenuItems = new List<NavigationViewItem>
					{
						new NavigationViewItem { Content = "SymbolIcon", Icon = new SymbolIcon{ Symbol=Symbol.Icons }, Tag = typeof(SymbolIconPage)},
						new NavigationViewItem { Content = "FontIcon", Icon = new SymbolIcon{ Symbol=Symbol.Font }, Tag = typeof(FontIconPage)},
						new NavigationViewItem { Content = "PathIcon", Icon = new SymbolIcon{ Symbol=Symbol.ColorLine }, Tag = typeof(PathIconPage)},
						new NavigationViewItem { Content = "BitmapIcon", Icon = new SymbolIcon{ Symbol=Symbol.Image }, Tag = typeof(BitmapIconPage)},
						new NavigationViewItem { Content = "ImageIcon", Icon = new SymbolIcon{ Symbol=Symbol.ImageAltText }, Tag = typeof(ImageIconPage)},
					}
				},
				new NavigationViewItem { Content = "NavigationView", Icon = new SymbolIcon{ Symbol=Symbol.Navigation }, Tag = typeof(NavViewPage)},
				new NavigationViewItem 
				{ 
					Content = "Menus and Toolbars", 
					Icon = new SymbolIcon {Symbol = Symbol.Save },
					SelectsOnInvoked = false,
					MenuItems = new List<NavigationViewItem>
					{
						new NavigationViewItem { Content = "XamlUICommand", Icon = new SymbolIcon{ Symbol=Symbol.Icons }, Tag = typeof(XamlUICommandPage)},
						new NavigationViewItem { Content = "StandardUICommand", Icon = new SymbolIcon{ Symbol=Symbol.Font }, Tag = typeof(StandardXamlUICommandPage)},
						new NavigationViewItem { Content = "CommandBarButton", Icon = new SymbolIcon{ Symbol=Symbol.ColorLine }, Tag = typeof(CommandBarButtonPage)},
						new NavigationViewItem { Content = "CommandBarToggleButton", Icon = new SymbolIcon{ Symbol=Symbol.ImageAltText }, Tag = typeof(CommandBarToggleButtonPage)},
						new NavigationViewItem { Content = "CommandBar", Icon = new SymbolIcon{ Symbol=Symbol.Icons }, Tag = typeof(CommandBarPage)},
						new NavigationViewItem { Content = "CommandBarFlyout", Icon = new SymbolIcon{ Symbol=Symbol.Font }, Tag = typeof(CommandBarFlyoutPage)},
						new NavigationViewItem { Content = "MenuFlyout", Icon = new SymbolIcon{ Symbol=Symbol.ColorLine }, Tag = typeof(MenuFlyoutPage)},
					}
				},
				new NavigationViewItem { Content = "Color Picker", Icon = new SymbolIcon{ Symbol=Symbol.ColorBackground }, Tag = typeof(ColorPickerPage)},
				new NavigationViewItem { Content = "Frame", Icon = new SymbolIcon{ Symbol=Symbol.Document }, Tag = typeof(FramePage)},
				new NavigationViewItem { Content = "NumberBox", Icon = new SymbolIcon{ Symbol=Symbol.Calculator }, Tag = typeof(NumberBoxPage)},
				new NavigationViewItem { Content = "InfoBadge", Icon = new SymbolIcon{ Symbol=Symbol.Help }, Tag = typeof(InfoBadgePage)},
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
