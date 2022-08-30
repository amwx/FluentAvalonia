using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides calculated values that can be referenced as TemplatedParent sources 
/// when defining templates for an InfoBadge.
/// </summary>
public class InfoBadgeTemplateSettings : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="InfoBadgeCornerRadius"/> property
    /// </summary>
    public static readonly StyledProperty<CornerRadius> InfoBadgeCornerRadiusProperty =
        AvaloniaProperty.Register<InfoBadgeTemplateSettings, CornerRadius>(nameof(InfoBadgeCornerRadius));

    /// <summary>
    /// Defines the <see cref="IconElement"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconElementProperty =
        AvaloniaProperty.Register<InfoBadgeTemplateSettings, FAIconElement>(nameof(IconElement));

    /// <summary>
    /// Gets or sets the corner radius for an InfoBadge.
    /// </summary>
    public CornerRadius InfoBadgeCornerRadius
    {
        get => GetValue(InfoBadgeCornerRadiusProperty);
        internal set => SetValue(InfoBadgeCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon element for an InfoBadge.
    /// </summary>
    public FAIconElement IconElement
    {
        get => GetValue(IconElementProperty);
        internal set => SetValue(IconElementProperty, value);
    }
}
