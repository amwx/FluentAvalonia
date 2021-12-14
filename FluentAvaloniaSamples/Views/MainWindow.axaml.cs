using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Diagnostics;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.Core.ApplicationModel;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Data;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Navigation;
using FluentAvaloniaSamples.Pages;
using FluentAvaloniaSamples.ViewModels;
//using FluentAvaloniaSamples.Views.Internal;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IconElement = FluentAvalonia.UI.Controls.IconElement;

namespace FluentAvaloniaSamples.Views
{
	public class TestItem
	{
		public TestItem(string h, string g)
		{
			Header = h;
			Group = g;
		}

		public string Header { get; }
		public string Group { get; }

		public override string ToString()
		{
			return $"{Header}, {Group}";
		}
	}

	public class TestGroup : ObservableCollection<TestItem>
	{
		public TestGroup(IEnumerable<TestItem> items, string groupName)
			: base(items)
		{
			GroupName = groupName;
		}

		public string GroupName { get; }
	}

	public class MainWindow : CoreWindow
    {
		public ObservableCollection<TestItem> Items { get; }

		public int SelectedIndex { get; } = 12;

        public MainWindow()
        {
			InitializeComponent();

			DataContext = new MainWindowViewModel();

			//var panel = new StackPanel();
			//panel.AttachedToVisualTree += (s, e) =>
			//{
			//	Debug.WriteLine("SDFSDF ATTACHED");
			//};

#if DEBUG
			this.AttachDevTools();
#endif

			var lv = this.FindControl<ListView>("ListView1");
			Items = new ObservableCollection<TestItem>
			{
				new TestItem("Paul", "Atreides"),
				new TestItem("Leto", "Atreides"),
				new TestItem("Duncan", "Atreides"),
				new TestItem("Gurney", "Atreides"),
				new TestItem("Feyd Rautha", "Harkonnen"),
				new TestItem("Baron", "Harkonnen"),
				new TestItem("Alia", "Abomination"),
				new TestItem("Gaius Helen Moiham", "Reverend Mother"),
				new TestItem("Jessica", "Reverend Mother")
			};

			for (int i = 0; i < 10; i++)
			{
				Items.Add(new TestItem($"Added {i + 1}", "TEST"));
			}
			Items.Add(new TestItem("LastItem", ""));

			//lv.Items = Items;

			PointerPressed += (s, e) =>
			{
				//this.FindControl<ListView>("ListView1").ItemContainerGenerator.ContainerFromIndex(Items.Count - 1)
				//.BringIntoView();
			};

			//this.FindControl<ListBox>("lb1").ItemsPanel =
			//	new FuncTemplate<IPanel>(() => panel);

			var gr = from item in Items
					 group item by item.Group
					 into g
					 select new TestGroup(g, g.Key);

			var obs = new ObservableCollection<TestGroup>(gr);

			var cvs = new CollectionViewSource();
			cvs.Source = obs;
			cvs.IsSourceGrouped = true;

			//this.FindControl<ItemsControl>("IC1").Items = cvs.View;
			lv.Items = cvs.View;


			this.FindControl<Avalonia.Controls.Button>("AddButton").Click += (s, e) =>
			{
				//Items.Insert(1, new TestItem("AddedItem", "Added"));

				//obs[1].Add(new TestItem("Rabban", "Harkonnen"));

				obs.Insert(0, new TestGroup(new[]
				{
					new TestItem("T1", "New Group"),
					new TestItem("T1", "New Group"),
				}, "New Group"));
			};

			this.FindControl<Avalonia.Controls.Button>("RemoveButton").Click += (s, e) =>
			{
				//Items.RemoveAt(4);

				//obs[0].RemoveAt(2);

				obs.RemoveAt(obs.Count - 1);
			};

			this.FindControl<Avalonia.Controls.Button>("ReplaceButton").Click += (s, e) =>
			{
				//Items[2] = new TestItem("Replaced", "R");

				//obs[0][0] = new TestItem("Muad'Dib", "Atreides");

				obs[1]= new TestGroup(new[]
				{
					new TestItem("R1", "Rep Group"),
					new TestItem("R1", "Rep Group"),
				}, "New Group");
			};

			this.FindControl<Avalonia.Controls.Button>("ResetButton").Click += (s, e) =>
			{
				//Items.Clear();

				//obs[2].Clear();

				obs.Clear();
			};

			this.FindControl<Avalonia.Controls.Button>("MoveButton").Click += (s, e) =>
			{
				//Items.Move(Items.Count - 1, 0);

				//obs[3].Move(1, 0);

				obs.Move(1, 0);
			};



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
			Debug.WriteLine(FocusManager.Instance.Current);
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
				new NavigationViewItem {Content = "ListView", Icon = new SymbolIcon {Symbol = Symbol.List }, Tag=typeof(ListViewPage) }
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
