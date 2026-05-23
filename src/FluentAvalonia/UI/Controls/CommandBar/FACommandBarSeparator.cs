using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a separator in between items in a <see cref="FACommandBar"/>
/// </summary>
[PseudoClasses(FASharedPseudoclasses.s_pcOverflow)]
public class FACommandBarSeparator : TemplatedControl, IFACommandBarElement
{
    /// <summary>
    /// Defines the <see cref="IsInOverflow"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarSeparator, bool> IsInOverflowProperty =
            AvaloniaProperty.RegisterDirect<FACommandBarSeparator, bool>(nameof(IsInOverflow),
                x => x.IsInOverflow);

    /// <summary>
    /// Defines the <see cref="DynamicOverflowOrder"/> property
    /// </summary>
    public static readonly DirectProperty<FACommandBarSeparator, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<FACommandBarSeparator, int>(nameof(DynamicOverflowOrder),
            x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<FACommandBarSeparator, bool>(nameof(IsCompact));

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
                PseudoClasses.Set(FASharedPseudoclasses.s_pcOverflow, value);
            }
        }
    }

    public int DynamicOverflowOrder
    {
        get => _dynamicOverflowOrder;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
    }

    private bool _isInOverflow;
    private int _dynamicOverflowOrder;
}
