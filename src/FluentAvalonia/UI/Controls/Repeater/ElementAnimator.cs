using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.Core;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

[Flags]
public enum AnimationContext
{
    None = 0,
    CollectionChangeAdd = 1,
    CollectionChangeRemove = 2,
    CollectionChangeReset = 4,
    LayoutTransition= 8
}

public abstract class ElementAnimator
{
    public bool HasShowAnimationsPending => _hasShowAnimationsPending;

    public bool HasHideAnimationsPending => _hasHideAnimationsPending;

    public bool HasBoundsChangeAnimationsPending => _hasBoundsChangeAnimationsPending;

    public AnimationContext SharedContext => _sharedContext;

    public event TypedEventHandler<ElementAnimator, Control> ShowAnimationCompleted;
    public event TypedEventHandler<ElementAnimator, Control> HideAnimationCompleted;
    public event TypedEventHandler<ElementAnimator, Control> BoundsChangeAnimationCompleted;

    public void OnElementShown(Control element, AnimationContext context)
    {
        if (HasShowAnimation(element, context))
        {
            _hasShowAnimationsPending = true;
            _sharedContext |= context;
            QueueElementForAnimation(
                new ElementInfo(element, AnimationTrigger.Show, context));
        }
    }

    public void OnElementHidden(Control element, AnimationContext context)
    {
        if (HasHideAnimation(element, context))
        {
            _hasHideAnimationsPending = true;
            _sharedContext |= context;
            QueueElementForAnimation(
                new ElementInfo(element, AnimationTrigger.Hide, context));
        }
    }

    public void OnElementBoundsChanged(Control element, AnimationContext context,
        Rect oldBounds, Rect newBounds)
    {
        if (HasBoundsChangeAnimation(element, context, oldBounds, newBounds))
        {
            _hasBoundsChangeAnimationsPending = true;
            _sharedContext |= context;
            QueueElementForAnimation(
                new ElementInfo(element, AnimationTrigger.BoundsChange, context, oldBounds, newBounds));
        }
    }

    public abstract bool HasShowAnimation(Control element, AnimationContext context);

    public abstract bool HasHideAnimation(Control element, AnimationContext context);

    public abstract bool HasBoundsChangeAnimation(Control element,
        AnimationContext context, Rect oldBounds, Rect newBounds);

    public abstract void StartShowAnimation(Control element, AnimationContext context);

    public abstract void StartHideAnimation(Control element, AnimationContext context);

    public abstract void StartBoundsChangeAnimation(Control element,
        AnimationContext context, Rect oldBounds, Rect newBounds);


    protected virtual void OnShowAnimationCompleted(Control element)
    {
        ShowAnimationCompleted?.Invoke(this, element);
    }

    protected virtual void OnHideAnimationCompleted(Control element)
    {
        HideAnimationCompleted?.Invoke(this, element);
    }

    protected virtual void OnBoundsChangeAnimationCompleted(Control element)
    {
        BoundsChangeAnimationCompleted?.Invoke(this, element);
    }

    private void QueueElementForAnimation(ElementInfo info)
    {
        _animatingElements ??= new List<ElementInfo>();
        _animatingElements.Add(info);
        if (_animatingElements.Count == 1)
        {
            Dispatcher.UIThread.Post(() => OnRendering(null, null), DispatcherPriority.Render);
           // CompositionTarget.Rendering += OnRendering;
        }
    }

    private void OnRendering(object sender, EventArgs args)
    {
        // CompositionTarget.Rendering -= OnRendering;
        try
        {
            foreach (var elementInfo in _animatingElements)
            {
                switch (elementInfo.Trigger)
                {
                    case AnimationTrigger.Show:
                        StartShowAnimation(elementInfo.Element, elementInfo.Context);
                        break;

                    case AnimationTrigger.Hide:
                        StartHideAnimation(elementInfo.Element, elementInfo.Context);
                        break;

                    case AnimationTrigger.BoundsChange:
                        StartBoundsChangeAnimation(elementInfo.Element,
                            elementInfo.Context, elementInfo.OldBounds, elementInfo.NewBounds);
                        break;
                }
            }
        }
        finally
        {
            ResetState();
        }
    }

    private void ResetState()
    {
        _animatingElements.Clear();
        _hasShowAnimationsPending = _hasHideAnimationsPending =
            _hasBoundsChangeAnimationsPending = false;
        _sharedContext = AnimationContext.None;
    }

    private List<ElementInfo> _animatingElements;
    private bool _hasShowAnimationsPending;
    private bool _hasHideAnimationsPending;
    private bool _hasBoundsChangeAnimationsPending;
    private AnimationContext _sharedContext;

    private enum AnimationTrigger
    {
        Show,
        Hide,
        BoundsChange
    }

    private readonly struct ElementInfo
    {
        public ElementInfo(Control element,
            AnimationTrigger trigger, AnimationContext context)
        {
            Element = element;
            Trigger = trigger;
            Context = context;
            Debug.Assert(trigger != AnimationTrigger.BoundsChange);
        }

        public ElementInfo(Control element,
            AnimationTrigger trigger, AnimationContext context,
            Rect oldBounds, Rect newBounds)
        {
            Element = element;
            Trigger = trigger;
            Context = context;
            OldBounds = oldBounds;
            NewBounds = newBounds;
            Debug.Assert(trigger == AnimationTrigger.BoundsChange);
        }

        public Control Element { get; }
        public AnimationTrigger Trigger { get; }
        public AnimationContext Context { get; }
        public Rect OldBounds { get; }
        public Rect NewBounds { get; }
    }
}
