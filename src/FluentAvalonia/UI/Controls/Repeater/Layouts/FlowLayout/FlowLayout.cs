#pragma warning disable
// Note this class has no documentation yet from Microsoft - disabling the warnings around
// public APIs with no documentation
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.Core;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

// So WinUI has this as a public facing enum, and retains a private one in
// FlowLayoutAlgorithm...because reasons. Following WinUI for now, even though
// enums are identical
public enum FlowLayoutLineAlignment
{
    Start,
    Center,
    End,
    SpaceAround,
    SpaceBetween,
    SpaceEvenly
}

public class FlowLayout : VirtualizingLayout, IOrientationBasedMeasures, IFlowLayoutAlgorithmDelegates
{
    public FlowLayout()
    {

    }

    public static readonly StyledProperty<FlowLayoutLineAlignment> LineAlignmentProperty = 
        AvaloniaProperty.Register<FlowLayout, FlowLayoutLineAlignment>(nameof(LineAlignment), 
            defaultValue: FlowLayoutLineAlignment.Start);

    public static readonly StyledProperty<double> MinColumnSpacingProperty =
        AvaloniaProperty.Register<FlowLayout, double>(nameof(MinColumnSpacing));

    public static readonly StyledProperty<double> MinRowSpacingProperty = 
        AvaloniaProperty.Register<FlowLayout, double>(nameof(MinRowSpacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<FlowLayout>();

    public FlowLayoutLineAlignment LineAlignment
    {
        get => GetValue(LineAlignmentProperty);
        set => SetValue(LineAlignmentProperty, value);
    }

    public double MinColumnSpacing
    {
        get => GetValue(MinColumnSpacingProperty);
        set => SetValue(MinColumnSpacingProperty, value);
    }

    public double MinRowSpacing
    {
        get => GetValue(MinRowSpacingProperty);
        set => SetValue(MinRowSpacingProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private ScrollOrientation ScrollOrientation { get; set; }

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation
    {
        get => ScrollOrientation;
        set => ScrollOrientation = value;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        base.OnPropertyChanged(args);
        var property = args.Property;
        if (property == OrientationProperty)
        {
            var orientation = args.GetNewValue<Orientation>();

            //Note: For FlowLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
            //i.e. the properties are the inverse of each other.
            var scrollOrientation = (orientation == Orientation.Horizontal) ? 
                ScrollOrientation.Vertical : ScrollOrientation.Horizontal;
            ScrollOrientation = scrollOrientation;
        }
        else if (property == MinColumnSpacingProperty)
        {
            _minColumnSpacing = args.GetNewValue<double>();
        }
        else if (property == MinRowSpacingProperty)
        {
            _minRowSpacing = args.GetNewValue<double>();
        }
        else if (property == LineAlignmentProperty)
        {
            _lineAlignment = (FlowLayoutAlgorithm.LineAlignment)args.GetNewValue<FlowLayoutLineAlignment>();
        }
    }

    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;
        FlowLayoutState flowState = null;
        if (state != null)
        {
            flowState = GetAsFlowState(state);
        }

        if (flowState == null)
        {
            if (state != null)
                throw new Exception();

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by FlowLayout ourselves.
            flowState = new FlowLayoutState();
        }

        flowState.InitializeForContext(context, this);
    }

    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var flowState = GetAsFlowState(context.LayoutState);
        flowState.UninitializeForContext(context);
    }

    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize, context, true /*isWrapping*/,
            MinItemSpacing(), LineSpacing(), int.MaxValue /*maxItemsPerLine*/,
            ScrollOrientation, false /*disableVirtualization*/, LayoutId);
        return desiredSize;
    }

    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var value = GetFlowAlgorithm(context).Arrange(
            finalSize, context, true /*isWrapping*/,
            _lineAlignment, LayoutId);
        return value;
    }

    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
        // Always invalidate layout to keep the view accurate.
        InvalidateLayout();
    }
        
    private FlowLayoutState GetAsFlowState(object state) =>
        state as FlowLayoutState;

    private void InvalidateLayout() => InvalidateMeasure();

    FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) =>
        GetAsFlowState(context.LayoutState).FlowAlgorithm;

    private bool DoesRealizationWindowOverlapExtent(Rect realizationWindow, Rect extent) =>
        this.MajorEnd(realizationWindow) >= this.MajorStart(extent) &&
        this.MajorStart(realizationWindow) <= this.MajorEnd(extent);

    private double LineSpacing() =>
        ScrollOrientation == ScrollOrientation.Vertical ? _minRowSpacing : _minColumnSpacing;

    private double MinItemSpacing() =>
        ScrollOrientation == ScrollOrientation.Vertical ? _minColumnSpacing : _minRowSpacing;

    // WinUI has these as separate methods and then has these call those, lets just skip that step

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, 
        VirtualizingLayoutContext context)
    {
        return availableSize;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, 
        Size desiredSize, VirtualizingLayoutContext context)
    {
        return desiredSize;
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
    {
        return remainingSpace < 0;
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, 
        VirtualizingLayoutContext context)
    {
        int anchorIndex = -1;
        double offset = double.NaN;

        int itemsCount = context.ItemCount;
        if (itemsCount > 0)
        {
            var realizationRect = context.RealizationRect;
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            var lastExtent = flowState.FlowAlgorithm.LastExtent;

            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing();
            Debug.Assert(averageItemsPerLine != 0);

            double extentMajorSize = this.MajorSize(lastExtent) == 0 ? (itemsCount / averageItemsPerLine) * averageLineSize : this.MajorSize(lastExtent);
            if (itemsCount > 0 &&
                this.MajorSize(realizationRect) >0 &&
                DoesRealizationWindowOverlapExtent(realizationRect, this.MinorMajorRect(this.MinorStart(lastExtent), this.MajorStart(lastExtent), this.Minor(availableSize), extentMajorSize)))
            {
                double realizationWindowStartWithExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
                int lineIndex = Math.Max(0, (int)(realizationWindowStartWithExtent / averageLineSize));
                anchorIndex = (int)(lineIndex * averageItemsPerLine);

                // Clamp it to be within valid range
                anchorIndex = MathHelpers.Clamp(anchorIndex, 0, itemsCount - 1);
                offset = lineIndex * averageLineSize + this.MajorStart(lastExtent);
            }
        }

        return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = offset };
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, 
        Size availableSize, VirtualizingLayoutContext context)
    {
        double offset = double.NaN;
        int index = -1;
        int itemsCount = context.ItemCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
        {
            index = targetIndex;
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing();
            int lineIndex = (int)(targetIndex / averageItemsPerLine);
            offset = lineIndex * averageLineSize + this.MajorStart(flowState.FlowAlgorithm.LastExtent);
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, 
        Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized, 
        int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        Rect extent = default;

        int itemsCount = context.ItemCount;
        if (itemsCount > 0)
        {
            double availableSizeMinor = this.Minor(availableSize);
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing();

            Debug.Assert(averageItemsPerLine != 0);
            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);
                int linesBeforeFirst = (int)(firstRealizedItemIndex / averageItemsPerLine);
                double extentMajorStart = this.MajorStart(firstRealizedLayoutBounds) - linesBeforeFirst * averageLineSize;
                this.SetMajorStart(ref extent, extentMajorStart);
                int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                int remainingLinesAfterLast = (int)(remainingItems / averageItemsPerLine);
                double extentMajorSize = this.MajorEnd(lastRealizedLayoutBounds) -
                    this.MajorStart(extent) + remainingLinesAfterLast * averageLineSize;
                this.SetMajorSize(ref extent, extentMajorSize);

                // If the available size is infinite, we will have realized all the items in one line.
                // In that case, the extent in the non virtualizing direction should be based on the
                // right/bottom of the last realized element.
                this.SetMinorSize(ref extent, !double.IsInfinity(availableSizeMinor) ?
                    availableSizeMinor : Math.Max(0, this.MinorEnd(lastRealizedLayoutBounds)));
            }
            else
            {
                var lineSpacing = LineSpacing();
                var minItemSpacing = MinItemSpacing();
                // We dont have anything realized. make an educated guess.
                int numLines = (int)Math.Ceiling(itemsCount / averageItemsPerLine);
                extent = !double.IsInfinity(availableSizeMinor) ?
                    this.MinorMajorRect(0, 0, availableSizeMinor, Math.Max(0, numLines * averageLineSize - lineSpacing)) :
                    this.MinorMajorRect(0, 0,
                    Math.Max(0, (this.Minor(flowState.SpecialElementDesiredSize) + minItemSpacing) * itemsCount - minItemSpacing),
                    Math.Max(0, averageLineSize - lineSpacing));
                //REPEATER_TRACE_INFO(L"%*s: \tEstimating extent with no realized elements. \n", winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data());
            }

            //REPEATER_TRACE_INFO(L"%*s: \tExtent is {%.0f,%.0f}. Based on average line size {%.0f} and average items per line {%.0f}. \n",
            //winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data(), extent.Width, extent.Height, averageLineSize, averageItemsPerLine);
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
            //        REPEATER_TRACE_INFO(L"%*s: \tExtent is {%.0f,%.0f}. ItemCount is 0 \n",
            //winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data(), extent.Width, extent.Height);
        }

        return extent;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize,
        VirtualizingLayoutContext context)
    {
        //REPEATER_TRACE_INFO(L"%*s: \tOnLineArranged startIndex:%d Count:%d LineHeight:%d \n",
        //winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data(), startIndex, countInLine, lineSize);

        var flowState = GetAsFlowState(context.LayoutState);
        flowState.OnLineArranged(startIndex, countInLine, lineSize, context);
    }

    private double GetAverageLineInfo(Size availableSize, VirtualizingLayoutContext context,
        FlowLayoutState flowState, ref double avgCountInLine)
    {
        // default to 1 item per line with 0 size
        double avgLineSize = 0;
        avgCountInLine = 1;

        Debug.Assert(context.ItemCountCore() > 0);
        if (flowState.TotalLinesMeasured == 0)
        {
            var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
            var desiredSize = flowState.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
            context.RecycleElement(tmpElement);

            int estimatedCountInLine = Math.Max(1, (int)(this.Major(availableSize) / this.Minor(desiredSize)));
            flowState.OnLineArranged(0, estimatedCountInLine, this.Major(desiredSize), context);
            flowState.SpecialElementDesiredSize = desiredSize;
        }

        avgCountInLine = Math.Max(1, (int)(flowState.TotalItemsPerLine / flowState.TotalLinesMeasured));
        avgLineSize = Math.Round(flowState.TotalLineSize / flowState.TotalLinesMeasured);

        return avgLineSize;
    }

    //internal void UpdateIndexBasedLayoutOrientation(Orientation orientation)
    //{
    //    SetIndexBasedLayoutOrientation(orientation == Orientation.Horizontal ?
    //        IndexBasedLayoutOrientation.LeftToRight : IndexBasedLayoutOrientation.TopToBottom);
    //}

    private FlowLayoutAlgorithm.LineAlignment _lineAlignment = FlowLayoutAlgorithm.LineAlignment.Start;
    private double _minColumnSpacing = 0.0;
    private double _minRowSpacing = 0.0;
    private Orientation _orientation = Orientation.Horizontal;
}
