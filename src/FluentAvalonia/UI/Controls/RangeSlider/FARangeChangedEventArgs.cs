namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate which value was changed in a <see cref="FARangeSlider"/>
/// </summary>
public enum FARangeSelectorProperty
{
    /// <summary>
    /// The RangeStart value was changed
    /// </summary>
    RangeStartValue,

    /// <summary>
    /// The RangeEnd value was changed
    /// </summary>
    RangeEndValue
}

/// <summary>
/// Event data for the <see cref="FARangeSlider.ValueChanged" /> event
/// </summary>
public class FARangeChangedEventArgs : EventArgs
{
    internal FARangeChangedEventArgs(double oldValue, double newValue, FARangeSelectorProperty prop)
    {
        OldValue = oldValue;
        NewValue = newValue;
        ChangedProperty = prop;
    }

    /// <summary>
    /// Gets the old value for the property identified by <see cref="ChangedProperty"/>
    /// </summary>
    public double OldValue { get; }

    /// <summary>
    /// Gets the new value for the property identified by <see cref="ChangedProperty"/>
    /// </summary>
    public double NewValue { get; }

    /// <summary>
    /// Gets the property that changed to trigger this event
    /// </summary>
    public FARangeSelectorProperty ChangedProperty { get; }
}
