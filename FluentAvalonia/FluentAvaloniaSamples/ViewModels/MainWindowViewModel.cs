using Avalonia.Controls;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ThemeManager themeManager;
        public MainWindowViewModel()
        {
            themeManager = new ThemeManager(AppThemeMode.Light, true);

            ThemeManagerDescription = GetAssemblyResource("ThemeManager.txt");
            StylesDescription = GetAssemblyResource("StylesDesc.txt");
        }

        private string GetAssemblyResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string ThemeManagerDescription { get; }
        public string StylesDescription { get; }

        private string _ClickMe;
        public string ButtonClickMeText
        {
            get => _ClickMe;
            set => this.RaiseAndSetIfChanged(ref _ClickMe, value);
        }

        private string _ClickMeAccent;
        public string AccentButtonClickMeText
        {
            get => _ClickMeAccent;
            set => this.RaiseAndSetIfChanged(ref _ClickMeAccent, value);
        }

        private string _ClickMeReveal;
        public string RevealButtonClickMeText
        {
            get => _ClickMeReveal;
            set => this.RaiseAndSetIfChanged(ref _ClickMeReveal, value);
        }


        public void SetLightTheme()
        {
            themeManager.SetTheme(AppThemeMode.Light);
        }

        public void SetDarkTheme()
        {
            themeManager.SetTheme(AppThemeMode.Dark);
        }

        public void SetHCTheme()
        {
            themeManager.SetTheme(AppThemeMode.HighContrast);
        }


        public void OnPreviewButtonClicked()
        {
            ButtonClickMeText = "You clicked me!";
        }
        public void OnPreviewAccentButtonClicked()
        {
            AccentButtonClickMeText = "You clicked me!";
        }
        public void OnPreviewRevealButtonClicked()
        {
            RevealButtonClickMeText = "You clicked me!";
        }


        public async void LaunchContentDialog(object param)
        {
            if (param is ContentDialog cd)
            {
                var res = await cd.ShowAsync();
            }
        }

        public async void LaunchContentDialogWithDeferral(object param)
        {
            if (param is ContentDialog cd)
            {
                cd.PrimaryButtonClick += async (s, v) =>
                {
                    var def = v.GetDeferral();
                    await Task.Delay(3000);
                    def.Complete();
                };


                var res = await cd.ShowAsync();
            }
        }
    }
}
