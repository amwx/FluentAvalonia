using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Defines settings used in the template of an <see cref="AppWindow"/> (Windows Only)
/// </summary>
public class AppWindowTemplateSettings : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="TitleBarHeight"/> property
    /// </summary>
    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, double>(nameof(TitleBarHeight), 32d);

    /// <summary>
    /// Defines the <see cref="ContentMargin"/> property
    /// </summary>
    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, Thickness>(nameof(ContentMargin));

    /// <summary>
    /// Defines the <see cref="IsTitleBarContentVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsTitleBarContentVisibleProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, bool>(nameof(IsTitleBarContentVisible));

    /// <summary>
    /// Defines the <see cref="WindowIcon"/> property
    /// </summary>
    public static readonly StyledProperty<IImage> WindowIconProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, IImage>(nameof(WindowIcon));

    /// <summary>
    /// Gets or sets the height of the managed titlebar for AppWindow
    /// </summary>
    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the window content margin for AppWindow
    /// </summary>
    /// <remarks>
    /// This value is calculated based on WindowState, title bar height and whether
    /// the content is extended into the titlebar area
    /// </remarks>
    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the titlebar content is visible (Icon and App name text)
    /// </summary>
    public bool IsTitleBarContentVisible
    {
        get => GetValue(IsTitleBarContentVisibleProperty);
        set => SetValue(IsTitleBarContentVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon used in the managed titlebar of AppWindow
    /// </summary>
    public IImage WindowIcon
    {
        get => GetValue(WindowIconProperty);
        set => SetValue(WindowIconProperty, value);
    }
}
