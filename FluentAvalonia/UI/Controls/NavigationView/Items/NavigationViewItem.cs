﻿using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    [PseudoClasses(":leftnav", ":topnav", ":topoverflow")]
    public class NavigationViewItem : NavigationViewItemBase
    {
        public NavigationViewItem()
        {
            _menuItems = new AvaloniaList<object>();
        }

        public static readonly StyledProperty<double> CompactPaneLengthProperty =
            AvaloniaProperty.Register<NavigationViewItem, double>(nameof(CompactPaneLength), 48.0);

        public static readonly DirectProperty<NavigationViewItem, bool> HasUnrealizedChildrenProperty =
            AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(HasUnrealizedChildren),
                x => x.HasUnrealizedChildren, (x, v) => x.HasUnrealizedChildren = v);

        public static readonly StyledProperty<IconElement> IconProperty =
            AvaloniaProperty.Register<NavigationViewItem, IconElement>(nameof(Icon));

        public static readonly DirectProperty<NavigationViewItem, bool> IsChildSelectedProperty =
            AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(IsChildSelectedProperty),
                x => x.IsChildSelected, (x, v) => x.IsChildSelected = v);

        public static readonly DirectProperty<NavigationViewItem, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(IsExpanded),
                x => x.IsExpanded, (x, v) => x.IsExpanded = v);

        public static readonly DirectProperty<NavigationViewItem, IEnumerable> MenuItemsProperty =
            AvaloniaProperty.RegisterDirect<NavigationViewItem, IEnumerable>(nameof(MenuItems),
                x => x.MenuItems, (x, v) => x.MenuItems = v);

        public static readonly DirectProperty<NavigationViewItem, bool> SelectsOnInvokedProperty =
            AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(SelectsOnInvoked),
                x => x.SelectsOnInvoked, (x, v) => x.SelectsOnInvoked = v);


        public double CompactPaneLength
        {
            get => GetValue(CompactPaneLengthProperty);
            private set => SetValue(CompactPaneLengthProperty, value);
        }

        public bool HasUnrealizedChildren
        {
            get => _hasUnrealizedChildren;
            set
            {
                if (SetAndRaise(HasUnrealizedChildrenProperty, ref _hasUnrealizedChildren, value))
                {
                    OnHasUnrealizedChildrenPropertyChanged();
                }
            }
        }

        public IconElement Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool IsChildSelected
        {
            get => _isChildSelected;
            set => SetAndRaise(IsChildSelectedProperty, ref _isChildSelected, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set 
            { 
                if (SetAndRaise(IsExpandedProperty, ref _isExpanded, value))
                {
                    OnIsExpandedPropertyChanged();
                }
            }
        }

        public IEnumerable MenuItems
        {
            get => _menuItems;
            set 
            {
                if (SetAndRaise(MenuItemsProperty, ref _menuItems, value))
                {
                    OnMenuItemsPropertyChanged();
                }
            }
        }

        public bool SelectsOnInvoked
        {
            get => _selectsOnInvoked;
            set => SetAndRaise(SelectsOnInvokedProperty, ref _selectsOnInvoked, value);
        }

        //HELPER PROPERTIES

        internal IControl SelectionIndicator => _presenter?.SelectionIndicator;

        internal NavigationViewItemPresenter NVIPresenter => _presenter;

        private bool HasChildren => (MenuItems != null && MenuItems.Count() > 0) || HasUnrealizedChildren;

        private bool ShouldShowIcon => Icon != null;

        private bool ShouldEnableToolTip => IsOnLeftNav && _isClosedCompact;

        private bool ShouldShowContent => Content != null;

        private bool IsOnLeftNav => Position == NavigationViewRepeaterPosition.LeftNav ||
            Position == NavigationViewRepeaterPosition.LeftFooter;

        private bool IsOnTopPrimary => Position == NavigationViewRepeaterPosition.TopPrimary;

        internal bool ShouldRepeaterShowInFlyout => (_isClosedCompact && IsTopLevelItem) || IsOnTopPrimary;

        internal bool IsRepeaterVisible => _repeater?.IsVisible ?? false;

        internal ItemsRepeater GetRepeater => _repeater;

        protected override void OnNavigationViewItemBaseDepthChanged()
        {
            UpdateItemIndentation();
            PropagateDepthToChildren(Depth + 1);
        }

        protected override void OnNavigationViewItemBaseIsSelectedChanged()
        {
            //
        }

        protected override void OnNavigationViewItemBasePositionChanged()
        {
            UpdateVisualState();
            ReparentRepeater();
            
            //We can't set the Flyout position in Styles, so we change the position here
            if (_rootGrid != null)
            {
                var flyout = _rootGrid.GetValue(FlyoutBase.AttachedFlyoutProperty);
                if (flyout != null)
                {
                    flyout.Placement = (Position == NavigationViewRepeaterPosition.TopPrimary ||
                        Position == NavigationViewRepeaterPosition.TopFooter) ?
                        FlyoutPlacementMode.BottomEdgeAlignedLeft :
                        FlyoutPlacementMode.TopEdgeAlignedRight;

                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _appliedTemplate = false;

            UnhookEventsAndClearFields();

            base.OnApplyTemplate(e);

            _presenter = e.NameScope.Get<NavigationViewItemPresenter>("NVIPresenter");

            _rootGrid = e.NameScope.Get<Grid>("NVIRootGrid");
            if (_rootGrid != null)
            {
                var flyout = FlyoutBase.GetAttachedFlyout(_rootGrid);
                if (flyout != null)
                {
                    flyout.Closing += OnFlyoutClosing;
                }
            }

            var navView = GetNavigationView;
            //WinUI must have a different order of doing things b/c OnApplyTemplate is called BEFORE
            //OnElementPrepared in Avalonia, so NavigationView & SplitView refs don't exist yet. WinUI
            //must do this reversed and not add the item to the tree until AFTER OnElementPrepared
            //To compensate, we'll set the target now
            if (navView == null)
            {
                navView = this.FindAncestorOfType<NavigationView>();
                SetNavigationViewParent(navView);
            }

            var splitView = GetSplitView;
            if (splitView != null)
            {
                _splitViewRevokers = new CompositeDisposable(
                    splitView.GetPropertyChangedObservable(SplitView.IsPaneOpenProperty).Subscribe(OnSplitViewPropertyChanged),
                    splitView.GetPropertyChangedObservable(SplitView.DisplayModeProperty).Subscribe(OnSplitViewPropertyChanged),
                    splitView.GetPropertyChangedObservable(SplitView.CompactPaneLengthProperty).Subscribe(OnSplitViewPropertyChanged));

                UpdateCompactPaneLength();
                UpdateIsClosedCompact();
            }

            //var navView = GetNavigationView;
            if (navView != null)
            {
                _repeater = e.NameScope.Get<ItemsRepeater>("NVIMenuItemsHost");
                if (_repeater != null)
                {
                    (_repeater.Layout as StackLayout).DisableVirtualization = true;

                    _repeater.ElementPrepared += navView.OnRepeaterElementPrepared;
                    _repeater.ElementClearing += navView.OnRepeaterElementClearing;

                    _repeater.ItemTemplate = navView.ItemsFactory;
                }

                UpdateRepeaterItemsSource();
            }

            _flyoutContentGrid = e.NameScope.Get<Panel>("FlyoutContentGrid");

            _appliedTemplate = true;

            UpdateItemIndentation();
            UpdateVisualState();
            ReparentRepeater();
            // We dont want to update the repeater visibilty during OnApplyTemplate if NavigationView is in a mode when items are shown in a flyout
            if (!ShouldRepeaterShowInFlyout)
            {
                ShowHideChildren();
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IconProperty)
            {
                OnIconPropertyChanged(change);
            }
            else if (change.Property == ContentProperty)
            {
                OnContentChanged(change);
            }
        }

        private void UpdateRepeaterItemsSource()
        {
            if (_repeater != null)
            {
                if (_repeater.ItemsSourceView != null)
                {
                    _repeater.ItemsSourceView.CollectionChanged -= OnItemsSourceViewChanged;
                }
                _repeater.Items = MenuItems;

                if (_repeater.ItemsSourceView != null)
                {
                    _repeater.ItemsSourceView.CollectionChanged += OnItemsSourceViewChanged;
                }
            }
        }

        private void OnItemsSourceViewChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateVisualStateForChevron();
        }

        private void OnSplitViewPropertyChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == SplitView.CompactPaneLengthProperty)
            {
                UpdateCompactPaneLength();
            }
            else if (args.Property == SplitView.IsPaneOpenProperty ||
                args.Property == SplitView.DisplayModeProperty)
            {
                UpdateIsClosedCompact();
                ReparentRepeater();
            }
        }

        private void UpdateCompactPaneLength()
        {
            var splitView = GetSplitView;
            if (splitView != null)
            {
                double paneLength = splitView.CompactPaneLength;
                CompactPaneLength = paneLength;

                if (_presenter != null)
                {
                    _presenter.UpdateCompactPaneLength(paneLength, IsOnLeftNav);
                }
            }
        }

        private void UpdateIsClosedCompact()
        {
            var splitView = GetSplitView;
            if (splitView != null)
            {
                _isClosedCompact = !splitView.IsPaneOpen &&
                    (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);

                UpdateVisualState();

                if (_presenter != null)
                {
                    _presenter.UpdateClosedCompactVisualState(IsTopLevelItem, _isClosedCompact);
                }
            }
        }

        private void UpdateNavigationViewItemToolTip()
        {
            var tip = ToolTip.GetTip(this);

            if (tip == null || tip == _suggestedToolTipContent)
            {
                if (ShouldEnableToolTip)
                {
                    ToolTip.SetTip(this, _suggestedToolTipContent);
                }
                else
                {
                    ToolTip.SetTip(this, null);
                }
            }
        }

        private void SuggestedToolTipChanged(object newContent)
        {
            object newToolTip = null;
            if (newContent is string s)
            {
                newToolTip = s;
            }

            // Both customer and NavigationViewItem can update ToolTipContent by winrt::ToolTipService::SetToolTip or XAML
            // If the ToolTipContent is not the same as m_suggestedToolTipContent, then it's set by customer.
            // Customer's ToolTip take high priority, and we never override Customer's ToolTip.
            var toolTip = ToolTip.GetTip(this);
            if (_suggestedToolTipContent != null)
            {
                if (toolTip == _suggestedToolTipContent)
                {
                    ToolTip.SetTip(this, null);
                }
            }

            _suggestedToolTipContent = newToolTip;
        }

        protected virtual void OnIsExpandedPropertyChanged()
        {
            //Added this...
            UpdateVisualStateForChevron();
        }

        protected virtual void OnIconPropertyChanged(AvaloniaPropertyChangedEventArgs args)
        {
            UpdateVisualState();
        }

        protected virtual void OnContentChanged(AvaloniaPropertyChangedEventArgs args)
        {
            SuggestedToolTipChanged(args.NewValue);
            UpdateVisualState();

            if (!IsOnLeftNav)
            {
                var navView = GetNavigationView;
                navView?.TopNavigationViewItemContentChanged();
            }
        }

        protected virtual void OnMenuItemsPropertyChanged()
        {
            UpdateRepeaterItemsSource();
            UpdateVisualStateForChevron();
        }

        private void OnHasUnrealizedChildrenPropertyChanged()
        {
            UpdateVisualStateForChevron();
        }

        private void ShowSelectionIndicator(bool vis)
        {
            if (SelectionIndicator != null)
            {
                SelectionIndicator.Opacity = vis ? 1.0 : 0.0;
            }
        }

        private void UpdateVisualStateForIconAndContent(bool showIcon, bool showContent)
        {
            if (_presenter != null)
            {
                //Possible states :iconleft, :icononly, :contentonly
                ((IPseudoClasses)_presenter.Classes).Set(":iconleft", showIcon && showContent);
                ((IPseudoClasses)_presenter.Classes).Set(":icononly", showIcon && !showContent);
                ((IPseudoClasses)_presenter.Classes).Set(":contentonly", !showIcon);
            }
        }

        private void UpdateVisualStateForNavigationViewPositionChange()
        {
            //Classes aren't styleable in Avalonia, so we must also propagate the state to
            //the NVIPresenter
            //Debug.WriteLine($"{this.Content} -- {Position}");
            switch (Position)
            {
                case NavigationViewRepeaterPosition.LeftNav:
                case NavigationViewRepeaterPosition.LeftFooter:
                    PseudoClasses.Set(":leftnav", true);
                    PseudoClasses.Set(":topnav", false);
                    PseudoClasses.Set(":topoverflow", false);

                    if (_presenter != null)
                    {
                        ((IPseudoClasses)_presenter.Classes).Set(":leftnav", true);
                        ((IPseudoClasses)_presenter.Classes).Set(":topnav", false);
                        ((IPseudoClasses)_presenter.Classes).Set(":topoverflow", false);
                    }
                   
                    break;

                case NavigationViewRepeaterPosition.TopPrimary:
                case NavigationViewRepeaterPosition.TopFooter:
                    PseudoClasses.Set(":leftnav", false);
                    PseudoClasses.Set(":topnav", true);
                    PseudoClasses.Set(":topoverflow", false);

                    if (_presenter != null)
                    {
                        ((IPseudoClasses)_presenter.Classes).Set(":leftnav", false);
                        ((IPseudoClasses)_presenter.Classes).Set(":topnav", true);
                        ((IPseudoClasses)_presenter.Classes).Set(":topoverflow", false);
                    }
                    break;

                case NavigationViewRepeaterPosition.TopOverflow:
                    PseudoClasses.Set(":leftnav", false);
                    PseudoClasses.Set(":topnav", false);
                    PseudoClasses.Set(":topoverflow", true);

                    if (_presenter != null)
                    {
                        ((IPseudoClasses)_presenter.Classes).Set(":leftnav", false);
                        ((IPseudoClasses)_presenter.Classes).Set(":topnav", false);
                        ((IPseudoClasses)_presenter.Classes).Set(":topoverflow", true);
                    }
                    break;
            }
        }

        private void UpdateVisualStateForToolTip()
        {
            UpdateNavigationViewItemToolTip();
        }

        internal void UpdateVisualState()
        {
            if (!_appliedTemplate)
                return;

            UpdateVisualStateForNavigationViewPositionChange();

            bool showIcon = ShouldShowIcon;
            bool showContent = ShouldShowContent;

            if (IsOnLeftNav)
            {
                if (_presenter != null)
                {
                    //This is supposed to be for backwards compatibility with RS4-, but
                    //is apparently still used in the NVIPresenterWhenOnLeftPane style
                    ((IPseudoClasses)_presenter.Classes).Set(":iconcollapsed", !showIcon);
                    //Only using IconCollapsed, IconVisible is default
                }
            }
            else
            {
                ((IPseudoClasses)_presenter.Classes).Set(":iconcollapsed", false);
            }

            UpdateVisualStateForToolTip();

            UpdateVisualStateForIconAndContent(showIcon, showContent);

            UpdateVisualStateForChevron();
        }

        private void UpdateVisualStateForChevron()
        {
            if (_presenter != null)
            {
                //auto const chevronState = HasChildren() && !(m_isClosedCompact && ShouldRepeaterShowInFlyout()) ? 
                //                          (IsExpanded() ? c_chevronVisibleOpen : c_chevronVisibleClosed) : c_chevronHidden;
                //winrt::VisualStateManager::GoToState(presenter, chevronState, true);

                //States :chevronopen, :chevronclosed, :chevronhidden

                bool show = HasChildren && !(_isClosedCompact && ShouldRepeaterShowInFlyout);
                bool expand = IsExpanded;
                var c = this.Content;
                ((IPseudoClasses)_presenter.Classes).Set(":chevronopen", show && expand);
                ((IPseudoClasses)_presenter.Classes).Set(":chevronclosed", show & !expand);
                ((IPseudoClasses)_presenter.Classes).Set(":chevronhidden", !show);
            }
        }

        internal void ShowHideChildren()
        {
            if (_repeater == null)
                return;

            bool shouldShowChildren = IsExpanded;
            _repeater.IsVisible = shouldShowChildren;

            if (ShouldRepeaterShowInFlyout)
            {
                if (shouldShowChildren)
                {
                    if (!_isRepeaterParentedToFlyout)
                    {
                        ReparentRepeater();
                    }

                    Dispatcher.UIThread.Post(() => FlyoutBase.ShowAttachedFlyout(_rootGrid));
                }
                else
                {
                    FlyoutBase.GetAttachedFlyout(_rootGrid)?.Hide();
                }
            }
        }

        private void ReparentRepeater()
        {
            if (HasChildren && _repeater != null)
            {
                if (ShouldRepeaterShowInFlyout && !_isRepeaterParentedToFlyout)
                {
                    _rootGrid.Children.Remove(_repeater);
                    _flyoutContentGrid.Children.Add(_repeater);
                    _isRepeaterParentedToFlyout = true;

                    PropagateDepthToChildren(0);
                }
                else if (!ShouldRepeaterShowInFlyout && _isRepeaterParentedToFlyout)
                {
                    _flyoutContentGrid.Children.Remove(_repeater);
                    _rootGrid.Children.Add(_repeater);
                    _isRepeaterParentedToFlyout = false;

                    PropagateDepthToChildren(1);
                }
            }
        }

        private void UpdateItemIndentation()
        {
            if (_presenter != null)
            {
                var newLeftmargin = Depth * _itemIndentation;
                _presenter.UpdateContentLeftIndentation(newLeftmargin);
            }
        }

        internal void PropagateDepthToChildren(int depth)
        {
            if (_repeater == null || _repeater.ItemsSourceView == null)
                return;

            var count = _repeater.ItemsSourceView.Count;

            for (int i = 0; i < count; i++)
            {
                if (_repeater.TryGetElement(i) is NavigationViewItemBase nvib)
                {
                    nvib.Depth = depth;
                }
            }
        }

        internal void OnExpandCollapseChevronTapped(object sender, RoutedEventArgs args)
        {
            IsExpanded = !IsExpanded;
            args.Handled = true;
        }

        private void OnFlyoutClosing(object sender, CancelEventArgs args)
        {
            IsExpanded = false;
        }

        internal void RotateExpandCollapseChevron(bool isExpanded)
        {
            if (_presenter != null)
            {
                _presenter.RotateExpandCollapseChevron(isExpanded);
            }
        }

        private void UnhookEventsAndClearFields()
        {
            if (_rootGrid != null)
            {
                var flyout = FlyoutBase.GetAttachedFlyout(_rootGrid);
                if (flyout != null)
                {
                    flyout.Closing -= OnFlyoutClosing;
                }
                _rootGrid = null;
            }

            _splitViewRevokers?.Dispose();
            _splitViewRevokers = null;

            var navView = GetNavigationView;
            if (navView != null && _repeater != null)
            {
                _repeater.ElementPrepared -= navView.OnRepeaterElementPrepared;
                _repeater.ElementClearing -= navView.OnRepeaterElementClearing;

                if (_repeater.ItemsSourceView != null)
                {
                    _repeater.ItemsSourceView.CollectionChanged -= OnItemsSourceViewChanged;
                }
                _repeater.Items = null;
                _repeater = null;
            }

            _presenter = null;
            _flyoutContentGrid = null;
        }

        public override string ToString()
        {
            return Content?.ToString() ?? "NavigationViewItem";
        }


        private CompositeDisposable _splitViewRevokers;
        private NavigationViewItemPresenter _presenter;
        private object _suggestedToolTipContent;
        private ItemsRepeater _repeater;
        private Panel _flyoutContentGrid;
        private Grid _rootGrid;

        private bool _isClosedCompact;
        private bool _appliedTemplate;
        //private bool _hasKeyboardFocus;//TODO: needed?
        private bool _isRepeaterParentedToFlyout;

        private bool _hasUnrealizedChildren;
        private bool _isChildSelected;
        private bool _isExpanded;
        private IEnumerable _menuItems;
        private bool _selectsOnInvoked = true;
    }
}
