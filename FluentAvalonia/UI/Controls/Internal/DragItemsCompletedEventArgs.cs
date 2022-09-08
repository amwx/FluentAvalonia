using System.Collections.Generic;
using Avalonia.Input;

public class DragItemsCompletedEventArgs
{
    internal DragItemsCompletedEventArgs(DragDropEffects result, IList<object> items)
    {
        DropResult = result;
        Items = new List<object>(items);
    }

    public DragDropEffects DropResult { get; internal init; }

    public IReadOnlyList<object> Items { get; internal init; }
}
