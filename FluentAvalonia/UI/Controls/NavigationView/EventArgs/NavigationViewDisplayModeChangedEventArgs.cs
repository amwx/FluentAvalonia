using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewDisplayModeChangedEventArgs : EventArgs
    {
        internal NavigationViewDisplayModeChangedEventArgs(NavigationViewDisplayMode mode)
        {
            DisplayMode = mode;
        }

        public NavigationViewDisplayMode DisplayMode { get; }
    }
}
