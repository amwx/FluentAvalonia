using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the base class for layout context types that do not support virtualization.
/// </summary>
public abstract class NonVirtualizingLayoutContext : LayoutContext
{
    /// <summary>
    /// Gets the collection of child UIElements from the container that provides the context.
    /// </summary>
    public IReadOnlyList<Control> Children => ChildrenCore();

    /// <summary>
    /// Implements the behavior for getting the return value of Children in a derived or custom NonVirtualizingLayoutContext.
    /// </summary>
    protected abstract IReadOnlyList<Control> ChildrenCore();

    internal VirtualizingLayoutContext GetVirtualizingContextAdapter()
    {
        _contextAdapter ??= new LayoutContextAdapter(this);

        return _contextAdapter;
    }

    private VirtualizingLayoutContext _contextAdapter;
}
