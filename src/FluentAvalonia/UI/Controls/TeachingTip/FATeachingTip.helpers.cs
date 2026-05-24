using System.Runtime.CompilerServices;
using Avalonia;

namespace FluentAvalonia.UI.Controls;

public partial class FATeachingTip
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailLongSideActualLength() =>
        _tailPolygon != null ? Math.Max(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailLongSideLength() =>
        TailLongSideActualLength() - (2 * s_tailOcclusionAmount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TailShortSideLength() =>
        _tailPolygon != null ? Math.Min(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) : 0;

    private double MinimumTipEdgeToTailEdgeMargin()
    {
        if (_tailOcclusionGrid != null)
        {
            return _tailOcclusionGrid.ColumnDefinitions.Count > 1 ?
                _tailOcclusionGrid.ColumnDefinitions[1].ActualWidth + s_tailOcclusionAmount
                : 0;
        }

        return 0;
    }

    private double MinimumTipEdgeToTailCenter()
    {
        if (_tailOcclusionGrid != null && _tailPolygon != null)
        {
            if (_tailOcclusionGrid.ColumnDefinitions.Count > 1)
            {
                return _tailOcclusionGrid.ColumnDefinitions[0].ActualWidth +
                    _tailOcclusionGrid.ColumnDefinitions[1].ActualWidth +
                    (Math.Max(_tailPolygon.Bounds.Height, _tailPolygon.Bounds.Width) / 2);
            }
        }

        return 0;
    }


    private CornerRadius GetTeachingTipCornerRadius() => CornerRadius;

    private void SetIsIdle(bool idle) => _isIdle = idle;


    private double TopLeftCornerRadius() => GetTeachingTipCornerRadius().TopLeft;

    private double TopRightCornerRadius() => GetTeachingTipCornerRadius().TopRight;

    // Helper functions
    private static bool IsPlacementTop(FATeachingTipPlacementMode p) =>
        p == FATeachingTipPlacementMode.Top ||
        p == FATeachingTipPlacementMode.TopLeft ||
        p == FATeachingTipPlacementMode.TopRight;

    private static bool IsPlacementBottom(FATeachingTipPlacementMode p) =>
        p == FATeachingTipPlacementMode.Bottom ||
        p == FATeachingTipPlacementMode.BottomLeft ||
        p == FATeachingTipPlacementMode.BottomRight;

    private static bool IsPlacementLeft(FATeachingTipPlacementMode p) =>
        p == FATeachingTipPlacementMode.Left ||
        p == FATeachingTipPlacementMode.TopLeft ||
        p == FATeachingTipPlacementMode.TopRight;

    private static bool IsPlacementRight(FATeachingTipPlacementMode p) =>
        p == FATeachingTipPlacementMode.Right ||
        p == FATeachingTipPlacementMode.RightTop ||
        p == FATeachingTipPlacementMode.RightBottom;

    // These values are shifted by one because this is the 1px highlight that sits adjacent to the tip border.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(width / 2 + (TailShortSideLength() - 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomRightPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(MinimumTipEdgeToTailEdgeMargin() + (TailLongSideLength() - 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomLeftPlacementTopRightHighlightMargin(double width, double height) =>
        new Thickness(width - (MinimumTipEdgeToTailEdgeMargin() + 1f), 0, TopRightCornerRadius() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness OtherPlacementTopRightHighlightMargin(double width, double height) => new Thickness();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1, 0, (width / 2) + (TailShortSideLength() - 1f), 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomRightPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 0, width - (MinimumTipEdgeToTailEdgeMargin() + 1f), 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness BottomLeftPlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 0, MinimumTipEdgeToTailEdgeMargin() + TailLongSideLength() - 1f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness TopEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 1, TopRightCornerRadius() - 1f, 0);

    // Shifted by one since the tail edge's border is not accounted for automatically.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness LeftEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 1f, 1, TopRightCornerRadius() - 2f, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Thickness RightEdgePlacementTopLeftHighlightMargin(double width, double height) =>
        new Thickness(TopLeftCornerRadius() - 2f, 1, TopRightCornerRadius() - 1f, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double UntargetedTipFarPlacementOffset(double farWindowCoordinateInCoreWindowSpace, double tipSize, double offset) =>
        farWindowCoordinateInCoreWindowSpace - (tipSize + s_untargetedTipWindowEdgeMargin + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double UntargetedTipCenterPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double farWindowCoordinateInCoreWindowSpace,
        double tipSize, double nearOffset, double farOffset) =>
        ((nearWindowCoordinateInCoreWindowSpace + farWindowCoordinateInCoreWindowSpace) / 2) - (tipSize / 2) + nearOffset - farOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double UntargetedTipNearPlacementOffset(double nearWindowCoordinateInCoreWindowSpace, double offset) =>
        s_untargetedTipWindowEdgeMargin + nearWindowCoordinateInCoreWindowSpace + offset;
}
