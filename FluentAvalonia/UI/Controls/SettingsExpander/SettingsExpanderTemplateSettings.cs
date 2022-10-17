using Avalonia;

namespace FluentAvalonia.UI.Controls;

public class SettingsExpanderTemplateSettings : AvaloniaObject
{
    internal SettingsExpanderTemplateSettings() { }

    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(Icon));

    public static readonly StyledProperty<FAIconElement> ActionIconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(ActionIcon));

    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public FAIconElement ActionIcon
    {
        get => GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }
}
