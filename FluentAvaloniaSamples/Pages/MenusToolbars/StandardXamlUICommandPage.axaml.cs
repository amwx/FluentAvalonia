using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Input;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class StandardXamlUICommandPage : UserControl
	{
		public StandardXamlUICommandPage()
		{
			InitializeComponent();

			DataContext = new StandardXamlUICommandPageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
