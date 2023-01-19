using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Stores settings for use in the template of a CommandBarButton
/// </summary>
public class CommandBarButtonTemplateSettings : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        MenuFlyoutItemTemplateSettings.IconProperty.AddOwner<CommandBarButtonTemplateSettings>();

    /// <summary>
    /// Gets the Icon for the CommandBarButton
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        internal set => SetValue(IconProperty, value);
    }
}
