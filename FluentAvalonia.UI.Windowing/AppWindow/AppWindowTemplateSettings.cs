using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Windowing;

public class AppWindowTemplateSettings : AvaloniaObject
{
    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, double>(nameof(TitleBarHeight), 32d);

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, Thickness>(nameof(ContentMargin));

    public static readonly StyledProperty<bool> IsTitleBarContentVisibleProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, bool>(nameof(IsTitleBarContentVisible));

    public static readonly StyledProperty<IImage> WindowIconProperty =
        AvaloniaProperty.Register<AppWindowTemplateSettings, IImage>(nameof(WindowIcon));

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public bool IsTitleBarContentVisible
    {
        get => GetValue(IsTitleBarContentVisibleProperty);
        set => SetValue(IsTitleBarContentVisibleProperty, value);
    }

    public IImage WindowIcon
    {
        get => GetValue(WindowIconProperty);
        set => SetValue(WindowIconProperty, value);
    }
}
