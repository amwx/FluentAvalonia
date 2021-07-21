using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Input;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
	public partial class XamlUICommandPage : UserControl
	{
		public XamlUICommandPage()
		{
			InitializeComponent();

			DataContext = new XamlUICommandPageViewModel();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void CustomXamlUICommand_ExecuteRequested(XamlUICommand command, ExecuteRequestedEventArgs args)
		{
			counter++;
			this.FindControl<TextBlock>("XamlUICommandOutput").Text = $"You fired the custom command {counter} times";			
		}

		int counter = 0;
	}
}
