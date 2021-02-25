using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ThemeManager themeManager;
        public MainWindowViewModel()
        {
            themeManager = new ThemeManager(true, true, true);
            themeManager.Refresh();

            ThemeManagerDescription = GetAssemblyResource("ThemeManager.txt");

            HowToUseText = GetAssemblyResource("FluentAvaloniaInfo2.txt");
        }


        public string ThemeManagerDescription { get; }
        
        public string HowToUseText { get; }

               
        public void SetLightTheme()
        {
            themeManager.PreferredTheme = Avalonia.Themes.Fluent.FluentThemeMode.Light;
            themeManager.Refresh();
        }

        public void SetDarkTheme()
        {
            themeManager.PreferredTheme = Avalonia.Themes.Fluent.FluentThemeMode.Dark;
            themeManager.Refresh();
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
