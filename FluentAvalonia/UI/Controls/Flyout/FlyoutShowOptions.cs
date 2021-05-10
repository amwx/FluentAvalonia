using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls
{
    public class FlyoutShowOptions
    {
        /// <summary>
        /// Gets or sets a rectangular area that the flyout tries to not overlap. NOT IMPLEMENTED
        /// </summary>
        public Rect? ExclusionRect { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates where the flyout is placed in relation to it's target element.
        /// </summary>
        public FlyoutPlacementMode Placement { get; set; }

        /// <summary>
        /// Gets or sets the position where the flyout opens. NOT IMPLEMENTED
        /// </summary>
        public Point? Position { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the flyout behaves when opened.
        /// </summary>
        public FlyoutShowMode ShowMode { get; set; }
    }
}
