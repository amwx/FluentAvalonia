using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Interop;
using ReactiveUI;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Styling
{
    public class ThemeManager : AvaloniaObject
    {
        /// <summary>
        /// Initializes the ThemeManager, default values (Light theme, use SystemAccentColor on Windows10)
        /// </summary>
        public ThemeManager()
            : this(AppThemeMode.Light, true) { }

        /// <summary>
        /// Initializes the ThemeManager with a specific settings
        /// </summary>
        /// <param name="desiredTheme"></param>
        /// <param name="useSystemAccentOnWindows"></param>
        public ThemeManager(AppThemeMode desiredTheme, bool useSystemAccentOnWindows)
        {
            //Unknown break point? if user heavily modified Win10 look, is DWM still enabled
            //and will the SystemAccentColor still return something?
            //May be worth a call to check if DWM is enabled

            //Set the Default themes
            LightThemeSource = new ThemeStylesLight();
            DarkThemeSource = new ThemeStylesDark();
            HCThemeSource = new ThemeStylesHighContrast();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OSVERSIONINFOEX osInfo = new OSVERSIONINFOEX();
                WindowsVersionInterop.RtlGetVersion(ref osInfo);
                IsOSWindows = true;
                WindowsMajorVersion = osInfo.MajorVersion;
                WindowsBuildVersion = osInfo.BuildNumber;

                if (WindowsMajorVersion >= 10 && useSystemAccentOnWindows)
                {
                    if (useSystemAccentOnWindows)
                    {
                        SetSystemAccentColorsFromSystem();
                        UseSystemAccentColor = true;
                    }
                    else
                        UseSystemAccentColor = false;

                    HandleSymbolFontFallBack();
                }
                else
                {
                    HandleSymbolFontFallBack();
                }  
            }
            else
            {
                IsOSWindows = false;
                UseSystemAccentColor = false;

                HandleSymbolFontFallBack();
            }

            SetTheme(AppThemeMode.Light);
        }

        public bool IsOSWindows { get; private set; }
        public int WindowsMajorVersion { get; private set; }
        public int WindowsBuildVersion { get; private set; }

        /// <summary>
        /// If true, on Windows 10 (and probably 8), the user's system accent
        /// color will be used, and set here. If not on Win10, or false, the
        /// ThemeAccentColor & variants can be set manually. By default, they
        /// are set as SlateBlue
        /// </summary>
        private bool UseSystemAccentColor
        {
            get => _useSystemAccent;
            set
            {
                _useSystemAccent = value;
            }
        }
        //To Do, check fnShouldAppsUseDarkMode & fnShouldSystemUseDarkMode

        /// <summary>
        /// Gets the active theme mode
        /// </summary>
        public AppThemeMode AppTheme => _AppTheme;
                
        /// <summary>
        /// Sets the system accent colors from the users settings
        /// </summary>
        private void SetSystemAccentColorsFromSystem()
        {
            //TO DO ALLOW HIGH CONTRAST MODE
            var themeAccent = Win32Interop.GetThemeColorRef("ImmersiveSystemAccent");
            var themeAccentLight1 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight1");
            var themeAccentLight2 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight2");
            var themeAccentLight3 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight3");
            var themeAccentDark1 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark1");
            var themeAccentDark2 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark2");
            var themeAccentDark3 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark3");

            SetThemeAccentColors(themeAccent, themeAccentLight1, themeAccentLight2, themeAccentLight3,
                themeAccentDark1, themeAccentDark2, themeAccentDark3);

        }

        /// <summary>
        /// Sets the ThemeAccentColors to a user defined color scheme. If on Win10 with UseSystemAccentColor,
        /// this will turn that setting off
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="light1"></param>
        /// <param name="light2"></param>
        /// <param name="light3"></param>
        /// <param name="dark1"></param>
        /// <param name="dark2"></param>
        /// <param name="dark3"></param>
        public void SetThemeAccentColors(Color theme, Color light1, Color light2, Color light3, Color dark1, Color dark2, Color dark3)
        {
            if (UseSystemAccentColor)
                UseSystemAccentColor = false;

            LightThemeSource.Resources["SystemAccentColor"] = theme;
            LightThemeSource.Resources["SystemAccentColorLight1"] = light1;
            LightThemeSource.Resources["SystemAccentColorLight2"] = light2;
            LightThemeSource.Resources["SystemAccentColorLight3"] = light3;
            LightThemeSource.Resources["SystemAccentColorDark1"] = dark1;
            LightThemeSource.Resources["SystemAccentColorDark2"] = dark2;
            LightThemeSource.Resources["SystemAccentColorDark3"] = dark3;

            DarkThemeSource.Resources["SystemAccentColor"] = theme;
            DarkThemeSource.Resources["SystemAccentColorLight1"] = light1;
            DarkThemeSource.Resources["SystemAccentColorLight2"] = light2;
            DarkThemeSource.Resources["SystemAccentColorLight3"] = light3;
            DarkThemeSource.Resources["SystemAccentColorDark1"] = dark1;
            DarkThemeSource.Resources["SystemAccentColorDark2"] = dark2;
            DarkThemeSource.Resources["SystemAccentColorDark3"] = dark3;

            HCThemeSource.Resources["SystemAccentColor"] = theme;
            HCThemeSource.Resources["SystemAccentColorLight1"] = light1;
            HCThemeSource.Resources["SystemAccentColorLight2"] = light2;
            HCThemeSource.Resources["SystemAccentColorLight3"] = light3;
            HCThemeSource.Resources["SystemAccentColorDark1"] = dark1;
            HCThemeSource.Resources["SystemAccentColorDark2"] = dark2;
            HCThemeSource.Resources["SystemAccentColorDark3"] = dark3;
        }

        /// <summary>
        /// Sets the desired AppThemeMode
        /// </summary>
        /// <param name="mode"></param>
        public void SetTheme(AppThemeMode mode)
        {
            if (AppTheme == mode)
                return;
            if (mode == AppThemeMode.Unset)
                throw new InvalidOperationException("Invalid AppThemeMode");

            //Binding Errors are thrown when switching themes, 
            //don't know how to avoid that

            if (_AppTheme == AppThemeMode.Unset)
            {
                Application.Current.Styles.Add(mode == AppThemeMode.Light ? LightThemeSource :
                    mode == AppThemeMode.Dark ? DarkThemeSource : HCThemeSource);
                _AppTheme = mode;
            }
            else
            {
                Application.Current.Styles[^1] = mode == AppThemeMode.Light ? LightThemeSource :
                    mode == AppThemeMode.Dark ? DarkThemeSource : HCThemeSource;

                _AppTheme = mode;
            }

        }

        /// <summary>
        /// Sets the Source for the LightTheme resources & styles
        /// </summary>
        /// <param name="newStyle"></param>
        public void SetLightThemeSource(Styles newStyle)
        {
            LightThemeSource = newStyle;
        }

        /// <summary>
        /// Sets the Source for the DarkTheme resources & styles
        /// </summary>
        /// <param name="newStyle"></param>
        public void SetDarkThemeSource(Styles newStyle)
        {
            DarkThemeSource = newStyle;
        }

        /// <summary>
        /// Sets the Source for the HighContrastTheme resources & styles
        /// </summary>
        /// <param name="newStyle"></param>
        public void SetHCThemeSource(Styles newStyle)
        {
            HCThemeSource = newStyle;
        }

       
        private void HandleSymbolFontFallBack()
        {
            //Since MS Fonts can't be redistributed, per licence terms, we need to
            //Provide opportunity to fall back to not Segoe MDL2 Assets on non-Win10
            //This is the open source MS symbol font made for web use, not sure how
            //closely this matches, it appears to be the replacment/alternative
            //Just check the App.xaml file to find the StyleInclude referencing 
            //ControlStyles.xaml & replace the Resource
            if (FontManager.Current.GetInstalledFontFamilyNames().Contains("Segoe MDL2 Assets"))
            {
                foreach (var item in Application.Current.Styles)
                {
                    if (item is StyleInclude si && si.Source.AbsolutePath.Contains("ControlStyles.xaml"))
                    {
                        Styles cstyles = si.Loaded as Styles;
                        cstyles.Resources["SymbolThemeFontFamily"] = new FontFamily(new Uri("resm:FluentAvalonia.Fonts.?assembly=FluentAvalonia#winjs-symbols"), "winjs-symbols");
                        var res = cstyles.Resources["SymbolThemeFontFamily"];
                    }
                }
            }
        }


        /// <summary>
        /// Gets the source for LightThemeStyles. To set, call SetLightThemeSource
        /// </summary>
        public Styles LightThemeSource { get; private set; }

        /// <summary>
        /// Gets the source for DarkThemeStyles. To set, call SetDarkThemeSource
        /// </summary>
        public Styles DarkThemeSource { get; private set; }

        /// <summary>
        /// Gets the source for HighConstrastStyles. To set, call SetHCThemeSource
        /// </summary>
        public Styles HCThemeSource { get; private set; }

        private bool _useSystemAccent;
        private AppThemeMode _AppTheme;
    }

    public enum AppThemeMode
    {
        Unset, //This is only used for initialization
        Light,
        Dark,
        HighContrast
    }
}
