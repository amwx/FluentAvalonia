using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides calculated values that can be referenced as TemplatedParent sources when 
/// defining templates for a <see cref="FATeachingTip"/>.
/// </summary>
public sealed class FATeachingTipTemplateSettings : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="TopRightHighlightMargin"/> property
    /// </summary>
    public static readonly StyledProperty<Thickness> TopRightHighlighMarginProperty =
        AvaloniaProperty.Register<FATeachingTipTemplateSettings, Thickness>(nameof(TopRightHighlightMargin));

    /// <summary>
    /// Defines the <see cref="TopLeftHighlightMargin"/> property
    /// </summary>
    public static readonly StyledProperty<Thickness> TopLeftHighlightMarginProperty =
        AvaloniaProperty.Register<FATeachingTipTemplateSettings, Thickness>(nameof(TopLeftHighlightMargin));

    /// <summary>
    /// Defines the <see cref="IconElement"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconElementProperty =
        AvaloniaProperty.Register<FATeachingTipTemplateSettings, FAIconElement>(nameof(IconElement));

    /// <summary>
    /// Gets the thickness value of the top right highlight margin.
    /// </summary>
    public Thickness TopRightHighlightMargin
    {
        get => GetValue(TopRightHighlighMarginProperty);
        internal set => SetValue(TopRightHighlighMarginProperty, value);
    }

    /// <summary>
    /// Gets the thickness value of the top left highlight margin.
    /// </summary>
    public Thickness TopLeftHighlightMargin
    {
        get => GetValue(TopLeftHighlightMarginProperty);
        internal set => SetValue(TopLeftHighlightMarginProperty, value);
    }

    /// <summary>
    /// Gets the icon element.
    /// </summary>
    public FAIconElement IconElement
    {
        get => GetValue(IconElementProperty);
        internal set => SetValue(IconElementProperty, value);
    }
}
