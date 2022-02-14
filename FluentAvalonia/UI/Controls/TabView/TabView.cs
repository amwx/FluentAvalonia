using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    public partial class TabView : TemplatedControl, IContentPresenterHost
    {
        public TabView()
        {
            TabItems = new AvaloniaList<object>();

            // Keyboard Accelerators (KeyBindings in Avalonia)
            // Require a Command, so we wire this up *slightly* differently
            // compared to WinUI

            _keyboardAcceleratorHandler = new TabViewCommand(OnKeyboardAcceleratorInvoked);

            var closeTabGesture = new KeyGesture(Key.F4, KeyModifiers.Control);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = closeTabGesture,
                Command = _keyboardAcceleratorHandler,
                CommandParameter = TabViewCommandType.CtrlF4
            });
            KeyBindings.Add(new KeyBinding
            {
                Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control),
                Command = _keyboardAcceleratorHandler,
                CommandParameter = TabViewCommandType.CtrlTab
            });
            KeyBindings.Add(new KeyBinding
            {
                Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control | KeyModifiers.Shift),
                Command = _keyboardAcceleratorHandler,
                CommandParameter = TabViewCommandType.CtrlShftTab
            });            

            _tabCloseButtonTooltipText = "Close tab (Ctrl+F4)";
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            UnhookEventsAndClearFields();

            base.OnApplyTemplate(e);

            _tabContentPresenter = e.NameScope.Find<ContentPresenter>("TabContentPresenter");
            _rightContentPresenter = e.NameScope.Find<ContentPresenter>("RightContentPresenter");

            _tabContainerGrid = e.NameScope.Find<Grid>("TabContainerGrid");
            if (_tabContainerGrid != null)
            {
                _leftContentColumn = _tabContainerGrid.ColumnDefinitions[0];
                _tabColumn = _tabContainerGrid.ColumnDefinitions[1];
                _addButtonColumn = _tabContainerGrid.ColumnDefinitions[2];
                _rightContentColumn = _tabContainerGrid.ColumnDefinitions[3];

                _tabContainerGrid.PointerEnter += OnTabStripPointerEnter;
                _tabContainerGrid.PointerLeave += OnTabStripPointerLeave;

                // Adding this to mimic XYFocusKeyboardNavigation in the tabstrip
                _tabContainerGrid.KeyDown += OnTabContainerGridKeyDown;
            }

            _listView = e.NameScope.Find<TabViewListView>("TabListView");
           
            if (_listView != null)
            {
                LogicalChildren.Add(_listView);
                _listView.SelectionChanged += OnListViewSelectionChanged;

                _listView.DragItemsStarting += OnListViewDragItemsStarting;
                _listView.DragItemsCompleted += OnListViewDragItemsCompleted;
                _listView.DragOver += OnListViewDragOver;
                _listView.Drop += OnListViewDrop;

                _listView.GotFocus += OnListViewGettingFocus;

                _listViewCanReorderItemsPropertyChangedRevoker =
                    _listView.GetPropertyChangedObservable(TabViewListView.CanReorderItemsProperty)
                    .Subscribe(_ => OnListViewDraggingPropertyChanged());
                _listViewAllowDropPropertyChangedRevoker =
                    _listView.GetPropertyChangedObservable(DragDrop.AllowDropProperty)
                    .Subscribe(_ => OnListViewDraggingPropertyChanged());

                // Since we don't have the loaded event, and the ListView is already in the tree
                // we can't use AttachedToVisualTree, which is the suggested replacement
                // We also can't call loaded now because the ItemsPresenter and Panel haven't been
                // established yet so the Loaded logic will fail. What we'll do instead is listen
                // for the first bounds change of the ListView as at that point all will be 
                // established - we'll then revoke the listener
                _listViewLoadedRevoker = _listView.GetPropertyChangedObservable(BoundsProperty)
                    .Subscribe(_ => OnListViewLoaded());
            }

            _addButton = e.NameScope.Find<Button>("AddButton");
            if (_addButton != null)
            {
                if (ToolTip.GetTip(_addButton) == null)
                {
                    ToolTip.SetTip(_addButton, "Add new tab");
                }

                _addButton.Click += OnAddButtonClick;
            }

            // Ignore ThemeShadow

            UpdateListViewItemContainerTransitions();

            // Again, no Loaded event, so just call Loaded here
            OnLoaded();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _ = base.MeasureOverride(availableSize);

            if (_previousAvailableSize.Width != availableSize.Width)
            {
                _previousAvailableSize = availableSize;
                UpdateTabWidths();
            }

            return base.MeasureOverride(availableSize);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CloseButtonOverlayModeProperty)
            {
                OnCloseButtonOverlayModePropertyChanged(change);
            }
            else if (change.Property == SelectedIndexProperty)
            {
                OnSelectedIndexPropertyChanged(change);
            }
            else if (change.Property == SelectedItemProperty)
            {
                OnSelectedItemPropertyChanged(change);
            }
            else if (change.Property == TabItemsProperty)
            {
                OnTabItemsSourcePropertyChanged(change);
            }
            else if (change.Property == TabWidthModeProperty)
            {
                OnTabWidthModePropertyChanged(change);
            }
        }


        internal void SetTabSeparatorOpacity(int index, double opacity) 
        { 
            if (ContainerFromIndex(index) is TabViewItem tvi)
            {
                // The reason we set the opacity directly instead of using VisualState
                // is because we want to hide the separator on hover/pressed
                // but the tab adjacent on the left to the selected tab
                // must hide the tab separator at all times.
                // It causes two visual states to modify the same property
                // what leads to undesired behaviour.
                if (tvi.TabSeparator != null)
                    tvi.TabSeparator.Opacity = opacity;
            }
        }

        internal void SetTabSeparatorOpacity(int index) 
        {
            var selIndex = SelectedIndex;

            // If Tab is adjacent on the left to selected one or
            // it is selected tab - we hide the tabSeparator.
            if (index == selIndex || index+1 == selIndex)
            {
                SetTabSeparatorOpacity(index, 0);
            }
            else
            {
                SetTabSeparatorOpacity(index, 1);
            }
        }

        private void OnListViewDraggingPropertyChanged()
        {
            // Callback from LV prop changed for canreorder and allow drop
            UpdateListViewItemContainerTransitions();
        }


        private void OnListViewGettingFocus(object sender, GotFocusEventArgs e)
        {
            // TabViewItems overlap each other by one pixel in order to get the desired visuals for the separator.
            // This causes problems with 2d focus navigation. Because the items overlap, pressing Down or Up from a
            // TabViewItem navigates to the overlapping item which is not desired.
            //
            // To resolve this issue, we detect the case where Up or Down focus navigation moves from one TabViewItem
            // to another.
            // How we handle it, depends on the input device.
            // For GamePad, we want to move focus to something in the direction of movement (other than the overlapping item)
            // For Keyboard, we cancel the focus movement.

            // Can ignore this...we don't have GettingFocus event, and no gamepad support
            // so there's nothing we can actually do here
        }

        private void OnSelectedIndexPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            // We update previous selected and adjacent on the left tab
            // as well as current selected and adjacent on the left tab
            // to show/hide tabSeparator accordingly.
            UpdateSelectedIndex();
            SetTabSeparatorOpacity(change.OldValue.GetValueOrDefault<int>());
            SetTabSeparatorOpacity(change.OldValue.GetValueOrDefault<int>() - 1);
            SetTabSeparatorOpacity(change.NewValue.GetValueOrDefault<int>() - 1);
            SetTabSeparatorOpacity(change.NewValue.GetValueOrDefault<int>());

            UpdateTabBottomBorderLineVisualStates();
        }

        private void UpdateTabBottomBorderLineVisualStates()
        {
            int numItems = TabItems.Count();
            int selIndex = SelectedIndex;

            for (int i = 0; i < numItems; i++)
            {
                // -1 = normal, 0 = no bottom border, 1 = leftofselectedtab, 2 = rightofselectedtab
                int state = -1;
                if (_isDragging)
                {
                    state = 0;
                }
                else if (selIndex != -1)
                {
                    if (i == selIndex)
                    {
                        state = 0;
                    }
                    else if (i == selIndex - 1)
                    {
                        state = 1;
                    }
                    else if (i == selIndex + 1)
                    {
                        state = 2;
                    }
                }

                if (ContainerFromIndex(i) is TabViewItem tvi)
                {
                    ((IPseudoClasses)tvi.Classes).Set(":noborder", state == 0);
                    ((IPseudoClasses)tvi.Classes).Set(":borderleft", state == 1);
                    ((IPseudoClasses)tvi.Classes).Set(":borderright", state == 2);
                }                
            }
        }

        private void UpdateBottomBorderLineVisualStates()
        {
            // Update border line on all tabs
            UpdateTabBottomBorderLineVisualStates();

            PseudoClasses.Set(":singleborder", _isDragging);

            // Update border lines in the inner TabViewListView
            if (_listView != null)
            {
                (_listView.Classes as IPseudoClasses).Set(":noborder", _isDragging);
            }

            // Update border lines in the ScrollViewer
            if (_scrollViewer != null)
            {
                (_scrollViewer.Classes as IPseudoClasses).Set(":noborder", _isDragging);
            }
        }
                
        private void OnSelectedItemPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            UpdateSelectedItem();
        }

        private void OnTabItemsSourcePropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            UpdateListViewItemContainerTransitions();
        }

        private void UpdateListViewItemContainerTransitions()
        {
            // IGNORE
        }

        private void OnTabWidthModePropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            UpdateTabWidths();

            var newValue = change.NewValue.GetValueOrDefault<TabViewWidthMode>();
            // Switch the visual states of all tab items to the correct TabViewWidthMode
            int itemCount = TabItems.Count();
            for (int i = 0; i < itemCount; i++)
            {
                if (ContainerFromIndex(i) is TabViewItem tvi)
                {
                    tvi.OnTabViewWidthModeChanged(newValue);
                }
            }
        }

        private void OnCloseButtonOverlayModePropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            var newValue = change.NewValue.GetValueOrDefault<TabViewCloseButtonOverlayMode>();
            // Switch the visual states of all tab items to the correct TabViewWidthMode
            int itemCount = TabItems.Count();
            for (int i = 0; i < itemCount; i++)
            {
                if (ContainerFromIndex(i) is TabViewItem tvi)
                {
                    tvi.OnCloseButtonOverlayModeChanged(newValue);
                }
            }
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs args)
        {
            AddTabButtonClick?.Invoke(this, args);
        }

        private void OnLoaded()
        {
            UpdateTabContent();
        }

        private void OnListViewLoaded()
        {
            _listViewLoadedRevoker.Dispose();
            _listViewLoadedRevoker = null;

            // WinUI does a bunch of weirdness here to add the items to the ListView
            // Probably because of the TabItems vs TabItemsSource thing
            // We can skip most of that and just assign the items to the ListView

            //_listView.Items = TabItems;

            if (this.GetDiagnostic(SelectedItemProperty).Priority != Avalonia.Data.BindingPriority.Unset)
            {
                UpdateSelectedItem();
            }
            else
            {
                // If SelectedItem wasn't set, default to selecting the first tab
                UpdateSelectedIndex();
            }

            SelectedIndex = _listView.SelectedIndex;
            SelectedItem = _listView.SelectedItem;

            _itemsPresenter = _listView.Presenter as ItemsPresenter;

            _itemsPresenterSizeChangedRevoker = _itemsPresenter.GetPropertyChangedObservable(BoundsProperty).Subscribe(OnItemsPresenterSizeChanged);

            _scrollViewer = _listView.Scroller;
            if (_scrollViewer != null)
            {
                // Since we don't have loaded event and it's already in the tree, we'll 
                // just call Loaded here
                OnScrollViewerLoaded();
            }

            UpdateTabBottomBorderLineVisualStates();
        }

        private void OnTabStripPointerLeave(object sender, PointerEventArgs e)
        {
            _pointerInTabstrip = false;
            if (_updateTabWidthOnPointerLeave)
            {
                try
                {
                    UpdateTabWidths();
                }
                finally
                {
                    _updateTabWidthOnPointerLeave = false;
                }
            }
        }

        private void OnTabStripPointerEnter(object sender, PointerEventArgs e)
        {
            _pointerInTabstrip = true;
        }

        private void OnScrollViewerLoaded()
        {
            var buttons = _scrollViewer.GetTemplateChildren()
                .Where(x => x is RepeatButton);

            foreach(RepeatButton button in buttons)
            {
                if (button.Name == "ScrollDecreaseButton")
                {
                    _scrollDecreaseButton = button;
                    ToolTip.SetTip(_scrollDecreaseButton, "Scroll tab list backward");
                    _scrollDecreaseButton.Click += OnScrollDecreaseClick;
                }
                else if (button.Name == "ScrollIncreaseButton")
                {
                    _scrollIncreaseButton = button;
                    ToolTip.SetTip(_scrollIncreaseButton, "Scroll tab list forward");
                    _scrollIncreaseButton.Click += OnScrollIncreaseClick;
                }                
            }

            _scrollViewerViewChangedRevoker = new CompositeDisposable(
                _scrollViewer.GetPropertyChangedObservable(ScrollViewer.OffsetProperty).Subscribe(OnScrollViewerViewChanged),
                _scrollViewer.GetPropertyChangedObservable(ScrollViewer.ExtentProperty).Subscribe(OnScrollViewerViewChanged),
                _scrollViewer.GetPropertyChangedObservable(ScrollViewer.ViewportProperty).Subscribe(OnScrollViewerViewChanged)
                );
        }

        private void OnScrollViewerViewChanged(AvaloniaPropertyChangedEventArgs args)
        {
            UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
        }

        private void UpdateScrollViewerDecreaseAndIncreaseButtonsViewState()
        {
            if (_scrollViewer != null)
            {
                const double minThreshold = 0.1d;
                var hOffset = _scrollViewer.Offset.X;
                var scrollableWidth = (_scrollViewer.Extent.Width - _scrollViewer.Viewport.Width);

                if (Math.Abs(hOffset - scrollableWidth) < minThreshold)
                {
                    if (_scrollDecreaseButton != null)
                    {
                        _scrollDecreaseButton.IsEnabled = true;
                    }
                    if (_scrollIncreaseButton != null)
                    {
                        _scrollIncreaseButton.IsEnabled = false;
                    }
                }
                else if (Math.Abs(hOffset) < minThreshold)
                {
                    if (_scrollDecreaseButton != null)
                    {
                        _scrollDecreaseButton.IsEnabled = false;
                    }
                    if (_scrollIncreaseButton != null)
                    {
                        _scrollIncreaseButton.IsEnabled = true;
                    }
                }
                else
                {
                    if (_scrollDecreaseButton != null)
                    {
                        _scrollDecreaseButton.IsEnabled = true;
                    }
                    if (_scrollIncreaseButton != null)
                    {
                        _scrollIncreaseButton.IsEnabled = true;
                    }
                }
            }
        }

        private void OnItemsPresenterSizeChanged(AvaloniaPropertyChangedEventArgs args)
        {
            var newValue = (Rect)args.NewValue;
            // We're observing bounds, make sure we only do this on size changed
            if (_lastItemsPresenterSize == newValue.Size)
                return;

            _lastItemsPresenterSize = newValue.Size;
            if (!_updateTabWidthOnPointerLeave)
            {
                // Presenter size didn't change because of item being removed, so update manually
                UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                UpdateTabWidths();
                // Make sure that the selected tab is fully in view and not cut off
                BringSelectedTabIntoView();
            }
        }

        private void BringSelectedTabIntoView()
        {
            if (SelectedItem != null)
            {
                var tvi = SelectedItem as TabViewItem;
                if (tvi == null)
                    tvi = ContainerFromItem(SelectedItem) as TabViewItem;

                tvi.StartBringTabIntoView();
            }
        }

        internal void OnItemsChanged(NotifyCollectionChangedEventArgs args)
        {
            if (_isDragging)
                return;

            // Change from WinUI b/c we don't have TabItems and TabItemsSource
            // if args is null, it was full items refresh

            int numItems = TabItems.Count();
            var lvSelIndex = _listView.SelectedIndex;
            var selIndex = SelectedIndex;

            if (selIndex != lvSelIndex && lvSelIndex != -1)
            {
                SelectedIndex = lvSelIndex;
                selIndex = lvSelIndex;
            }

            if (args is NotifyCollectionChangedEventArgs incc)
            {
                TabItemsChanged?.Invoke(this, incc);

                if (incc.Action == NotifyCollectionChangedAction.Remove)
                {
                    _updateTabWidthOnPointerLeave = true;
                    if (numItems > 0)
                    {
                        // SelectedIndex might also already be -1
                        if (selIndex == -1 || selIndex == incc.OldStartingIndex)
                        {
                            // Find the closest tab to select instead
                            int startIndex = incc.OldStartingIndex;
                            if (startIndex >= numItems)                                
                            {
                                startIndex = numItems - 1;
                            }
                            int index = startIndex;

                            do
                            {
                                var nextitem = ContainerFromIndex(index) as TabViewItem;

                                if (nextitem != null && nextitem.IsEffectivelyEnabled
                                    && nextitem.IsEffectivelyVisible)
                                {
                                    SelectedItem = ItemFromContainer(nextitem);
                                    break;
                                }

                                // try the next item
                                index++;
                                if (index >= numItems)
                                {
                                    index = 0;
                                }
                            }
                            while (index != startIndex);
                        }
                    }
                    
                    if (TabWidthMode == TabViewWidthMode.Equal)
                    {
                        if (!_pointerInTabstrip || args.OldStartingIndex == TabItems.Count())
                        {
                            UpdateTabWidths(true, false);
                        }
                    }
                }
                else
                {
                    UpdateTabWidths();
                    SetTabSeparatorOpacity(numItems - 1);
                }
            }
            else
            {
                // Added this for full collection change - Set content to first item
                if (lvSelIndex == -1 && numItems > 0)
                {
                    SelectedIndex = 0;
                }
            }

            UpdateTabBottomBorderLineVisualStates();
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            SelectedIndex = _listView.SelectedIndex;
            SelectedItem = _listView.SelectedItem;

            UpdateTabContent();

            SelectionChanged?.Invoke(this, args);
        }

        private TabViewItem FindTabViewItemFromDragItem(object item)
        {
            // This *should* always work for us and we don't need the WinUI fallbacks
            // because we only have TabItems - and everything, regardless of binding
            // or direct items, goes through the ICG
            return ContainerFromItem(item) as TabViewItem;
        }

        private void OnListViewDragItemsStarting(object sender, DragItemsStartingEventArgs args) 
        {
            _isDragging = true;

            var item = args.Items[0];
            var tab = FindTabViewItemFromDragItem(item);
            var myArgs = new TabViewTabDragStartingEventArgs(args, item, tab);

            TabDragStarting?.Invoke(this, myArgs);

            UpdateBottomBorderLineVisualStates();
        }

        private void OnListViewDragOver(object sender, DragEventArgs args) 
        {
            TabStripDragOver?.Invoke(this, args);
        }

        private void OnListViewDrop(object sender, DragEventArgs args) 
        {
            TabStripDrop?.Invoke(this, args);
        }

        private void OnListViewDragItemsCompleted(TabViewListView sender, DragItemsCompletedEventArgs args)
        {
            _isDragging = false;

            // Selection change was disabled during drag, update SelectedIndex now
            if (_listView != null)
            {
                SelectedIndex = _listView.SelectedIndex;
                SelectedItem = _listView.SelectedItem;
            }

            var item = args.Items[0];
            var tab = FindTabViewItemFromDragItem(item);
            var myArgs = new TabViewTabDragCompletedEventArgs(args, item, tab);

            TabDragCompleted?.Invoke(this, myArgs);

            // None means it's outside of the tab strip area
            if (args.DropResult == DragDropEffects.None)
            {
                var tabDroppedArgs = new TabViewTabDroppedOutsideEventArgs(item, tab);
                TabDroppedOutside?.Invoke(this, tabDroppedArgs);
            }

            UpdateBottomBorderLineVisualStates();
        }

        private void UpdateTabContent()
        {
            if (_tabContentPresenter == null)
                return;

            if (SelectedItem == null)
            {
                _tabContentPresenter.Content = null;
                _tabContentPresenter.ContentTemplate = null;
            }
            else
            {
                var tvi = (SelectedItem as TabViewItem) ?? ContainerFromItem(SelectedItem) as TabViewItem;

                if (tvi != null)
                {
                    // If the focus was in the old tab content, we will lose focus when it is removed from the visual tree.
                    // We should move the focus to the new tab content.
                    // The new tab content is not available at the time of the LosingFocus event, so we need to
                    // move focus later.
                    bool shouldMoveFocusToNewTab = false;
                    _tabContentPresenter.LostFocus += (s, e) =>
                    {
                        shouldMoveFocusToNewTab = true;
                    };

                    _tabContentPresenter.Content = tvi.Content;
                    _tabContentPresenter.ContentTemplate = tvi.ContentTemplate;

                    // It is not ideal to call UpdateLayout here, but it is necessary to ensure that the ContentPresenter has expanded its content
                    // into the live visual tree.
                    InvalidateMeasure();
                    InvalidateArrange();

                    if (shouldMoveFocusToNewTab)
                    {
                        var focusable = KeyboardNavigationHandler.GetNext(_tabContentPresenter, NavigationDirection.Next);
                        if (focusable == null)
                        {
                            // If there is nothing focusable in the new tab, just move focus to the TabViewItem itself.
                            focusable = tvi;
                        }

                        if (focusable !=null)
                        {
                            FocusManager.Instance?.Focus(focusable, NavigationMethod.Unspecified);
                        }
                    }
                }
            }
        }

        internal void RequestCloseTab(TabViewItem container, bool updateTabWidths) 
        {
            if (_listView != null)
            {
                var args = new TabViewTabCloseRequestedEventArgs(ItemFromContainer(container), container);

                TabCloseRequested?.Invoke(this, args);

                container.RaiseRequestClose(args);
            }

            UpdateTabWidths(updateTabWidths);
        }

        private void OnScrollDecreaseClick(object sender, RoutedEventArgs args)
        {
            if (_scrollViewer != null)
            {
                var current = _scrollViewer.Offset;
                _scrollViewer.Offset = current.WithX(current.X - c_scrollAmount);                
            }
        }

        private void OnScrollIncreaseClick(object sender, RoutedEventArgs args)
        {
            if (_scrollViewer != null)
            {
                var current = _scrollViewer.Offset;
                _scrollViewer.Offset = current.WithX(current.X + c_scrollAmount);
            }
        }

        private void UpdateTabWidths(bool shouldUpdateWidths = true, bool fillAllAvailableSpace = true)
        {
            var tabWidth = double.NaN;

            if (_tabContainerGrid != null)
            {
                // Add up width taken by custom content and + button
                double widthTaken = 0.0;
                if (_leftContentColumn != null)
                {
                    widthTaken += _leftContentColumn.ActualWidth;
                }
                if (_addButtonColumn != null)
                {
                    widthTaken += _addButtonColumn.ActualWidth;
                }
                if (_rightContentColumn != null)
                {
                    if (_rightContentPresenter != null)
                    {
                        var size = _rightContentPresenter.DesiredSize;
                        _rightContentColumn.MinWidth = size.Width;
                        widthTaken += size.Width;
                    }
                }

                if (_tabColumn != null)
                {
                    // Note: can be infinite
                    var availableWidth = _previousAvailableSize.Width - widthTaken;

                    // Size can be 0 when window is first created; in that case, skip calculations; we'll get a new size soon
                    if (availableWidth > 0)
                    {
                        if (TabWidthMode == TabViewWidthMode.Equal)
                        {
                            var minTabWidth = this.TryFindResource(c_tabViewItemMinWidthName, out var value) ? (double)value : c_tabMinimumWidth;
                            var maxTabWidth = this.TryFindResource(c_tabViewItemMaxWidthName, out value) ? (double)value : c_tabMaximumWidth;

                            // If we should fill all of the available space, use scrollviewer dimensions
                            var padding = Padding;

                            double headerWidth = 0.0;
                            double footerWidth = 0.0;
                            // don't have header/footer so skip

                            if (fillAllAvailableSpace)
                            {
                                // Calculate the proportional width of each tab given the width of the ScrollViewer.
                                var tabWidthForScroller = (availableWidth - (padding.Horizontal() + headerWidth + footerWidth)) / (double)TabItems.Count();
                                tabWidth = MathHelpers.Clamp(tabWidthForScroller, minTabWidth, maxTabWidth);
                            }
                            else
                            {
                                double availableTabViewSpace = (_tabColumn.ActualWidth - (padding.Horizontal() + headerWidth + footerWidth));
                                if (_scrollIncreaseButton != null)
                                {
                                    if (_scrollIncreaseButton.IsVisible)
                                    {
                                        availableTabViewSpace -= _scrollIncreaseButton.Bounds.Width;
                                    }
                                }

                                if (_scrollDecreaseButton != null)
                                {
                                    if (_scrollDecreaseButton.IsVisible)
                                    {
                                        availableTabViewSpace -= _scrollDecreaseButton.Bounds.Width;
                                    }
                                }

                                // Use current size to update items to fill the currently occupied space
                                var tabWidthUnclamped = availableTabViewSpace / (double)TabItems.Count();
                                tabWidth = MathHelpers.Clamp(tabWidthUnclamped, minTabWidth, maxTabWidth);
                            }

                            // Size tab column to needed size
                            _tabColumn.MaxWidth = availableWidth + headerWidth + footerWidth;
                            var requiredWidth = tabWidth * TabItems.Count() + headerWidth + footerWidth;
                            if (requiredWidth > availableWidth - padding.Horizontal())
                            {
                                _tabColumn.Width = new GridLength(availableWidth);
                                if (_listView != null)
                                {
                                    ScrollViewer.SetHorizontalScrollBarVisibility(_listView,
                                            ScrollBarVisibility.Visible);
                                    UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                                }
                            }
                            else
                            {
                                _tabColumn.Width = new GridLength(1, GridUnitType.Auto);
                                if (_listView != null)
                                {
                                    if (shouldUpdateWidths && fillAllAvailableSpace)
                                    {
                                        ScrollViewer.SetHorizontalScrollBarVisibility(_listView,
                                            ScrollBarVisibility.Hidden);
                                    }
                                    else
                                    {
                                        if (_scrollDecreaseButton != null)
                                        {
                                            _scrollDecreaseButton.IsEnabled = false;
                                        }

                                        if (_scrollIncreaseButton != null)
                                        {
                                            _scrollIncreaseButton.IsEnabled = false;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _tabColumn.MaxWidth = availableWidth;
                            _tabColumn.Width = new GridLength(1, GridUnitType.Auto);

                            if (_listView != null)
                            {
                                _listView.MaxWidth = availableWidth;

                                if (_itemsPresenter != null)
                                {
                                    var visible = _itemsPresenter.Bounds.Width > availableWidth;
                                    ScrollViewer.SetHorizontalScrollBarVisibility(_listView,
                                        visible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden);

                                    if (visible)
                                    {
                                        UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if DEBUG
            Logger.TryGet(LogEventLevel.Debug, "TabView")?
                .Log("TabView", $"TabView UpdateTabWidths: \nShould Update:{shouldUpdateWidths} Fill:{fillAllAvailableSpace} - TabWidth: {tabWidth}\n");
#endif

            if (shouldUpdateWidths || TabWidthMode != TabViewWidthMode.Equal)
            {
                var count = TabItems.Count();
                for (int i = 0; i < count; i++)
                {
                    if (ContainerFromIndex(i) is TabViewItem tvi)
                    {
                        tvi.Width = tabWidth;
                    }
                }
            }
        }

        private void UpdateSelectedItem()
        {
            if (_listView != null)
                _listView.SelectedItem = SelectedItem;
        }

        private void UpdateSelectedIndex()
        {
            if (_listView != null)
            {
                var index = SelectedIndex;
                if (index < _listView.ItemCount)
                {
                    _listView.SelectedIndex = index;
                }
            }
        }

        public IControl ContainerFromItem(object item) =>
            _listView?.ContainerFromItem(item);

        public IControl ContainerFromIndex(int index) =>
            _listView?.ContainerFromIndex(index);

        public int IndexFromContainer(IControl container) =>
            _listView?.IndexFromContainer(container) ?? -1;

        public object ItemFromContainer(IControl container) =>
            _listView?.ItemFromContainer(container);

        private int GetItemCount() => TabItems.Count();

        private bool SelectNextTab(int increment)
        {
            bool handled = false;
            int itemSize = GetItemCount();
            if (itemSize > 1)
            {
                var index = SelectedIndex;
                index = (index + increment + itemSize) % itemSize;
                SelectedIndex = index;
                handled = true;
            }

            return handled;
        }

        private bool RequestCloseCurrentTab()
        {
            bool handled = false;
            if (SelectedItem is TabViewItem tvi)
            {
                if (tvi.IsClosable)
                {
                    RequestCloseTab(tvi, true);
                    handled = true;
                }
            }

            return handled;
        }

        internal string GetTabCloseButtonTooltipText() =>
            _tabCloseButtonTooltipText;

        protected virtual void OnKeyboardAcceleratorInvoked(object parameter)
        {
            switch ((TabViewCommandType)parameter)
            {
                case TabViewCommandType.CtrlF4:
                    RequestCloseCurrentTab();
                    break;

                case TabViewCommandType.CtrlTab:
                    SelectNextTab(1);
                    break;

                case TabViewCommandType.CtrlShftTab:
                    SelectNextTab(-1);
                    break;
            }
        }

        private void OnTabContainerGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                var direction = e.Key == Key.Left ? NavigationDirection.Previous : NavigationDirection.Next;

                var current = FocusManager.Instance?.Current;

                if (current is Control c && _tabContainerGrid.IsVisualAncestorOf(c))
                {
                    if (_listView != null)
                    {
                        if (current is TabViewItem tvi)
                        {
                            var index = _listView.IndexFromContainer(tvi);

                            if (index == -1)
                                return;

                            index = direction == NavigationDirection.Previous ? index - 1 : index + 1;

                            if (index >= 0 && index < _listView.ItemCount)
                            {
                                var container = _listView.ContainerFromIndex(index);
                                if (container != null)
                                {
                                    FocusManager.Instance?.Focus(container, NavigationMethod.Directional);
                                }
                            }
                            else if (index == _listView.ItemCount && _addButton != null)
{
                                FocusManager.Instance?.Focus(_addButton, NavigationMethod.Directional);
                            }
                        }
                        else if (current == _addButton)
                        {
                            if (direction == NavigationDirection.Previous && _listView.ItemCount > 0)
{
                                var container = _listView.ContainerFromIndex(_listView.ItemCount - 1);
                                if (container != null)
                                {
                                    FocusManager.Instance?.Focus(container, NavigationMethod.Directional);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_listView != null && _listView.ItemCount > 0 && direction == NavigationDirection.Next)
                    {
                        FocusManager.Instance?.Focus(_listView.ContainerFromIndex(0), 
                            NavigationMethod.Directional);
                    }
                    else if (_addButton != null)
                    {
                        FocusManager.Instance?.Focus(_addButton, NavigationMethod.Directional);
                    }                    
                }
            }
        }

        private void UnhookEventsAndClearFields()
        {
            if (_tabContainerGrid != null)
            {
                _tabContainerGrid.PointerEnter -= OnTabStripPointerEnter;
                _tabContainerGrid.PointerLeave -= OnTabStripPointerLeave;
            }

            if (_listView != null)
            {
                LogicalChildren.Remove(_listView);
                _listView.SelectionChanged -= OnListViewSelectionChanged;
                _listView.GotFocus -= OnListViewGettingFocus;

                _listView.DragItemsStarting -= OnListViewDragItemsStarting;
                _listView.DragItemsCompleted -= OnListViewDragItemsCompleted;
                _listView.DragOver -= OnListViewDragOver;
                _listView.Drop -= OnListViewDrop;
                _listViewAllowDropPropertyChangedRevoker?.Dispose();
                _listViewCanReorderItemsPropertyChangedRevoker?.Dispose();
            }

            if (_addButton != null)
            {
                _addButton.Click -= OnAddButtonClick;
            }

            _itemsPresenterSizeChangedRevoker?.Dispose();

            if (_scrollDecreaseButton != null)
                _scrollDecreaseButton.Click -= OnScrollDecreaseClick;

            if (_scrollIncreaseButton != null)
                _scrollIncreaseButton.Click -= OnScrollIncreaseClick;

            _scrollViewerViewChangedRevoker?.Dispose();


            _leftContentColumn = null;
            _tabColumn = null;
            _addButtonColumn = null;
            _rightContentColumn = null;

            _listView = null;
            _tabContentPresenter = null;
            _rightContentPresenter = null;
            _tabContainerGrid = null;
            _scrollViewer = null;
            _scrollDecreaseButton = null;
            _scrollIncreaseButton = null;
            _addButton = null;
            _itemsPresenter = null;
        }

        bool IContentPresenterHost.RegisterContentPresenter(IContentPresenter presenter)
        {
            if (presenter.Name == "TabContentPresenter")
                return true;

            return false;
        }

        IAvaloniaList<ILogical> IContentPresenterHost.LogicalChildren => LogicalChildren;


        private TabViewCommand _keyboardAcceleratorHandler;

        private bool _updateTabWidthOnPointerLeave = false;
        private bool _pointerInTabstrip = false;

        private ColumnDefinition _leftContentColumn;
        private ColumnDefinition _tabColumn;
        private ColumnDefinition _addButtonColumn;
        private ColumnDefinition _rightContentColumn;

        private TabViewListView _listView;
        private ContentPresenter _tabContentPresenter;
        private ContentPresenter _rightContentPresenter;
        private Grid _tabContainerGrid;
        private ScrollViewer _scrollViewer;
        private RepeatButton _scrollDecreaseButton;
        private RepeatButton _scrollIncreaseButton;
        private Button _addButton;
        private ItemsPresenter _itemsPresenter;

        // private Grid _shadowReceiver

        // A bunch of event revokers
        private IDisposable _scrollViewerViewChangedRevoker;
        private IDisposable _itemsPresenterSizeChangedRevoker;
        private IDisposable _listViewLoadedRevoker;
        private IDisposable _listViewCanReorderItemsPropertyChangedRevoker;
        private IDisposable _listViewAllowDropPropertyChangedRevoker;
        private string _tabCloseButtonTooltipText;
        private Size _previousAvailableSize;

        private bool _isDragging = false;

        private Size _lastItemsPresenterSize;


        private static double c_tabMinimumWidth = 48d;
        private static double c_tabMaximumWidth = 200d;

        private static string c_tabViewItemMinWidthName = "TabViewItemMinWidth";
        private static string c_tabViewItemMaxWidthName = "TabViewItemMaxWidth";

        // (WinUI) TODO: what is the right number and should this be customizable?
        private static double c_scrollAmount = 50d;

        class TabViewCommand : ICommand
        {
            public TabViewCommand(Action<object> execute)
            {
                ExecuteHandler = execute;
            }

            public event EventHandler CanExecuteChanged;

            public Action<object> ExecuteHandler { get; }
            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                ExecuteHandler.Invoke(parameter);
            }
        }

        enum TabViewCommandType
        {
            CtrlF4,
            CtrlTab,
            CtrlShftTab
        }
    }
}
