using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

internal class RepeaterLayoutContext : VirtualizingLayoutContext
{
    public RepeaterLayoutContext(ItemsRepeater owner)
    {
        _owner = new WeakReference<ItemsRepeater>(owner);
    }

    protected internal override int ItemCountCore()
    {
        var dataSource = GetOwner()?.ItemsSourceView;
        return dataSource?.Count ?? 0;
    }

    protected override Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
    {
        return GetOwner()?.GetElementImpl(index,
            (options & ElementRealizationOptions.ForceCreate) == ElementRealizationOptions.ForceCreate,
            (options & ElementRealizationOptions.SuppressAutoRecycle) == ElementRealizationOptions.SuppressAutoRecycle);
    }

    protected internal override object LayoutStateCore
    {
        get => GetOwner()?.LayoutState;
        set
        {
            if (GetOwner() is ItemsRepeater ir)
            {
                ir.LayoutState = value;
            }
        }
    }

    protected override object GetItemAtCore(int index)
    {
        return GetOwner()?.ItemsSourceView?.GetAt(index);
    }

    protected override void RecycleElementCore(Control element)
    {
        var owner = GetOwner();
#if DEBUG && REPEATER_TRACE
        var x = Log.Logger;
        Log.Debug("RepeaterLayout - RecycleElement {Index}", owner.GetElementIndex(element));
#endif
        owner?.ClearElementImpl(element);
    }

    protected override Rect VisibleRectCore()
    {
        return GetOwner()?.VisibleWindow ?? default;
    }

    protected override Rect RealizationRectCore()
    {
        return GetOwner()?.RealizationWindow ?? default;
    }

    protected override int RecommendedAnchorIndexCore()
    {
        int anchorIndex = -1;
        var repeater = GetOwner();
        var anchor = repeater?.SuggestedAnchor;
        if (anchor != null)
        {
            anchorIndex = repeater.GetElementIndex(anchor);
        }

        return anchorIndex;
    }

    protected override Point LayoutOriginCore() =>
        GetOwner()?.LayoutOrigin ?? default;

    protected override void LayoutOriginCore(Point value)
    {
        if (GetOwner() is ItemsRepeater ir)
            ir.LayoutOrigin = value;
    }

    private ItemsRepeater GetOwner()
    {
        if (_owner.TryGetTarget(out var target))
            return target;

        return null;
    }

    private WeakReference<ItemsRepeater> _owner;
}
