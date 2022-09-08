using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides event data for the NavigationViewItem.ItemCollapsed event.
/// </summary>
public class NavigationViewItemCollapsedEventArgs : EventArgs
{
    public NavigationViewItemCollapsedEventArgs(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    /// <summary>
    /// Gets the object that has been collapsed after the NavigationViewItem.ItemCollapsed event.
    /// </summary>
    public object CollapsedItem
    {
        get
        {
            if (_collapsedItem != null)
            {
                return _collapsedItem;
            }
            if (_navigationView != null)
            {
                _collapsedItem = _navigationView.MenuItemFromContainer(CollapsedItemContainer);
                return _collapsedItem;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets the container of the object that was collapsed in the NavigationViewItem.ItemCollapsed event.
    /// </summary>
    public NavigationViewItemBase CollapsedItemContainer { get; internal set; }

    private object _collapsedItem;
    private NavigationView _navigationView;
}
