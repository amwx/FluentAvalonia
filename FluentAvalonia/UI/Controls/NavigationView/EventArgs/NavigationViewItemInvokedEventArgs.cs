using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides event data for the NavigationView.ItemInvoked event.
    /// </summary>
    public class NavigationViewItemInvokedEventArgs : EventArgs		
    {
        /// <summary>
        /// Gets a reference to the invoked item.
        /// </summary>
        public object InvokedItem { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the InvokedItem is the menu item for Settings.
        /// </summary>
        public bool IsSettingsInvoked { get; internal set; }

        /// <summary>
        /// Gets the container for the invoked item.
        /// </summary>
        public NavigationViewItemBase InvokedItemContainer { get; internal set; }

        /// <summary>
        /// Gets the navigation transition recommended for the direction of the navigation.
        /// </summary>
        public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }        
    }
}
