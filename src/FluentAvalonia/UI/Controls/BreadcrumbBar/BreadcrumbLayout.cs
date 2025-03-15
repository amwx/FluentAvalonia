using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

internal class BreadcrumbLayout : NonVirtualizingLayout
{
    public BreadcrumbLayout() { }

    public BreadcrumbLayout(BreadcrumbBar breadcrumb)
    {
        _breadcrumb = new WeakReference<BreadcrumbBar>(breadcrumb);        
    }

    internal ref readonly bool EllipsisIsRendered => ref _ellipsisIsRendered;

    internal ref readonly int FirstRenderedItemIndexAfterEllipsis =>
        ref _firstRenderedItemIndexAfterEllipsis;

    internal ref readonly int GetVisibleItemsCount =>
        ref _visibleItemsCount;

    protected internal override void InitializeForContextCore(LayoutContext context)
    {

    }

    protected internal override void UninitializeForContextCore(LayoutContext context)
    {

    }

    public int GetItemCount(NonVirtualizingLayoutContext context) =>
        context.Children.Count;

    Control GetElementAt(NonVirtualizingLayoutContext context, int index) =>
        context.Children[index];

    // Measuring is performed in a single step, every element is measured, including the ellipsis
    // item, but the total amount of space needed is only composed of the non-ellipsis breadcrumbs
    protected internal override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
    {
        _availableSize = availableSize;

        double accumWidth = 0, accumHeight = 0;

        for (int i = 0; i < GetItemCount(context); i++)
        {
            var item = GetElementAt(context, i);
            item.Measure(availableSize);

            if (i != 0)
            {
                accumWidth += item.DesiredSize.Width;
                accumHeight = Math.Max(accumHeight, item.DesiredSize.Height);
            }
        }

        // Save a reference to the ellipsis button to avoid querying for it multiple times
        if (GetItemCount(context) > 0)
        {
            if (GetElementAt(context, 0) is BreadcrumbBarItem eb)
            {
                _ellipsisButton = eb;
            }
        }

        if (accumWidth > availableSize.Width)
        {
            _ellipsisIsRendered = true;
        }
        else
        {
            _ellipsisIsRendered = false;
        }

        return new Size(accumWidth, accumHeight);
    }

    // Arranging is performed in a single step, as many elements are tried to be drawn going from the last element
    // towards the first one, if there's not enough space, then the ellipsis button is drawn
    protected internal override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
    {
        int itemCount = GetItemCount(context);
        int firstElementToRender = 0;
        _firstRenderedItemIndexAfterEllipsis = itemCount - 1;
        _visibleItemsCount = 0;

        // If the ellipsis must be drawn, then we find the index (x) of the first element to be rendered, any element with
        // a lower index than x will be hidden (except for the ellipsis button) and every element after x (including x) will
        // be drawn. At the very least, the ellipis and the last item will be rendered
        if (_ellipsisIsRendered)
        {
            firstElementToRender = GetFirstBreadcrumbBarItemToArrange(context);
            _firstRenderedItemIndexAfterEllipsis = firstElementToRender;
        }

        double accumWid = 0;
        double maxElementHeight = GetBreadcrumbBarItemsHeight(context, firstElementToRender);

        // If there is at least one element, we may render the ellipsis item
        if (itemCount > 0)
        {
            var eb = _ellipsisButton;

            if (_ellipsisIsRendered)
            {
                ArrangeItem(eb, ref accumWid, maxElementHeight);
            }
            else
            {
                HideItem(eb);
            }
        }

        // For each item, if the item has an equal or larger index to the first element to render, then
        // render it, otherwise, hide it and add it to the list of hidden items
        for (int i = 1; i < itemCount; i++)
        {
            if (i < firstElementToRender)
            {
                HideItem(context, i);
            }
            else
            {
                ArrangeItem(context, i, ref accumWid, maxElementHeight);
                ++_visibleItemsCount;
            }
        }

        if (_breadcrumb.TryGetTarget(out var target))
        {
            target.ReIndexVisibleElementsForAccessibility();
        }

        return finalSize;
    }

    private void ArrangeItem(Control breadcrumbItem, ref double accumWidth, double maxElementHeight)
    {
        var elementSize = breadcrumbItem.DesiredSize;
        var rect = new Rect(accumWidth, 0, elementSize.Width, maxElementHeight);
        breadcrumbItem.Arrange(rect);

        accumWidth += elementSize.Width;
    }

    private void ArrangeItem(NonVirtualizingLayoutContext context, int index, ref double accumWidth, double maxElementHeight)
    {
        var element = GetElementAt(context, index);
        ArrangeItem(element, ref accumWidth, maxElementHeight);
    }

    private void HideItem(Control item)
    {
        item.Arrange(default(Rect));
    }

    private void HideItem(NonVirtualizingLayoutContext context, int index)
    {
        var element = GetElementAt(context, index);
        HideItem(element);
    }

    private int GetFirstBreadcrumbBarItemToArrange(NonVirtualizingLayoutContext context)
    {
        int itemCount = GetItemCount(context);
        double accumLength = GetElementAt(context, itemCount - 1).DesiredSize.Width +
            _ellipsisButton.DesiredSize.Width;

        for (int i = itemCount - 2; i >= 0; i--)
        {
            double newAccumLength = accumLength + GetElementAt(context, i).DesiredSize.Width;
            if (newAccumLength > _availableSize.Width)
            {
                return i + 1;
            }

            accumLength = newAccumLength;
        }

        return 0;
    }

    private double GetBreadcrumbBarItemsHeight(NonVirtualizingLayoutContext context, int firstItemToRender)
    {
        double maxHeight = 0;

        if (_ellipsisIsRendered)
        {
            maxHeight = _ellipsisButton.DesiredSize.Height;
        }

        for (int i = firstItemToRender; i < GetItemCount(context); i++)
        {
            maxHeight = Math.Max(maxHeight, GetElementAt(context, i).DesiredSize.Height);
        }

        return maxHeight;
    }



    private Size _availableSize;
    private BreadcrumbBarItem _ellipsisButton;
    private WeakReference<BreadcrumbBar> _breadcrumb; // weak_ref because the BreadcrumbBar already points to us via m_itemsRepeaterLayout
    private bool _ellipsisIsRendered;
    private int _firstRenderedItemIndexAfterEllipsis;
    private int _visibleItemsCount;
}
