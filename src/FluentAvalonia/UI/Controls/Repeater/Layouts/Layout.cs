using Avalonia;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Gets the orientation, if any, in which items are laid out based on their index in the source collection.
/// </summary>
public enum IndexBasedLayoutOrientation
{
    /// <summary>
    /// There is no correlation between the items' layout and their index number.
    /// </summary>
    None = 0,

    /// <summary>
    /// Items are laid out vertically with increasing indices.
    /// </summary>
    TopToBottom = 1,

    /// <summary>
    /// Items are laid out horizontally with increasing indices.
    /// </summary>
    LeftToRight = 2
}

/// <summary>
/// Represents the base class for an object that sizes and arranges child elements for a host.
/// </summary>
public abstract class Layout : AvaloniaObject
{    
    internal string LayoutId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public IndexBasedLayoutOrientation IndexBasedLayoutOrientation { get; protected internal set; }

    /// <summary>
    /// Occurs when the measurement state (layout) has been invalidated.
    /// </summary>
    public event TypedEventHandler<Layout, EventArgs> MeasureInvalidated;

    /// <summary>
    /// Occurs when the arrange state(layout) has been invalidated.
    /// </summary>
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

    /// <summary>
    /// Initializes any per-container state the layout requires when it is attached to a UIElement container.
    /// </summary>
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

    /// <summary>
    /// Removes any state the layout previously stored on the UIElement container.
    /// </summary>
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

    /// <summary>
    /// Suggests a DesiredSize for a container element. A container element that supports attached layouts 
    /// should call this method from their own MeasureOverride implementations to form a recursive layout update. 
    /// The attached layout is expected to call the Measure for each of the container’s UIElement children.
    /// </summary>
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

    /// <summary>
    /// Positions child elements and determines a size for a container UIElement. Container elements that 
    /// support attached layouts should call this method from their layout override implementations to 
    /// form a recursive layout update.
    /// </summary>
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

    /// <summary>
    /// Invalidates the measurement state (layout) for all UIElement containers that reference this layout.
    /// </summary>
    protected void InvalidateMeasure() =>
        MeasureInvalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Invalidates the arrange state (layout) for all UIElement containers that reference this layout. 
    /// After the invalidation, the UIElement will have its layout updated, which occurs asynchronously.
    /// </summary>
    protected void InvalidateArrange() =>
        ArrangeInvalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// 
    /// </summary>
    protected internal virtual ItemCollectionTransitionProvider CreateDefaultItemTransitionProvider() => null;
}
