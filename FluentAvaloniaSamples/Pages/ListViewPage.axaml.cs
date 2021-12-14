using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class ListViewPage : UserControl
	{
		public ListViewPage()
		{
			InitializeComponent();

			DataContext = new ListViewPageViewModel();

			_stickyHeadersToggle = this.FindControl<ToggleSwitch>("StickyHeadersToggle");

			_stickyHeadersToggle.Checked += (s, e) =>
			{
				var lv = this.FindControl<ListView>("ListView5");
				if (lv.ItemsPanelRoot is ItemsStackPanel isp)
				{
					isp.AreStickyGroupHeadersEnabled = (DataContext as ListViewPageViewModel).AreStickyHeadersEnabled = true;
				}
			};

			_stickyHeadersToggle.Unchecked += (s, e) =>
			{
				var lv = this.FindControl<ListView>("ListView5");
				if (lv.ItemsPanelRoot is ItemsStackPanel isp)
				{
					isp.AreStickyGroupHeadersEnabled = (DataContext as ListViewPageViewModel).AreStickyHeadersEnabled = false;
				}
			};
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private ToggleSwitch _stickyHeadersToggle;
	}
}
