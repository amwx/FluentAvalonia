using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;
using System;

namespace FluentAvaloniaSamples.Pages
{
	public partial class CommandBarFlyoutPage : UserControl
	{
		public CommandBarFlyoutPage()
		{
			InitializeComponent();

			DataContext = new CommandBarFlyoutPageViewModel();

			this.FindControl<Avalonia.Controls.Button>("myImageButton").AddHandler(PointerReleasedEvent, OnImageButtonPointerReleased, RoutingStrategies.Tunnel);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void MyImageButton_Click(object sender, RoutedEventArgs args)
		{
			ShowMenu(true);
		}

		private void OnImageButtonPointerReleased(object sender, PointerReleasedEventArgs e)
		{
			if (e.InitialPressMouseButton == MouseButton.Right 
				&& e.GetCurrentPoint(sender as IVisual).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased) 
			{
				ShowMenu(false);
				e.Handled = true;
			}
		}

		private void ShowMenu(bool isTransient)
		{
			var flyout = Resources["CommandBarFlyout1"] as CommandBarFlyout;
			flyout.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;

			flyout.ShowAt(this.FindControl<Image>("Image1"));
		}
	}
}
