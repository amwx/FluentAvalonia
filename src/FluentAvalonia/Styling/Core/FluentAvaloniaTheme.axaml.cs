using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.Interop;
using FluentAvalonia.UI.Media;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Styling;

/// <summary>
/// Theme manager for FluentAvalonia, managing various components of the Fluentv2 theme
/// like AccentColor, styles, and platform settings
/// </summary>
public partial class FluentAvaloniaTheme : Styles, IResourceProvider
{
    /// <summary>
    /// Create new instance of <see cref="FluentAvaloniaTheme"/>.
    /// </summary>
    public FluentAvaloniaTheme()
    {
        MergedDictionaries = new AvaloniaList<IResourceDictionary>();
        MergedDictionaries.CollectionChanged += MergedDictionariesCollectionChanged;
        Init();
    }

    /// <summary>
    /// High Contrast Theme
    /// </summary>
    public static readonly ThemeVariant HighContrastTheme = new ThemeVariant(HighContrastModeString,
        ThemeVariant.Light);

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
    /// the detection is different depending on the user's desktop environment. On KDE,
    /// Cinnamon, LXDE and LXQt, it requires the user's theme (color scheme in the
    /// case of KDE) name to contain "dark". On GNOME or Xfce, it requires 'color-scheme'
    /// to be set to either 'prefer-light', 'prefer-dark', or 'gtk-theme' to contain 'dark'.
    /// Also note, that high contrast theme will only resolve here on Windows.
    /// </remarks>
    public bool PreferSystemTheme
    {
        get => _preferSystemTheme;
        set
        {
            if (_preferSystemTheme != value)
            {
                _preferSystemTheme = value;

                // Only call this if PreferSystemTheme is true to invalidate the current theme.
                if (value)
                {
                    ResolveThemeAndInitializeSystemResources();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to use the current user's accent color as the resource SystemAccentColor
    /// </summary>
    /// <remarks>
    /// On Linux, accent color detection is only supported on KDE (from current scheme,
    /// from wallpaper and custom), LXQt (from selection color) and LXDE (from custom selection
    /// color).
    /// </remarks>
    public bool PreferUserAccentColor
    {
        get => _preferUserAccentColor;
        set
        { 
            if(_preferUserAccentColor != value)
            {
                _preferUserAccentColor = value;

                // Unlike PreferSystemTheme, we call this everytime as LoadCustomAccentColor handles
                // switching between a system and custom color (and back)
                LoadCustomAccentColor();
            }            
        }
    }

    /// <summary>
    /// Gets or sets a <see cref="Color"/> to use as the SystemAccentColor for the app. Note this takes precedence over the
    /// <see cref="PreferUserAccentColor"/> property and must be set to null to restore the system color, if desired
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

    /// <summary>
    /// Gets or sets a value that determines if/when style overrides should be used to alleviate issues
    /// with text alignment in some controls caused when Segoe UI or Segoe UI Variable font
    /// families do not exist. The default value is <see cref="TextVerticalAlignmentOverride.EnabledNonWindows"/>
    /// </summary>
    /// <remarks>
    /// These overrides apply to controls like RadioButton, CheckBox, ComboBox where the first line of text
    /// is explicitly aligned with the control. Adding the overrides modify the styles to use VerticalAlignment=Center
    /// to get a consistent experience, at the (small) expense of breaking Fluent design principles. If your controls
    /// never use multi-line text, you'll never see the effect of this property.
    /// </remarks>
    public TextVerticalAlignmentOverride TextVerticalAlignmentOverrideBehavior { get; set; } =
        TextVerticalAlignmentOverride.EnabledNonWindows;

    public AvaloniaList<IResourceDictionary> MergedDictionaries { get; }
      
    bool IResourceNode.HasResources => true;

    /// <inheritdoc />
    public new bool TryGetResource(object key, ThemeVariant theme, out object value)
    {
        // Github build failing with this not being set, even tho it passes locally
        value = null;

        // We also search the app level resources so resources can be overridden.
        // Do not search App level styles though as we'll have to iterate over them
        // to skip the FluentAvaloniaTheme instance or we'll stack overflow
        if (Application.Current?.Resources.TryGetResource(key, theme, out value) == true)
            return true;

        if (base.TryGetResource(key, theme, out value) == true)
            return true;

        value = null;
        return false;
    }

    bool IResourceNode.TryGetResource(object key, ThemeVariant theme, out object value) =>
        this.TryGetResource(key, theme, out value);

    private void Init()
    {
        AvaloniaXamlLoader.Load(this);

        // First load our base and theme resources

        // When initializing, UseSystemTheme overrides any setting of RequestedTheme, this must be
        // explicitly disabled to enable setting the theme manually
        ResolveThemeAndInitializeSystemResources();

        if (OSVersionHelper.IsWindows())
        {
            // Load this in all cases since with ThemeDictionaries, we always have a ref to the 
            // HighContrast dictionary
            TryLoadHighContrastThemeColors();
        }

        SetTextAlignmentOverrides();

        _hasLoaded = true;
    }

    private void ResolveThemeAndInitializeSystemResources()
    {
        ThemeVariant theme = null;

        // PlatformSettings on the Application should be immutable so we can store them here
        if (_platformSettings == null)
        {
            _platformSettings = Application.Current.PlatformSettings;
            _platformSettings.ColorValuesChanged += OnPlatformColorValuesChanged;
        }
                        
        if (OSVersionHelper.IsWindows())
        {
            theme = ResolveWindowsSystemSettings(_platformSettings);
        }
        else if (OSVersionHelper.IsLinux())
        {
            theme = ResolveLinuxSystemSettings(_platformSettings);
        }
        else if (OSVersionHelper.IsMacOS())
        {
            theme = ResolveMacOSSystemSettings(_platformSettings);
        }
        else
        {
            // WASM & Mobile

            // Don't read from PlatformSettings if PreferSystemTheme = false, Issue #497
            if (PreferSystemTheme)
                theme = GetThemeFromIPlatformSettings(_platformSettings);

            // MacOS logic is also used for WASM/Mobile since it just pulls from 
            // IPlatformSettings Color Values
            TryLoadMacOSAccentColor(_platformSettings);

            AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);
        }

        // The Resolve...Settings will return null if PreferSystemTheme is false
        if (theme != null)
        {
            Application.Current.RequestedThemeVariant = theme;
        }     
    }

    private void OnPlatformColorValuesChanged(object sender, PlatformColorValues e)
    {
        if (OSVersionHelper.IsWindows())
        {
            TryLoadHighContrastThemeColors();
        }

        if (PreferSystemTheme)
        {
            ThemeVariant theme;
            if (e.ContrastPreference == ColorContrastPreference.High)
            {
                theme = HighContrastTheme;
            }
            else
            {
                theme = e.ThemeVariant == PlatformThemeVariant.Light ?
                    ThemeVariant.Light : ThemeVariant.Dark;
            }

            Application.Current.RequestedThemeVariant = theme;
        }

        if (!CustomAccentColor.HasValue && PreferUserAccentColor)
        {
            if (OSVersionHelper.IsWindows())
            {
                TryLoadWindowsAccentColor();
            }
            else if (OSVersionHelper.IsMacOS())
            {
                TryLoadMacOSAccentColor(_platformSettings);
            }
            else if (OSVersionHelper.IsLinux())
            {
                TryLoadLinuxAccentColor();
            }
        }
    }

    private ThemeVariant ResolveMacOSSystemSettings(IPlatformSettings platformSettings)
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
            TryLoadMacOSAccentColor(platformSettings);
        }
        else
        {
            LoadDefaultAccentColor();
        }

        AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);

        return theme;
    }

    private ThemeVariant ResolveLinuxSystemSettings(IPlatformSettings platformSettings)
    {
        ThemeVariant theme = null;
        if (PreferSystemTheme)
        {
            // See TryLoadLinuxAccentColor() for note on what Avalonia IPlatformSettings supports
            // on Linux. We'll try the existing logic first before attempting IPlatformSettings
            var resolvedTheme = LinuxThemeResolver.TryLoadSystemTheme();
            if (resolvedTheme != null)
            {
                theme = resolvedTheme;
            }
            else
            {
                theme = GetThemeFromIPlatformSettings(platformSettings);
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

    private ThemeVariant GetThemeFromIPlatformSettings(IPlatformSettings platformSettings)
    {
        var platformColors = platformSettings.GetColorValues();
        bool isSystemInHighContrast = platformColors.ContrastPreference == ColorContrastPreference.High;
        if (!isSystemInHighContrast)
        {
           return platformColors.ThemeVariant == PlatformThemeVariant.Light ?
                ThemeVariant.Light : ThemeVariant.Dark;
        }
        else
        {
            return HighContrastTheme;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetTextAlignmentOverrides()
    {
        if (TextVerticalAlignmentOverrideBehavior == TextVerticalAlignmentOverride.Disabled ||
            (TextVerticalAlignmentOverrideBehavior == TextVerticalAlignmentOverride.EnabledNonWindows &&
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)))
            return;

        // The following resources are added to remove the larger bottom margin/padding value
        // on some controls added to accomodate Segoe UI - this will allow vertical centering
        // These are added to the internal _themeResources dictionary, so user can still
        // override these elsewhere if desired

        Resources.Add("CheckBoxPadding", new Thickness(8, 5, 0, 5));
        Resources.Add("ComboBoxPadding", new Thickness(12, 5, 0, 5));
        Resources.Add("ComboBoxItemThemePadding", new Thickness(11, 5, 11, 5));
        // Note that this is a theme resource, but as of now is the same for all three themes
        Resources.Add("TextControlThemePadding", new Thickness(10, 5, 6, 5));

        // Now we add some style overrides to adjust some properties
        // Yes, I'm doing this in C# rather than Xaml - I don't want to create a Xaml file
        // because that will get compiled into AvaloniaXamlResource even if never used or I
        // could use a normal file and us the AvaloniaXamlLoader but that's still an additional
        // AvaloniaResource that's not necessary. Plus, not using Xaml is fun =D

        // Set VerticalContentAlignment on CheckBox to center the content
        var s = new Style(x =>
        {
            return x.OfType(typeof(CheckBox));
        });
        s.Setters.Add(new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        Add(s);

        // Set Padding & VCA on RadioButton to center the content
        var s2 = new Style(x =>
        {
            return x.OfType(typeof(RadioButton));
        });
        s2.Setters.Add(new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        s2.Setters.Add(new Setter(Decorator.PaddingProperty, new Thickness(8, 6, 0, 6)));
        Add(s2);

        // Center the TextBlock in ComboBox
        // This is special - we only want to do this if the content is a string - otherwise custom content
        // may get messed up b/c of the centered alignment
        var s3 = new Style(x =>
        {
            return x.OfType<ComboBox>().Template().OfType<ContentControl>().Child().OfType<TextBlock>();
        });
        s3.Setters.Add(new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center));
        Add(s3);
    }
       
    private void LoadCustomAccentColor()
    {
        if (!_customAccentColor.HasValue)
        {
            if (PreferUserAccentColor)
            {
                if (OSVersionHelper.IsWindows())
                {
                    TryLoadWindowsAccentColor();
                }                
                else if (OSVersionHelper.IsLinux())
                {
                    TryLoadLinuxAccentColor();
                }
                else // Mac & WASM/Mobile
                {
                    TryLoadMacOSAccentColor(_platformSettings);
                }
            }
            else
            {
                LoadDefaultAccentColor();
            }

            return;
        }

        Color2 col = _customAccentColor.Value;

        UpdateAccentColors((Color)_customAccentColor.Value,
            (Color)col.LightenPercent(0.15f),
            (Color)col.LightenPercent(0.30f),
            (Color)col.LightenPercent(0.45f),
            (Color)col.LightenPercent(-0.15f),
            (Color)col.LightenPercent(-0.30f),
            (Color)col.LightenPercent(-0.45f));
    }
        
    private void TryLoadMacOSAccentColor(IPlatformSettings platformSettings)
    {
        try
        {
            // Replaced old logic with PlatformSettings from Avalonia
            Color2 aColor = platformSettings.GetColorValues().AccentColor1;

            UpdateAccentColors((Color)aColor,
                (Color)aColor.LightenPercent(0.15f),
                (Color)aColor.LightenPercent(0.30f),
                (Color)aColor.LightenPercent(0.45f),
                (Color)aColor.LightenPercent(-0.15f),
                (Color)aColor.LightenPercent(-0.30f),
                (Color)aColor.LightenPercent(-0.45f));
        }
        catch
        {
            LoadDefaultAccentColor();
        }
    }

    private void TryLoadLinuxAccentColor()
    {
        // Per GH#9913:
        // Only works if distro implements newest (~2021) standard of FreeDesktop. GTK and others specific settings are ignored.
        // Accent colors are not supported, and frame theme isn't changeable from the app (not sure if it's possible, if anybody wants to help - please do).
        // No high contrast support.
        // So we'll keep the existing logic here

        var aColor = LinuxThemeResolver.TryLoadAccentColor();
        if (aColor != null)
        {
            Color2 col = aColor.Value;

            UpdateAccentColors((Color)col,
                (Color)col.LightenPercent(0.15f),
                (Color)col.LightenPercent(0.30f),
                (Color)col.LightenPercent(0.45f),
                (Color)col.LightenPercent(-0.15f),
                (Color)col.LightenPercent(-0.30f),
                (Color)col.LightenPercent(-0.45f));
        }
        else
        {
            LoadDefaultAccentColor();
        }
    }

    private void LoadDefaultAccentColor()
    {
        UpdateAccentColors(Colors.SlateBlue,
            Color.Parse("#7F69FF"),
            Color.Parse("#9B8AFF"),
            Color.Parse("#B9ADFF"),
            Color.Parse("#43339C"),
            Color.Parse("#33238C"),
            Color.Parse("#1D115C"));
    }

    private void AddOrUpdateSystemResource(object key, object value)
    {
        if (Resources.ContainsKey(key))
        {
            Resources[key] = value;
        }
        else
        {
            Resources.Add(key, value);
        }
    }

    private void UpdateAccentColors(Color accent,
        Color light1, Color light2, Color light3,
        Color dark1, Color dark2, Color dark3)
    {
        if (_accentColorsDictionary != null)
            Resources.MergedDictionaries.Remove(_accentColorsDictionary);

        _accentColorsDictionary = new ResourceDictionary
        {
            { "SystemAccentColor", accent },
            { "SystemAccentColorLight1", light1 },
            { "SystemAccentColorLight2", light2 },
            { "SystemAccentColorLight3", light3 },
            { "SystemAccentColorDark1", dark1 },
            { "SystemAccentColorDark2", dark2 },
            { "SystemAccentColorDark3", dark3 }
        };

        Resources.MergedDictionaries.Add(_accentColorsDictionary);
    }

    private void MergedDictionariesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (IResourceDictionary item in e.OldItems)
            {
                Resources.MergedDictionaries.Remove(item);
            }
        }

        if (e.NewItems != null)
        {
            foreach (IResourceDictionary item in e.NewItems)
            {
                Resources.MergedDictionaries.Add(item);
            }
        }
    }

    private bool _hasLoaded;
    private Color? _customAccentColor;
    private bool _preferSystemTheme;
    private bool _preferUserAccentColor;
    private ResourceDictionary _accentColorsDictionary;
    private IPlatformSettings _platformSettings;

    public const string LightModeString = "Light";
    public const string DarkModeString = "Dark";
    public const string HighContrastModeString = "HighContrast";
}
