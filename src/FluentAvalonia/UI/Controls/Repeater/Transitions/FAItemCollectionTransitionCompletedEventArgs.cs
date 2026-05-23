#pragma warning disable
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class FAItemCollectionTransitionCompletedEventArgs : EventArgs
{
    public FAItemCollectionTransitionCompletedEventArgs(FAItemCollectionTransition transition)
    {

    }

    public FAItemCollectionTransition Transition { get; }

    public Control Element { get; }
}
