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
    Size Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context);
    Size Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize,
        VirtualizingLayoutContext context);
    bool Algorithm_ShouldBreakLine(int index, double remainingSpace);
    FlowLayoutAnchorInfo Algorithm_GetAnchorForRealizationRect(Size availableSize, 
        VirtualizingLayoutContext context);
    FlowLayoutAnchorInfo Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize,
        VirtualizingLayoutContext ccontext);
    Rect Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context,
        Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds,
        Control lastRealized, int lastRealizedItemIndex, Rect lastRealizedLayoutBounds);
    void Algorithm_OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize,
        VirtualizingLayoutContext context);
    void Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize,
        VirtualizingLayoutContext context);


}
