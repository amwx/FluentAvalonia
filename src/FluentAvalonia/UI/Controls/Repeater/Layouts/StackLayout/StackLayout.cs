using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an <i>attached layout</i> that arranges child elements into a single line that can be
/// oriented horizontally or vertically
/// </summary>
public class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates, IOrientationBasedMeasures
{
    public StackLayout()
    {
        LayoutId = "StackLayout";

        UpdateIndexBasedLayoutOrientation(Orientation.Vertical);
    }

    /// <summary>
    /// Defines the <see cref="Spacing"/> property
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        StackPanel.SpacingProperty.AddOwner<StackLayout>();

    /// <summary>
    /// Defines the <see cref="Orientation"/> property
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        StackPanel.OrientationProperty.AddOwner<StackLayout>(
            new StyledPropertyMetadata<Orientation>(
                defaultValue: Orientation.Vertical));

    /// <summary>
    /// Gets or sets a uniform distance (in pixels) between stacked items. It is applied
    /// in the direction of the StackLayout's Orientation
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the dimension by which child elements are stacked
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool DisableVirtualization { get; set; }

    private ScrollOrientation ScrollOrientation { get; set; } = ScrollOrientation.Vertical;

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation 
    {
        get => this.ScrollOrientation;
        set => this.ScrollOrientation = value;
    }

    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;
        StackLayoutState stackState = null;
        if (state != null)
            stackState = GetAsStackState(state);

        if (stackState == null)
        {
            if (state != null)
                throw new InvalidOperationException("LayoutState must derive from StackLayoutState.");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            stackState = new StackLayoutState();
        }

        stackState.InitializeForContext(context, this);
    }

    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var stackState = GetAsStackState(context.LayoutState);
        stackState?.UninitializeForContext(context);
    }

    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        if (context.LayoutState == null)
            return default;

        GetAsStackState(context.LayoutState).OnMeasureStart();

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize, context, false /*isWrapping*/, 0/*minItemsSpacing*/,
            _itemSpacing, int.MaxValue /*maxItemsPerLine*/,
            ScrollOrientation, DisableVirtualization, LayoutId);

        return desiredSize;
    }

    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        if (context.LayoutState == null)
            return default;

        var value = GetFlowAlgorithm(context).Arrange(
            finalSize, context, false /*isWrapping*/,
            FlowLayoutAlgorithm.LineAlignment.Start, LayoutId);

        return value;
    }

    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        if (context.LayoutState != null)
        {
            var flow = GetAsStackState(context.LayoutState).FlowAlgorithm;
            flow.OnItemsSourceChanged(source, args, context);
        }

        InvalidateLayout();
    }

    private FlowLayoutAnchorInfo GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        int anchorIndex = -1;
        double offset = double.NaN;

        var itemsCount = context.ItemCount;
        if (itemsCount > 0)
        {
            var realizationRect = context.RealizationRect;
            var state = GetAsStackState(context.LayoutState);
            var lastExtent = state.FlowAlgorithm.LastExtent;

            double averageElementSize = GetAverageElementSize(availableSize, context, state) + _itemSpacing;
            double realizationWindowOffsetInExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
            double majorSize = this.MajorSize(lastExtent) == 0 ?
                Math.Max(0, averageElementSize * itemsCount - _itemSpacing) : this.MajorSize(lastExtent);
            if (itemsCount > 0 && this.MajorSize(realizationRect) >= 0 &&
                // MajorSize = 0 will account for when a nested repeater is outside the realization rect but still being measured. Also,
                // note that if we are measuring this repeater, then we are already realizing an element to figure out the size, so we could
                // just keep that element alive. It also helps in XYFocus scenarios to have an element realized for XYFocus to find a candidate
                // in the navigating direction.
                realizationWindowOffsetInExtent + this.MajorSize(realizationRect) >= 0 &&
                realizationWindowOffsetInExtent <= majorSize)
            {
                anchorIndex = (int)(realizationWindowOffsetInExtent / averageElementSize);
                offset = anchorIndex * averageElementSize + this.MajorStart(lastExtent);
                anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorIndex));
            }
        }

        return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = offset };
    }

    private Rect GetExtent(Size availableSize, VirtualizingLayoutContext context, Control firstRealized,
        int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized,
        int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        var extent = new Rect();

        int itemsCount = context.ItemCount;
        var stackState = GetAsStackState(context.LayoutState);
        double averageElementSize = GetAverageElementSize(availableSize, context, stackState) + _itemSpacing;

        this.SetMinorSize(ref extent, stackState.MaxArrangeBounds);
        this.SetMajorSize(ref extent, Math.Max(0, itemsCount * averageElementSize - _itemSpacing));
        if (itemsCount > 0)
        {
            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);
                this.SetMajorStart(ref extent, this.MajorStart(firstRealizedLayoutBounds) - firstRealizedItemIndex * averageElementSize);
                var remainingItems = itemsCount - lastRealizedItemIndex - 1;
                this.SetMajorSize(ref extent, this.MajorEnd(lastRealizedLayoutBounds) - this.MajorStart(extent) + (remainingItems * averageElementSize));
            }
            else
            {
#if DEBUG && REPEATER_TRACE
                Log.Debug("{Layout} Estimating extent with no realized elements", LayoutId);
#endif
            }
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
        }

#if DEBUG && REPEATER_TRACE
        Log.Debug("{Layout}: Extent is {Extent} based on average {Avg}",
            LayoutId, extent, averageElementSize);
#endif
        return extent;
    }

    private void OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize,
        VirtualizingLayoutContext context)
    {
        if (context is VirtualizingLayoutContext ctx)
        {
            var stackState = GetAsStackState(ctx.LayoutState);
            stackState.OnElementMeasured(index,
                this.Major(provisionalArrangeSize),
                this.Minor(provisionalArrangeSize));
        }
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, 
        VirtualizingLayoutContext context)
    {
        return availableSize;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, 
        Size desiredSize, VirtualizingLayoutContext context)
    {
        var measureSizeMinor = this.Minor(measureSize);
        return this.MinorMajorSize(
            !double.IsInfinity(measureSizeMinor) ?
                Math.Max(measureSizeMinor, this.Minor(desiredSize)) :
                this.Minor(desiredSize),
            this.Major(desiredSize));
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace) =>
        true;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, 
        VirtualizingLayoutContext context) =>
        GetAnchorForRealizationRect(availableSize, context);

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, 
        Size availableSize, VirtualizingLayoutContext context)
    {
        double offset = double.NaN;
        int index = -1;
        int itemsCount = context.ItemCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
        {
            index = targetIndex;
            var state = GetAsStackState(context.LayoutState);
            double averageElementSize = GetAverageElementSize(availableSize, context, state) + _itemSpacing;
            offset = index * averageElementSize + this.MajorStart(state.FlowAlgorithm.LastExtent);
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, 
        Control firstRealized, int firstRealizedIndex, Rect firstRealizedLayoutBounds, Control lastRealized, 
        int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        return GetExtent(availableSize, context, firstRealized,
            firstRealizedIndex, firstRealizedLayoutBounds,
            lastRealized, lastRealizedItemIndex, lastRealizedLayoutBounds);
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, 
        Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
        OnElementMeasured(element, index, availableSize, measureSize, desiredSize,
            provisionalArrangeSize, context);
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine,
        double lineSize, VirtualizingLayoutContext context)
    { }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();
            ScrollOrientation = orientation == Orientation.Horizontal ? ScrollOrientation.Horizontal :
                ScrollOrientation.Vertical;

            UpdateIndexBasedLayoutOrientation(orientation);
        }
        else if (change.Property == SpacingProperty)
        {
            _itemSpacing = change.GetNewValue<double>();
        }

        InvalidateLayout();
    }

    private double GetAverageElementSize(Size availableSize, VirtualizingLayoutContext context,
        StackLayoutState state)
    {
        double averageElementSize = 0;
        if (context.ItemCount > 0)
        {
            if (state.TotalElementsMeasured == 0)
            {
                var tmpElement = context.GetOrCreateElementAt(0,
                    ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
                state.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
                context.RecycleElement(tmpElement);
            }

            Debug.Assert(state.TotalElementsMeasured > 0);
            averageElementSize = Math.Round(state.TotalElementSize / state.TotalElementsMeasured);
        }

        return averageElementSize;
    }

    private void UpdateIndexBasedLayoutOrientation(Orientation orientation)
    {
        IndexBasedLayoutOrientation = orientation == Orientation.Horizontal ?
            IndexBasedLayoutOrientation.LeftToRight : IndexBasedLayoutOrientation.TopToBottom;
    }

    private void InvalidateLayout() => InvalidateMeasure();

    private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) =>
        GetAsStackState(context.LayoutState).FlowAlgorithm;

    private StackLayoutState GetAsStackState(object state) =>
        state as StackLayoutState;

    private double _itemSpacing;

    // !!! WARNING !!!
    // Any storage here needs to be related to layout configuration. 
    // layout specific state needs to be stored in StackLayoutState.
}
