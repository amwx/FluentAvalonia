using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a control for indicating notifications, alerts, new content, 
/// or to attract focus to an area within an app.
/// </summary>
[PseudoClasses(s_pcValue, s_pcFontIcon, FASharedPseudoclasses.s_pcIcon, s_pcDot)]
public partial class FAInfoBadge : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Value"/> property
    /// </summary>
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<FAInfoBadge, int>(nameof(Value), -1);

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FAInfoBadge>();

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FAInfoBadgeTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FAInfoBadge, FAInfoBadgeTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Gets or sets the integer to be displayed in a numeric InfoBadge.
    /// </summary>
    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon to be used in an InfoBadge.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Provides calculated values that can be referenced as TemplatedParent sources when defining 
    /// templates for an InfoBadge. Not intended for general use.
    /// </summary>
    public FAInfoBadgeTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        internal set => SetValue(TemplateSettingsProperty, value);
    }

    private const string s_pcValue = ":value";
    private const string s_pcFontIcon = ":fonticon";
    private const string s_pcDot = ":dot";
}
