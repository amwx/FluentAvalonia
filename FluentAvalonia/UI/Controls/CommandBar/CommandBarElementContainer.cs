using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a container that allows an element that doesn't implement ICommandBarElement 
/// to be displayed in a command bar.
/// </summary>
public class CommandBarElementContainer : ContentControl, ICommandBarElement, IStyleable
{
    Type IStyleable.StyleKey => typeof(CommandBarElementContainer);

    /// <summary>
    /// Defines the <see cref="IsInOverflow"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBarElementContainer, bool> IsInOverflowProperty =
        AvaloniaProperty.RegisterDirect<CommandBarElementContainer, bool>(nameof(IsInOverflow),
                x => x.IsInOverflow);

    /// <summary>
    /// Defines the <see cref="DynamicOverflowOrder"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBarElementContainer, int> DynamicOverflowOrderProperty =
        AvaloniaProperty.RegisterDirect<CommandBarElementContainer, int>(nameof(DynamicOverflowOrder),
            x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<CommandBarElementContainer, bool>(nameof(IsCompact));

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

    public int DynamicOverflowOrder
    {
        get => _dynamicOverflowOrder;
        set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
    }

    private bool _isInOverflow;
    private int _dynamicOverflowOrder;
}
