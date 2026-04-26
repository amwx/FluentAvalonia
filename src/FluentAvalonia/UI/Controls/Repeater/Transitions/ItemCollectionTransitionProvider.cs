#pragma warning disable
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

[Flags]
public enum ItemCollectionTransitionTriggers
{
    CollectionChangeAdd = 1,
    CollectionChangeRemove = 2,
    CollectionChangeReset = 4,
    LayoutTransition = 8
}

public enum ItemCollectionTransitionOperation
{
    Add = 0,
    Remove = 1,
    Move = 2
}

public abstract class ItemCollectionTransitionProvider
{
    public event TypedEventHandler<ItemCollectionTransitionProvider, ItemCollectionTransitionCompletedEventArgs> TransitionCompleted;

    public void QueueTransition(ItemCollectionTransition transition)
    {
        _transitions ??= new List<ItemCollectionTransition>();
        _transitionsWithAnimations ??= new List<ItemCollectionTransition>();

        if (FAUISettings.AreAnimationsEnabled() && ShouldAnimate(transition))
        {
            _transitionsWithAnimations.Add(transition);
        }

        // To ensure proper VirtualizationInfo ordering, we still need to raise TransitionCompleted in a
        // CompositionTarget.Rendering handler, even if we aren't going to animate anything for this transition.
        _transitions.Add(transition);

        if (!_rendering)
        {
            Dispatcher.UIThread.Post(OnRendering, DispatcherPriority.Render);
        }
    }

    public bool ShouldAnimate(ItemCollectionTransition transition)
    {
        return ShouldAnimateCore(transition);
    }

    public abstract void StartTransitions(IList<ItemCollectionTransition> transitions);

    public abstract bool ShouldAnimateCore(ItemCollectionTransition transition);

    public void NotifyTransitionComplete(ItemCollectionTransition transition)
    {
        TransitionCompleted?.Invoke(this, new ItemCollectionTransitionCompletedEventArgs(transition));
    }

    private void OnRendering()
    {
        _rendering = false;

        try
        {
            StartTransitions(_transitionsWithAnimations);

            // We'll automatically raise TransitionCompleted on all of the transitions that were not actually animated
            // in order to guarantee that every transition queued receives a corresponding TransitionCompleted event.
            foreach (var transition in _transitions)
            {
                if (!transition.HasStarted)
                    NotifyTransitionComplete(transition);
            }
        }
        finally
        {
            ResetState();
        }
    }

    private void ResetState()
    {
        _transitions.Clear();
        _transitionsWithAnimations.Clear();
    }

    private List<ItemCollectionTransition> _transitions;
    private List<ItemCollectionTransition> _transitionsWithAnimations;
    private bool _rendering;
}
