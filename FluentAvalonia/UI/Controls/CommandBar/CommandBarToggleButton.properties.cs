using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Controls;

public partial class CommandBarToggleButton : ToggleButton, ICommandBarElement, IStyleable
{
    /// <summary>
    /// Defines the <see cref="IsInOverflow"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBarToggleButton, bool> IsInOverflowProperty =
            AvaloniaProperty.RegisterDirect<CommandBarToggleButton, bool>(nameof(IsInOverflow),
                x => x.IsInOverflow);

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<CommandBarToggleButton, FAIconElement>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="Label"/> property
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<CommandBarToggleButton, string>(nameof(Label));

    /// <summary>
    /// Defines the <see cref="DynamicOverflowOrder"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBarToggleButton, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<CommandBarToggleButton, int>(nameof(DynamicOverflowOrder),
            x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<CommandBarToggleButton, bool>(nameof(IsCompact));

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public bool IsInOverflow
    {
        get => _isInOverflow;
        internal set
        {
            if (SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value))
            {
                PseudoClasses.Set(":overflow", value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the graphic content of the command bar toggle button.
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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
        get => _dynamicOverflowOrder;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
    }

    private bool _isInOverflow;
    private int _dynamicOverflowOrder;
}
