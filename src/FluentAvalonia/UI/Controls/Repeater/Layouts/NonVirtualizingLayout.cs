using Avalonia;

namespace FluentAvalonia.UI.Controls;

public abstract class NonVirtualizingLayout : Layout
{
    protected internal abstract void InitializeForContextCore(LayoutContext context);

    protected internal abstract void UninitializeForContextCore(LayoutContext context);

    protected internal abstract Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize);

    protected internal abstract Size ArrangeOverride(NonVirtualizingLayoutContext context, Size availableSize);
}
