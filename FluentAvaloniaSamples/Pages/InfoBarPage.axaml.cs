using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;
using System;

namespace FluentAvaloniaSamples.Pages
{
	public partial class InfoBarPage : UserControl
	{
		public InfoBarPage()
		{
			InitializeComponent();
			DataContext = new InfoBarPageViewModel { Owner = this };

			_apiInActionBar = this.FindControl<InfoBar>("Bar4");

			var cb = this.Find<Avalonia.Controls.ComboBox>("Bar4IconType");
			cb.SelectionChanged += (s, e) =>
			{
				if (s is Avalonia.Controls.ComboBox c)
				{
					if (c.SelectedIndex == 0)
					{
						_apiInActionBar.IconSource = null;
					}
					else
					{
						_apiInActionBar.IconSource = new SymbolIconSource { Symbol = Symbol.MapPin };
					}
				}
			};

			var cb2 = this.Find<Avalonia.Controls.ComboBox>("Bar4ButtonType");
			cb2.SelectionChanged += (s, e) =>
			{
				if (s is Avalonia.Controls.ComboBox c)
				{
					if (c.SelectedIndex == 0)
					{
						_apiInActionBar.ActionButton = null;
					}
					else if (c.SelectedIndex == 1)
					{
						_apiInActionBar.ActionButton = new Avalonia.Controls.Button { Content = "Button" };
					}
					else if (c.SelectedIndex == 2)
					{
						_apiInActionBar.ActionButton = new HyperlinkButton { Content = "Hyperlink" };
					}
					else if (c.SelectedIndex == 3)
					{
						_apiInActionBar.ActionButton = new Avalonia.Controls.Button 
						{ Content = "Button", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
					}
					else if (c.SelectedIndex == 4)
					{
						_apiInActionBar.ActionButton = new HyperlinkButton
						{ Content = "Hyperlink", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
					}
				}
			};
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		internal void SetButton(int type)
		{
			var bar2 = this.FindControl<InfoBar>("Bar2");
			if (type == 0)
			{
				bar2.ActionButton = null;
			}
			else if (type == 1)
			{
				bar2.ActionButton = new FluentAvalonia.UI.Controls.Button { Content = "Action" };
			}
			else if (type == 2)
			{
				bar2.ActionButton = new HyperlinkButton { Content = "Informational Link", NavigateUri = new Uri("https://github.com/amwx/FluentAvalonia") };
			}
		}

		bool hasCustomBG = false;
		public void SetCustomBackground()
		{
			if (hasCustomBG)
				_apiInActionBar.Background = Brushes.Transparent;
			else
				_apiInActionBar.Background = Brushes.Purple;

			hasCustomBG = !hasCustomBG;
		}

		public void SetForeground()
		{
			_apiInActionBar.Foreground = Brushes.Red;
		}

		private InfoBar _apiInActionBar;
	}
}
