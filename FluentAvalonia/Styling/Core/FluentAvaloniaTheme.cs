using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.Interop;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Styling;

public partial class FluentAvaloniaTheme : IStyle, IResourceProvider
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
    /// the detection is different depending on the user's desktop environment. On KDE,
    /// Cinnamon, LXDE and LXQt, it requires the user's theme (color scheme in the
    /// case of KDE) name to contain "dark". On GNOME or Xfce, it requires 'color-scheme'
    /// to be set to either 'prefer-light', 'prefer-dark', or 'gtk-theme' to contain 'dark'.
    /// Also note, that high contrast theme will only resolve here on Windows.
    /// </remarks>
    public bool PreferSystemTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use the current user's accent color as the resource SystemAccentColor
    /// </summary>
    /// <remarks>
    /// On Linux, accent color detection is only supported on KDE (from current scheme,
    /// from wallpaper and custom), LXQt (from selection color) and LXDE (from custom selection
    /// color).
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
            if (OSVersionHelper.IsWindows())
            {
                TryLoadWindowsAccentColor();
            }
            else if (OSVersionHelper.IsMacOS())
            {
                TryLoadMacOSAccentColor();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                TryLoadLinuxAccentColor();
            }
        }

        if (PreferSystemTheme)
        {
            Refresh(null);
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

        SetTextAlignmentOverrides();

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

        if (OSVersionHelper.IsWindows())
        {
            theme = ResolveWindowsSystemSettings();
        }
        else if (OSVersionHelper.IsLinux())
        {
            theme = ResolveLinuxSystemSettings();
        }
        else if (OSVersionHelper.IsMacOS())
        {
            theme = ResolveMacOSSystemSettings();
        }
        else
        {
            // Needed for mobile/unhandled platforms
            AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);
        }

        // Load the SymbolThemeFontFamily
        AddOrUpdateSystemResource("SymbolThemeFontFamily", new FontFamily(new Uri("avares://FluentAvalonia"), "/Fonts/#Symbols"));

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
        if (PreferSystemTheme)
        {
            var resolvedTheme = LinuxThemeResolver.TryLoadSystemTheme();
            if (resolvedTheme != null)
            {
                theme = resolvedTheme;
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

        _themeResources.Add("CheckBoxPadding", new Thickness(8, 5, 0, 5));
        _themeResources.Add("ComboBoxPadding", new Thickness(12, 5, 0, 5));
        _themeResources.Add("ComboBoxItemThemePadding", new Thickness(11, 5, 11, 5));
        // Note that this is a theme resource, but as of now is the same for all three themes
        _themeResources.Add("TextControlThemePadding", new Thickness(10, 5, 6, 5));

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
        _styles.Add(s);

        // Set Padding & VCA on RadioButton to center the content
        var s2 = new Style(x =>
        {
            return x.OfType(typeof(RadioButton));
        });
        s2.Setters.Add(new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        s2.Setters.Add(new Setter(Decorator.PaddingProperty, new Thickness(8, 6, 0, 6)));
        _styles.Add(s2);

        // Center the TextBlock in ComboBox
        // This is special - we only want to do this if the content is a string - otherwise custom content
        // may get messed up b/c of the centered alignment
        var s3 = new Style(x =>
        {
            return x.OfType<ComboBox>().Template().OfType<ContentControl>().Child().OfType<TextBlock>();
        });
        s3.Setters.Add(new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center));
        _styles.Add(s3);
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
                else if (OSVersionHelper.IsMacOS())
                {
                    TryLoadMacOSAccentColor();
                }
                else if (OSVersionHelper.IsLinux())
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
        var aColor = LinuxThemeResolver.TryLoadAccentColor();
        if (aColor != null)
        {
            AddOrUpdateSystemResource("SystemAccentColor", (Color)aColor.Value);

            AddOrUpdateSystemResource("SystemAccentColorLight1", (Color)aColor.Value.LightenPercent(0.15f));
            AddOrUpdateSystemResource("SystemAccentColorLight2", (Color)aColor.Value.LightenPercent(0.30f));
            AddOrUpdateSystemResource("SystemAccentColorLight3", (Color)aColor.Value.LightenPercent(0.45f));

            AddOrUpdateSystemResource("SystemAccentColorDark1", (Color)aColor.Value.LightenPercent(-0.15f));
            AddOrUpdateSystemResource("SystemAccentColorDark2", (Color)aColor.Value.LightenPercent(-0.30f));
            AddOrUpdateSystemResource("SystemAccentColorDark3", (Color)aColor.Value.LightenPercent(-0.45f));
        }
        else
        {
            LoadDefaultAccentColor();
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

    private bool _hasLoaded;
    private Styles _styles;
    private readonly StyleCache _styleCache = new StyleCache();
    private readonly ResourceDictionary _themeResources = new ResourceDictionary();
    //private ResourceDictionary _controlThemes;
    private IResourceHost _owner;
    private string _requestedTheme = null;
    private Uri _baseUri;
    private Color? _customAccentColor;

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
