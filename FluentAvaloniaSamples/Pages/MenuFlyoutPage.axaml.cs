using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class MenuFlyoutPage : UserControl
	{
		public MenuFlyoutPage()
		{
			InitializeComponent();

			DataContext = new MenuFlyoutPageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
