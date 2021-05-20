using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            			
		}

		//HomePage
		public string Header1 => DescriptionServiceProvider.Instance.GetInfo("Home", "Header1");
		public string SubHeader1 => DescriptionServiceProvider.Instance.GetInfo("Home", "SubHeader1");
		public string Header2 => DescriptionServiceProvider.Instance.GetInfo("Home", "Header2");
		public string SubHeader2 => DescriptionServiceProvider.Instance.GetInfo("Home", "SubHeader2");

		//Themes Page
		public string ThemesHeader => DescriptionServiceProvider.Instance.GetInfo("Themes", "Header");

		//Dialogs page
		public string ContentDialogUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "ContentDialog", "UsageNotes");
		public string PickerFlyoutBaseUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "PickerFlyoutBase", "UsageNotes");
		public string PickerFlyoutPresenterUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "PickerFlyoutPresenter", "UsageNotes");

		public IconsPageViewModel IconsViewModel { get; } = new IconsPageViewModel();
		          
		public int ControlsVersion
		{
			get => _controlsVersion;
			set
			{
				if (this.RaiseAndSetIfChanged(ref _controlsVersion, value))
				{
					AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ControlsVersion = value;
				}
			}
		}

        public void SetLightTheme()
        {
			AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme = "Light";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
				App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdl)
			{
				AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()?.ForceNativeTitleBarToTheme(cdl.MainWindow);
			}

			App.Current.Resources["ControlExampleStrokeColor"] = new SolidColorBrush(Color.Parse("#0F000000"));
			App.Current.Resources["ControlExampleBackgroundFill"] = new SolidColorBrush(Color.Parse("#B3FFFFFF"));
		}

        public void SetDarkTheme()
        {
			AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme = "Dark";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
				App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdl)
			{
				AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()?.ForceNativeTitleBarToTheme(cdl.MainWindow);
			}

			App.Current.Resources["ControlExampleStrokeColor"] = new SolidColorBrush(Color.Parse("#12FFFFFF"));
			App.Current.Resources["ControlExampleBackgroundFill"] = new SolidColorBrush(Color.Parse("#0FFFFFFF"));
		}

        public async void LaunchContentDialog(object param)
        {
            if (param is ContentDialog cd)
            {
                _ = await cd.ShowAsync();
            }
        }

        public async void LaunchContentDialogWithDeferral(object param)
        {
            if (param is ContentDialog cd)
            {
				cd.PrimaryButtonClick += OnPrimaryButtonClick;
                _ = await cd.ShowAsync();
				cd.PrimaryButtonClick -= OnPrimaryButtonClick;
			}
        }

		private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			var def = args.GetDeferral();
			await Task.Delay(3000);
			def.Complete();
		}

		private int _controlsVersion = 2;
    }
}
