using FluentAvalonia.UI.Data;

/// <summary>
/// Represents the method that will handle the DragItemsStarting event
/// </summary>
public delegate void DragItemsStartingEventHandler(object sender, DragItemsStartingEventArgs args);

/// <summary>
/// Provides event data for the DragItemsStarting event
/// </summary>
public class DragItemsStartingEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value that indicates whether the item drag action should be cancelled
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the data payload associated with an items drag action
    /// </summary>
    public DataPackage Data { get; internal init; }

    /// <summary>
    /// Gets the loosely typed collection of objects that are selected for the item drag action.
    /// </summary>
    public IList<object> Items { get; internal init; }
}
