using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewItemInvokedEventArgs : EventArgs		
    {
        public object InvokedItem { get; internal set; }

        public bool IsSettingsInvoked { get; internal set; }

        public NavigationViewItemBase InvokedItemContainer { get; internal set; }

        public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }        
    }
}
