#pragma warning disable
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class ItemCollectionTransition
{
    public ItemCollectionTransition(ItemCollectionTransitionProvider provider, Control element,
        ItemCollectionTransitionOperation operation, ItemCollectionTransitionTriggers triggers)
        : this(provider, element, operation, triggers, default, default)
    {
        Debug.Assert(operation != ItemCollectionTransitionOperation.Move);
    }

    public ItemCollectionTransition(ItemCollectionTransitionProvider provider, Control element,
        ItemCollectionTransitionTriggers triggers,
        Rect oldBounds, Rect newBounds)
        : this(provider, element, ItemCollectionTransitionOperation.Move, triggers, 
              oldBounds, newBounds)
    {

    }

    public ItemCollectionTransition(ItemCollectionTransitionProvider provider, Control element,
        ItemCollectionTransitionOperation operation, ItemCollectionTransitionTriggers triggers,
        Rect oldBounds, Rect newBounds)
    {
        _owningProvider = new WeakReference<ItemCollectionTransitionProvider>(provider);
        _element = element;
        _operation = operation;
        _triggers = triggers;
        _oldBounds = oldBounds;
        _newBounds = newBounds;
    }

    public ItemCollectionTransitionProvider OwningProvider => _owningProvider.TryGetTarget(out var target) ? target : null;

    public Control Element => _element;

    public bool HasStarted => _progress != null;

    public ItemCollectionTransitionOperation Operation => _operation;

    public ItemCollectionTransitionTriggers Triggers => _triggers;

    public ItemCollectionTransitionProgress Start()
    {
        _progress ??= new ItemCollectionTransitionProgress(this);

        return _progress;
    }

    private WeakReference<ItemCollectionTransitionProvider> _owningProvider;
    private Control _element;
    private ItemCollectionTransitionOperation _operation;
    private ItemCollectionTransitionTriggers _triggers;
    private Rect _oldBounds;
    private Rect _newBounds;
    private ItemCollectionTransitionProgress _progress;
}
