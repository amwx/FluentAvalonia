using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the NavigationView.DisplayModeChanged event.
    /// </summary>
    public class NavigationViewDisplayModeChangedEventArgs : EventArgs
    {
        internal NavigationViewDisplayModeChangedEventArgs(NavigationViewDisplayMode mode)
        {
            DisplayMode = mode;
        }

        /// <summary>
        /// Gets the new display mode.
        /// </summary>
        public NavigationViewDisplayMode DisplayMode { get; }
    }
}
