using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class CommandBarPage : UserControl
	{
		public CommandBarPage()
		{
			InitializeComponent();

			DataContext = new CommandBarPageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
