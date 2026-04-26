using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Windowing;

public partial class FAAppWindow : Window
{
    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FAAppWindowTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FAAppWindow, FAAppWindowTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly new StyledProperty<IImage> IconProperty =
        AvaloniaProperty.Register<FAAppWindow, IImage>(nameof(Icon));

    /// <summary>
    /// Provides calculated data for items within the Template of AppWindow
    /// </summary>
    public FAAppWindowTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon for the window
    /// </summary>
    /// <remarks>
    /// Note that this type is <see cref="IImage"/> and not <see cref="WindowIcon"/>, like on Window
    /// This is done to allow using a window icon in managed titlebar. Provided the
    /// image is an <see cref="IBitmap"/>, it should convert to a WindowIcon without 
    /// issue and you'll still get the icon in the taskbar, on other OS's, etc.
    /// </remarks>
    public new IImage Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets a value whether the AppWindow should hide its minimize/maximize buttons like 
    /// a dialog window. This property is only respected on Windows.
    /// </summary>
    public bool ShowAsDialog
    {
        get => _hideSizeButtons;
        set
        {
            _hideSizeButtons = value;
            PseudoClasses.Set(":dialog", value);
        }
    }

    /// <summary>
    /// Gets or sets the splash screen that should show when the window first loads
    /// </summary>
    public IFAApplicationSplashScreen SplashScreen
    {
        get => _splashContext?.SplashScreen;
        set
        {
            if (value == null)
            {
                if (_splashContext != null)
                {
                    _splashContext.Host.SplashScreen = null;
                }

                _splashContext = null;
                PseudoClasses.Set(":splashScreen", false);
            }
            else
            {
                _splashContext = new SplashScreenContext(value);
                PseudoClasses.Set(":splashScreen", true);
            }
        }
    }

    /// <summary>
    /// Gets the Titlebar description information for the AppWindow
    /// </summary>
    /// <remarks>
    /// Use this property to customize the colors, height, and whether the window contents should
    /// display in the titlebar area
    /// </remarks>
    public FAAppWindowTitleBar TitleBar => _titleBar;

    /// <summary>
    /// Gets the interface for custom platform-specific features through the AppWindow class
    /// NOTE: Only implemented on Windows right now
    /// </summary>
    public IFAAppWindowPlatformFeatures PlatformFeatures { get; private set; }

    protected internal bool IsWindows11 { get; internal set; }

    protected internal bool IsWindows { get; internal set; }

    protected override Type StyleKeyOverride => typeof(FAAppWindow);

    internal FAMinMaxCloseControl SystemCaptionControl => _captionButtons;


    private SplashScreenContext _splashContext;
    private Border _templateRoot;
    private FAMinMaxCloseControl _captionButtons;
    private Panel _defaultTitleBar;
    private FAAppWindowTitleBar _titleBar;
    private List<WeakReference<Control>> _excludeHitTestList;
    private bool _hideSizeButtons;

    // Resource names used in SetTitleBarColors
    private static readonly string s_SystemAccentColor = "SystemAccentColor";
    private static readonly string s_SystemAccentColorLight1 = "SystemAccentColorLight1";
    private static readonly string s_SystemAccentColorDark1 = "SystemAccentColorDark1";
    private static readonly string s_TextFillColorPrimary = "TextFillColorPrimary";

    private static readonly string s_TitleBarBackground = "TitleBarBackground";
    private static readonly string s_TitleBarForeground = "TitleBarForeground";
    private static readonly string s_TitleBarInactiveBackground = "TitleBarBackgroundInactive";
    private static readonly string s_TitleBarInactiveForeground = "TitleBarForegroundInactive";
    private static readonly string s_SysCaptionBackground = "CaptionButtonBackground";
    private static readonly string s_SysCaptionForeground = "CaptionButtonForeground";
    private static readonly string s_SysCaptionBackgroundHover = "CaptionButtonBackgroundPointerOver";
    private static readonly string s_SysCaptionForegroundHover = "CaptionButtonForegroundPointerOver";
    private static readonly string s_SysCaptionBackgroundPressed = "CaptionButtonBackgroundPressed";
    private static readonly string s_SysCaptionForegroundPressed = "CaptionButtonForegroundPressed";
    private static readonly string s_SysCaptionBackgroundInactive = "CaptionButtonBackgroundInactive";
    private static readonly string s_SysCaptionForegroundInactive = "CaptionButtonForegroundInactive";
}
