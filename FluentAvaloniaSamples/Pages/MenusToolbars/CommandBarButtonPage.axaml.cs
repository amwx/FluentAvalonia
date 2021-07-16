using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages
{
	public partial class CommandBarButtonPage : UserControl
	{
		public CommandBarButtonPage()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
