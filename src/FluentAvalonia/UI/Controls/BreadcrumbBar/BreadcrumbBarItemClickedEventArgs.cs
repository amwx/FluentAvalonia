namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="BreadcrumbBar.ItemClicked"/> event
/// </summary>
public class BreadcrumbBarItemClickedEventArgs : EventArgs
{
    internal BreadcrumbBarItemClickedEventArgs(int index, object item)
    {
        Index = index;
        Item = item;
    }

    /// <summary>
    /// Gets the index of the item that was clicked.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the Content property value of the BreadcrumbBarItem that is clicked.
    /// </summary>
    public object Item { get; }
}
