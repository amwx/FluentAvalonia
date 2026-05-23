using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a templated button control to be displayed in an <see cref="FACommandBar"/>
/// </summary>
[PseudoClasses(FASharedPseudoclasses.s_pcIcon, FASharedPseudoclasses.s_pcLabel, FASharedPseudoclasses.s_pcCompact)]
[PseudoClasses(FASharedPseudoclasses.s_pcFlyout, s_pcSubmenuOpen, FASharedPseudoclasses.s_pcOverflow)]
[PseudoClasses(FASharedPseudoclasses.s_pcHotkey)]
public partial class FACommandBarButton : Button, IFACommandBarElement
{
    /// <summary>
    /// Defines the <see cref="IsInOverflow"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarButton, bool> IsInOverflowProperty =
        AvaloniaProperty.RegisterDirect<FACommandBarButton, bool>(nameof(IsInOverflow),
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
        AvaloniaProperty.Register<FACommandBarButton, string>(nameof(Label));

    /// <summary>
    /// Defines the <see cref="DynamicOverflowOrder"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarButton, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<FACommandBarButton, int>(nameof(DynamicOverflowOrder),
            x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<FACommandBarButton, bool>(nameof(IsCompact));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FACommandBarButtonTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FACommandBarButton, FACommandBarButtonTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Gets or sets a value that indicates whether the button is shown with no label and reduced padding.
    /// </summary>
    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    /// Gets a value that indicates whether this item is in the overflow menu.
    /// </summary>
    public bool IsInOverflow
    {
        get => _isInOverflow;
        internal set
        {
            if (SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value))
            {
                PseudoClasses.Set(FASharedPseudoclasses.s_pcOverflow, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the graphic content of the app bar toggle button.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the text description displayed on the app bar toggle button.
    /// </summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <inheritdoc/>
    public int DynamicOverflowOrder
    {
        get => _dynamicOverflowOrder;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
    }

    /// <summary>
    /// Gets the template settings for this CommandBarButton
    /// </summary>
    public FACommandBarButtonTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    private bool _isInOverflow;
    private int _dynamicOverflowOrder;

    private const string s_pcSubmenuOpen = ":submenuopen";
}
