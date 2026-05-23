using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FAItemsRepeater.ElementClearing"/> event
/// </summary>
public class FAItemsRepeaterElementClearingEventArgs : EventArgs
{
    internal FAItemsRepeaterElementClearingEventArgs(Control element) => Element = element;

    /// <summary>
    /// Gets the element that is being cleared for re-use.
    /// </summary>
    public Control Element { get; private set; }

    internal void Update(Control element) => Element = element;
}
