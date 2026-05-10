namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides event data for the NavigationViewItem.ItemExpanding event.
/// </summary>
public class FANavigationViewItemExpandingEventArgs : EventArgs
{
    public FANavigationViewItemExpandingEventArgs(FANavigationView navigationView)
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
            if (field != null)
                return field;

            if (_navigationView != null)
            {
                field = _navigationView.MenuItemFromContainer(ExpandingItemContainer);
                return field;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets the container of the expanding item after a NavigationViewItem.Expanding event.
    /// </summary>
    public FANavigationViewItemBase ExpandingItemContainer { get; internal set; }

    private FANavigationView _navigationView;
}
