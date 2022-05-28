using Avalonia;
using Avalonia.Controls;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class NavigationViewItemBase : ListBoxItem
    {
        internal NavigationViewRepeaterPosition Position
        {
            get => _position;
            set
            {
                _position = value;
                OnNavigationViewItemBasePositionChanged();
            }
        }

        internal int Depth
        {
            get => _depth;
            set
            {
                if (_depth != value)
                {
                    _depth = value;
                    OnNavigationViewItemBaseDepthChanged();
                }
            }
        }

        internal NavigationView GetNavigationView
        {
            get
            {
                if (_navView != null && _navView.TryGetTarget(out NavigationView target))
                {
                    return target;
                }

                return null;
            }
        }

        internal SplitView GetSplitView
        {
            get
            {
                var navView = GetNavigationView;
                if (navView != null)
                {
                    return navView.GetSplitView;
                }

                return null;
            }
        }

        internal bool IsTopLevelItem { get; set; }

        // Flag to keep track of whether this item was created by the custom internal NavigationViewItemsFactory.
        // This is required in order to achieve proper recycling
        internal bool CreatedByNavigationViewItemsFactory { get; set; }

        internal void SetNavigationViewParent(NavigationView navView)
        {
            _navView = new WeakReference<NavigationView>(navView);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsSelectedProperty)
            {
                OnNavigationViewItemBaseIsSelectedChanged();
            }
        }

        protected virtual void OnNavigationViewItemBasePositionChanged() { }

        protected virtual void OnNavigationViewItemBaseDepthChanged() { }

        protected virtual void OnNavigationViewItemBaseIsSelectedChanged() { }


        // (WinUI) TODO: Constant is a temporary measure. Potentially expose using TemplateSettings.
        protected readonly int _itemIndentation = 31;

        private WeakReference<NavigationView> _navView;
        private int _depth;
        private NavigationViewRepeaterPosition _position;
    }
}
