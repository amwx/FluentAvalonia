using Avalonia;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

public abstract class VirtualizingLayout : Layout
{
    protected internal abstract void InitializeForContextCore(VirtualizingLayoutContext context);

    protected internal abstract void UninitializeForContextCore(VirtualizingLayoutContext context);

    protected internal abstract Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize);

    protected internal virtual Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize) =>
        // Do not throw. If the layout decides to arrange its
        // children during measure, then an ArrangeOverride is not required.
        finalSize;

    protected internal virtual void OnItemsChangedCore(VirtualizingLayoutContext context, object source,
        NotifyCollectionChangedEventArgs args)
    {
        InvalidateMeasure();
    }
}
