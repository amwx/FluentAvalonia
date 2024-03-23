using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

internal class AnimationManager
{
    public AnimationManager(ItemsRepeater owner)
    {
        _owner = owner;
    }

    public void OnAnimatorChanged(ElementAnimator newAnimator)
    {
        // While an element is hiding, we have ownership of it. We need
        // to know when its animation completes so that we give it back
        // to the view generator.
        if (_animator != null)
        {
            _animator.HideAnimationCompleted -= OnHideAnimationCompleted;
        }

        _animator = newAnimator;

        if (newAnimator != null)
        {
            newAnimator.HideAnimationCompleted += OnHideAnimationCompleted;
        }
    }

    public void OnLayoutChanging()
    {
        _hasRecordedLayoutTransitions = true;
    }

    public void OnItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs args)
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
        if (_animator == null)
            return;

        var context = AnimationContext.None;
        if (_hasRecordedAdds)
            context |= AnimationContext.CollectionChangeAdd;
        if (_hasRecordedResets)
            context |= AnimationContext.CollectionChangeReset;
        if (_hasRecordedLayoutTransitions)
            context |= AnimationContext.LayoutTransition;

        if (context != AnimationContext.None)
            _animator.OnElementShown(element, context);
    }

    public bool ClearElement(Control element)
    {
        bool canClear = false;

        if (_animator != null)
        {
            var context = AnimationContext.None;
            if (_hasRecordedRemoves)
                context |= AnimationContext.CollectionChangeRemove;
            if (_hasRecordedResets)
                context |= AnimationContext.CollectionChangeReset;

            canClear = context != AnimationContext.None &&
                _animator.HasHideAnimation(element, context);

            if (canClear)
                _animator.OnElementHidden(element, context);
        }

        return canClear;
    }

    public void OnElementBoundsChanged(Control element, Rect oldBounds, Rect newBounds)
    {
        if (_animator == null)
            return;

        var context = AnimationContext.None;
        if (_hasRecordedAdds) 
            context |= AnimationContext.CollectionChangeAdd;
        if (_hasRecordedRemoves) 
            context |= AnimationContext.CollectionChangeRemove;
        if (_hasRecordedResets) 
            context |= AnimationContext.CollectionChangeReset;
        if (_hasRecordedLayoutTransitions) 
            context |= AnimationContext.LayoutTransition;

        _animator.OnElementBoundsChanged(element, context, oldBounds, newBounds);
    }

    public void OnOwnerArranged()
    {
        _hasRecordedAdds = _hasRecordedLayoutTransitions =
            _hasRecordedRemoves = _hasRecordedResets = false;
    }

    public void OnHideAnimationCompleted(ElementAnimator sender, Control element)
    {
        var parent = element.GetVisualParent();
        if (parent != _owner)
            return;

        _owner.ViewManager.ClearElementToElementFactory(element);
        _owner.InvalidateArrange();
    }

    private ItemsRepeater _owner;
    private ElementAnimator _animator;
    private bool _hasRecordedAdds;
    private bool _hasRecordedRemoves;
    private bool _hasRecordedResets;
    private bool _hasRecordedLayoutTransitions;
}
