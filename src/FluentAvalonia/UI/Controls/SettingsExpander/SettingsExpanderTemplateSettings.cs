using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents data for use in a SettingsExpander temlate
/// </summary>
public class SettingsExpanderTemplateSettings : AvaloniaObject
{
    internal SettingsExpanderTemplateSettings() { }

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="ActionIcon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> ActionIconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(ActionIcon));

    /// <summary>
    /// Defines the FAIconElement to be used for the SettingsExpander
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Defines the FAIconElement to be used for the SettingsExpander ActionIcon
    /// </summary>
    public FAIconElement ActionIcon
    {
        get => GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }
}
