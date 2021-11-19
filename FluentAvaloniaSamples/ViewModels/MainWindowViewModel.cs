using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAvaloniaSamples.Pages.DialogsFlyouts;

namespace FluentAvaloniaSamples.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
		{
			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
			using (var stream = assets.Open(new Uri("avares://FluentAvaloniaSamples/DescriptionTexts/WhatsNew.txt")))
			using (StreamReader reader = new StreamReader(stream))
			{
				var txt = reader.ReadToEnd();
				var infos = JsonSerializer.Deserialize<List<WhatsNewInfo>>(txt);// reader.ReadToEnd());

				WhatsNewVersions = infos;
			}

			SelectedWhatsNew = WhatsNewVersions[0];
		}

		// TODO: Check that all this works in the next version release
		public List<WhatsNewInfo> WhatsNewVersions { get; set; }

		public WhatsNewInfo SelectedWhatsNew
		{
			get => _selectedWhatsNew;
			set
			{
				if (RaiseAndSetIfChanged(ref _selectedWhatsNew, value))
				{
					if (UpdateInfos == null)
					{
						UpdateInfos = new AvaloniaList<ItemUpdateDescription>();
						RaisePropertyChanged("UpdateInfos");
					}
					else
					{
						UpdateInfos.Clear();
					}

					UpdateInfos.AddRange(value.Changes);
				}
			}
		}

		public AvaloniaList<ItemUpdateDescription> UpdateInfos { get; set; }

		//HomePage
		public string Header1 => DescriptionServiceProvider.Instance.GetInfo("Home", "Header1");
		public string SubHeader1 => DescriptionServiceProvider.Instance.GetInfo("Home", "SubHeader1");
		public string Header2 => DescriptionServiceProvider.Instance.GetInfo("Home", "Header2");
		public string SubHeader2 => DescriptionServiceProvider.Instance.GetInfo("Home", "SubHeader2");

		//Themes Page
		public string ThemesHeader => DescriptionServiceProvider.Instance.GetInfo("Themes", "Header");

		//Dialogs page
		public string ContentDialogUsageNotes => DescriptionServiceProvider.Instance.GetInfo("DialogFlyouts", "ContentDialog", "UsageNotes");
		
		public IconsPageViewModel IconsViewModel { get; } = new IconsPageViewModel();

		public string CommandBarButtonPageHeader => DescriptionServiceProvider.Instance.GetInfo("CommandBarButton", "Header");
		public string CommandBarToggleButtonPageHeader => DescriptionServiceProvider.Instance.GetInfo("CommandBarToggleButton", "Header");

		public int ControlsVersion
		{
			get => _controlsVersion;
			set
			{
				if (RaiseAndSetIfChanged(ref _controlsVersion, value))
				{
					AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ControlsVersion = value;
				}
			}
		}

		public bool UseCustomAccent
		{
			get => _useCustomAccentColor;
			set
			{
				if (RaiseAndSetIfChanged(ref _useCustomAccentColor, value))
				{
					if (value)
					{

						CustomAccentColor = Colors.SlateBlue;
						AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = CustomAccentColor;
					}
					else
					{
						CustomAccentColor = default;
						AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = null;
					}
				}
			}
		}

		public Color CustomAccentColor
		{
			get => _customAccentColor;
			set
			{
				if (RaiseAndSetIfChanged(ref _customAccentColor, value))
				{
					AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = value;
				}
			}
		}

        public void SetLightTheme()
        {
			// Only line needed to switch to Light Theme
			AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme = "Light";

			// Optional, if you want the native win32 titlebar to switch themes too (1809+ only)
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
				App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdl)
			{
				AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()?.ForceNativeTitleBarToTheme(cdl.MainWindow);
			}

			// Ignore these, these are for the SampleApp only
			App.Current.Resources["ControlExampleStrokeColor"] = new SolidColorBrush(Color.Parse("#0F000000"));
			App.Current.Resources["ControlExampleBackgroundFill"] = new SolidColorBrush(Color.Parse("#F3F3F3"));
			App.Current.Resources["ControlExampleBackgroundFill2"] = new SolidColorBrush(Color.Parse("#F9F9F9"));
		}

        public void SetDarkTheme()
        {
			// Only line needed to switch to Dark Theme
			AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme = "Dark";

			// Optional, if you want the native win32 titlebar to switch themes too (1809+ only)
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
				App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdl)
			{
				AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()?.ForceNativeTitleBarToTheme(cdl.MainWindow);
			}

			// Ignore these, these are for the SampleApp only
			App.Current.Resources["ControlExampleStrokeColor"] = new SolidColorBrush(Color.Parse("#12FFFFFF"));
			App.Current.Resources["ControlExampleBackgroundFill"] = new SolidColorBrush(Color.Parse("#202020"));
			App.Current.Resources["ControlExampleBackgroundFill2"] = new SolidColorBrush(Color.Parse("#292929"));
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

        public async void ShowInputDialogAsync()
        {
	        var dialog = new ContentDialog()
	        {
		        Title = "Let's go ...",
		        PrimaryButtonText = "Ok :-)", SecondaryButtonText = "Not OK :-(", CloseButtonText = "Leave me alone!"
	        };

	        var viewModel = new ContentDialogViewModel(dialog);
	        dialog.Content = new ContentDialogInputExample()
	        {
		        DataContext = viewModel
	        };

	        _ = await dialog.ShowAsync();
        }
        
		private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			var def = args.GetDeferral();
			await Task.Delay(3000);
			def.Complete();
		}

		public void OnXamlCommandExecuted(object param)
		{
			Debug.WriteLine($"HEY IT WORKS!!!! {param}");
		}

		private int _controlsVersion = 2;
		private WhatsNewInfo _selectedWhatsNew;
		private bool _useCustomAccentColor;
		private Color _customAccentColor;
    }

	public class WhatsNewInfo
	{
		public string Version { get; set; }
		public List<ItemUpdateDescription> Changes { get;set; }
	}

	public class ItemUpdateDescription
	{
		public string Header { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public string PageType { get; set; }

		// Only uses V2 Styles
		// This will be obsolete once V1 styles are deprecated
		public bool V2Only { get; set; }

		// Is this a new control added?
		public bool NewControl { get; set; }

		// Or is this a new feature to an existing control
		public bool NewFeature { get; set; }

		// Or is this just a style update
		public bool StyleOnly { get; set; }
	}
}
