using Avalonia.Input;

/// <summary>
/// Provides event data for the DragItemsCompleted event
/// </summary>
public class DragItemsCompletedEventArgs
{
    internal DragItemsCompletedEventArgs(DragDropEffects result, IList<object> items)
    {
        DropResult = result;
        Items = new List<object>(items);
    }

    /// <summary>
    /// Gets a value that indicates what operation was performed on the dragged data, and
    /// whether it was successful
    /// </summary>
    public DragDropEffects DropResult { get; internal init; }

    /// <summary>
    /// Gets a loosely typed collection of objects that are selected for item drag action
    /// </summary>
    public IReadOnlyList<object> Items { get; internal init; }
}
