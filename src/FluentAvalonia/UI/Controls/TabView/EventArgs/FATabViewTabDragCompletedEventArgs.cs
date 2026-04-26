using Avalonia.Input;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FATabView.TabDragCompleted"/> event
/// </summary>
public class FATabViewTabDragCompletedEventArgs : EventArgs
{
    internal FATabViewTabDragCompletedEventArgs(DragItemsCompletedEventArgs args, object item, FATabViewItem tab)
    {
        _innerArgs = args;
        Item = item;
        Tab = tab;
    }

    /// <summary>
    /// Gets a value that indicates what operation was performed on the dragged data,
    /// and whether it was successful
    /// </summary>
    public DragDropEffects DropResult => _innerArgs.DropResult;

    /// <summary>
    /// Gets the item that was selected for the drag action
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the TabViewItem that was selected for the drag action
    /// </summary>
    public FATabViewItem Tab { get; }

    private DragItemsCompletedEventArgs _innerArgs;
}
