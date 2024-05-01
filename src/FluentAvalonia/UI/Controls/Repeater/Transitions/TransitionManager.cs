using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;
internal sealed class TransitionManager
{
    public TransitionManager(ItemsRepeater owner)
    {
        _owner = owner;
    }

    public void OnTransitionProviderChanged(ItemCollectionTransitionProvider newProvider)
    {
        // While an element is hiding, we have ownership of it. We need
        // to know when its animation completes so that we give it back
        // to the view generator.
        if (_transitionProvider != null)
        {
            //m_transitionProvider.get().TransitionCompleted(m_transitionCompleted);
            _transitionProvider.TransitionCompleted -= OnTransitionProviderTransitionCompleted;
        }

        _transitionProvider = newProvider;

        if (newProvider != null)
        {
            newProvider.TransitionCompleted += OnTransitionProviderTransitionCompleted;
        }
    }

    public void OnLayoutChanging()
    {
        _hasRecordedLayoutTransitions = true;
    }

    public void OnItemsSourceChanged(object _, NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _hasRecordedAdds = true;
                break;

            case NotifyCollectionChangedAction.Remove:
                _hasRecordedRemoves = true;
                break;

            case NotifyCollectionChangedAction.Replace:
                _hasRecordedAdds = true;
                _hasRecordedRemoves = true;
                break;

            case NotifyCollectionChangedAction.Reset:
                _hasRecordedResets = true;
                break;
        }
    }

    public void OnElementPrepared(Control element)
    {
        if (_transitionProvider != null)
        {
            var triggers = (ItemCollectionTransitionTriggers)0;

            if (_hasRecordedAdds)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeAdd;

            if (_hasRecordedResets)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeReset;

            if (_hasRecordedLayoutTransitions)
                triggers |= ItemCollectionTransitionTriggers.LayoutTransition;

            if (triggers != 0)
            {
                _transitionProvider.QueueTransition(
                    new ItemCollectionTransition(_transitionProvider, element, ItemCollectionTransitionOperation.Add, triggers));
            }
        }
    }

    public bool ClearElement(Control element)
    {
        bool canClear = false;

        if (_transitionProvider != null)
        {
            var triggers = (ItemCollectionTransitionTriggers)0;
            if (_hasRecordedRemoves)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeRemove;
            if (_hasRecordedResets)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeReset;

            var transition = new ItemCollectionTransition(_transitionProvider, element, ItemCollectionTransitionOperation.Remove, triggers);

            canClear = triggers != 0 && _transitionProvider.ShouldAnimate(transition);

            if (canClear)
                _transitionProvider.QueueTransition(transition);
        }

        return canClear;
    }

    public void OnElementBoundsChanged(Control element, Rect oldBounds, Rect newBounds)
    {
        if (_transitionProvider != null)
        {
            var triggers = (ItemCollectionTransitionTriggers)0;
            if (_hasRecordedAdds)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeAdd;
            if (_hasRecordedRemoves)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeRemove;
            if (_hasRecordedRemoves)
                triggers |= ItemCollectionTransitionTriggers.CollectionChangeReset;
            if (_hasRecordedLayoutTransitions)
                triggers |= ItemCollectionTransitionTriggers.LayoutTransition;

            // A bounds change can occur during initial layout or when resizing the owning control,
            // which won't trigger an explicit layout transition but should still be treated as one.
            if (triggers == 0)
                triggers = ItemCollectionTransitionTriggers.LayoutTransition;

            _transitionProvider.QueueTransition(
                new ItemCollectionTransition(_transitionProvider, element, triggers, oldBounds, newBounds));
        }
    }

    public void OnOwnerArranged()
    {
        _hasRecordedAdds = _hasRecordedRemoves = _hasRecordedLayoutTransitions = _hasRecordedResets = false;
    }

    private void OnTransitionProviderTransitionCompleted(ItemCollectionTransitionProvider sender, ItemCollectionTransitionCompletedEventArgs args)
    {
        if (args.Transition.Operation == ItemCollectionTransitionOperation.Remove)
        {
            var element = args.Element;

            if (element.GetVisualParent() == _owner)
            {
                _owner.ViewManager.ClearElementToElementFactory(element);

                // Invalidate arrange so that repeater can arrange this element off-screen.
                _owner.InvalidateArrange();
            }
        }
    }

    private readonly ItemsRepeater _owner;
    private ItemCollectionTransitionProvider _transitionProvider;

    private bool _hasRecordedAdds;
    private bool _hasRecordedRemoves;
    private bool _hasRecordedResets;
    private bool _hasRecordedLayoutTransitions;
}
