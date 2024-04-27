using Avalonia;
using Avalonia.Layout;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public enum IndexBasedLayoutOrientation
{
    None = 0,
    TopToBottom = 1,
    LeftToRight = 2
}

public abstract class Layout : AvaloniaObject
{
    public string LayoutId { get; set; }

    protected internal IndexBasedLayoutOrientation IndexBasedLayoutOrientation { get; set; }

    public event TypedEventHandler<Layout, EventArgs> MeasureInvalidated;
    public event TypedEventHandler<Layout, EventArgs> ArrangeInvalidated;

    private static VirtualizingLayoutContext GetVirtualizingLayoutContext(LayoutContext context)
    {
        if (context is VirtualizingLayoutContext vlc)
        {
            return vlc;
        }
        else if (context is NonVirtualizingLayoutContext nvlc)
        {
            var adapter = nvlc.GetVirtualizingContextAdapter();
            return adapter;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static NonVirtualizingLayoutContext GetNonVirtualizingLayoutContext(LayoutContext context)
    {
        if (context is NonVirtualizingLayoutContext nvlc)
        {
            return nvlc;
        }
        else if (context is VirtualizingLayoutContext vlc)
        {
            var adapter = vlc.GetNonVirtualizingContextAdapter();
            return adapter;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void InitializeForContext(LayoutContext context)
    {
        if (this is VirtualizingLayout vl)
        {
            var vc = GetVirtualizingLayoutContext(context);
            vl.InitializeForContextCore(vc);
        }
        else if (this is NonVirtualizingLayout nvl)
        {
            var nvc = GetNonVirtualizingLayoutContext(context);
            nvl.InitializeForContextCore(nvc);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void UninitializeForContext(LayoutContext context)
    {
        if (this is VirtualizingLayout vl)
        {
            var vc = GetVirtualizingLayoutContext(context);
            vl.UninitializeForContextCore(vc);
        }
        else if (this is NonVirtualizingLayout nvl)
        {
            var nvc = GetNonVirtualizingLayoutContext(context);
            nvl.UninitializeForContextCore(nvc);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public Size Measure(LayoutContext context, Size availableSize)
    {
        if (this is VirtualizingLayout vl)
        {
            var vc = GetVirtualizingLayoutContext(context);
            return vl.MeasureOverride(vc, availableSize);
        }
        else if (this is NonVirtualizingLayout nvl)
        {
            var nvc = GetNonVirtualizingLayoutContext(context);
            return nvl.MeasureOverride(nvc, availableSize);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public Size Arrange(LayoutContext context, Size finalSize)
    {
        if (this is VirtualizingLayout vl)
        {
            var vc = GetVirtualizingLayoutContext(context);
            return vl.ArrangeOverride(vc, finalSize);
        }
        else if (this is NonVirtualizingLayout nvl)
        {
            var nvc = GetNonVirtualizingLayoutContext(context);
            return nvl.ArrangeOverride(nvc, finalSize);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    protected void InvalidateMeasure() =>
        MeasureInvalidated?.Invoke(this, EventArgs.Empty);

    protected void InvalidateArrange() =>
        ArrangeInvalidated?.Invoke(this, EventArgs.Empty);

    protected internal virtual ItemCollectionTransitionProvider CreateDefaultItemTransitionProvider() => null;
}
