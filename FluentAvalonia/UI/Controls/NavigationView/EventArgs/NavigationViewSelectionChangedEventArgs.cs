using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the NavigationView.SelectionChanged event.
    /// </summary>
    public class NavigationViewSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the newly selected menu item.
        /// </summary>
        public object SelectedItem { get; internal set; }

        /// <summary>
        /// Gets the container for the selected item.
        /// </summary>
        public NavigationViewItemBase SelectedItemContainer { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the SelectedItem is the menu item for Settings.
        /// </summary>
        public bool IsSettingsSelected { get; internal set; }

        /// <summary>
        /// Gets the navigation transition recommended for the direction of the navigation.
        /// </summary>
        public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }
    }
}
