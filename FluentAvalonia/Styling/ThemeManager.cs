using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using FluentAvalonia.Interop;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Styling
{
    public class ThemeManager : AvaloniaObject
    {
        public ThemeManager(bool useWinSysAccent, bool useWinDefFont, bool includeWindowsTitleBar)
        {
            UseSegoeUIOnWindows = useWinDefFont;
            UseSystemAccentOnWindows = useWinSysAccent;
            IncludeWindowsTitleBarInThemeChange = includeWindowsTitleBar;

            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (_isWindows)
            {
                Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
                Win32Interop.RtlGetVersion(ref osInfo);

                bool useDark = Win32Interop.GetSystemTheme(osInfo);
                PreferredTheme = useDark ? FluentThemeMode.Dark : FluentThemeMode.Light;
            }
            else
            {
                PreferredTheme = FluentThemeMode.Light;
            }
        }

        public ThemeManager(FluentThemeMode prefMode, bool useWinSysAccent, bool useWinDefFont, 
            bool includeWindowsTitleBar)
        {
            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            UseSystemAccentOnWindows = useWinSysAccent;
            UseSegoeUIOnWindows = useWinDefFont;
            PreferredTheme = prefMode;
            IncludeWindowsTitleBarInThemeChange = includeWindowsTitleBar;
        }


        public FluentThemeMode PreferredTheme { get; set; }

        public bool UseSystemAccentOnWindows { get; set; }

        public bool UseSegoeUIOnWindows { get; set; }

        public bool IncludeWindowsTitleBarInThemeChange { get; set; }

        /// <summary>
        /// Gets/Sets an array of 7 colors representing
        /// SystemAccentColor,Light1,Light2,Light3,Dark1,Dark2,Dark3
        /// Call Refresh() & set UseSystemAccentOnWindows = false to apply
        /// </summary>
        public Color[] CustomAccentColors { get; set; }

        public FontFamily DefaultFontFamily { get; set; }


        public void Refresh()
        {
            var appStyles = Application.Current.Styles;
            for (int i = 0; i < appStyles.Count; i++)
            {
                if (appStyles[i] is FluentTheme ft)
                {
                    ft.Mode = PreferredTheme;
                }
                else if (appStyles[i] is FluentAvaloniaTheme fa)
                {
                    fa.Mode = PreferredTheme;

                    if (UseSystemAccentOnWindows && _isWindows)
                    {
                        SetSystemAccentColorsFromSystem(fa.ThemeStyles.CommonResources);
                    }
                    else if (CustomAccentColors != null && CustomAccentColors.Length == 7)
                    {
                        SetCustomAccentColor(fa.ThemeStyles.CommonResources);
                    }

                    if (UseSegoeUIOnWindows && _isWindows)
                    {
                        fa.ThemeStyles.CommonResources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI");
                    }
                }
                else if (appStyles[i] is ThemeStyles thmSty)
                {
                    thmSty.CurrentTheme = PreferredTheme;

                    if (UseSystemAccentOnWindows && _isWindows)
                    {
                        SetSystemAccentColorsFromSystem(thmSty.CommonResources);
                    }
                    else if (CustomAccentColors != null && CustomAccentColors.Length == 7)
                    {
                        SetCustomAccentColor(thmSty.CommonResources);
                    }

                    if (UseSegoeUIOnWindows && _isWindows)
                    {
                        thmSty.CommonResources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI");
                    }
                }
                else if (appStyles[i] is StyleInclude si)
                {
                    if (si.Loaded != null && si.Loaded is ThemeStyles ts)
                    {
                        ts.CurrentTheme = PreferredTheme == FluentThemeMode.Light ? "Light" : "Dark";

                        if (UseSystemAccentOnWindows && _isWindows)
                        {
                            SetSystemAccentColorsFromSystem(ts.CommonResources);
                        }
                        else if (CustomAccentColors != null && CustomAccentColors.Length == 7)
                        {
                            SetCustomAccentColor(ts.CommonResources);
                        }

                        if (UseSegoeUIOnWindows && _isWindows)
                        {
                            ts.CommonResources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI");
                        }
                    }
                }
            }

            if (IncludeWindowsTitleBarInThemeChange && _isWindows)
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cd)
                {
                    Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
                    Win32Interop.RtlGetVersion(ref osInfo);

                    for (int i = 0; i < cd.Windows.Count; i++)
                    {
                        Win32Interop.ApplyTheme(cd.Windows[i].PlatformImpl.Handle.Handle, PreferredTheme == FluentThemeMode.Dark, osInfo);
                    }
                }
            }
        }


        /// <summary>
        /// Sets the system accent colors from the users settings
        /// </summary>
        private void SetSystemAccentColorsFromSystem(IResourceDictionary resources)
        {
            var themeAccent = Win32Interop.GetThemeColorRef("ImmersiveSystemAccent");
            var themeAccentLight1 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight1");
            var themeAccentLight2 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight2");
            var themeAccentLight3 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight3");
            var themeAccentDark1 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark1");
            var themeAccentDark2 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark2");
            var themeAccentDark3 = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark3");

            resources["SystemAccentColor"] = themeAccent;
            resources["SystemAccentColorLight1"] = themeAccentLight1;
            resources["SystemAccentColorLight2"] = themeAccentLight2;
            resources["SystemAccentColorLight3"] = themeAccentLight3;
            resources["SystemAccentColorDark1"] = themeAccentDark1;
            resources["SystemAccentColorDark2"] = themeAccentDark2;
            resources["SystemAccentColorDark3"] = themeAccentDark3;
        }

        private void SetCustomAccentColor(IResourceDictionary resources)
        {
            resources["SystemAccentColor"] = CustomAccentColors[0];
            resources["SystemAccentColorLight1"] = CustomAccentColors[1];
            resources["SystemAccentColorLight2"] = CustomAccentColors[2];
            resources["SystemAccentColorLight3"] = CustomAccentColors[3];
            resources["SystemAccentColorDark1"] = CustomAccentColors[4];
            resources["SystemAccentColorDark2"] = CustomAccentColors[5];
            resources["SystemAccentColorDark3"] = CustomAccentColors[6];
        }


        private bool _isWindows;
    }

    public enum AppThemeMode
    {
        Unset, //This is only used for initialization
        Light,
        Dark,
        HighContrast
    }
}
