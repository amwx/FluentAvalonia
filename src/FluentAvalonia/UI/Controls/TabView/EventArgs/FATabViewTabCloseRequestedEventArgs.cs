namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for a tab close event
/// </summary>
public class FATabViewTabCloseRequestedEventArgs : EventArgs
{
    internal FATabViewTabCloseRequestedEventArgs(object item, FATabViewItem tab)
    {
        Item = item;
        Tab = tab;
    }

    /// <summary>
    /// Gets a value that represents the data context for the tab in which 
    /// a close is being requested
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the tab in which a close is being requested
    /// </summary>
    public FATabViewItem Tab { get; }
}
