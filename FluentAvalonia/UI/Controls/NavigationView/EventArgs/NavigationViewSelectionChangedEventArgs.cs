using FluentAvalonia.UI.Media.Animation;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewSelectionChangedEventArgs : EventArgs
    {
        public object SelectedItem { get; internal set; }

        public NavigationViewItemBase SelectedItemContainer { get; internal set; }

        public bool IsSettingsSelected { get; internal set; }

        public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }
    }
}
