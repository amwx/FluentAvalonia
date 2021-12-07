using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewItemExpandingEventArgs : EventArgs
    {
        public NavigationViewItemExpandingEventArgs(NavigationView navigationView)
        {
            _navigationView = navigationView;
        }

        public object ExpandingItem
        {
            get
            {
                if (_expandingItem != null)
                    return _expandingItem;

                if(_navigationView != null)
                {
                    _expandingItem = _navigationView.MenuItemFromContainer(ExpandingItemContainer);
                    return _expandingItem;
                }
                return null;
            }
        }

        public NavigationViewItemBase ExpandingItemContainer { get; internal set; }

        private NavigationView _navigationView;
        private object _expandingItem;
    }
}
