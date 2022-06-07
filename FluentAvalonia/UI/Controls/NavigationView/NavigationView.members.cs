using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
    public partial class NavigationView : HeaderedContentControl
    {
		//Con't logic for pane arrow key navigation
		private bool VerifyInPane(IVisual focus, IVisual parent)
		{
			if (parent == null)
				return false;

			//First test the back button, close button, and panetogglebutton
			//since they don't reside in the content grids
			if (_backButton != null && focus == _backButton)
				return true;

			if (_closeButton != null && focus == _closeButton)
				return true;

			if (_paneToggleButton != null && focus == _paneToggleButton)
				return true;

			while (focus != null)
			{
				if (focus == parent)
					return true;

				focus = focus.VisualParent;
			}
			return false;
		}

		private IControl SearchTreeForLowestFocusItem(NavigationViewItem start)
		{
			if (DoesNavigationViewItemHaveChildren(start) && start.IsExpanded)
			{
				var ct = start.GetRepeater.ItemsSourceView.Count;
				for (int j = ct - 1; j >= 0; j--)
				{
					if (start.GetRepeater.TryGetElement(j) is NavigationViewItem nvi)
					{
						return SearchTreeForLowestFocusItem(nvi);
					}
				}
			}

			return start;
		}

		//Helpers

		private int SelectedItemIndex => _topDataProvider.IndexOf(SelectedItem);

        internal bool IsTopNavigationView => PaneDisplayMode == NavigationViewPaneDisplayMode.Top;

        private bool IsTopPrimaryListVisible => _topNavRepeater != null && TemplateSettings.TopPaneVisibility;

        private double GetPaneToggleButtonWidth() => 
            this.TryFindResource("PaneToggleButtonWidth", out object value) ? (double)value : 40;

        private double GetPaneToggleButtonHeight() => 
            this.TryFindResource("PaneToggleButtonHeight", out object value) ? (double)value : 40;

        internal bool IsOverlay => _splitView != null && _splitView.DisplayMode == SplitViewDisplayMode.Overlay;

        private bool IsLightDismissable => _splitView != null && (
            _splitView.DisplayMode != SplitViewDisplayMode.Inline && 
            _splitView.DisplayMode != SplitViewDisplayMode.CompactInline);

        internal bool ShouldShowBackButton
        {
            get
            {
                if (DisplayMode == NavigationViewDisplayMode.Minimal && IsPaneOpen)
                    return false;

                return ShouldShowBackOrCloseButton;
            }
        }

        internal bool ShouldShowCloseButton
        {
            get
            {
                if (_backButton != null && _closeButton != null)
                {
                    if (!IsPaneOpen)
                    {
                        return false;
                    }

                    var pdm = PaneDisplayMode;

                    if (pdm != NavigationViewPaneDisplayMode.LeftMinimal &&
                        (pdm != NavigationViewPaneDisplayMode.Auto || 
                        DisplayMode != NavigationViewDisplayMode.Minimal))
                    {
                        return false;
                    }

                    return ShouldShowBackOrCloseButton;
                }

                return false;
            }
        }

        internal bool ShouldShowBackOrCloseButton
        {
            get
            {
                bool vis = IsBackButtonVisible;
                return vis;
            }
        }
        
        private bool IsTopLevelItem(NavigationViewItemBase nvib)
        {
            return IsRootItemsRepeater(GetParentItemsRepeaterForContainer(nvib));
        }


        private bool DoesNavigationViewItemHaveChildren(NavigationViewItem nvi)
        {
            return nvi != null && 
                ((nvi.MenuItems != null && nvi.MenuItems.Count() > 0) || nvi.HasUnrealizedChildren);
        }

        private bool IsSelectionSuppressed(object item)
        {
            if (item != null)
            {
                return !NavigationViewItemOrSettingsContentFromData(item)?.SelectsOnInvoked ?? false;
            }

            return false;
        }

        private bool IsRootItemsRepeater(object ir)
        {
            return ir != null &&
                (ir == _topNavRepeater ||
                ir == _leftNavRepeater ||
                ir == _topNavRepeaterOverflowView ||
                ir == _leftNavFooterMenuRepeater ||
                ir == _topNavFooterMenuRepeater);
        }

        private bool IsRootGridOfFlyout(object item)
        {
            //TODO: Why do we need the root grid of the flyout?
            return item is Panel p && p.Name == "FlyoutRootGrid";
        }

        private ItemsRepeater GetParentRootItemsRepeaterForContainer(NavigationViewItemBase nvib)
        {
            var parentIR = GetParentItemsRepeaterForContainer(nvib);

            while (!IsRootItemsRepeater(parentIR))
            {
                nvib = GetParentNavigationViewItemForContainer(nvib);
                if (nvib == null)
                {
                    return null;
                }

                parentIR = GetParentItemsRepeaterForContainer(nvib);
            }

            return parentIR;
        }

        private ItemsRepeater GetParentItemsRepeaterForContainer(NavigationViewItemBase nvib)
        {
            return nvib?.FindAncestorOfType<ItemsRepeater>();
        }

        private NavigationViewItem GetParentNavigationViewItemForContainer(NavigationViewItemBase nvib)
        {
            // (WinUI) TODO: This scenario does not find parent items when in a flyout, which causes problems
            // if item if first loaded straight in the flyout. Fix.This logic can be merged with the
            // 'GetIndexPathForContainer' logic below.
            var parent = GetParentItemsRepeaterForContainer(nvib);
            if (!IsRootItemsRepeater(parent))
            {
                return parent.FindAncestorOfType<NavigationViewItem>();
            }

            return null;
        }

        private IndexPath GetIndexPathForContainer(NavigationViewItemBase nvib)
        {
            var path = new List<int>(4);

            IControl child = nvib;
            var parent = nvib.GetVisualParent();
            if (parent == null)
            {
                return IndexPath.CreateFromIndices(path);
            }

            // Search through VisualTree for a root ItemsRepeater
            while (parent != null && !IsRootItemsRepeater(parent) && !IsRootGridOfFlyout(parent))
                {
                    if (parent is ItemsRepeater ir)
                    {
                        path.Insert(0, ir.GetElementIndex(child));
                    }
                    child = (IControl)parent;
                    parent = parent.GetVisualParent();
                }

            // If the item is in a flyout, then we need to final index of its parent
            if (IsRootGridOfFlyout(parent))
            {
                if (_lastItemExpandedIntoFlyout != null)
                {
                    child = _lastItemExpandedIntoFlyout;
                    parent = IsTopNavigationView ? _topNavRepeater : _leftNavRepeater;
                }
            }

            // If item is in one of the disconnected ItemRepeaters, account for that in IndexPath calculations
            if (parent == _topNavRepeaterOverflowView)
                {
                    // Convert index of selected item in overflow to index in datasource
                    var contIndex = _topNavRepeaterOverflowView.GetElementIndex(child);
                    var item = _topDataProvider.GetOverflowItems()[contIndex];
                    var indexAtRoot = _topDataProvider.IndexOf(item);
                    path.Insert(0, indexAtRoot);
                }
                else if (parent == _topNavRepeater)
                {
                    // Convert index of selected item in overflow to index in datasource
                    var contIndex = _topNavRepeater.GetElementIndex(child);
                    var item = _topDataProvider.GetPrimaryItems()[contIndex];
                    var indexAtRoot = _topDataProvider.IndexOf(item);
                    path.Insert(0, indexAtRoot);
                }
                else if (parent is ItemsRepeater parentIR)
                {
                    path.Insert(0, parentIR.GetElementIndex(child));
                }

            var isInFooterMenu = parent == _leftNavFooterMenuRepeater || parent == _topNavFooterMenuRepeater;
            path.Insert(0, isInFooterMenu ? _footerMenuBlockIndex : _mainMenuBlockIndex);

            return IndexPath.CreateFromIndices(path);
        }


        private NavigationViewItemBase NavigationViewItemBaseOrSettingsContentFromData(object data)
            => GetContainerForData<NavigationViewItemBase>(data);

        private NavigationViewItem NavigationViewItemOrSettingsContentFromData(object data)
            => GetContainerForData<NavigationViewItem>(data);


        internal object MenuItemFromContainer(object container)
        {
            if (container is NavigationViewItemBase nvib)
            {
                var parentIR = GetParentItemsRepeaterForContainer(nvib);
                if (parentIR != null)
                {
                    var contIndex = parentIR.GetElementIndex(nvib);
                    if (contIndex >= 0)
                        return GetItemFromIndex(parentIR, contIndex);
                }
            }

            return null;
        }

        private IControl ContainerFromMenuItem(object item)
        {
            return NavigationViewItemBaseOrSettingsContentFromData(item);
        }

        private int GetNavigationViewItemCountInPrimaryList => 
            _topDataProvider?.NavigationViewItemCountInPrimaryList ?? 0;

        private int GetNavigationViewItemCountInTopNav => 
            _topDataProvider?.NavigationViewItemCountInTopNav ?? 0;

        private bool IsSettingsItem(object item)
        {
            if (item != null && _settingsItem != null)
            {
                return (item == _settingsItem) || (_settingsItem.Content == item);
            }

            return false;
        }

        private double MeasureTopNavigationViewDesiredWidth(Size availableSize) => 
            LayoutHelper.MeasureChild(_topNavGrid, availableSize, new Thickness()).Width;

        private double MeasureTopNavMenuItemsHostDesiredWidth(Size availableSize) =>
            LayoutHelper.MeasureChild(_topNavRepeater, availableSize, new Thickness()).Width;

        private double GetTopNavigationViewActualWidth => _topNavGrid.Bounds.Width;

        private bool HasTopNavigationViewItemNotInPrimaryList() =>
            _topDataProvider.PrimaryListSize != _topDataProvider.Size;

        private void SetOverflowButtonVisibility(bool vis) 
        {
            TemplateSettings.OverflowButtonVisibility = vis;
        }

        private bool NeedTopPadding() => false;//TitleBar stuff

        private int GetContainerCountInRepeater(ItemsRepeater ir)
        {
            if (ir != null && ir.ItemsSourceView != null)
            {
                return ir.ItemsSourceView.Count;
            }

            return -1;
        }

        private bool DoesRepeaterHaveRealizedContainers(ItemsRepeater ir)
        {
            return ir != null && ir.TryGetElement(0) != null;
        }

        private int GetIndexFromItem(ItemsRepeater ir, object data)
        {
            if (ir != null && ir.ItemsSourceView != null)
            {
                return ir.ItemsSourceView.IndexOf(data);
            }

            return -1;
        }

        private object GetItemFromIndex(ItemsRepeater ir, int index)
        {
            if (ir != null && ir.ItemsSourceView != null)
            {
                return ir.ItemsSourceView.GetAt(index);
            }

            return null;
        }

        private IndexPath GetIndexPathOfItem(object item)
        {
            if (item is NavigationViewItemBase nvib)
            {
                return GetIndexPathForContainer(nvib);
            }

            // In the databinding scenario, we need to conduct a search where we go through every item,
            // realizing it if necessary.
            if (IsTopNavigationView)
            {
                // First search through primary list
                var ip = SearchEntireTreeForIndexPath(_topNavRepeater, item, false);
                if (ip != IndexPath.Unselected)
                {
                    return ip;
                }

                // If item was not located in primary list, search through overflow
                ip = SearchEntireTreeForIndexPath(_topNavRepeaterOverflowView, item, false);
                if (ip != IndexPath.Unselected)
                {
                    return ip;
                }

                // If item was not located in primary list and overflow, search through footer
                ip = SearchEntireTreeForIndexPath(_topNavFooterMenuRepeater, item, true);
                if (ip != IndexPath.Unselected)
                {
                    return ip;
                }
            }
            else
            {
                var ip = SearchEntireTreeForIndexPath(_leftNavFooterMenuRepeater, item, true);
                if (ip != IndexPath.Unselected)
                {
                    return ip;
                }

                ip = SearchEntireTreeForIndexPath(_leftNavFooterMenuRepeater, item, true);
                if (ip != IndexPath.Unselected)
                {
                    return ip;
                }
            }

            return IndexPath.Unselected;
        }

        private bool IsContainerTheSelectedItemInTheSelectionModel(NavigationViewItemBase nvib)
        {
            var selItem = _selectionModel.SelectedItem;
            
            if (selItem == null)
                return false;

            var selItemCont = selItem as NavigationViewItemBase;
            if (selItemCont == null)
            {
                selItemCont = GetContainerForIndexPath(_selectionModel.SelectedIndex);
            }

            return selItemCont == nvib;
        }

        private NavigationViewItem GetSelectedContainer()
        {
            if (SelectedItem == null)
                return null;

            if (SelectedItem is NavigationViewItem nvi)
            {
                return nvi;
            }
            else
            {
                return NavigationViewItemOrSettingsContentFromData(SelectedItem);
            }
        }

        private IEnumerable GetChildren(NavigationViewItem nvi)
        {
            return nvi.MenuItems;
        }

        private ItemsRepeater GetChildRepeaterForIndexPath(IndexPath ip)
        {
            if (GetContainerForIndexPath(ip) is NavigationViewItem nvi)
            {
                return nvi.GetRepeater;
            }

            return null;
        }

        private NavigationRecommendedTransitionDirection GetRecommendedTransitionDirection(IControl prev, IControl next)
        {
            var recTransDir = NavigationRecommendedTransitionDirection.Default;
            var ir = _topNavRepeater;

            if (prev != null && next != null && ir != null)
            {
                var prevIndexPath = GetIndexPathForContainer(prev as NavigationViewItemBase);
                var nextIndexPath = GetIndexPathForContainer(next as NavigationViewItemBase);

                var compare = prevIndexPath.CompareTo(nextIndexPath);

                switch (compare)
                {
                    case -1:
                        recTransDir = NavigationRecommendedTransitionDirection.FromRight;
                        break;
                    case 1:
                        recTransDir = NavigationRecommendedTransitionDirection.FromLeft;
                        break;
                    default:
                        recTransDir = NavigationRecommendedTransitionDirection.Default;
                        break;
                }
            }

            return recTransDir;
        }

        private NavigationTransitionInfo CreateNavigationTransitionInfo(NavigationRecommendedTransitionDirection recDir)
        {
            // In current implementation, if click is from overflow item, just recommend FromRight Slide animation.
            if (recDir == NavigationRecommendedTransitionDirection.FromOverflow)
            {
                recDir = NavigationRecommendedTransitionDirection.FromRight;
            }

            if ((recDir == NavigationRecommendedTransitionDirection.FromLeft ||
                recDir == NavigationRecommendedTransitionDirection.FromRight))
            {
                return new SlideNavigationTransitionInfo
                {
                    Effect = recDir == NavigationRecommendedTransitionDirection.FromRight ?
                     SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft
                };
            }
            else
            {
                return new EntranceNavigationTransitionInfo();
            }
        }

        internal NavigationViewItemsFactory ItemsFactory => _itemsFactory;

        private void UnhookEventsAndClearFields()
        {
            if (_paneToggleButton != null)
            {
                _paneToggleButton.Click -= OnPaneToggleButtonClick;
                _paneToggleButton = null;
            }

            if (_splitView != null)
            {
                _splitViewRevokers?.Dispose();
                _splitView.PaneClosed -= OnSplitViewPaneClosed;
                _splitView.PaneClosing -= OnSplitViewPaneClosing;
                _splitView.PaneOpened -= OnSplitViewPaneOpened;
                _splitView.PaneOpening -= OnSplitViewPaneOpening;
                _splitView = null;
            }

            if (_leftNavRepeater != null)
            {
                _leftNavRepeater.ElementClearing -= OnRepeaterElementClearing;
                _leftNavRepeater.ElementPrepared -= OnRepeaterElementPrepared;

                //loaded event;
                _leftNavRepeater.GotFocus -= OnRepeaterGettingFocus;
                _leftNavRepeater = null;
            }

            if (_topNavRepeater != null)
            {
                _topNavRepeater.ElementClearing -= OnRepeaterElementClearing;
                _topNavRepeater.ElementPrepared -= OnRepeaterElementPrepared;

                //loaded event;
                _topNavRepeater.GotFocus -= OnRepeaterGettingFocus;
                _topNavRepeater = null;
            }

            if (_topNavRepeaterOverflowView != null)
            {
                _topNavRepeaterOverflowView.ElementClearing -= OnRepeaterElementClearing;
                _topNavRepeaterOverflowView.ElementPrepared -= OnRepeaterElementPrepared;

                _topNavRepeaterOverflowView = null;
            }

            if (_topNavOverflowButton != null)
            {
                var flyout = _topNavOverflowButton.Flyout;
                if (flyout != null)
                {
                    flyout.Closing -= OnFlyoutClosing;
                }
            }

            if (_leftNavFooterMenuRepeater != null)
            {
                _leftNavFooterMenuRepeater.ElementClearing -= OnRepeaterElementClearing;
                _leftNavFooterMenuRepeater.ElementPrepared -= OnRepeaterElementPrepared;

                //loaded event;
                _leftNavFooterMenuRepeater.GotFocus -= OnRepeaterGettingFocus;
                _leftNavFooterMenuRepeater = null;
            }

            if (_topNavFooterMenuRepeater != null)
            {
                _topNavFooterMenuRepeater.ElementClearing -= OnRepeaterElementClearing;
                _topNavFooterMenuRepeater.ElementPrepared -= OnRepeaterElementPrepared;

                //loaded event;
                _topNavFooterMenuRepeater.GotFocus -= OnRepeaterGettingFocus;
                _topNavFooterMenuRepeater = null;
            }

            _paneTitleHolderRevoker?.Dispose();
            _paneTitleHolderRevoker = null;

            if (_paneSearchButton != null)
            {
                _paneSearchButton.Click -= OnPaneSearchButtonClick;
            }

            if (_backButton != null)
            {
                _backButton.Click -= OnBackButtonClicked;
            }

            //titlebar?

            if (_closeButton != null)
            {
                _closeButton.Click -= OnPaneToggleButtonClick;
            }

            _itemsContainerSizeRevoker?.Dispose();
            _itemsContainerSizeRevoker = null;

			_itemsContainerSizeRevoker?.Dispose();
		}

        private NavigationViewItemsFactory _itemsFactory;
        internal SplitView GetSplitView => _splitView;

        //Template Items
        private Button _paneToggleButton;
        private SplitView _splitView;
        private RowDefinition _itemsContainerRow;
        private ScrollViewer _menuItemsScrollViewer;
        private ScrollViewer _footerItemsScrollViewer;
        private Grid _paneContentGrid;
        //private ColumnDefinition _paneToggleButtonIconGridColumn;
        private Control _paneTitleHolderFrameworkElement;
        private IControl _paneTitleFrameworkElement;
        //private IControl _visualItemsSeparator;
        private Button _paneSearchButton;
        private Button _backButton;
        private Button _closeButton;
        private ItemsRepeater _leftNavRepeater;
        private ItemsRepeater _topNavRepeater;
        private ItemsRepeater _leftNavFooterMenuRepeater;
        private ItemsRepeater _topNavFooterMenuRepeater;
        private Button _topNavOverflowButton;
        private ItemsRepeater _topNavRepeaterOverflowView;
        private Grid _topNavGrid;
        private Border _topNavContentOverlayAreaGrid;
		private IControl _itemsContainer;

        //Indicator animations
        //private IControl _prevIndicator;
        //private IControl _nextIndicator;
        private IControl _activeIndicator;
        private object _lastSelectedItemPendingAnimationInTopNav;

        //private IControl _togglePaneTopPadding;
        //private IControl _contentPaneTopPadding;
        private Control _contentLeftPadding;

        //Titlebar

        private ContentControl _leftNavAutoSuggestBoxPresenter;
        private ContentControl _topNavAutoSuggestBoxPresenter;

        private ContentControl _leftNavPaneHeaderContentBorder;
        private ContentControl _leftNavPaneCustomContentBorder;
        private ContentControl _leftNavFooterContentBorder;

        private ContentControl _paneHeaderOnTopPane;
        private ContentControl _paneTitleOnTopPane;
        private ContentControl _paneCustomContentOnTopPane;
        private ContentControl _paneFooterOnTopPane;
        private ContentControl _paneTitlePresenter;

        private ColumnDefinition _paneHeaderCloseButtonColumn;
        private ColumnDefinition _paneHeaderToggleButtonColumn;
        private RowDefinition _paneHeaderContentBorderRow;

        private NavigationViewItem _lastItemExpandedIntoFlyout;

        private IDisposable _splitViewRevokers;
        private IDisposable _sizeChangedRevoker;
        private IDisposable _paneTitleHolderRevoker;
        private IDisposable _itemsContainerSizeRevoker;

        bool _wasForceClosed;
        bool _isClosedCompact;
        bool _blockNextClosingEvent;
        //bool _initialListSizeStateSet;
		bool _isLeftPaneTitleEmpty;

        private TopNavigationViewDataProvider _topDataProvider;

        private SelectionModel _selectionModel;
        private AvaloniaList<IEnumerable> _selectionModelSource;

        //private ItemsSourceView _menuItemsSource;
        //private ItemsSourceView _footerItemsSource;

        private bool _appliedTemplate;

        // Identifies whenever a call is the result of OnApplyTemplate
        private bool _fromOnApplyTemplate;

        // Used to defer updating the SplitView displaymode property
        private bool _updateVisualStateForDisplayModeFromOnLoaded;


        // flag is used to stop recursive call. eg:
        // Customer select an item from SelectedItem property->ChangeSelection update ListView->LIstView raise OnSelectChange(we want stop here)->change property do do animation again.
        // Customer clicked listview->listview raised OnSelectChange->SelectedItem property changed->ChangeSelection->Undo the selection by SelectedItem(prevItem) (we want it stop here)->ChangeSelection again ->...
        private bool _shouldIgnoreNextSelectionChange;

        // A flag to track that the selectionchange is caused by selection a item in topnav overflow menu
        private bool _selectionChangeFromOverflowMenu;

        // Flag indicating whether selection change should raise item invoked. This is needed to be able to raise ItemInvoked before SelectionChanged while SelectedItem should point to the clicked item
        private bool _shouldRaiseItemInvokedAfterSelection;

        private TopNavigationViewLayoutState _topNavigationMode = TopNavigationViewLayoutState.Uninitialized;

        // A threshold to stop recovery from overflow to normal happens immediately on resize.
        private readonly float _topNavigationRecoveryGracePeriodWidth = 5f;

        // There are three ways to change IsPaneOpen:
        // 1, customer call IsPaneOpen=true/false directly or nav.IsPaneOpen is binding with a variable and the value is changed.
        // 2, customer click ToggleButton or splitView.IsPaneOpen->nav.IsPaneOpen changed because of window resize
        // 3, customer changed PaneDisplayMode.
        // 2 and 3 are internal implementation and will call by ClosePane/OpenPane. the flag is to indicate 1 if it's false
        private bool _isOpenPaneForInteraction;

        private bool _moveTopNavOverflowItemOnFlyoutClose;

        //private bool _shouldIgnoreUIASelectionRaiseAsExpandCollapseWillRaise;

        private bool _orientationChangedPendingAnimation;

        private bool _tabKeyPrecedesFocusChange;

        private bool _initialNonForcedModeUpdate = true;


        private const int _backButtonHeight = 40;
        private const int _backButtonWidth = 40;
        private const int _paneToggleButtonHeight = 40;
        private const int _paneToggleButtonWidth = 40;
        private const int _backButtonRowDefinition = 1;
        private const float paneElevationTranslationZ = 32;

        private const int _mainMenuBlockIndex = 0;
        private const int _footerMenuBlockIndex = 1;

        private const int _itemNotFound = -1;

        private double _openPaneWidth = 320; //WinUI #5800
    }
}
