using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public abstract class NonVirtualizingLayoutContext : LayoutContext
{
    public IReadOnlyList<Control> Children => ChildrenCore();

    protected abstract IReadOnlyList<Control> ChildrenCore();

    internal VirtualizingLayoutContext GetVirtualizingContextAdapter()
    {
        _contextAdapter ??= new LayoutContextAdapter(this);

        return _contextAdapter;
    }

    private VirtualizingLayoutContext _contextAdapter;
}