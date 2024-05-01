using System.Xml.Linq;
using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the base class for an object that sizes and arranges child elements for a host and and does not support virtualization.
/// </summary>
/// <remarks>
/// NonVirtualizingLayout is the base class for layouts that do not support virtualization. You can inherit from it to create your own layout.
/// A non-virtualizing layout can measure and arrange child elements.
/// </remarks>
public abstract class NonVirtualizingLayout : Layout
{
    /// <summary>
    /// When overridden in a derived class, initializes any per-container state the layout requires when it is attached to a UIElement container.
    /// </summary>
    protected internal abstract void InitializeForContextCore(LayoutContext context);

    /// <summary>
    /// When overridden in a derived class, removes any state the layout previously stored on the UIElement container.
    /// </summary>
    protected internal abstract void UninitializeForContextCore(LayoutContext context);

    /// <summary>
    /// Provides the behavior for the "Measure" pass of the layout cycle. Classes can override this method to define their own "Measure" pass behavior.
    /// </summary>
    protected internal abstract Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize);

    /// <summary>
    /// When implemented in a derived class, provides the behavior for the "Arrange" pass of layout. Classes can override this method to define their own "Arrange" pass behavior.
    /// </summary>
    protected internal abstract Size ArrangeOverride(NonVirtualizingLayoutContext context, Size availableSize);
}
