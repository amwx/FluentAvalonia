using System;
using Avalonia.Media.Immutable;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia;

namespace FluentAvalonia.UI.Controls;

// This is a modified version of the BorderRenderHelper taken from the Avalonia source
// In addition to supporting BackgroundSizing for FABorder, I've also make some changes:
// 1- Cached the properties from the border here instead of passing them in on render
//    this save several StyledProperty reads ever OnRender call to populate the BRH,
//    even if the properties don't change. This is of course at the trade off of
//    the extra memory to store the local values to the FABorder
// 2- BorderGeometryKeypoints was changed from a class to a readonly struct
// 3- Added ComplexBorderRender class to store the complex render path geometries rather than
//    always have 2 StreamGeometry instances on the BRH, we only have 1 pointer if we need it
// 4- Removed support for the 'Pen' properties (e.g., DashStyle). IMO those properties don't
//    belong on Border in the first place, and they don't work with the complex render path
//    anyway.

internal class BorderRenderHelper
{
    public BorderRenderHelper(IBrush bg, IBrush bb, Thickness thickness, CornerRadius cr,
        BoxShadows bs, BackgroundSizing sizing)
    {
        Background = bg;
        BorderBrush = bb;
        BorderThickness = thickness;
        CornerRadius = cr;
        BoxShadow = bs;
        BackgroundSizing = sizing;
    }

    public IBrush Background { get; set; }

    public IBrush BorderBrush { get; set; }

    public Thickness BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            _initialized = false;
        }
    }

    public CornerRadius CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = value;
            _initialized = false;
        }
    }

    public BoxShadows BoxShadow { get; set; }

    public BackgroundSizing BackgroundSizing
    {
        get => _backgroundSizing;
        set
        {
            _backgroundSizing = value;
            _initialized = false;
        }
    }

    void Update(Size finalSize)
    {
        _backendSupportsIndividualCorners ??= AvaloniaLocator.Current.GetRequiredService<IPlatformRenderInterface>()
            .SupportsIndividualRoundRects;
        _size = finalSize;

        _initialized = true;

        if (BorderThickness.IsUniform && (CornerRadius.IsUniform || _backendSupportsIndividualCorners == true))
        {
            _complexRender = null;
        }
        else
        {
            if (_complexRender != null)
            {
                _complexRender.Update(finalSize, BorderThickness, CornerRadius, BackgroundSizing);
            }
            else
            {
                _complexRender = new ComplexBorderRender(finalSize, BorderThickness, CornerRadius, BackgroundSizing);
            }
        }
    }

    public void Render(DrawingContext context, Size finalSize)
    {
        if (_size != finalSize || !_initialized)
            Update(finalSize);

        RenderCore(context);
    }

    void RenderCore(DrawingContext context)
    {
        if (_complexRender != null)
        {
            _complexRender.Render(context, Background, BorderBrush);
        }
        else
        {
            var borderThickness = _borderThickness.Top;
            IPen pen = null;

            if (BorderBrush != null && borderThickness > 0)
            {
                pen = new ImmutablePen(BorderBrush.ToImmutable(), borderThickness);
            }


            var rect = new Rect(_size);

            if (_backgroundSizing == BackgroundSizing.InnerBorderEdge)
            {
                if (!MathUtilities.IsZero(borderThickness))
                {
                    rect = rect.Deflate(borderThickness);
                }
                var rrect = new RoundedRect(rect, _cornerRadius.TopLeft, _cornerRadius.TopRight,
                    _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                context.PlatformImpl.DrawRectangle(Background, null, rrect, BoxShadow);

                rrect = new RoundedRect(new Rect(_size).Deflate(borderThickness / 2), _cornerRadius.TopLeft, _cornerRadius.TopRight,
                    _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                context.PlatformImpl.DrawRectangle(null, pen, rrect, BoxShadow);
            }
            else
            {
                var rrect = new RoundedRect(rect, _cornerRadius.TopLeft, _cornerRadius.TopRight,
                    _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                context.PlatformImpl.DrawRectangle(Background, null, rrect, BoxShadow);

                rrect = new RoundedRect(new Rect(_size).Deflate(borderThickness / 2), _cornerRadius.TopLeft, _cornerRadius.TopRight,
                    _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                context.PlatformImpl.DrawRectangle(null, pen, rrect, BoxShadow);
            }
        }
    }

    private static void CreateGeometry(StreamGeometryContext context, Rect boundRect, BorderGeometryKeypoints keypoints)
    {
        context.BeginFigure(keypoints.TopLeft, true);

        // Top
        context.LineTo(keypoints.TopRight);

        // TopRight corner
        var radiusX = boundRect.TopRight.X - keypoints.TopRight.X;
        var radiusY = keypoints.RightTop.Y - boundRect.TopRight.Y;
        if (radiusX != 0 || radiusY != 0)
        {
            context.ArcTo(keypoints.RightTop, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise);
        }

        // Right
        context.LineTo(keypoints.RightBottom);

        // BottomRight corner
        radiusX = boundRect.BottomRight.X - keypoints.BottomRight.X;
        radiusY = boundRect.BottomRight.Y - keypoints.RightBottom.Y;
        if (radiusX != 0 || radiusY != 0)
        {
            context.ArcTo(keypoints.BottomRight, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise);
        }

        // Bottom
        context.LineTo(keypoints.BottomLeft);

        // BottomLeft corner
        radiusX = keypoints.BottomLeft.X - boundRect.BottomLeft.X;
        radiusY = boundRect.BottomLeft.Y - keypoints.LeftBottom.Y;
        if (radiusX != 0 || radiusY != 0)
        {
            context.ArcTo(keypoints.LeftBottom, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise);
        }

        // Left
        context.LineTo(keypoints.LeftTop);

        // TopLeft corner
        radiusX = keypoints.TopLeft.X - boundRect.TopLeft.X;
        radiusY = keypoints.LeftTop.Y - boundRect.TopLeft.Y;

        if (radiusX != 0 || radiusY != 0)
        {
            context.ArcTo(keypoints.TopLeft, new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise);
        }

        context.EndFigure(true);
    }

    private bool? _backendSupportsIndividualCorners;
    private Size _size;
    private Thickness _borderThickness;
    private CornerRadius _cornerRadius;
    private bool _initialized;
    private BackgroundSizing _backgroundSizing;
    private ComplexBorderRender _complexRender;


    private readonly struct BorderGeometryKeypoints
    {
        internal BorderGeometryKeypoints(Rect boundRect, Thickness borderThickness, CornerRadius cornerRadius,
            BackgroundSizing backgroundSizing, bool inner)
        {
            double leftTopY;
            double topLeftX;
            double topRightX;
            double rightTopY;
            double rightBottomY;
            double bottomRightX;
            double bottomLeftX;
            double leftBottomY;

            if (inner)
            {
                if (backgroundSizing == BackgroundSizing.InnerBorderEdge)
                {
                    var left = 0.5 * borderThickness.Left;
                    var top = 0.5 * borderThickness.Top;
                    var right = 0.5 * borderThickness.Right;
                    var bottom = 0.5 * borderThickness.Bottom;

                    // The default from the copied code is InnerBorderEdge
                    leftTopY = Math.Max(0, cornerRadius.TopLeft - top) + boundRect.TopLeft.Y;
                    topLeftX = Math.Max(0, cornerRadius.TopLeft - left) + boundRect.TopLeft.X;
                    topRightX = boundRect.Width - Math.Max(0, cornerRadius.TopRight - top) + boundRect.TopLeft.X;
                    rightTopY = Math.Max(0, cornerRadius.TopRight - right) + boundRect.TopLeft.Y;
                    rightBottomY = boundRect.Height - Math.Max(0, cornerRadius.BottomRight - bottom) +
                                   boundRect.TopLeft.Y;
                    bottomRightX = boundRect.Width - Math.Max(0, cornerRadius.BottomRight - right) +
                                   boundRect.TopLeft.X;
                    bottomLeftX = Math.Max(0, cornerRadius.BottomLeft - left) + boundRect.TopLeft.X;
                    leftBottomY = boundRect.Height - Math.Max(0, cornerRadius.BottomLeft - bottom) +
                                  boundRect.TopLeft.Y;
                }
                else
                {
                    var left = 0.5 * borderThickness.Left;
                    var top = 0.5 * borderThickness.Top;
                    var right = 0.5 * borderThickness.Right;
                    var bottom = 0.5 * borderThickness.Bottom;

                    leftTopY = Math.Max(0, cornerRadius.TopLeft - top) + boundRect.TopLeft.Y;
                    topLeftX = Math.Max(0, cornerRadius.TopLeft - left) + boundRect.TopLeft.X;
                    topRightX = boundRect.Width - Math.Max(0, cornerRadius.TopRight - top) + boundRect.TopLeft.X;
                    rightTopY = Math.Max(0, cornerRadius.TopRight - right) + boundRect.TopLeft.Y;
                    rightBottomY = boundRect.Height - Math.Max(0, cornerRadius.BottomRight - bottom) +
                                   boundRect.TopLeft.Y;
                    bottomRightX = boundRect.Width - Math.Max(0, cornerRadius.BottomRight - right) +
                                   boundRect.TopLeft.X;
                    bottomLeftX = Math.Max(0, cornerRadius.BottomLeft - left) + boundRect.TopLeft.X;
                    leftBottomY = boundRect.Height - Math.Max(0, cornerRadius.BottomLeft - bottom) +
                                  boundRect.TopLeft.Y;
                }
            }
            else
            {
                if (backgroundSizing == BackgroundSizing.InnerBorderEdge)
                {
                    var left = 0.5 * borderThickness.Left;
                    var top = 0.5 * borderThickness.Top;
                    var right = 0.5 * borderThickness.Right;
                    var bottom = 0.5 * borderThickness.Bottom;

                    leftTopY = cornerRadius.TopLeft + top + boundRect.TopLeft.Y;
                    topLeftX = cornerRadius.TopLeft + left + boundRect.TopLeft.X;
                    topRightX = boundRect.Width - (cornerRadius.TopRight + right) + boundRect.TopLeft.X;
                    rightTopY = cornerRadius.TopRight + top + boundRect.TopLeft.Y;
                    rightBottomY = boundRect.Height - (cornerRadius.BottomRight + bottom) + boundRect.TopLeft.Y;
                    bottomRightX = boundRect.Width - (cornerRadius.BottomRight + right) + boundRect.TopLeft.X;
                    bottomLeftX = cornerRadius.BottomLeft + left + boundRect.TopLeft.X;
                    leftBottomY = boundRect.Height - (cornerRadius.BottomLeft + bottom) + boundRect.TopLeft.Y;
                }
                else
                {
                    var left = 0.5 * borderThickness.Left;
                    var top = 0.5 * borderThickness.Top;
                    var right = 0.5 * borderThickness.Right;
                    var bottom = 0.5 * borderThickness.Bottom;

                    leftTopY = cornerRadius.TopLeft + top + boundRect.TopLeft.Y;
                    topLeftX = cornerRadius.TopLeft + left + boundRect.TopLeft.X;
                    topRightX = boundRect.Width - (cornerRadius.TopRight + right) + boundRect.TopLeft.X;
                    rightTopY = cornerRadius.TopRight + top + boundRect.TopLeft.Y;
                    rightBottomY = boundRect.Height - (cornerRadius.BottomRight + bottom) + boundRect.TopLeft.Y;
                    bottomRightX = boundRect.Width - (cornerRadius.BottomRight + right) + boundRect.TopLeft.X;
                    bottomLeftX = cornerRadius.BottomLeft + left + boundRect.TopLeft.X;
                    leftBottomY = boundRect.Height - (cornerRadius.BottomLeft + bottom) + boundRect.TopLeft.Y;
                }
            }

            var leftTopX = boundRect.TopLeft.X;
            var topLeftY = boundRect.TopLeft.Y;
            var topRightY = boundRect.TopLeft.Y;
            var rightTopX = boundRect.Width + boundRect.TopLeft.X;
            var rightBottomX = boundRect.Width + boundRect.TopLeft.X;
            var bottomRightY = boundRect.Height + boundRect.TopLeft.Y;
            var bottomLeftY = boundRect.Height + boundRect.TopLeft.Y;
            var leftBottomX = boundRect.TopLeft.X;

            LeftTop = new Point(leftTopX, leftTopY);
            TopLeft = new Point(topLeftX, topLeftY);
            TopRight = new Point(topRightX, topRightY);
            RightTop = new Point(rightTopX, rightTopY);
            RightBottom = new Point(rightBottomX, rightBottomY);
            BottomRight = new Point(bottomRightX, bottomRightY);
            BottomLeft = new Point(bottomLeftX, bottomLeftY);
            LeftBottom = new Point(leftBottomX, leftBottomY);

            // Fix overlap
            if (TopLeft.X > TopRight.X)
            {
                var scaledX = topLeftX / (topLeftX + topRightX) * boundRect.Width;
                TopLeft = new Point(scaledX, TopLeft.Y);
                TopRight = new Point(scaledX, TopRight.Y);
            }

            if (RightTop.Y > RightBottom.Y)
            {
                var scaledY = rightBottomY / (rightTopY + rightBottomY) * boundRect.Height;
                RightTop = new Point(RightTop.X, scaledY);
                RightBottom = new Point(RightBottom.X, scaledY);
            }

            if (BottomRight.X < BottomLeft.X)
            {
                var scaledX = bottomLeftX / (bottomLeftX + bottomRightX) * boundRect.Width;
                BottomRight = new Point(scaledX, BottomRight.Y);
                BottomLeft = new Point(scaledX, BottomLeft.Y);
            }

            if (LeftBottom.Y < LeftTop.Y)
            {
                var scaledY = leftTopY / (leftTopY + leftBottomY) * boundRect.Height;
                LeftBottom = new Point(LeftBottom.X, scaledY);
                LeftTop = new Point(LeftTop.X, scaledY);
            }
        }

        internal Point LeftTop { get; }

        internal Point TopLeft { get; }

        internal Point TopRight { get; }

        internal Point RightTop { get; }

        internal Point RightBottom { get; }

        internal Point BottomRight { get; }

        internal Point BottomLeft { get; }

        internal Point LeftBottom { get; }
    }

    private class ComplexBorderRender
    {
        public ComplexBorderRender(Size finalSize, Thickness borderThickness, CornerRadius cornerRadius, BackgroundSizing backgroundSizing)
        {
            CreateGeometry(finalSize, borderThickness, cornerRadius, backgroundSizing);
        }

        public void Update(Size finalSize, Thickness borderThickness, CornerRadius cornerRadius, BackgroundSizing backgroundSizing)
        {
            CreateGeometry(finalSize, borderThickness, cornerRadius, backgroundSizing);
        }

        public void Render(DrawingContext context, IBrush background, IBrush borderBrush)
        {
            if (_backgroundGeometry != null)
            {
                context.DrawGeometry(background, null, _backgroundGeometry);
            }

            if (_borderGeometry != null)
            {
                context.DrawGeometry(borderBrush, null, _borderGeometry);
            }
        }

        private void CreateGeometry(Size finalSize, Thickness borderThickness, CornerRadius cornerRadius, BackgroundSizing backgroundSizing)
        {
            BorderGeometryKeypoints backgroundKeypoints = default;
            StreamGeometry backgroundGeometry = null;

            if (backgroundSizing == BackgroundSizing.InnerBorderEdge)
            {
                var boundRect = new Rect(finalSize);
                var innerRect = boundRect.Deflate(borderThickness);

                if (innerRect.Width != 0 && innerRect.Height != 0)
                {
                    backgroundGeometry = new StreamGeometry();
                    backgroundKeypoints = new BorderGeometryKeypoints(innerRect, borderThickness, cornerRadius,
                        backgroundSizing, true);

                    using (var ctx = backgroundGeometry.Open())
                    {
                        BorderRenderHelper.CreateGeometry(ctx, innerRect, backgroundKeypoints);
                    }

                    _backgroundGeometry = backgroundGeometry;
                }
                else
                {
                    _backgroundGeometry = null;
                }

                if (boundRect.Width != 0 && boundRect.Height != 0)
                {
                    innerRect = boundRect.Deflate(borderThickness);
                    var borderGeometryKeypoints =
                        new BorderGeometryKeypoints(boundRect, borderThickness, cornerRadius, backgroundSizing, false);
                    var borderGeometry = new StreamGeometry();

                    using (var ctx = borderGeometry.Open())
                    {

                        BorderRenderHelper.CreateGeometry(ctx, innerRect, backgroundKeypoints!);

                        if (backgroundGeometry != null)
                        {
                            BorderRenderHelper.CreateGeometry(ctx, boundRect, borderGeometryKeypoints);

                        }
                    }

                    _borderGeometry = borderGeometry;
                }
                else
                {
                    _borderGeometry = null;
                }
            }
            else
            {
                var boundRect = new Rect(finalSize);
                var innerRect = boundRect.Deflate(borderThickness);

                if (innerRect.Width != 0 && innerRect.Height != 0)
                {
                    backgroundGeometry = new StreamGeometry();
                    backgroundKeypoints = new BorderGeometryKeypoints(boundRect, borderThickness, cornerRadius,
                        backgroundSizing, false);

                    using (var ctx = backgroundGeometry.Open())
                    {
                        BorderRenderHelper.CreateGeometry(ctx, boundRect, backgroundKeypoints);
                    }

                    _backgroundGeometry = backgroundGeometry;
                }
                else
                {
                    _backgroundGeometry = null;
                }

                if (boundRect.Width != 0 && boundRect.Height != 0)
                {
                    var borderGeometryKeypoints =
                        new BorderGeometryKeypoints(innerRect, borderThickness, cornerRadius, backgroundSizing, false);
                    var borderGeometry = new StreamGeometry();

                    using (var ctx = borderGeometry.Open())
                    {

                        BorderRenderHelper.CreateGeometry(ctx, innerRect, borderGeometryKeypoints);

                        if (backgroundGeometry != null)
                        {
                            BorderRenderHelper.CreateGeometry(ctx, boundRect, backgroundKeypoints);

                        }
                    }

                    _borderGeometry = borderGeometry;
                }
                else
                {
                    _borderGeometry = null;
                }
            }
        }

        private StreamGeometry _backgroundGeometry;
        private StreamGeometry _borderGeometry;
    }
}
