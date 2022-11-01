using Avalonia.Controls;
using Avalonia.Logging;
using FluentAvalonia.Interop;
using System;
using FluentAvalonia.Interop.WinRT;
using FluentAvalonia.UI.Media;
using Avalonia.Media;

namespace FluentAvalonia.Styling;

public partial class FluentAvaloniaTheme
{
    private string ResolveWindowsSystemSettings()
    {
        string theme = IsValidRequestedTheme(_requestedTheme) ? _requestedTheme : LightModeString;
        
        IAccessibilitySettings accessibility = null;
        bool isSystemInHighContrast = false;
        try
        {
            accessibility = WinRTInterop.CreateInstance<IAccessibilitySettings>("Windows.UI.ViewManagement.AccessibilitySettings");

            isSystemInHighContrast = accessibility.HighContrast == 1;
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                    .Log("FluentAvaloniaTheme", "Unable to create instance of ComObject IAccessibilitySettings");
        }

        IUISettings3 uiSettings3 = null;
        try
        {
            uiSettings3 = WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                    .Log("FluentAvaloniaTheme", "Unable to create instance of ComObject IUISettings");
        }

        if (PreferSystemTheme)
        {
            if (!isSystemInHighContrast)
            {
                try
                {
                    var background = (Color2)uiSettings3.GetColorValue(UIColorType.Background);
                    var foreground = (Color2)uiSettings3.GetColorValue(UIColorType.Foreground);

                    // There doesn't seem to be a solid way to detect system theme here, so we check if the background
                    // color is darker than the foreground for lightmode
                    bool isDarkMode = background.Lightness < foreground.Lightness;

                    theme = isDarkMode ? DarkModeString : LightModeString;
                }
                catch
                {
                    Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Detecting system theme failed, defaulting to Light mode");

                    theme = RequestedTheme ?? LightModeString;
                }
            }
            else
            {
                theme = HighContrastModeString;
            }
        }

        if (CustomAccentColor != null)
        {
            LoadCustomAccentColor();
        }
        else if (PreferUserAccentColor)
        {
            TryLoadWindowsAccentColor(uiSettings3);
        }
        else
        {
            LoadDefaultAccentColor();
        }

        if (UseSystemFontOnWindows)
        {
            try
            {
                if (OSVersionHelper.IsWindows11())
                {
                    AddOrUpdateSystemResource("ContentControlThemeFontFamily", new FontFamily("Segoe UI Variable Text"));
                }
                else
                {
                    AddOrUpdateSystemResource("ContentControlThemeFontFamily", new FontFamily("Segoe UI"));
                }
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                    .Log("FluentAvaloniaTheme", "Error in detecting Windows system font.");

                AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);
            }
        }

        return theme;
    }

    private void TryLoadHighContrastThemeColors()
    {
        IUISettings settings = null;
        try
        {
            settings = WinRTInterop.CreateInstance<IUISettings>("Windows.UI.ViewManagement.UISettings");
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                .Log("FluentAvaloniaTheme", "Loading high contrast theme resources failed. Unable to create ComObject IUISettings");
            return;
        }

        void TryAddResource(string resKey, UIElementType element)
        {
            try
            {
                var color = (Color)settings.UIElementColor(element);
                (_themeResources.MergedDictionaries[1] as ResourceDictionary)[resKey] = color;
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                .Log("FluentAvaloniaTheme", $"Loading high contrast theme resources failed. Unable to load {resKey} resource");
            }
        }

        TryAddResource("SystemColorWindowTextColor", UIElementType.WindowText);
        TryAddResource("SystemColorGrayTextColor", UIElementType.GrayText);
        TryAddResource("SystemColorButtonFaceColor", UIElementType.ButtonFace);
        TryAddResource("SystemColorWindowColor", UIElementType.Window);
        TryAddResource("SystemColorButtonTextColor", UIElementType.ButtonText);
        TryAddResource("SystemColorHighlightColor", UIElementType.Highlight);
        TryAddResource("SystemColorHighlightTextColor", UIElementType.HighlightText);
        TryAddResource("SystemColorHotlightColor", UIElementType.Hotlight);
    }

    private void TryLoadWindowsAccentColor(IUISettings3 settings3 = null)
    {
        try
        {
            settings3 ??= WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");

            // TODO
            AddOrUpdateSystemResource("SystemAccentColor", (Color)settings3.GetColorValue(UIColorType.Accent));

            AddOrUpdateSystemResource("SystemAccentColorLight1", (Color)settings3.GetColorValue(UIColorType.AccentLight1));
            AddOrUpdateSystemResource("SystemAccentColorLight2", (Color)settings3.GetColorValue(UIColorType.AccentLight2));
            AddOrUpdateSystemResource("SystemAccentColorLight3", (Color)settings3.GetColorValue(UIColorType.AccentLight3));

            AddOrUpdateSystemResource("SystemAccentColorDark1", (Color)settings3.GetColorValue(UIColorType.AccentDark1));
            AddOrUpdateSystemResource("SystemAccentColorDark2", (Color)settings3.GetColorValue(UIColorType.AccentDark2));
            AddOrUpdateSystemResource("SystemAccentColorDark3", (Color)settings3.GetColorValue(UIColorType.AccentDark3));
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                .Log("FluentAvaloniaTheme", "Loading system accent color failed, using fallback (SlateBlue)");

            // We don't know where it failed, so override all
            LoadDefaultAccentColor();
        }
    }




    /// <summary>
    /// On Windows, forces a specific <see cref="Window"/> to the current theme
    /// </summary>
    /// <param name="window">The window to force</param>
    /// <param name="theme">The theme to use, or null to use the current RequestedTheme</param>
    /// <exception cref="ArgumentNullException">If window is null</exception>
    public void ForceWin32WindowToTheme(Window window, string theme = null)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (!OSVersionHelper.IsWindows())
            return;

        try
        {
            if (string.IsNullOrEmpty(theme))
            {
                theme = IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
            }
            else
            {
                theme = IsValidRequestedTheme(theme) ? theme : IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
            }

            Win32Interop.ApplyTheme(window.PlatformImpl.Handle.Handle, theme.Equals(DarkModeString, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to set window to theme.");
        }
    }
}
