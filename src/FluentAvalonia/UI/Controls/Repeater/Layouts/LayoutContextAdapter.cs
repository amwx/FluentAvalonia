using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

internal class LayoutContextAdapter : VirtualizingLayoutContext
{
    public LayoutContextAdapter(NonVirtualizingLayoutContext nonVirtualizingContext)
    {
        _nonVirtualizingContext = nonVirtualizingContext;
    }

    protected internal override object LayoutStateCore 
    { 
        get => _nonVirtualizingContext?.LayoutState; 
        set
        {
            if (_nonVirtualizingContext != null)
                _nonVirtualizingContext.LayoutState = value;
        }
    }

    protected internal override int ItemCountCore() => _nonVirtualizingContext?.Children.Count ?? 0;

    protected override object GetItemAtCore(int index) =>
        _nonVirtualizingContext?.Children[index] ?? null;

    protected override Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
    {
        if (_nonVirtualizingContext != null)
        {
            return _nonVirtualizingContext.Children[index];
        }

        return null;
    }

    protected override void RecycleElementCore(Control element) { }

    private int GetElementIndexCore(Control element)
    {
        int idx = -1;
        if (_nonVirtualizingContext != null)
        {
            var children = _nonVirtualizingContext.Children;
            idx = children.IndexOf(element);
        }

        return idx;
    }

    protected override Rect VisibleRectCore() =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    protected override Rect RealizationRectCore() =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    protected override int RecommendedAnchorIndexCore() => -1;

    protected override Point LayoutOriginCore() => default;

    protected override void LayoutOriginCore(Point value)
    {
        if (value != default)
        {
            throw new ArgumentException("LayoutOrigin must be at (0,0) when RealizationRect is infinite sized.");
        }
    }

    private NonVirtualizingLayoutContext _nonVirtualizingContext;
}
