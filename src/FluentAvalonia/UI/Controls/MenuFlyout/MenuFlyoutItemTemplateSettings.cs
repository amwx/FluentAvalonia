using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines objects used in the template of a <see cref="MenuFlyoutItem"/> and related classes
/// </summary>
public class MenuFlyoutItemTemplateSettings : AvaloniaObject
{
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<MenuFlyoutItemTemplateSettings, FAIconElement>(nameof(Icon));

    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        internal set => SetValue(IconProperty, value);
    }
}
