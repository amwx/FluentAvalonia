using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public abstract class VirtualizingLayoutContext : LayoutContext
{
    public int ItemCount => ItemCountCore();

    public object GetItemAt(int index) =>
        GetItemAtCore(index);

    public Control GetOrCreateElementAt(int index) =>
        GetOrCreateElementAtCore(index, ElementRealizationOptions.None);

    public Control GetOrCreateElementAt(int index, ElementRealizationOptions options) =>
        GetOrCreateElementAtCore(index, options);

    public void RecycleElement(Control element) =>
        RecycleElementCore(element);

    public Rect RealizationRect => RealizationRectCore();

    public int RecommendedAnchorIndex => RecommendedAnchorIndexCore();

    public Point LayoutOrigin
    {
        get => LayoutOriginCore();
        set => LayoutOriginCore(value);
    }

    protected abstract object GetItemAtCore(int index);

    protected abstract Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options);

    protected abstract void RecycleElementCore(Control element);

    protected abstract Rect VisibleRectCore();

    protected abstract Rect RealizationRectCore();

    protected abstract int RecommendedAnchorIndexCore();

    protected abstract Point LayoutOriginCore();

    protected abstract void LayoutOriginCore(Point value);

    protected internal abstract int ItemCountCore();

    internal NonVirtualizingLayoutContext GetNonVirtualizingContextAdapter()
    {
        if (_contextAdapter == null)
            _contextAdapter = new VirtualLayoutContextAdapter(this);

        return _contextAdapter;
    }

    private NonVirtualizingLayoutContext _contextAdapter;
}
