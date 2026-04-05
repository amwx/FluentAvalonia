#pragma warning disable
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

[Flags]
public enum FAItemCollectionTransitionTriggers
{
    CollectionChangeAdd = 1,
    CollectionChangeRemove = 2,
    CollectionChangeReset = 4,
    LayoutTransition = 8
}

public enum FAItemCollectionTransitionOperation
{
    Add = 0,
    Remove = 1,
    Move = 2
}

public abstract class FAItemCollectionTransitionProvider
{
    public event TypedEventHandler<FAItemCollectionTransitionProvider, FAItemCollectionTransitionCompletedEventArgs> TransitionCompleted;

    public void QueueTransition(FAItemCollectionTransition transition)
    {
        _transitions ??= new List<FAItemCollectionTransition>();
        _transitionsWithAnimations ??= new List<FAItemCollectionTransition>();

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

    public bool ShouldAnimate(FAItemCollectionTransition transition)
    {
        return ShouldAnimateCore(transition);
    }

    public abstract void StartTransitions(IList<FAItemCollectionTransition> transitions);

    public abstract bool ShouldAnimateCore(FAItemCollectionTransition transition);

    public void NotifyTransitionComplete(FAItemCollectionTransition transition)
    {
        TransitionCompleted?.Invoke(this, new FAItemCollectionTransitionCompletedEventArgs(transition));
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

    private List<FAItemCollectionTransition> _transitions;
    private List<FAItemCollectionTransition> _transitionsWithAnimations;
    private bool _rendering;
}
