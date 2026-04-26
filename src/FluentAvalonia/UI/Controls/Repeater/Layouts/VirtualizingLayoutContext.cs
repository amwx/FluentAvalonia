using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the base class for layout context types that support virtualization.
/// </summary>
public abstract class VirtualizingLayoutContext : LayoutContext
{
    /// <summary>
    /// Gets the number of items in the data.
    /// </summary>
    public int ItemCount => ItemCountCore();

    /// <summary>
    /// Retrieves the data item in the source found at the specified index.
    /// </summary>
    public object GetItemAt(int index) =>
        GetItemAtCore(index);

    /// <summary>
    /// Retrieves a UIElement that represents the data item in the source found at the specified index.By default, if an element already exists, it is returned; otherwise, a new element is created.
    /// </summary>
    public Control GetOrCreateElementAt(int index) =>
        GetOrCreateElementAtCore(index, ElementRealizationOptions.None);

    /// <summary>
    /// Retrieves a UIElement that represents the data item in the source found at the specified index using the specified options.
    /// </summary>
    public Control GetOrCreateElementAt(int index, ElementRealizationOptions options) =>
        GetOrCreateElementAtCore(index, options);

    /// <summary>
    /// Clears the specified UIElement and allows it to be either re-used or released.
    /// </summary>
    public void RecycleElement(Control element) =>
        RecycleElementCore(element);

    /// <summary>
    /// Gets an area that represents the viewport and buffer that the layout should fill with realized elements.
    /// </summary>
    public Rect RealizationRect => RealizationRectCore();

    /// <summary>
    /// Gets the recommended index from which to start the generation and layout of elements.
    /// </summary>
    public int RecommendedAnchorIndex => RecommendedAnchorIndexCore();

    /// <summary>
    /// Gets or sets the origin point for the estimated content size.
    /// </summary>
    public Point LayoutOrigin
    {
        get => LayoutOriginCore();
        set => LayoutOriginCore(value);
    }

    /// <summary>
    /// When implemented in a derived class, retrieves the data item in the source found at the specified index.
    /// </summary>
    protected abstract object GetItemAtCore(int index);

    /// <summary>
    /// When implemented in a derived class, retrieves a UIElement that represents the data item in the 
    /// source found at the specified index using the specified options.
    /// </summary>
    protected abstract Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options);

    /// <summary>
    /// When implemented in a derived class, clears the specified UIElement and allows it to be either re-used or released.
    /// </summary>
    protected abstract void RecycleElementCore(Control element);

    /// <summary>
    /// Provides the value that is assigned to the VisibleRect property.
    /// </summary>
    protected abstract Rect VisibleRectCore();

    /// <summary>
    /// When implemented in a derived class, retrieves an area that represents the viewport and buffer that the 
    /// layout should fill with realized elements.
    /// </summary>
    protected abstract Rect RealizationRectCore();

    /// <summary>
    /// Implements the behavior for getting the return value of RecommendedAnchorIndex in a derived or custom VirtualizingLayoutContext.
    /// </summary>
    protected abstract int RecommendedAnchorIndexCore();

    /// <summary>
    /// Implements the behavior of LayoutOrigin in a derived or custom VirtualizingLayoutContext.
    /// </summary>
    protected abstract Point LayoutOriginCore();

    // TODO: Not in WinUI 1.5?
    /// <summary>
    /// 
    /// </summary>
    protected abstract void LayoutOriginCore(Point value);

    // TODO: Not in WinUI 1.5
    /// <summary>
    /// 
    /// </summary>
    protected internal abstract int ItemCountCore();

    internal NonVirtualizingLayoutContext GetNonVirtualizingContextAdapter()
    {
        if (_contextAdapter == null)
            _contextAdapter = new VirtualLayoutContextAdapter(this);

        return _contextAdapter;
    }

    private NonVirtualizingLayoutContext _contextAdapter;
}
