using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
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

namespace FluentAvalonia.Styling
{
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
        /// Gets or sets whether the system user accent color should be used on Windows
        /// </summary>
        /// <remarks>
        /// If false, this defaults to SlateBlue as SystemAccentColor unless overridden
        /// </remarks>
        public bool UseUserAccentColorOnWindows { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use the current Windows system theme. Respects Light, Dark, and HighContrast modes
        /// </summary>
        public bool UseSystemThemeOnWindows { get; set; } = true;

        /// <summary>
        /// Gets or sets a semi-colon (;) delimited string of controls that should be skipped when loading the control templates
        /// into FluentAvaloniaTheme. This property is only respected at startup
        /// </summary>
        /// <remarks>
        /// Use this property to skip loading the templates of controls you never use. This reduces the number of active templates
        /// stored (save memory, even if only KBs) and reducing the number of Style Selectors that need to be evaluated when templates
        /// are applied to controls - increasing runtime speed. Example: SkipControls="NavigationView;DataGrid" will skip the NavigationView
        /// and DataGrid. For something like the NavigationView where multiple template files exist, you only need to specify NavigationView
        /// as all the other files are contained within NavigationView.axaml.
        /// Note: because of other limitations, resources (brushes/colors) are still loaded even if the template isn't
        /// </remarks>
        public string SkipControls { get; set; }

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

        public IReadOnlyList<IStyle> Children => _controlStyles;

        public event EventHandler OwnerChanged;
        public event TypedEventHandler<FluentAvaloniaTheme, RequestedThemeChangedEventArgs> RequestedThemeChanged;

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost host)
        {
            if (_cache.TryGetValue(target.StyleKey, out var cached))
            {
                if (cached is object)
                {
                    foreach (var style in cached)
                    {
                        style.TryAttach(target, host);
                    }

                    return SelectorMatchResult.AlwaysThisType;
                }
                else
                {
                    return SelectorMatchResult.NeverThisType;
                }
            }
            else
            {
                List<IStyle> matches = null;

                foreach (var child in Children)
                {
                    if (child.TryAttach(target, host) != SelectorMatchResult.NeverThisType)
                    {
                        matches ??= new List<IStyle>();
                        matches.Add(child);
                    }
                }

                _cache.Add(target.StyleKey, matches);

                return matches is null ?
                    SelectorMatchResult.NeverThisType :
                    SelectorMatchResult.AlwaysThisType;
            }
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

            for (int i = _controlStyles.Count - 1; i >= 0; i--)
            {
                if (_controlStyles[i] is IResourceProvider rp && rp.TryGetResource(key, out value))
                    return true;
            }

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
           // (_systemResources as IResourceProvider).AddOwner(owner);

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
               // (_systemResources as IResourceProvider).RemoveOwner(owner);

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (UseUserAccentColorOnWindows)
                {                   
                    try
                    {
                        IUISettings3 settings = WinRTInterop.CreateInstance<IUISettings3>("Windows.UI.ViewManagement.UISettings");

                        TryLoadWindowsAccentColor(settings);
                    }
                    catch
                    {

                    }                   
                }

                if (UseSystemThemeOnWindows)
                {
                    Refresh(null);
                }
            }

            // Ignore this on non-Windows OSs as it has no effect
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

            bool hasControlsToSkip = !string.IsNullOrEmpty(SkipControls);

            // ALL keyword should only be used in UnitTests since it doesn't load anything
            if (SkipControls != "ALL")
			{
                string[] controls = hasControlsToSkip ? SkipControls.Split(';') : null;
                var prefix = "avares://FluentAvalonia";
                // Now load all the Avalonia Controls
                var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using (var stream = assetLoader.Open(new Uri("avares://FluentAvalonia/Styling/StylesV2/Controls.txt")))
                using (var streamReader = new StreamReader(stream))
                {
                    while (streamReader.Peek() != -1)
                    {
                        var file = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(file) || file.StartsWith("["))
                            continue;

                        if (hasControlsToSkip)
                        {
                            // The time spent here checking (and splitting the string above) may be made up by not
                            // loading those particular styles so it actually seems to balance out nicely
                            var span = file.AsSpan();
                            bool skip = false;
                            foreach (var item in controls)
                            {
                                if (span.Contains(item.AsSpan(), StringComparison.OrdinalIgnoreCase))
                                {
                                    skip = true;
                                    break;
                                }
                            }

                            if (skip)
                                continue;
                        }

                        var styles = (IStyle)AvaloniaXamlLoader.Load(new Uri($"{prefix}{file}"), _baseUri);

                        _controlStyles.Add(styles);

                    }
                }
            }

            _hasLoaded = true;
        }

        private void Refresh(string newTheme)
        {
            if (newTheme == null)
            {
                _requestedTheme = ResolveThemeAndInitializeSystemResources();
            }

            var old = _requestedTheme;
            if (!string.Equals(newTheme, old, StringComparison.OrdinalIgnoreCase))
            {
                _requestedTheme = newTheme;

                // Remove the old theme if any resources
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
            string theme = _requestedTheme;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
                
                if (UseSystemThemeOnWindows)
                {
                    try
                    {                        
                        int isUsingHighContrast = accessibility.HighContrast;
                        if (isUsingHighContrast == 1)
                        {
                            theme = HighContrastModeString;
                        }
                    }
                    catch
                    {
                        Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                            .Log("FluentAvaloniaTheme", "Unable to retrieve High contrast settings.");
                    }

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

                        theme = LightModeString;
                    }
                }
                else
                {
                    theme = string.IsNullOrEmpty(theme) ? LightModeString : theme;
                }
                
                if (CustomAccentColor != null)
                {
                    LoadCustomAccentColor();
                }
                else if (UseUserAccentColorOnWindows)
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
            }
            else
            {
                theme = string.IsNullOrEmpty(theme) ? LightModeString : theme;

                if (CustomAccentColor != null)
                {
                    LoadCustomAccentColor();
                }
                else
                {
                    LoadDefaultAccentColor();
                }
               
                AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);
            }

            // Load the SymbolThemeFontFamily
            AddOrUpdateSystemResource("SymbolThemeFontFamily", new FontFamily(new Uri("avares://FluentAvalonia"), "/Fonts/#Symbols"));

            // If not on Windows or if not using the system them, we default to LightMode
            // if user didn't specify the theme to use
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
                if (UseUserAccentColorOnWindows && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                else
                {
                    LoadDefaultAccentColor();
                }

                return;
            }

            Color2 col = _customAccentColor.Value;
            AddOrUpdateSystemResource("SystemAccentColor", _customAccentColor.Value);

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
        private readonly List<IStyle> _controlStyles = new List<IStyle>();
        private readonly Dictionary<Type, List<IStyle>> _cache = new Dictionary<Type, List<IStyle>>();
        private readonly ResourceDictionary _themeResources = new ResourceDictionary();
        private IResourceHost _owner;
        private string _requestedTheme = null;
        private Uri _baseUri;
        private Color? _customAccentColor;

        public static readonly string LightModeString = "Light";
        public static readonly string DarkModeString = "Dark";
        public static readonly string HighContrastModeString = "HighContrast";
    }
}
