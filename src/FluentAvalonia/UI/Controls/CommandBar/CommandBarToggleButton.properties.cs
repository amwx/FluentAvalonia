using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public partial class FACommandBarToggleButton : ToggleButton, IFACommandBarElement
{
    /// <summary>
    /// Defines the <see cref="IsInOverflow"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarToggleButton, bool> IsInOverflowProperty =
            AvaloniaProperty.RegisterDirect<FACommandBarToggleButton, bool>(nameof(IsInOverflow),
                x => x.IsInOverflow);

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FACommandBarButton>();

    /// <summary>
    /// Defines the <see cref="Label"/> property
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<FACommandBarToggleButton, string>(nameof(Label));

    /// <summary>
    /// Defines the <see cref="DynamicOverflowOrder"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarToggleButton, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<FACommandBarToggleButton, int>(nameof(DynamicOverflowOrder),
            x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<FACommandBarToggleButton, bool>(nameof(IsCompact));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FACommandBarButtonTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FACommandBarToggleButton, FACommandBarButtonTemplateSettings>(nameof(TemplateSettings));

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public bool IsInOverflow
    {
        get;
        internal set
        {
            if (SetAndRaise(IsInOverflowProperty, ref field, value))
            {
                PseudoClasses.Set(FASharedPseudoclasses.s_pcOverflow, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the graphic content of the command bar toggle button.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the text description displayed on the command bar toggle button.
    /// </summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public int DynamicOverflowOrder
    {
        get;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref field, value);
    }

    /// <summary>
    /// Gets the template settings for this CommandBarButton
    /// </summary>
    public FACommandBarButtonTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }
}
