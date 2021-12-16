using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewItemCollapsedEventArgs : EventArgs
    {
        public NavigationViewItemCollapsedEventArgs(NavigationView navigationView)
        {
            _navigationView = navigationView;
        }

        public object CollapsedItem
        {
            get
            {
                if(_collapsedItem != null)
                {
                    return _collapsedItem;
                }
                if(_navigationView != null)
                {
                    _collapsedItem = _navigationView.MenuItemFromContainer(CollapsedItemContainer);
                    return _collapsedItem;
                }
                return null;
            }
        }

        public NavigationViewItemBase CollapsedItemContainer { get; internal set; }

        private object _collapsedItem;
        private NavigationView _navigationView;
    }
}
