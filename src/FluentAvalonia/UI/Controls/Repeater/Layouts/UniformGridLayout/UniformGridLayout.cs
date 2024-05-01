using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Positions elements sequentially from left to right or top to bottom in a wrapping layout.
/// </summary>
public class UniformGridLayout : VirtualizingLayout, IOrientationBasedMeasures, IFlowLayoutAlgorithmDelegates
{
    public UniformGridLayout()
    {
        LayoutId = "UniformGridLayout";

        UpdateIndexBasedLayoutOrientation(Orientation.Horizontal);
    }

    /// <summary>
    /// Defines the <see cref="Orientation"/> property
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<UniformGridLayout>(
            new StyledPropertyMetadata<Orientation>(
                defaultValue: Orientation.Horizontal));

    /// <summary>
    /// Defines the <see cref="MinItemWidth"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinItemWidthProperty = 
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemWidth));

    /// <summary>
    /// Defines the <see cref="MinItemHeight"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinItemHeightProperty = 
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemHeight));

    /// <summary>
    /// Defines the <see cref="MinRowSpacing"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinRowSpacingProperty = 
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinRowSpacing));

    /// <summary>
    /// Defines the <see cref="MinColumnSpacing"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinColumnSpacingProperty = 
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinColumnSpacing));

    /// <summary>
    /// Defines the <see cref="ItemsJustification"/> property
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsJustification> ItemsJustificationProperty = 
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsJustification>(nameof(ItemsJustification));

    /// <summary>
    /// Defines the <see cref="ItemsStretch"/> property
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsStretch> ItemsStretchProperty = 
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsStretch>(nameof(ItemsStretch));

    /// <summary>
    /// Defines the <see cref="MaximumRowsOrColumns"/> property
    /// </summary>
    public static readonly StyledProperty<int> MaximumRowsOrColumnsProperty = 
        AvaloniaProperty.Register<UniformGridLayout, int>(nameof(MaximumRowsOrColumns), defaultValue: -1);

    /// <summary>
    /// Gets or sets the axis along which items are laid out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of each item.
    /// </summary>
    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of each item.
    /// </summary>
    public double MinItemHeight
    {
        get => GetValue(MinItemHeightProperty);
        set => SetValue(MinItemHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the vertical axis.
    /// </summary>
    public double MinRowSpacing
    {
        get => GetValue(MinRowSpacingProperty);
        set => SetValue(MinRowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the horizontal axis.
    /// </summary>
    public double MinColumnSpacing
    {
        get => GetValue(MinColumnSpacingProperty);
        set => SetValue(MinColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how items are aligned on the non-scrolling or non-virtualizing axis.
    /// </summary>
    public UniformGridLayoutItemsJustification ItemsJustification
    {
        get => GetValue(ItemsJustificationProperty);
        set => SetValue(ItemsJustificationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how items are sized to fill the available space.
    /// </summary>
    public UniformGridLayoutItemsStretch ItemsStretch
    {
        get => GetValue(ItemsStretchProperty);
        set => SetValue(ItemsStretchProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of items rendered per row or column, based on the orientation of the UniformGridLayout.
    /// </summary>
    public int MaximumRowsOrColumns
    {
        get => GetValue(MaximumRowsOrColumnsProperty);
        set => SetValue(MaximumRowsOrColumnsProperty, value);
    }

    private ScrollOrientation ScrollOrientation { get; set; } = ScrollOrientation.Vertical;

    private double LineSpacing =>
       Orientation == Orientation.Horizontal ? _minRowSpacing : _minColumnSpacing;

    private double MinItemSpacing =>
        Orientation == Orientation.Horizontal ? _minColumnSpacing : _minRowSpacing;

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation
    {
        get => ScrollOrientation;
        set => ScrollOrientation = value;
    }

    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;
        if (!(state is UniformGridLayoutState gridState))
        {
            if (state != null)
                throw new InvalidOperationException("LayoutState must derive from UniformGridLayoutState");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            gridState = new UniformGridLayoutState();
        }

        gridState.InitializeForContext(context, this);
    }

    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var gridState = GetAsGridState(context.LayoutState);
        gridState.UninitializeForContext(context);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        var property = change.Property;

        if (property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();
            //Note: For UniformGridLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
            //i.e. the properties are the inverse of each other.
            ScrollOrientation = orientation == Orientation.Horizontal ? ScrollOrientation.Vertical :
                ScrollOrientation.Horizontal;

            UpdateIndexBasedLayoutOrientation(orientation);
        }
        else if (property == MinColumnSpacingProperty)
        {
            _minColumnSpacing = change.GetNewValue<double>();
        }
        else if (property == MinRowSpacingProperty)
        {
            _minRowSpacing = change.GetNewValue<double>();
        }
        else if (property == ItemsJustificationProperty)
        {
            _itemsJustification = change.GetNewValue<UniformGridLayoutItemsJustification>();
        }
        else if (property == ItemsStretchProperty)
        {
            _itemsStretch = change.GetNewValue<UniformGridLayoutItemsStretch>();
        }
        else if (property == MinItemWidthProperty)
        {
            _minItemWidth = change.GetNewValue<double>();
        }
        else if (property == MinItemHeightProperty)
        {
            _minItemHeight = change.GetNewValue<double>();
        }
        else if (property == MaximumRowsOrColumnsProperty)
        {
            _maximumRowsOrColumns = change.GetNewValue<int>();
        }

        InvalidateLayout();
    }

    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        // Set the width and height on the grid state. If the user already set them then use the preset.
        // If not, we have to measure the first element and get back a size which we're going to be using for the rest of the items.
        var gridState = GetAsGridState(context.LayoutState);
        gridState.EnsureElementSize(availableSize, context, _minItemWidth, _minItemHeight,
            _itemsStretch, Orientation, MinRowSpacing, MinColumnSpacing, _maximumRowsOrColumns);

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize, context, true /*isWrapping*/,
            MinItemSpacing, LineSpacing,
            _maximumRowsOrColumns /*maxItemsPerLine*/,
            ScrollOrientation,
            false /*disableVirtualization*/,
            LayoutId);

        return desiredSize;
    }

    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var value = GetFlowAlgorithm(context).Arrange(
            finalSize, context, true /*isWrapping*/,
            (FlowLayoutAlgorithm.LineAlignment)_itemsJustification,
            LayoutId);

        GetAsGridState(context.LayoutState).InvalidateElementSize();

        return value;
    }

    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, 
        NotifyCollectionChangedEventArgs args)
    {
        GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
        // Always invalidate layout to keep the view accurate.
        InvalidateLayout();
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, 
        VirtualizingLayoutContext context)
    {
        var gridState = GetAsGridState(context.LayoutState);
        return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, 
        Size desiredSize, VirtualizingLayoutContext context)
    {
        var gridState = GetAsGridState(context.LayoutState);
        return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
    }
    
    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace) =>
        remainingSpace < 0;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, 
        VirtualizingLayoutContext context)
    {
        Rect bounds = new Rect(double.NaN, double.NaN, double.NaN, double.NaN);
        int anchorIndex = -1;

        int itemsCount = context.ItemCount;
        var realizationRect = context.RealizationRect;
        if (itemsCount > 0 && this.MajorSize(realizationRect) > 0)
        {
            var gridState = GetAsGridState(context.LayoutState);
            var lastExtent = gridState.FlowAlgorithm.LastExtent;
            int itemsPerLine = GetItemsPerLine(availableSize, context);
            double majorSize = (itemsCount / itemsPerLine) * GetMajorSizeWithSpacing(context);
            double realizationWindowWithinExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
            if ((realizationWindowWithinExtent + this.MajorSize(realizationRect)) >= 0 &&
                realizationWindowWithinExtent <= majorSize)
            {
                double offset = Math.Max(0, this.MajorStart(realizationRect) - this.MajorStart(lastExtent));
                int anchorRowIndex = (int)(offset / GetMajorSizeWithSpacing(context));

                anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorRowIndex * itemsPerLine));
                bounds = GetLayoutRectForDataIndex(availableSize, anchorIndex, lastExtent, context);
            }
        }

        return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = this.MajorStart(bounds) };
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex,
        Size availableSize, VirtualizingLayoutContext context)
    {
        int index = -1;
        double offset = double.NaN;
        int count = context.ItemCount;
        if (targetIndex >= 0 && targetIndex < count)
        {
            int itemsPerLine = GetItemsPerLine(availableSize, context);
            int indexOfFirstInLine = (targetIndex / itemsPerLine) * itemsPerLine;
            index = indexOfFirstInLine;
            var state = GetAsGridState(context.LayoutState);
            offset = this.MajorStart(GetLayoutRectForDataIndex(availableSize, indexOfFirstInLine,
                state.FlowAlgorithm.LastExtent, context));
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context,
        Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds,
        Control lastRealized, int lastRealizedItemIndex, Rect lastRealiedLayoutBounds)
    {
        var extent = new Rect();

        // Constants
        int itemsCount = context.ItemCount;
        double availableSizeMinor = this.Minor(availableSize);
        

        int itemsPerLine = (int)Math.Min(// note use of unsigned ints
            Math.Max(1u, !double.IsInfinity(availableSizeMinor) ?
                (uint)((availableSizeMinor + MinItemSpacing) / GetMinorSizeWithSpacing(context)) :
                itemsCount),
            Math.Max(1u, _maximumRowsOrColumns));
        double lineSize = GetMajorSizeWithSpacing(context);

        if (itemsCount > 0)
        {
            // Only use all of the space if item stretch is fill, otherwise size layout according to items placed
            this.SetMinorSize(ref extent,
                !double.IsInfinity(availableSizeMinor) && _itemsStretch == UniformGridLayoutItemsStretch.Fill ?
                    availableSizeMinor :
                    Math.Max(0, itemsPerLine * GetMinorSizeWithSpacing(context) - MinItemSpacing));

            this.SetMajorSize(ref extent,
                Math.Max(0, (itemsCount / itemsPerLine) * lineSize - LineSpacing));

            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);

                this.SetMajorStart(ref extent,
                    this.MajorStart(firstRealizedLayoutBounds) - (firstRealizedItemIndex / itemsPerLine) * lineSize);
                int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                this.SetMajorSize(ref extent,
                    this.MajorEnd(lastRealiedLayoutBounds) - this.MajorStart(extent) +
                    (remainingItems / itemsPerLine) * lineSize);
            }
            else
            {
#if DEBUG && REPEATER_TRACE
                Log.Debug("{Layout}: Estimating extent with no realized items", LayoutId);
#endif
            }
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
        }

#if DEBUG && REPEATER_TRACE
        Log.Debug("{Layout}: Extent is {Extent}. Based on lineSize {Line} and items per line {PerLine",
            LayoutId, extent, itemsPerLine);
#endif

        return extent;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context) { }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize,
        VirtualizingLayoutContext context) { }

    private int GetItemsPerLine(Size availableSize, VirtualizingLayoutContext context)
    {
        int itemsPerLine = (int)Math.Min(// note use of unsigned ints
            Math.Max(1u, (uint)((this.Minor(availableSize) + MinItemSpacing) / GetMinorSizeWithSpacing(context))),
            Math.Max(1u, _maximumRowsOrColumns));

        return itemsPerLine;
    }

    private double GetMinorSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var minItemSpacing = MinItemSpacing;
        var gridState = GetAsGridState(context.LayoutState);
        return ScrollOrientation == ScrollOrientation.Vertical ?
            gridState.EffectiveItemWidth + minItemSpacing :
            gridState.EffectiveItemHeight + minItemSpacing;
    }

    private double GetMajorSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var lineSpacing = LineSpacing;
        var gridState = GetAsGridState(context.LayoutState);
        return ScrollOrientation == ScrollOrientation.Vertical ?
            gridState.EffectiveItemHeight + lineSpacing :
            gridState.EffectiveItemWidth + lineSpacing;
    }

    private Rect GetLayoutRectForDataIndex(Size availableSize, int index,
        Rect lastExtent, VirtualizingLayoutContext context)
    {
        int itemsPerLine = GetItemsPerLine(availableSize, context);
        int rowIndex = index / itemsPerLine;
        int indexInRow = index - (rowIndex * itemsPerLine);

        var gridState = GetAsGridState(context.LayoutState);
        Rect bounds = this.MinorMajorRect(
            indexInRow * GetMinorSizeWithSpacing(context) + this.MinorStart(lastExtent),
            rowIndex * GetMajorSizeWithSpacing(context) + this.MajorStart(lastExtent),
            ScrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemWidth : gridState.EffectiveItemHeight,
            ScrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemHeight : gridState.EffectiveItemWidth);

        return bounds;
    }

    private UniformGridLayoutState GetAsGridState(object state) =>
        state as UniformGridLayoutState;

    private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) =>
        GetAsGridState(context.LayoutState).FlowAlgorithm;

    private void InvalidateLayout() => InvalidateMeasure();

    private void UpdateIndexBasedLayoutOrientation(Orientation orientation)
    {
        IndexBasedLayoutOrientation = orientation == Orientation.Horizontal ?
            IndexBasedLayoutOrientation.LeftToRight : IndexBasedLayoutOrientation.TopToBottom;
    }
   

    private double _minItemWidth = double.NaN;
    private double _minItemHeight = double.NaN;
    private double _minRowSpacing = 0;
    private double _minColumnSpacing = 0;
    private UniformGridLayoutItemsJustification _itemsJustification = UniformGridLayoutItemsJustification.Start;
    private UniformGridLayoutItemsStretch _itemsStretch = UniformGridLayoutItemsStretch.None;
    private int _maximumRowsOrColumns = int.MaxValue;
    // !!! WARNING !!!
    // Any storage here needs to be related to layout configuration.
    // layout specific state needs to be stored in UniformGridLayoutState.
}
