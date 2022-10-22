using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.Interop;
using FluentAvalonia.Interop.WinRT;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FluentAvalonia.Styling;

public class FluentAvaloniaTheme : IStyle, IResourceProvider
{
    public FluentAvaloniaTheme(Uri baseUri)
    {
        _baseUri = baseUri;
    }

    public FluentAvaloniaTheme(IServiceProvider serviceProvider)
    {
        _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
    }

    /// <summary>
    /// Gets or sets the desired theme mode (Light, Dark, or HighContrast) for the app
    /// </summary>
    /// <remarks>
    /// If <see cref="PreferSystemTheme"/> is set to true, on startup this value will
    /// be overwritten with the system theme unless the attempt to read from the system
    /// fails, in which case setting this can provide a fallback.
    /// </remarks>
    public string RequestedTheme
    {
        get => _requestedTheme;
        set
        {
            if (_hasLoaded)
                Refresh(value);
            else
                _requestedTheme = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the system font should be used on Windows. Value only applies at startup
    /// </summary>
    /// <remarks>
    /// On Windows 10, this is "Segoe UI", and Windows 11, this is "Segoe UI Variable Text".
    /// </remarks>
    public bool UseSystemFontOnWindows { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use the current system theme (light or dark mode).
    /// </summary>
    /// <remarks>
    /// This property is respected on Windows, MacOS, and Linux. However, on linux,
    /// it requires 'gtk-theme' setting to work and the current theme to be appended
    /// with '-dark'.
    /// Also note, that high contrast theme will only resolve here on Windows.
    /// </remarks>
    public bool PreferSystemTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use the current user's accent color as the resource SystemAccentColor
    /// </summary>
    /// <remarks>
    /// This property has no effect on Linux
    /// </remarks>
    public bool PreferUserAccentColor { get; set; } = true;

    /// <summary>
    /// Gets or sets a <see cref="Color"/> to use as the SystemAccentColor for the app. Note this takes precedence over the
    /// <see cref="UseUserAccentColorOnWindows"/> property and must be set to null to restore the system color, if desired
    /// </summary>
    /// <remarks>
    /// The 6 variants (3 light/3 dark) are pregenerated from the given color. FluentAvalonia makes no checks to ensure the legibility and
    /// accessibility of the chosen color and places that responsibility upon you. For more control over the accent color variants, directly
    /// override SystemAccentColor or the variants in the Application level resource dictionary.
    /// </remarks>
    public Color? CustomAccentColor
    {
        get => _customAccentColor;
        set
        {
            if (_customAccentColor != value)
            {
                _customAccentColor = value;
                if (_hasLoaded)
                {
                    LoadCustomAccentColor();
                }
            }
        }
    }

    public IResourceHost Owner
    {
        get => _owner;
        set
        {
            if (_owner != value)
            {
                _owner = value;
                OwnerChanged?.Invoke(this, EventArgs.Empty);

                if (!_hasLoaded)
                {
                    Init();
                }
            }
        }
    }

    bool IResourceNode.HasResources => true;

    public IReadOnlyList<IStyle> Children => _styles;

    public event EventHandler OwnerChanged;
    public event TypedEventHandler<FluentAvaloniaTheme, RequestedThemeChangedEventArgs> RequestedThemeChanged;

    public SelectorMatchResult TryAttach(IStyleable target, object host)
    {
        return _styleCache.TryAttach(_styles, target, host);
    }

    public bool TryGetResource(object key, out object value)
    {
        // Github build failing with this not being set, even tho it passes locally
        value = null;

        // We also search the app level resources so resources can be overridden.
        // Do not search App level styles though as we'll have to iterate over them
        // to skip the FluentAvaloniaTheme instance or we'll stack overflow
        if (Application.Current?.Resources.TryGetResource(key, out value) == true)
            return true;

        // This checks the actual ResourceDictionary where the SystemResources are stored
        // and checks the merged dictionaries where base resources and theme resources are
        if (_themeResources.TryGetResource(key, out value))
            return true;

        if (_styles.TryGetResource(key, out value))
            return true;

        value = null;
        return false;
    }

    void IResourceProvider.AddOwner(IResourceHost owner)
    {
        owner = owner ?? throw new ArgumentNullException(nameof(owner));

        if (Owner != null)
            throw new InvalidOperationException("An owner already exists");

        Owner = owner;

        (_themeResources as IResourceProvider).AddOwner(owner);

        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is IResourceProvider rp)
            {
                rp.AddOwner(owner);
            }
        }
    }

    void IResourceProvider.RemoveOwner(IResourceHost owner)
    {
        owner = owner ?? throw new ArgumentNullException(nameof(owner));

        if (Owner == owner)
        {
            Owner = null;

            (_themeResources as IResourceProvider).RemoveOwner(owner);

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is IResourceProvider rp)
                {
                    rp.RemoveOwner(owner);
                }
            }
        }
    }

    /// <summary>
    /// Call this method if you monitor for notifications from the OS that theme colors have changed. This can be
    /// SystemAccentColor or Light/Dark/HighContrast theme. This method only works for AccentColors if
    /// <see cref="UseUserAccentColorOnWindows"/> is true, and for app theme if <see cref="UseSystemThemeOnWindows"/> is true
    /// </summary>
    public void InvalidateThemingFromSystemThemeChanged()
    {
        if (PreferUserAccentColor)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    IUISettings3 settings = WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");

                    TryLoadWindowsAccentColor(settings);
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                TryLoadMacOSAccentColor();
            }
        }

        if (PreferSystemTheme)
        {
            Refresh(null);
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

        try
        {
            Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
            Win32Interop.RtlGetVersion(ref osInfo);

            if (string.IsNullOrEmpty(theme))
            {
                theme = IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
            }
            else
            {
                theme = IsValidRequestedTheme(theme) ? theme : IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
            }

            Win32Interop.ApplyTheme(window.PlatformImpl.Handle.Handle, theme.Equals(DarkModeString, StringComparison.OrdinalIgnoreCase), osInfo);
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to set window to theme.");
        }
    }

    private bool IsValidRequestedTheme(string thm)
    {
        if (LightModeString.Equals(thm, StringComparison.OrdinalIgnoreCase) ||
            DarkModeString.Equals(thm, StringComparison.OrdinalIgnoreCase) ||
            HighContrastModeString.Equals(thm, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private void Init()
    {
        AvaloniaLocator.CurrentMutable.Bind<FluentAvaloniaTheme>().ToConstant(this);

        // First load our base and theme resources

        // When initializing, UseSystemTheme overrides any setting of RequestedTheme, this must be
        // explicitly disabled to enable setting the theme manually
        _requestedTheme = ResolveThemeAndInitializeSystemResources();

        // Base resources
        _themeResources.MergedDictionaries.Add(
            (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/StylesV2/BaseResources.axaml"), _baseUri));

        // Theme resource colors/brushes
        _themeResources.MergedDictionaries.Add(
            (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/{_requestedTheme}Resources.axaml"), _baseUri));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && string.Equals(_requestedTheme, HighContrastModeString, StringComparison.OrdinalIgnoreCase))
        {
            TryLoadHighContrastThemeColors();
        }

        // Load the controls
        _styles = (Styles)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/ControlThemes/Controls.axaml"), _baseUri);

        _hasLoaded = true;
    }

    private void Refresh(string newTheme)
    {
        newTheme ??= ResolveThemeAndInitializeSystemResources();

        var old = _requestedTheme;
        if (!string.Equals(newTheme, old, StringComparison.OrdinalIgnoreCase))
        {
            _requestedTheme = newTheme;

            // Remove the old theme of any resources
            if (_themeResources.Count > 0)
            {
                _themeResources.MergedDictionaries.RemoveAt(1);
            }

            _themeResources.MergedDictionaries.Add(
                (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/{_requestedTheme}Resources.axaml"), _baseUri));

            if (string.Equals(_requestedTheme, HighContrastModeString, StringComparison.OrdinalIgnoreCase))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    TryLoadHighContrastThemeColors();
                }
            }

            Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);

            RequestedThemeChanged?.Invoke(this, new RequestedThemeChangedEventArgs(_requestedTheme));
        }
    }

    private string ResolveThemeAndInitializeSystemResources()
    {
        string theme = IsValidRequestedTheme(_requestedTheme) ? _requestedTheme : LightModeString;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            theme = ResolveWindowsSystemSettings();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            theme = ResolveLinuxSystemSettings();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            theme = ResolveMacOSSystemSettings();
        }

        // Load the SymbolThemeFontFamily
        AddOrUpdateSystemResource("SymbolThemeFontFamily", new FontFamily(new Uri("avares://FluentAvalonia"), "/Fonts/#Symbols"));

        return theme;
    }

    private string ResolveWindowsSystemSettings()
    {
        string theme = IsValidRequestedTheme(_requestedTheme) ? _requestedTheme : LightModeString;
        IAccessibilitySettings accessibility = null;
        try
        {
            accessibility = WinRTInterop.CreateInstance<IAccessibilitySettings>("Windows.UI.ViewManagement.AccessibilitySettings");
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
            bool isHighContrast = false;
            try
            {
                int isUsingHighContrast = accessibility.HighContrast;
                if (isUsingHighContrast == 1)
                {
                    theme = HighContrastModeString;
                    isHighContrast = true;
                }
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                    .Log("FluentAvaloniaTheme", "Unable to retrieve High contrast settings.");
            }

            if (!isHighContrast)
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
                }
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
                Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
                Win32Interop.RtlGetVersion(ref osInfo);

                if (osInfo.BuildNumber >= 22000) // Windows 11
                {
                    AddOrUpdateSystemResource("ContentControlThemeFontFamily", new FontFamily("Segoe UI Variable Text"));
                }
                else // Windows 10
                {
                    //This is defined in the BaseResources.axaml file
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

    private string ResolveMacOSSystemSettings()
    {
        string theme = IsValidRequestedTheme(_requestedTheme) ? _requestedTheme : LightModeString;
        if (PreferSystemTheme)
        {
            try
            {
                // https://stackoverflow.com/questions/25207077/how-to-detect-if-os-x-is-in-dark-mode
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        FileName = "defaults",
                        Arguments = "read -g AppleInterfaceStyle"
                    },
                };

                p.Start();
                var str = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();

                theme = str.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? DarkModeString : LightModeString;
            }
            catch { }
        }

        if (CustomAccentColor != null)
        {
            LoadCustomAccentColor();
        }
        else if (PreferUserAccentColor)
        {
            TryLoadMacOSAccentColor();
        }
        else
        {
            LoadDefaultAccentColor();
        }

        AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);

        return theme;
    }

    private string ResolveLinuxSystemSettings()
    {
        string theme = IsValidRequestedTheme(_requestedTheme) ? _requestedTheme : LightModeString;
        string themeName = null;
        if (PreferSystemTheme)
        {
            try
            {
                if (_linuxDesktopEnvironment.Contains("KDE"))
                {
                    if (File.Exists(_kdeGlobalsFile))
                    {
                        var kdeGlobals = File.ReadAllText(_kdeGlobalsFile);
                        var match = new Regex("^ColorScheme=(.*)$", RegexOptions.Multiline).Match(kdeGlobals);
                        if (match.Success)
                        {
                            themeName = match.Groups[1].Value;
                        }
                    }
                }
                else if (_linuxDesktopEnvironment.Contains("GNOME"))
                {
                    var color = ReadGsettingsKey("org.gnome.desktop.interface", "color-scheme");
                    if (color != null && color != "default")
                    {
                        theme = color == "prefer-light" ? LightModeString : DarkModeString;
                    }
                    else
                    {
                        themeName = ReadGsettingsKey("org.gnome.desktop.interface", "gtk-theme");
                    }
                }
                else if (_linuxDesktopEnvironment.Contains("Cinnamon"))
                {
                    themeName = ReadGsettingsKey("org.cinnamon.desktop.interface", "gtk-theme");
                }
                else if (_linuxDesktopEnvironment.Contains("LXDE"))
                {
                    var lxdeConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lxsession/LXDE/desktop.conf");
                    if (File.Exists(lxdeConfigFile))
                    {
                        var lxdeConfig = File.ReadAllText(lxdeConfigFile);
                        var match = new Regex("^sNet\\/ThemeName=(.*)$", RegexOptions.Multiline).Match(lxdeConfig);
                        if (match.Success)
                        {
                            themeName = match.Groups[1].Value;
                        }
                    }
                }

            }
            catch { }

            if (themeName != null)
            {
                if (themeName.IndexOf("dark", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    theme = DarkModeString;
                }
                else
                {
                    theme = LightModeString;
                }
            }
        }

        if (CustomAccentColor != null)
        {
            LoadCustomAccentColor();
        }
        else if (PreferUserAccentColor)
        {
            TryLoadLinuxAccentColor();
        }
        else
        {
            LoadDefaultAccentColor();
        }

        AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);

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

    private void LoadCustomAccentColor()
    {
        if (!_customAccentColor.HasValue)
        {
            if (PreferUserAccentColor)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        var settings = WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");
                        TryLoadWindowsAccentColor(settings);
                    }
                    catch
                    {
                        LoadDefaultAccentColor();
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    TryLoadMacOSAccentColor();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    TryLoadLinuxAccentColor();
                }
                else
                {
                    LoadDefaultAccentColor();
                }

            }
            else
            {
                LoadDefaultAccentColor();
            }

            return;
        }

        Color2 col = _customAccentColor.Value;
        AddOrUpdateSystemResource("SystemAccentColor", (Color)_customAccentColor.Value);

        AddOrUpdateSystemResource("SystemAccentColorLight1", (Color)col.LightenPercent(0.15f));
        AddOrUpdateSystemResource("SystemAccentColorLight2", (Color)col.LightenPercent(0.30f));
        AddOrUpdateSystemResource("SystemAccentColorLight3", (Color)col.LightenPercent(0.45f));

        AddOrUpdateSystemResource("SystemAccentColorDark1", (Color)col.LightenPercent(-0.15f));
        AddOrUpdateSystemResource("SystemAccentColorDark2", (Color)col.LightenPercent(-0.30f));
        AddOrUpdateSystemResource("SystemAccentColorDark3", (Color)col.LightenPercent(-0.45f));

    }

    private void TryLoadWindowsAccentColor(IUISettings3 settings3)
    {
        try
        {
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
            AddOrUpdateSystemResource("SystemAccentColor", Colors.SlateBlue);

            AddOrUpdateSystemResource("SystemAccentColorLight1", Color.Parse("#7F69FF"));
            AddOrUpdateSystemResource("SystemAccentColorLight2", Color.Parse("#9B8AFF"));
            AddOrUpdateSystemResource("SystemAccentColorLight3", Color.Parse("#B9ADFF"));

            AddOrUpdateSystemResource("SystemAccentColorDark1", Color.Parse("#43339C"));
            AddOrUpdateSystemResource("SystemAccentColorDark2", Color.Parse("#33238C"));
            AddOrUpdateSystemResource("SystemAccentColorDark3", Color.Parse("#1D115C"));
        }
    }

    private void TryLoadMacOSAccentColor()
    {
        try
        {
            // https://stackoverflow.com/questions/51695755/how-can-i-determine-the-macos-10-14-accent-color/51695756#51695756
            // The following assumes MacOS 11 (Big Sur) for color matching
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = "defaults",
                    Arguments = "read -g AppleAccentColor"
                },
            };

            p.Start();
            var str = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();

            if (str.StartsWith("\'"))
            {
                str = str.Substring(1, str.Length - 2);
            }

            int accentColor = int.MinValue;
            int.TryParse(str, out accentColor);

            Color2 aColor;
            switch (accentColor)
            {
                case 0: // red
                    aColor = Color2.FromRGB(255, 82, 89);
                    break;

                case 1: // orange
                    aColor = Color2.FromRGB(248, 131, 28);
                    break;

                case 2: // yellow
                    aColor = Color2.FromRGB(253, 186, 45);
                    break;

                case 3: // green
                    aColor = Color2.FromRGB(99, 186, 71);
                    break;

                case 5: //purple
                    aColor = Color2.FromRGB(164, 82, 167);
                    break;

                case 6: // pink
                    aColor = Color2.FromRGB(248, 80, 158);
                    break;

                default: // blue, multicolor (maps to blue), graphite (maps to blue)
                    aColor = Color2.FromRGB(16, 125, 254);
                    break;
            }

            AddOrUpdateSystemResource("SystemAccentColor", (Color)aColor);

            AddOrUpdateSystemResource("SystemAccentColorLight1", (Color)aColor.LightenPercent(0.15f));
            AddOrUpdateSystemResource("SystemAccentColorLight2", (Color)aColor.LightenPercent(0.30f));
            AddOrUpdateSystemResource("SystemAccentColorLight3", (Color)aColor.LightenPercent(0.45f));

            AddOrUpdateSystemResource("SystemAccentColorDark1", (Color)aColor.LightenPercent(-0.15f));
            AddOrUpdateSystemResource("SystemAccentColorDark2", (Color)aColor.LightenPercent(-0.30f));
            AddOrUpdateSystemResource("SystemAccentColorDark3", (Color)aColor.LightenPercent(-0.45f));
        }
        catch
        {
            LoadDefaultAccentColor();
        }
    }

    private void TryLoadLinuxAccentColor()
    {
        Color2? aColor = null;

        try
        {
            if (_linuxDesktopEnvironment.Contains("KDE"))
            {
                if (File.Exists(_kdeGlobalsFile))
                {
                    var kdeGlobals = File.ReadAllText(_kdeGlobalsFile);
                    var match = new Regex("^AccentColor=(\\d+),(\\d+),(\\d+)$", RegexOptions.Multiline).Match(kdeGlobals);
                    if (!match.Success)
                    {
                        // Accent color is from the current color scheme
                        match = new Regex("^DecorationFocus=(\\d+),(\\d+),(\\d+)$", RegexOptions.Multiline).Match(kdeGlobals);
                    }

                    if (match.Success)
                    {
                        aColor = Color2.FromRGB(byte.Parse(match.Groups[1].Value), byte.Parse(match.Groups[2].Value), byte.Parse(match.Groups[3].Value));
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        if (aColor == null)
        {
            LoadDefaultAccentColor();
        }
        else
        {
            AddOrUpdateSystemResource("SystemAccentColor", (Color)aColor);

            AddOrUpdateSystemResource("SystemAccentColorLight1", (Color)aColor.Value.LightenPercent(0.15f));
            AddOrUpdateSystemResource("SystemAccentColorLight2", (Color)aColor.Value.LightenPercent(0.30f));
            AddOrUpdateSystemResource("SystemAccentColorLight3", (Color)aColor.Value.LightenPercent(0.45f));

            AddOrUpdateSystemResource("SystemAccentColorDark1", (Color)aColor.Value.LightenPercent(-0.15f));
            AddOrUpdateSystemResource("SystemAccentColorDark2", (Color)aColor.Value.LightenPercent(-0.30f));
            AddOrUpdateSystemResource("SystemAccentColorDark3", (Color)aColor.Value.LightenPercent(-0.45f));
        }
    }

    private void LoadDefaultAccentColor()
    {
        AddOrUpdateSystemResource("SystemAccentColor", Colors.SlateBlue);

        AddOrUpdateSystemResource("SystemAccentColorLight1", Color.Parse("#7F69FF"));
        AddOrUpdateSystemResource("SystemAccentColorLight2", Color.Parse("#9B8AFF"));
        AddOrUpdateSystemResource("SystemAccentColorLight3", Color.Parse("#B9ADFF"));

        AddOrUpdateSystemResource("SystemAccentColorDark1", Color.Parse("#43339C"));
        AddOrUpdateSystemResource("SystemAccentColorDark2", Color.Parse("#33238C"));
        AddOrUpdateSystemResource("SystemAccentColorDark3", Color.Parse("#1D115C"));
    }

    private void AddOrUpdateSystemResource(object key, object value)
    {
        if (_themeResources.ContainsKey(key))
        {
            _themeResources[key] = value;
        }
        else
        {
            _themeResources.Add(key, value);
        }
    }

    private string ReadGsettingsKey(string schema, string key)
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = "gsettings",
                Arguments = $"get {schema} {key}"
            },
        };
        p.Start();
        p.WaitForExit();

        return p.ExitCode == 0 ? p.StandardOutput.ReadToEnd().Trim() : null;
    }

    private bool _hasLoaded;
    private Styles _styles;
    private readonly StyleCache _styleCache = new StyleCache();
    private readonly ResourceDictionary _themeResources = new ResourceDictionary();
    //private ResourceDictionary _controlThemes;
    private IResourceHost _owner;
    private string _requestedTheme = null;
    private Uri _baseUri;
    private Color? _customAccentColor;

    private readonly string _linuxDesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
    private readonly string _kdeGlobalsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "kdeglobals");

    public static readonly string LightModeString = "Light";
    public static readonly string DarkModeString = "Dark";
    public static readonly string HighContrastModeString = "HighContrast";

    private class StyleCache : Dictionary<Type, List<IStyle>>
    {
        public SelectorMatchResult TryAttach(IList<IStyle> styles, IStyleable target, object host)
        {
            if (TryGetValue(target.StyleKey, out var cached))
            {
                if (cached is object)
                {
                    var result = SelectorMatchResult.NeverThisType;

                    foreach (var style in cached)
                    {
                        var childResult = style.TryAttach(target, host);
                        if (childResult > result)
                            result = childResult;
                    }

                    return result;
                }
                else
                {
                    return SelectorMatchResult.NeverThisType;
                }
            }
            else
            {
                List<IStyle> matches = null;

                var key = target.StyleKey;
                foreach (var child in styles)
                {
                    if (child.TryAttach(target, host) != SelectorMatchResult.NeverThisType)
                    {
                        matches ??= new List<IStyle>();
                        matches.Add(child);
                    }
                }

                Add(target.StyleKey, matches);

                return matches is null ?
                    SelectorMatchResult.NeverThisType :
                    SelectorMatchResult.AlwaysThisType;
            }
        }
    }
}
