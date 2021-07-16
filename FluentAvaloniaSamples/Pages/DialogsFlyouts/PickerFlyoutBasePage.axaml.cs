using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages
{
	public partial class PickerFlyoutBasePage : UserControl
	{
		public PickerFlyoutBasePage()
		{
			InitializeComponent();

			DataContext = this;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public string PickerFlyoutBaseUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "PickerFlyoutBase", "UsageNotes");
		public string PickerFlyoutPresenterUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "PickerFlyoutPresenter", "UsageNotes");

	}
}
