using Avalonia.Controls;
using Avalonia.Logging;
using FluentAvalonia.Interop;
using FluentAvalonia.Interop.WinRT;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace FluentAvalonia.Styling;

public partial class FluentAvaloniaTheme
{
    private ThemeVariant ResolveWindowsSystemSettings(IPlatformSettings platformSettings)
    {
        ThemeVariant theme = null;
        if (PreferSystemTheme)
        {
            theme = GetThemeFromIPlatformSettings(platformSettings);
        }

        if (CustomAccentColor != null)
        {
            LoadCustomAccentColor();
        }
        else if (PreferUserAccentColor)
        {
            try
            {
                TryLoadWindowsAccentColor();
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to create instance of ComObject IUISettings");
                LoadDefaultAccentColor();
            }            
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
        IUISettings settings;
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
                var res = Resources.MergedDictionaries[0] as ResourceDictionary;
                (res.ThemeDictionaries[HighContrastTheme] as ResourceDictionary)[resKey] = color;
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

    private void TryLoadWindowsAccentColor()
    {
        try
        {
            var settings3 = WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");

            UpdateAccentColors((Color)settings3.GetColorValue(UIColorType.Accent),
                (Color)settings3.GetColorValue(UIColorType.AccentLight1),
                (Color)settings3.GetColorValue(UIColorType.AccentLight2),
                (Color)settings3.GetColorValue(UIColorType.AccentLight3),
                (Color)settings3.GetColorValue(UIColorType.AccentDark1),
                (Color)settings3.GetColorValue(UIColorType.AccentDark2),
                (Color)settings3.GetColorValue(UIColorType.AccentDark3));
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
    public void ForceWin32WindowToTheme(Window window, ThemeVariant theme = null)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            Win32Interop.ApplyTheme(window.TryGetPlatformHandle().Handle, theme == ThemeVariant.Dark);
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to set window to theme.");
        }
    }
}
