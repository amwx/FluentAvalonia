using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides event data for the NavigationViewItem.ItemExpanding event.
/// </summary>
public class NavigationViewItemExpandingEventArgs : EventArgs
{
    public NavigationViewItemExpandingEventArgs(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    /// <summary>
    /// Gets the object that is expanding after the NavigationViewItem.Expanding event.
    /// </summary>
    public object ExpandingItem
    {
        get
        {
            if (_expandingItem != null)
                return _expandingItem;

            if (_navigationView != null)
            {
                _expandingItem = _navigationView.MenuItemFromContainer(ExpandingItemContainer);
                return _expandingItem;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets the container of the expanding item after a NavigationViewItem.Expanding event.
    /// </summary>
    public NavigationViewItemBase ExpandingItemContainer { get; internal set; }

    private NavigationView _navigationView;
    private object _expandingItem;
}
