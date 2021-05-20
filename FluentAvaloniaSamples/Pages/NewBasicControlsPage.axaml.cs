using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class NewBasicControlsPage : UserControl
	{
		public NewBasicControlsPage()
		{
			InitializeComponent();
			DataContext = new NewBasicControlsPageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
