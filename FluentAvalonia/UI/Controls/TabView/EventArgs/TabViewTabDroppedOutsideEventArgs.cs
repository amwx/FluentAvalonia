using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="TabView.TabDroppedOutside"/> event
/// </summary>
public class TabViewTabDroppedOutsideEventArgs : EventArgs
{
    internal TabViewTabDroppedOutsideEventArgs(object item, TabViewItem tab)
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
    public TabViewItem Tab { get; }
}
