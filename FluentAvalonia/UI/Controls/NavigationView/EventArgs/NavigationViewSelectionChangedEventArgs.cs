using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewSelectionChangedEventArgs
    {
        public object SelectedItem { get; internal set; }
        public NavigationViewItemBase SelectedItemContainer { get; internal set; }
        public bool IsSettingsSelected { get; internal set; }

        public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }
    }
}
