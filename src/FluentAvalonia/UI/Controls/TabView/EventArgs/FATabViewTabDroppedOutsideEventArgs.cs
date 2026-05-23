namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FATabView.TabDroppedOutside"/> event
/// </summary>
public class FATabViewTabDroppedOutsideEventArgs : EventArgs
{
    internal FATabViewTabDroppedOutsideEventArgs(object item, FATabViewItem tab)
    {
        Item = item;
        Tab = tab;
    }

    /// <summary>
    /// Gets the item that was dropped outside of the TabStrip
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the TabViewItem that was dropped outside of the TabStrip
    /// </summary>
    public FATabViewItem Tab { get; }
}
