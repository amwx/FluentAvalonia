using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines objects used in the template of a <see cref="MenuFlyoutItem"/> and related classes
/// </summary>
public class MenuFlyoutItemTemplateSettings : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<MenuFlyoutItemTemplateSettings, FAIconElement>(nameof(Icon));

    /// <summary>
    /// Represents the FAIconElement for the MenuFlyoutItem
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        internal set => SetValue(IconProperty, value);
    }
}
