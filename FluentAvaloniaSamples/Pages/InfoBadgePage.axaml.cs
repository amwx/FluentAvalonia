using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class InfoBadgePage : UserControl
	{
		public InfoBadgePage()
		{
			InitializeComponent();

			DataContext = new InfoBadgePageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
