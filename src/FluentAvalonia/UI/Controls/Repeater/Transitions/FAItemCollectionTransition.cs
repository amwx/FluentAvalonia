#pragma warning disable
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class FAItemCollectionTransition
{
    public FAItemCollectionTransition(FAItemCollectionTransitionProvider provider, Control element,
        FAItemCollectionTransitionOperation operation, FAItemCollectionTransitionTriggers triggers)
        : this(provider, element, operation, triggers, default, default)
    {
        Debug.Assert(operation != FAItemCollectionTransitionOperation.Move);
    }

    public FAItemCollectionTransition(FAItemCollectionTransitionProvider provider, Control element,
        FAItemCollectionTransitionTriggers triggers,
        Rect oldBounds, Rect newBounds)
        : this(provider, element, FAItemCollectionTransitionOperation.Move, triggers, 
              oldBounds, newBounds)
    {

    }

    public FAItemCollectionTransition(FAItemCollectionTransitionProvider provider, Control element,
        FAItemCollectionTransitionOperation operation, FAItemCollectionTransitionTriggers triggers,
        Rect oldBounds, Rect newBounds)
    {
        _owningProvider = new WeakReference<FAItemCollectionTransitionProvider>(provider);
        _element = element;
        _operation = operation;
        _triggers = triggers;
        _oldBounds = oldBounds;
        _newBounds = newBounds;
    }

    public FAItemCollectionTransitionProvider OwningProvider => _owningProvider.TryGetTarget(out var target) ? target : null;

    public Control Element => _element;

    public bool HasStarted => _progress != null;

    public FAItemCollectionTransitionOperation Operation => _operation;

    public FAItemCollectionTransitionTriggers Triggers => _triggers;

    public FAItemCollectionTransitionProgress Start()
    {
        _progress ??= new FAItemCollectionTransitionProgress(this);

        return _progress;
    }

    private WeakReference<FAItemCollectionTransitionProvider> _owningProvider;
    private Control _element;
    private FAItemCollectionTransitionOperation _operation;
    private FAItemCollectionTransitionTriggers _triggers;
    private Rect _oldBounds;
    private Rect _newBounds;
    private FAItemCollectionTransitionProgress _progress;
}
