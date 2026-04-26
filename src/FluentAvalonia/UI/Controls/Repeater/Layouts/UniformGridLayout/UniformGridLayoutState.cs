using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace FluentAvalonia.UI.Controls;

internal class UniformGridLayoutState
{
    public FlowLayoutAlgorithm FlowAlgorithm => _flowAlgorithm;

    public double EffectiveItemWidth => _effectiveItemWidth;

    public double EffectiveItemHeight => _effectiveItemHeight;

    public void InitializeForContext(VirtualizingLayoutContext context,
        IFlowLayoutAlgorithmDelegates callbacks)
    {
        _flowAlgorithm ??= new FlowLayoutAlgorithm();
        _flowAlgorithm.InitializeForContext(context, callbacks);
        context.LayoutStateCore = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context)
    {
        _flowAlgorithm.UninitializeForContext(context);
    }

    public void EnsureElementSize(Size availableSize, VirtualizingLayoutContext context,
        double layoutItemWidth, double layoutItemHeight,
        UniformGridLayoutItemsStretch stretch, Orientation orientation,
        double minRowSpacing, double minColumnSpacing, int maxItemsPerLine)
    {
        if (maxItemsPerLine == 0)
        {
            maxItemsPerLine = 1;
        }

        if (context.ItemCount > 0)
        {
            // If the first element is realized we don't need to get it from the context
            if (_flowAlgorithm.GetElementIfRealized(0) is Control realizedElement)
            {
                // This is relatively cheap, when item 0 is realized, always use it to find the size. 
                realizedElement.Measure(CalculateAvailableSize(
                    availableSize, orientation, stretch, maxItemsPerLine,
                    layoutItemWidth, layoutItemHeight, minRowSpacing, minColumnSpacing));
                SetSize(realizedElement.DesiredSize, layoutItemWidth, layoutItemHeight,
                    availableSize, stretch, orientation, minRowSpacing, minColumnSpacing, maxItemsPerLine);
            }
            else
            {
                // Not realized by flowlayout, so do this now but just once per layout pass since this is expensive and
                // has the potential to repeatedly invalidate layout due to recycling causing layout cycles.
                if (!_isEffectiveSizeValid)
                {
                    if (context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate) is Control firstElement)
                    {
                        firstElement.Measure(CalculateAvailableSize(availableSize, orientation,
                            stretch, maxItemsPerLine, layoutItemWidth, layoutItemHeight,
                            minRowSpacing, minColumnSpacing));
                        SetSize(firstElement.DesiredSize, layoutItemWidth, layoutItemHeight,
                            availableSize, stretch, orientation, minRowSpacing, minColumnSpacing,
                            maxItemsPerLine);
                        context.RecycleElement(firstElement);

                        // BUG: WinUI recycles the element here, but that is causing a hang when the Repeater is loaded
                        // What seems to be happening is recycle element is called which unrealizes the first item in the
                        // viewport that is used here to estimate size
                        // For some reason, MeasureOverride gets called infinitely, and EffectiveViewportChanged is never
                        // fired from the ScrollViewer so we always have an invalid viewport, and because the item was
                        // unrealized here, this path is always called and we never progress past the first item
                        // Take out the recycling here and all seems to work ok
                        //context.RecycleElement(firstElement);

                        // HACK: Add SuppressAutoRecycle above in GetOrCreateElementAt, and force add the item which
                        // moves ownership to the ElementManager, and this works. This path still gets called twice
                        // as the first time an invalid anchor index (-1) is found which clears the realized range
                        // and thus we get another call on this path, which then succeeds. The downside there is 
                        // that we get an extra element realized that is never removed from the Panel, but at least
                        // this works again, and that extra element is treated like an unrealized element arranged
                        // offscreen so its not that big of a deal. Hopefully, MS will have an actual fix for this...
                        // | ElementRealizationOptions.SuppressAutoRecycle
                        //_flowAlgorithm.TryAddElement0(firstElement);
                    }
                }
            }

            _isEffectiveSizeValid = true;
        }
    }

    public Size CalculateAvailableSize(Size availableSize, Orientation orientation,
        UniformGridLayoutItemsStretch stretch, int maxItemsPerLine,
        double itemWidth, double itemHeight, double minRowSpacing, double minColumnSpacing)
    {
        // Since some controls might have certain requirements when rendering (e.g. maintaining an aspect ratio),
        // we will let elements know the actual size they will get within our layout and let them measure based on that assumption.
        // That way we ensure that no gaps will be created within our layout because of a control deciding it doesn't need as much height (or width)
        // for the column width (or row height) being provided.
        if (orientation == Orientation.Horizontal)
        {
            if (!double.IsNaN(itemWidth))
            {
                double allowedColumnWidth = itemWidth;
                if (stretch != UniformGridLayoutItemsStretch.None)
                {
                    allowedColumnWidth += CalculateExtraPixelsInLine(maxItemsPerLine,
                        availableSize.Width, itemWidth, minColumnSpacing);
                }

                return new Size(allowedColumnWidth, availableSize.Height);
            }
        }
        else
        {
            if (!double.IsNaN(itemHeight))
            {
                double allowedRowHeight = itemHeight;
                if (stretch != UniformGridLayoutItemsStretch.None)
                {
                    allowedRowHeight += CalculateExtraPixelsInLine(maxItemsPerLine,
                        availableSize.Height, itemHeight, minRowSpacing);
                }

                // Fixed typo in WinUI - Size.height is itemHeight in WinUI, corrected to allowedRowHeight
                return new Size(availableSize.Width, allowedRowHeight);
            }
        }

        return availableSize;
    }

    private double CalculateExtraPixelsInLine(int maxItemsPerLine, double availableSizeMinor,
        double itemSizeMinor, double minorItemSpacing)
    {
        int numItemsPerColumn;
        int numItemsBasedOnSize = (int)Math.Max(1, availableSizeMinor / (itemSizeMinor + minorItemSpacing));
        if (numItemsBasedOnSize == 0)
        {
            numItemsPerColumn = maxItemsPerLine;
        }
        else
        {
            numItemsPerColumn = Math.Min(maxItemsPerLine, numItemsBasedOnSize);
        }

        var usedSpace = (numItemsPerColumn * (itemSizeMinor + minorItemSpacing)) - minorItemSpacing;
        var remainingSpace = (int)(availableSizeMinor - usedSpace);
        return remainingSpace / numItemsPerColumn;
    }

    private void SetSize(Size desiredItemSize, double layoutItemWidth, double layoutItemHeight,
        Size availableSize, UniformGridLayoutItemsStretch stretch, Orientation orientation,
        double minRowSpacing, double minColumnSpacing, int maxItemsPerLine)
    {
        maxItemsPerLine = maxItemsPerLine == 0 ? 1 : maxItemsPerLine;

        _effectiveItemWidth = double.IsNaN(layoutItemWidth) ? desiredItemSize.Width : layoutItemWidth;
        _effectiveItemHeight = double.IsNaN(layoutItemHeight) ? desiredItemSize.Height : layoutItemHeight;

        var availableSizeMinor = orientation == Orientation.Horizontal ? availableSize.Width : availableSize.Height;
        var minorItemSpacing = orientation == Orientation.Vertical ? minRowSpacing : minColumnSpacing;

        var itemSizeMinor = orientation == Orientation.Horizontal ? _effectiveItemWidth : _effectiveItemHeight;

        double extraMinorPixelsForEachItem = 0;
        if (!double.IsInfinity(availableSizeMinor))
        {
            extraMinorPixelsForEachItem = CalculateExtraPixelsInLine(maxItemsPerLine,
                availableSizeMinor, itemSizeMinor, minorItemSpacing);
        }

        if (stretch == UniformGridLayoutItemsStretch.Fill)
        {
            if (orientation == Orientation.Horizontal)
            {
                _effectiveItemWidth += extraMinorPixelsForEachItem;
            }
            else
            {
                _effectiveItemHeight += extraMinorPixelsForEachItem;
            }
        }
        else if (stretch == UniformGridLayoutItemsStretch.Uniform)
        {
            var itemSizeMajor = orientation == Orientation.Horizontal ? _effectiveItemHeight : _effectiveItemWidth;
            var extraMajorPixelsForEachItem = itemSizeMajor * (extraMinorPixelsForEachItem / itemSizeMinor);
            if (orientation == Orientation.Horizontal)
            {
                _effectiveItemWidth += extraMinorPixelsForEachItem;
                _effectiveItemHeight += extraMajorPixelsForEachItem;
            }
            else
            {
                _effectiveItemHeight += extraMinorPixelsForEachItem;
                _effectiveItemWidth += extraMajorPixelsForEachItem;
            }
        }
    }

    internal void InvalidateElementSize()
    {
        _isEffectiveSizeValid = false;
    }

    private FlowLayoutAlgorithm _flowAlgorithm;
    private double _effectiveItemWidth;
    private double _effectiveItemHeight;
    private bool _isEffectiveSizeValid;
}
