using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class ItemsRepeaterElementClearingEventArgs : EventArgs
{
    internal ItemsRepeaterElementClearingEventArgs(Control element) => Element = element;

    /// <summary>
    /// Gets the element that is being cleared for re-use.
    /// </summary>
    public Control Element { get; private set; }

    internal void Update(Control element) => Element = element;
}
