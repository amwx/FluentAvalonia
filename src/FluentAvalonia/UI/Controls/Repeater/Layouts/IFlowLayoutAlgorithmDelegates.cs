using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

internal struct FlowLayoutAnchorInfo
{
    public int Index;
    public double Offset;
}

internal interface IFlowLayoutAlgorithmDelegates
{
    Size Algorithm_GetMeasureSize(int index, Size availableSize, FAVirtualizingLayoutContext context);
    Size Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize,
        FAVirtualizingLayoutContext context);
    bool Algorithm_ShouldBreakLine(int index, double remainingSpace);
    FlowLayoutAnchorInfo Algorithm_GetAnchorForRealizationRect(Size availableSize, 
        FAVirtualizingLayoutContext context);
    FlowLayoutAnchorInfo Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize,
        FAVirtualizingLayoutContext ccontext);
    Rect Algorithm_GetExtent(Size availableSize, FAVirtualizingLayoutContext context,
        Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds,
        Control lastRealized, int lastRealizedItemIndex, Rect lastRealizedLayoutBounds);
    void Algorithm_OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize,
        FAVirtualizingLayoutContext context);
    void Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize,
        FAVirtualizingLayoutContext context);


}
