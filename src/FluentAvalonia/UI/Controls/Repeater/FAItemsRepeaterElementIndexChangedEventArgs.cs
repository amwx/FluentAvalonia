using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FAItemsRepeater.ElementIndexChanged"/> event
/// </summary>
public class FAItemsRepeaterElementIndexChangedEventArgs : EventArgs
{
    internal FAItemsRepeaterElementIndexChangedEventArgs(Control element, int oldIndex, int newIndex)
    {
        Element = element;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    /// <summary>
    /// Get the element for which the index changed.
    /// </summary>
    public Control Element { get; private set; }

    /// <summary>
    /// Gets the index of the element after the change.
    /// </summary>
    public int NewIndex { get; private set; }

    /// <summary>
    /// Gets the index of the element before the change.
    /// </summary>
    public int OldIndex { get; private set; }

    internal void Update(Control element, int oldIndex, int newIndex)
    {
        Element = element;
        NewIndex = newIndex;
        OldIndex = oldIndex;
    }
}
