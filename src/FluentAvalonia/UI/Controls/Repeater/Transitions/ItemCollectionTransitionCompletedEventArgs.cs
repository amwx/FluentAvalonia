#pragma warning disable
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class ItemCollectionTransitionCompletedEventArgs : EventArgs
{
    public ItemCollectionTransitionCompletedEventArgs(ItemCollectionTransition transition)
    {

    }

    public ItemCollectionTransition Transition { get; }

    public Control Element { get; }
}
