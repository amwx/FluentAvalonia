using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewDisplayModeChangedEventArgs
    {
        internal NavigationViewDisplayModeChangedEventArgs(NavigationViewDisplayMode mode)
        {
            DisplayMode = mode;
        }

        public NavigationViewDisplayMode DisplayMode { get; }
    }
}
