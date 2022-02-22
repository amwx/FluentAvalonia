using System;
using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls.Primitives
{
    /// <summary>
    /// Represents a panel that arranges the buttons in a <see cref="TaskDialog"/>
    /// </summary>
    public class TaskDialogButtonsPanel : Panel
    {
        /// <summary>
        /// Defines the <see cref="Spacing"/> property
        /// </summary>
        public static readonly DirectProperty<TaskDialogButtonsPanel, double> SpacingProperty =
            AvaloniaProperty.RegisterDirect<TaskDialogButtonsPanel, double>(nameof(Spacing),
                x => x.Spacing, (x, v) => x.Spacing = v);

        /// <summary>
        /// Gets or sets the spacing between the buttons
        /// </summary>
        public double Spacing
        {
            get => _spacing;
            set => SetAndRaise(SpacingProperty, ref _spacing, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double wid = 0;
            double hgt = 0;
            var ct = Children.Count;
            for (int i = 0; i < ct; i++)
            {
                Children[i].Measure(Size.Infinity);

                var size = Children[i].DesiredSize;
                wid += size.Width;
                hgt = Math.Max(hgt, size.Height);
            }

            wid += _spacing * (ct - 1);

            return new Size(wid, hgt);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var ct = Children.Count;

            if (ct != 0)
            {
                if (ct == 1)
                {
                    var halfWid = finalSize.Width / 2;
                    var rc = new Rect(halfWid, 0, halfWid, finalSize.Height);
                    Children[0].Arrange(rc);
                }
                else
                {
                    var spacingSpace = _spacing * (ct - 1);

                    var buttonWidth = (finalSize.Width - spacingSpace) / ct;
                    Rect rc;
                    double x = 0;
                    for (int i = 0; i < ct; i++)
                    {
                        rc = new Rect(x, 0, buttonWidth, finalSize.Height);
                        Children[i].Arrange(rc);

                        x += rc.Width + _spacing;
                    }
                }                
            }

            return finalSize;
        }

        private double _spacing;
    }
}
