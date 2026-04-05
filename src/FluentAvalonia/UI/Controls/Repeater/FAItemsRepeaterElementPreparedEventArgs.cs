using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FAItemsRepeater.ElementPrepared"/> event
/// </summary>
public class FAItemsRepeaterElementPreparedEventArgs : EventArgs
{
    internal FAItemsRepeaterElementPreparedEventArgs(Control element, int index)
    {
        Element = element;
        Index = index;
    }

    /// <summary>
    /// Gets the prepared element.
    /// </summary>
    public Control Element { get; private set; }

    /// <summary>
    /// Gets the index of the item the element was prepared for.
    /// </summary>
    public int Index { get; private set; }

    internal void Update(Control element, int index)
    {
        Element = element;
        Index = index;
    }
}
