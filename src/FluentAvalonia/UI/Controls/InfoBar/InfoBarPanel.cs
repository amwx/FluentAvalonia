using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents a panel that arranges its items horizontally if there is available space, otherwise vertically.
/// </summary>
/// <remarks>
/// This control is specific to the <see cref="InfoBar"/> and generally should not be used elsewhere
/// </remarks>
public partial class InfoBarPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWid = 0;
        double totalHgt = 0;
        double widOfWidest = 0;
        double hgtOfTallest = 0;
        double hgtOfTallestInHorizontal = 0;
        int nItems = 0;

        var parent = this.Parent;
        var minHeight = parent == null ? 0d : (parent.MinHeight - Margin.Vertical());

        var children = Children;
        var childCount = children.Count;

        for (int i = 0; i < children.Count; i++)
        {
            children[i].Measure(availableSize);
            var childDesSize = children[i].DesiredSize;

            if (childDesSize.Width != 0 && childDesSize.Height != 0)
            {
                var horMarg = GetHorizontalOrientationMargin(children[i]);

                totalWid += childDesSize.Width +
                    (nItems > 0 ? horMarg.Left : 0) +
                    (nItems < childCount - 1 ? horMarg.Right : 0);

                var vertMarg = GetVerticalOrientationMargin(children[i]);
                totalHgt += childDesSize.Height +
                    (nItems > 0 ? vertMarg.Top : 0) +
                    (nItems < childCount - 1 ? vertMarg.Bottom : 0);

                if (childDesSize.Width > widOfWidest)
                    widOfWidest = childDesSize.Width;

                if (childDesSize.Height > hgtOfTallest)
                    hgtOfTallest = childDesSize.Height;

                var childHeightInHor = childDesSize.Height + horMarg.Vertical();
                if (childHeightInHor > hgtOfTallestInHorizontal)
                    hgtOfTallestInHorizontal = childHeightInHor;

                nItems++;
            }
        }

        // Since this panel is inside a *-sized grid column, availableSize.Width should not be infinite
        // If there is only one item inside the panel, we will count it as vertical (the margins work out better that way)
        // Also, if the height of any item is taller than the desired min height of the InfoBar,
        // the items should be laid out vertically even though they may seem to fit due to text wrapping.
        if (nItems == 1 || totalWid > availableSize.Width || (minHeight > 0 && hgtOfTallestInHorizontal > minHeight))
        {
            _isVertical = true;
            var vertPad = VerticalOrientationPadding;

            return new Size(widOfWidest + vertPad.Horizontal(),
                totalHgt + vertPad.Vertical());
        }
        else
        {
            _isVertical = false;
            var horPad = HorizontalOrientationPadding;

            return new Size(totalWid + horPad.Horizontal(),
                hgtOfTallest + horPad.Vertical());
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Size result = finalSize;


        if (_isVertical)
        {
            // Layout elements vertically
            var vertPad = VerticalOrientationPadding;
            var vertOff = vertPad.Top;

            bool hasPreviousElement = false;
            for (int i = 0; i < Children.Count; i++)
            {
                var desSize = Children[i].DesiredSize;
                if (desSize.Width != 0 && desSize.Height != 0)
                {
                    var vertMarg = GetVerticalOrientationMargin(Children[i]);

                    vertOff += hasPreviousElement ? vertMarg.Top : 0;
                    Children[i].Arrange(new Rect(vertPad.Left + vertMarg.Left, vertOff, desSize.Width, desSize.Height));
                    vertOff += desSize.Height + vertMarg.Bottom;

                    hasPreviousElement = true;
                }
            }
        }
        else
        {
            var horPad = HorizontalOrientationPadding;
            var horOff = horPad.Left;
            bool hasPreviousElement = false;

            var count = Children.Count;
            for (int i = 0; i < count; i++)
            {
                var desSize = Children[i].DesiredSize;
                if (desSize.Width != 0 && desSize.Height != 0)
                {
                    var horMarg = GetHorizontalOrientationMargin(Children[i]);

                    horOff += hasPreviousElement ? horMarg.Left : 0;
                    if (i < count - 1)
                    {
                        Children[i].Arrange(new Rect(horOff, horPad.Top + horMarg.Top, desSize.Width, desSize.Height));
                    }
                    else
                    {
                        // Give the rest of the horizontal space to the last child.
                        Children[i].Arrange(new Rect(horOff, horPad.Top + horMarg.Top,
                            Math.Max(desSize.Width, finalSize.Width - horOff), desSize.Height));
                    }

                    horOff += desSize.Width + horMarg.Right;
                    hasPreviousElement = true;
                }
            }
        }

        return result;
    }

    private bool _isVertical;
}
