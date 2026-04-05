using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Collections;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// A control used to display a set of tabs and their respective content
/// </summary>
public partial class TabView : TemplatedControl
{
    public TabView()
    {
        TabItems = new AvaloniaList<object>();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Get the current platform's Command Modifier instead of just assuming Control
        var ctrl = Application.Current.PlatformSettings.HotkeyConfiguration.CommandModifiers;

        // Keyboard Accelerators (KeyBindings in Avalonia)
        // Require a Command, so we wire this up *slightly* differently
        // compared to WinUI

        _keyboardAcceleratorHandler = new TabViewCommand(OnKeyboardAcceleratorInvoked);

        // TODO v3: Be sure to grab these key strokes from Platform settings to ensure
        // we're using the correct key (Control vs Meta)
        var closeTabGesture = new KeyGesture(Key.F4, ctrl);
        KeyBindings.Add(new KeyBinding
        {
            Gesture = closeTabGesture,
            Command = _keyboardAcceleratorHandler,
            CommandParameter = TabViewCommandType.CtrlF4
        });
        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.Tab, ctrl),
            Command = _keyboardAcceleratorHandler,
            CommandParameter = TabViewCommandType.CtrlTab
        });
        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.Tab, ctrl | KeyModifiers.Shift),
            Command = _keyboardAcceleratorHandler,
            CommandParameter = TabViewCommandType.CtrlShftTab
        });

        _tabCloseButtonTooltipText = FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TabViewCloseButtonTooltipWithKA);
        PseudoClasses.Set(s_pcTop, true);
        DragDrop.SetAllowDrop(this, true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UnhookEventsAndClearFields();
        _isItemBeingDragged = false;
        _isItemDraggedOver = false;
        _expandedWidthForDragOver = null;

        base.OnApplyTemplate(e);

        _tabContentPresenter = e.NameScope.Find<ContentPresenter>(s_tpTabContentPresenter);
        _rightContentPresenter = e.NameScope.Find<ContentPresenter>(s_tpRightContentPresenter);

        _tabContainerGrid = e.NameScope.Get<Grid>(s_tpTabContainerGrid);
        if (_tabContainerGrid.ColumnDefinitions.Count > 0)
        {
            _leftContentColumn = _tabContainerGrid.ColumnDefinitions[0];
            _tabColumn = _tabContainerGrid.ColumnDefinitions[1];
            _addButtonColumn = _tabContainerGrid.ColumnDefinitions[2];
            _rightContentColumn = _tabContainerGrid.ColumnDefinitions[3];
        }
        else
        {
            _tabContainerGrid.SizeChanged += HandleTabContainerGridSizeChangedForVerticalTabView;
        }
        
        _tabContainerGrid.PointerEntered += OnTabStripPointerEnter;
        _tabContainerGrid.PointerExited += OnTabStripPointerLeave;

        _listView = e.NameScope.Get<TabViewListView>(s_tpTabListView);
        if (_listView != null)
        {
            LogicalChildren.Add(_listView);
            _listView.Loaded += OnListViewLoaded;
            _listView.SelectionChanged += OnListViewSelectionChanged;
            _listView.SizeChanged += OnListViewSizeChanged;

            _listView.DragItemsStarting += OnListViewDragItemsStarting;
            _listView.DragItemsCompleted += OnListViewDragItemsCompleted;

            _listView.DragOver += OnListViewDragOver;
            _listView.Drop += OnListViewDrop;
            _listView.DragEnter += OnListViewDragEnter;
            _listView.DragLeave += OnListViewDragLeave;

            _listView.GettingFocus += OnListViewGettingFocus;

            _listViewCanReorderItemsPropertyChangedRevoker =
                _listView.GetPropertyChangedObservable(TabViewListView.CanReorderItemsProperty)
                .Subscribe(_ => OnListViewDraggingPropertyChanged());
            _listViewAllowDropPropertyChangedRevoker =
                _listView.GetPropertyChangedObservable(DragDrop.AllowDropProperty)
                .Subscribe(_ => OnListViewDraggingPropertyChanged());
        }

        _addButton = e.NameScope.Find<Button>(s_tpAddButton);
        if (_addButton != null)
        {
            // TODO: Automation

            if (ToolTip.GetTip(_addButton) == null)
            {
                ToolTip.SetTip(_addButton, FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TabViewAddButtonTooltip));
            }

            _addButton.Click += OnAddButtonClick;
            _addButton.KeyDown += OnAddButtonKeyDown;
        }

        var handle = e.NameScope.Get<Border>(s_tpPaneResizeHandle);
        if (handle != null) // Null in Top/Bottom modes
        {
            handle.PointerPressed += OnPaneResizeHandlePointerPressed;
            handle.PointerMoved += OnPaneResizeHandlePointerMoved;
            handle.PointerReleased += OnPaneResizeHandlePointerReleased;
            handle.PointerCaptureLost += OnPaneResizeHandlePointerCaptureLost;
            _verticalPaneResizeHandle = handle;
        }

        //UpdateListViewItemContainerTransitions();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_previousAvailableSize.Width != availableSize.Width)
        {
            _previousAvailableSize = availableSize;
            UpdateTabWidths();
        }

        return base.MeasureOverride(availableSize);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new TabViewAutomationPeer(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
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
        else if (change.Property == TabItemsSourceProperty)
        {
            OnTabItemsSourcePropertyChanged(change);
        }
        else if (change.Property == TabWidthModeProperty)
        {
            OnTabWidthModePropertyChanged(change);
        }
        else if (change.Property == TabStripLocationProperty)
        {
            OnTabStripLocationPropertyChanged(change);
        }
    }

    internal void SetTabSeparatorOpacity(int index, int opacityValue)
    {
        if (ContainerFromIndex(index) is TabViewItem tvi)
        {
            // The reason we set the opacity directly instead of using VisualState
            // is because we want to hide the separator on hover/pressed
            // but the tab adjacent on the left to the selected tab
            // must hide the tab separator at all times.
            // It causes two visual states to modify the same property
            // what leads to undesired behaviour.
            tvi.TabSeparator?.Opacity = opacityValue;
        }
    }

    internal void SetTabSeparatorOpacity(int index)
    {
        var selIndex = SelectedIndex;

        // If Tab is adjacent on the left to selected one or
        // it is selected tab - we hide the tabSeparator.
        if (index == selIndex || index + 1 == selIndex)
        {
            SetTabSeparatorOpacity(index, 0);
        }
        else
        {
            SetTabSeparatorOpacity(index, 1);
        }
    }

    protected virtual void OnTabStripLocationPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var (oldValue, newValue) = args.GetOldAndNewValue<TabViewTabStripLocation>();

        // TODO v3: Is this needed or left over from my testing?
        //if ((IsHorizontal(oldValue) && !IsHorizontal(newValue)) ||
        //    (!IsHorizontal(oldValue) && IsHorizontal(newValue)) &&
        //    _listView != null && _listView.ItemsSource == null)
        //{
        //    // We're switching from vertical to horizontal or horizontal to vertical
        //    // If we're not using the TabItemsSource, we need to make a copy of the
        //    // TabItems to unhook them from the ItemsControl
        //    var l = new List<object>();
        //    foreach (var item in TabItems)
        //        l.Add(item);

        //    _listView.Items.Clear();
        //    TabItems = l;
        //}


        _isSwitchingTabLocation = true;

        if ((IsHorizontal(oldValue) && !IsHorizontal(newValue)) ||
            (!IsHorizontal(oldValue) && IsHorizontal(newValue)))
        {
            // Only set TabContent to null if we're truly switching orientations
            UpdateTabContent();
        }
        

        var oldClass = GetClassForStripLocation(args.GetOldValue<TabViewTabStripLocation>());
        var newClass = GetClassForStripLocation(args.GetNewValue<TabViewTabStripLocation>());
        PseudoClasses.Remove(oldClass);
        PseudoClasses.Add(newClass);

        _listView?.HandleTabStripLocationChanged(args.GetNewValue<TabViewTabStripLocation>(), oldClass, newClass);
        
        UpdateTabWidths();

        static bool IsHorizontal(TabViewTabStripLocation loc) =>
            loc == TabViewTabStripLocation.Top || loc == TabViewTabStripLocation.Bottom;
    }

    private void OnListViewDraggingPropertyChanged()
    {
        //UpdateListViewItemContainerTransitions();
    }

    private void OnListViewGettingFocus(object sender, FocusChangingEventArgs e)
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

        // TODO: v3
    }

    private void OnSelectedIndexPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        // We update previous selected and adjacent on the left tab
        // as well as current selected and adjacent on the left tab
        // to show/hide tabSeparator accordingly.
        UpdateSelectedIndex();
        SetTabSeparatorOpacity(args.GetOldValue<int>());
        SetTabSeparatorOpacity(args.GetOldValue<int>() - 1);
        SetTabSeparatorOpacity(args.GetNewValue<int>() - 1);
        SetTabSeparatorOpacity(args.GetNewValue<int>());

        UpdateBottomBorderLineVisualStates();
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
                ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcNoBorder, state == 0);
                ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderLeft, state == 1);
                ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderRight, state == 2);
            }
        }
    }

    private void UpdateBottomBorderLineVisualStates()
    {
        // Update border line on all tabs
        UpdateTabBottomBorderLineVisualStates();

        PseudoClasses.Set(s_pcSingleBorder, _isDragging);

        // Update border lines in the inner TabViewListView
        if (_listView != null)
        {
            (_listView.Classes as IPseudoClasses).Set(SharedPseudoclasses.s_pcNoBorder, _isDragging);
        }

        // Update border lines in the ScrollViewer
        if (_scrollViewer != null)
        {
            (_scrollViewer.Classes as IPseudoClasses).Set(SharedPseudoclasses.s_pcNoBorder, _isDragging);
        }
    }

    private void OnSelectedItemPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdateSelectedItem();
    }

    private void OnTabItemsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdateListViewItemContainerTransitions();
    }

    private void UpdateListViewItemContainerTransitions() { }

    private void OnCanTearOutTabsPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateTabViewWithTearOutList();
        AttachMoveSizeLoopEvents();
        UpdateNonClientRegion();
    }

    private void OnTabWidthModePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdateTabWidths();

        var newValue = change.GetNewValue<TabViewWidthMode>();
        // Switch the visual states of all tab items to the correct TabViewWidthMode
        int itemCount = TabItems.Count;
        for (int i = 0; i < itemCount; i++)
        {
            if (ContainerFromIndex(i) is TabViewItem tvi)
            {
                tvi.OnTabViewWidthModeChanged(newValue);
            }
        }
    }

    private void OnCloseButtonOverlayModePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<TabViewCloseButtonOverlayMode>();
        // Switch the visual states of all tab items to the correct TabViewWidthMode
        int itemCount = TabItems.Count;
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

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        UpdateTabContent();
        UpdateTabViewWithTearOutList();
        AttachMoveSizeLoopEvents();
        UpdateNonClientRegion();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        UpdateTabViewWithTearOutList();
    }

    private void OnListViewLoaded(object sender, RoutedEventArgs args)
    {
        var lv = _listView;

        // Now that ListView exists, we can start using its Items collection.
        var lvItems = lv.Items;
        // 2nd condition added, if TabItems is already the ListView's ItemCollection, we just swapped in the same
        // orientation (top - bottom / left - right), so the ListView was reloaded, but its still the same one
        if (lvItems != null && lvItems != TabItems)
        {
            if (lv.ItemsSource == null)
            {
                if (_isSwitchingTabLocation)
                {
                    // Unhook the TabItems from the old ItemCollection
                    var tabItems = TabItems;

                    foreach (var item in tabItems)
                    {
                        if (item is TabViewItem tvi && tvi.GetVisualParent() is Panel p)
                        {
                            p.Children.Remove(tvi);
                        }

                        lvItems.Add(item);
                    }

                    TabItems.Clear();
                }
                else
                {
                    // copy the list, because clearing lvItems may also clear TabItems
                    using var l = new PooledList<object>(lvItems.Count);

                    foreach (var item in TabItems)
                        l.Add(item);

                    lvItems.Clear();

                    foreach (var item in l.AsSpan())
                        lvItems.Add(item);
                }                
            }

            TabItems = lvItems;
        }


        // Ensure the ListView is configured correctly when it loads
        var stripLocation = TabStripLocation;
        lv.HandleTabStripLocationChanged(stripLocation, null, GetClassForStripLocation(stripLocation));

        if (SelectedItem != null)
        {
            UpdateSelectedItem();
        }
        else
        {
            // If SelectedItem wasn't set, default to selecting the first tab
            UpdateSelectedIndex();
        }

        SelectedIndex = lv.SelectedIndex;
        SelectedItem = lv.SelectedItem;

        if (_isSwitchingTabLocation)
        {
            _isSwitchingTabLocation = false;
            UpdateTabContent();
        }

        if (_itemsPresenter != null)
        {
            _itemsPresenter = _listView.Presenter;
            _itemsPresenter.SizeChanged += OnItemsPresenterSizeChanged;
        }
        
        var scrollViewer = _listView.Scroller;
        _scrollViewer = scrollViewer;
        if (scrollViewer != null)
        {
            if (scrollViewer.IsLoaded)
            {
                OnScrollViewerLoaded(null, null);
            }
            else
            {
                scrollViewer.Loaded += OnScrollViewerLoaded;
            }
        }        

        UpdateBottomBorderLineVisualStates();
        UpdateNonClientRegion();
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

    private void OnScrollViewerLoaded(object sender, RoutedEventArgs args)
    {
        var buttons = _scrollViewer.GetTemplateChildren()
            .Where(x => x is RepeatButton);

        foreach (RepeatButton button in buttons)
        {
            if (button.Name == s_tpScrollDecreaseButton)
            {
                _scrollDecreaseButton = button;
                ToolTip.SetTip(_scrollDecreaseButton,
                    FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TabViewScrollDecreaseButtonTooltip));
                _scrollDecreaseButton.Click += OnScrollDecreaseClick;
            }
            else if (button.Name == s_tpScrollIncreaseButton)
            {
                _scrollIncreaseButton = button;
                ToolTip.SetTip(_scrollIncreaseButton,
                    FALocalizationHelper.Instance.GetLocalizedStringResource(SR_TabViewScrollIncreaseButtonTooltip));
                _scrollIncreaseButton.Click += OnScrollIncreaseClick;
            }
        }

        // TODO: We now have ScrollChanged event, can probably switch to that
        _scrollViewerViewChangedRevoker = new FACompositeDisposable(
            _scrollViewer.GetPropertyChangedObservable(ScrollViewer.OffsetProperty).Subscribe(OnScrollViewerViewChanged),
            _scrollViewer.GetPropertyChangedObservable(ScrollViewer.ExtentProperty).Subscribe(OnScrollViewerViewChanged),
            _scrollViewer.GetPropertyChangedObservable(ScrollViewer.ViewportProperty).Subscribe(OnScrollViewerViewChanged)
            );

        UpdateTabWidths();
    }

    private void OnScrollViewerViewChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();

        // Another case where we have to do something WinUI doesn't. Scrolling (recycling) tabs
        // doesn't ensure their widths are set correctly and so some will autosize to their
        // content and be just slightly bigger. Ensure that doesn't happen
        UpdateTabWidths();
    }

    private void UpdateScrollViewerDecreaseAndIncreaseButtonsViewState()
    {
        if (_scrollViewer != null && _scrollDecreaseButton != null && _scrollIncreaseButton != null)
        {
            const double minThreshold = 0.1d;
            var hOffset = _scrollViewer.Offset.X;
            var scrollableWidth = (_scrollViewer.Extent.Width - _scrollViewer.Viewport.Width);

            if (double.Abs(hOffset - scrollableWidth) < minThreshold)
            {
                _scrollDecreaseButton?.IsEnabled = true;
                _scrollIncreaseButton?.IsEnabled = false;
            }
            else if (double.Abs(hOffset) < minThreshold)
            {
                _scrollDecreaseButton.IsEnabled = false;
                _scrollIncreaseButton.IsEnabled = true;
            }
            else
            {
                _scrollDecreaseButton.IsEnabled = true;
                _scrollIncreaseButton.IsEnabled = true;
            }
        }
    }

    private void OnItemsPresenterSizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (!_updateTabWidthOnPointerLeave)
        {
            // Presenter size didn't change because of item being removed, so update manually
            UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
            UpdateTabWidths();
            // Make sure that the selected tab is fully in view and not cut off
            BringSelectedTabIntoView();
        }
    }

    private void HandleTabContainerGridSizeChangedForVerticalTabView(object sender, SizeChangedEventArgs e)
    {
        UpdateTabWidths();
    }

    private void BringSelectedTabIntoView()
    {
        if (SelectedItem != null)
        {
            var tvi = SelectedItem as TabViewItem ?? ContainerFromItem(SelectedItem) as TabViewItem;            
            tvi?.StartBringTabIntoView();
        }
    }

    internal void OnItemsChanged(object item)
    {
        if (item is NotifyCollectionChangedEventArgs args)
        {
            TabItemsChanged?.Invoke(this, args);

            int numItems = TabItems.Count;
            var listViewInnerSelectedIndex = _listView.SelectedIndex;
            var selectedIndex = SelectedIndex;

            if (selectedIndex != listViewInnerSelectedIndex && listViewInnerSelectedIndex != -1)
            {
                SelectedIndex = listViewInnerSelectedIndex;
                selectedIndex = listViewInnerSelectedIndex;
            }

            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                _updateTabWidthOnPointerLeave = true;
                if (numItems > 0)
                {
                    // SelectedIndex might also already be -1
                    if (selectedIndex == -1 || selectedIndex == args.OldStartingIndex)
                    {
                        // Find the closest tab to select instead
                        int startIndex = args.OldStartingIndex;
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
                    if (!_pointerInTabstrip || args.OldStartingIndex == TabItems.Count)
                    {
                        UpdateTabWidths(true, false);
                    }
                }
            }
            else
            {
                // GH#424, Adding a tab item wouldn't set the size correctly until pointer exit,
                // as when this is called following a collection change, the items haven't been
                // materialized yet in the panel so UpdateTabWidths using the old previous item
                // Posting to Dispatcher so delay calling this until after next layout pass
                // when items are all realized and ContainerFromIndex works
                // TODO: Do we still need to post to dispatcher
                
                Dispatcher.UIThread.Post(() =>
                {
                    UpdateTabWidths();
                    SetTabSeparatorOpacity(numItems - 1);
                });
            }
        }

        UpdateBottomBorderLineVisualStates();
    }

    private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        // If we're currently switching TabLocation, ignore this selected item change
        // because it just got set to -1. We'll set it back to the correct index
        // when the ListView loaded handler is called
        if (_isSwitchingTabLocation)
            return;

        SelectedIndex = _listView.SelectedIndex;
        SelectedItem = _listView.SelectedItem;

        UpdateTabContent();

        SelectionChanged?.Invoke(this, args);
    }

    private void OnListViewSizeChanged(object sender, SizeChangedEventArgs args)
    {
        UpdateNonClientRegion();
    }

    private TabViewItem FindTabViewItemFromDragItem(object item)
    {
        var tab = ContainerFromItem(item) as TabViewItem;
        tab ??= tab.FindAncestorOfType<TabViewItem>();
        
        if (tab == null)
        {
            // This is a fallback scenario for tabs without a data context
            var numItems = TabItems.Count;
            for (int i = 0; i < numItems; i++)
            {
                var tabItem = ContainerFromIndex(i) as TabViewItem;
                if (tabItem.Content == item)
                {
                    tab = tabItem;
                    break;
                }
            }
        }

        return tab;
    }

    private void OnListViewDragItemsStarting(object sender, DragItemsStartingEventArgs args)
    {
        _isItemBeingDragged = true;

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
        if (!args.Handled)
        {
            TabStripDrop?.Invoke(this, args);
        }

        UpdateIsItemDraggedOver(false);
    }

    private void OnListViewDragEnter(object sender, DragEventArgs args)
    {
        foreach (var item in TabItems)
        {
            if (ContainerFromItem(item) is TabViewItem tvi)
            {
                if (tvi.IsBeingDragged)
                    return;
            }
        }

        UpdateIsItemDraggedOver(true);
    }

    private void OnListViewDragLeave(object sender, DragEventArgs args)
    {
        UpdateIsItemDraggedOver(false);
    }

    private void OnListViewDragItemsCompleted(object sender, DragItemsCompletedEventArgs args)
    {
        _isItemBeingDragged = false;

        // Selection may have changed during drag if dragged outside, so we update SelectedIndex again.
        if (_listView != null)
        {
            SelectedIndex = _listView.SelectedIndex;
            SelectedItem = _listView.SelectedItem;

            BringSelectedTabIntoView();
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

        if (SelectedItem == null || _isSwitchingTabLocation)
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
                // TODO: v3: Switch to LosingFocus
                _tabContentPresenter.LostFocus += (s, e) =>
                {
                    shouldMoveFocusToNewTab = true;
                };

                _tabContentPresenter.Content = tvi.Content;
                _tabContentPresenter.ContentTemplate = tvi.ContentTemplate;

                // It is not ideal to call UpdateLayout here, but it is necessary to ensure that the ContentPresenter has expanded its content
                // into the live visual tree.
                

                if (shouldMoveFocusToNewTab)
                {
                    // TODO: v3
                    //var focusable = KeyboardNavigationHandler.GetNext(_tabContentPresenter, NavigationDirection.Next);
                    //if (focusable == null)
                    //{
                    //    // If there is nothing focusable in the new tab, just move focus to the TabViewItem itself.
                    //    focusable = tvi;
                    //}

                    //if (focusable != null)
                    //{
                    //    focusable.Focus(NavigationMethod.Unspecified);
                    //}
                }
            }
        }
    }

    internal void RequestCloseTab(TabViewItem container, bool updateTabWidths)
    {
        // If the tab being closed is the currently focused tab, we'll move focus to the next tab
        // when the tab closes.
        bool tabIsFocused = false;
        var focusedObject = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement();
        var focusedElement = focusedObject as Visual;

        while (focusedElement != null)
        {
            if (focusedElement == container)
            {
                tabIsFocused = true;
                break;
            }

            focusedElement = focusedElement.GetVisualParent();
        }

        if (tabIsFocused)
        {
            // TODO: v3
            container.LostFocus += (s, args) =>
            {
                // TODO
            };
        }

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
        // Don't update any tab widths when we're in the middle of a tab tear-out loop -
        // we'll update tab widths when it's done.
        if (_isInTabTearOutLoop)
        {
            return;
        }

        var maxTabWidth = this.TryFindResource(c_tabViewItemMaxWidthName, out var mtw) ? (double)mtw : c_tabMaximumWidth;
        double tabWidth = double.NaN;
        int tabCount = TabItems.Count;

        // If an item is being dragged over this TabView, then we'll want to act like there's an extra item
        // when updating tab widths, which will create a hole into which the item can be dragged.
        if (_isItemDraggedOver)
        {
            tabCount++;
        }
        var stripLocation = TabStripLocation;
        var isHorizontal = (stripLocation == TabViewTabStripLocation.Top || stripLocation == TabViewTabStripLocation.Bottom);
        if (_tabContainerGrid != null && isHorizontal)
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
                        var padding = Padding;

                        // We don't have this, so skip what WinUI does, but to avoid messing up the math
                        // just keep these variables around
                        double headerWidth = 0, footerWidth = 0;

                        if (fillAllAvailableSpace)
                        {
                            // Calculate the proportional width of each tab given the width of the ScrollViewer.
                            var tabWidthForScroller = (availableWidth - (padding.Horizontal() + headerWidth + footerWidth)) / (double)tabCount;
                            tabWidth = double.Clamp(tabWidthForScroller, minTabWidth, maxTabWidth);
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
                            var tabWidthUnclamped = availableTabViewSpace / (double)tabCount;
                            tabWidth = double.Clamp(tabWidthUnclamped, minTabWidth, maxTabWidth);
                        }

                        _tabColumn.MaxWidth = availableWidth + headerWidth + footerWidth;
                        var requiredWidth = tabWidth * tabCount + headerWidth + footerWidth + padding.Horizontal();
                        if (requiredWidth > availableWidth)
                        {
                            _tabColumn.Width = new GridLength(availableWidth, GridUnitType.Pixel);
                            if (_listView != null)
                            {
                                _listView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Visible);
                                UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                            }
                        }
                        else
                        {
                            // If we're dragging over the TabView, we need to set the width to a specific value,
                            // since we want it to be larger than the items actually in it in order to accommodate
                            // the item being dragged into the TabView.  Otherwise, we can just set its width to Auto.
                            _tabColumn.Width = _isItemDraggedOver ?
                                new GridLength(requiredWidth, GridUnitType.Pixel) :
                                new GridLength(1, GridUnitType.Auto);

                            if (_listView != null)
                            {
                                if (shouldUpdateWidths && fillAllAvailableSpace)
                                {
                                    _listView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
                                }
                                else
                                {
                                    _scrollDecreaseButton?.IsEnabled = false;
                                    _scrollIncreaseButton?.IsEnabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Case: TabWidthMode "Compact" or "SizeToContent"
                        _tabColumn.MaxWidth = availableWidth;

                        if (_listView != null)
                        {
                            // When an item is being dragged over, we need to reserve extra space for the potential new tab,
                            // so we can't rely on auto sizing in that case.  However, the ListView expands to the size of the column,
                            // so we need to store the value lest we keep expanding the width of the column every time we call this method.
                            if (_isItemDraggedOver)
                            {
                                if (!_expandedWidthForDragOver.HasValue)
                                {
                                    _expandedWidthForDragOver = _listView.Bounds.Width + maxTabWidth;
                                }

                                _tabColumn.Width = new GridLength(_expandedWidthForDragOver.Value, GridUnitType.Pixel);
                            }
                            else
                            {
                                if (_expandedWidthForDragOver.HasValue)
                                {
                                    _expandedWidthForDragOver = null;
                                }

                                _tabColumn.Width = new GridLength(1, GridUnitType.Auto);
                            }

                            _listView.MaxWidth = availableWidth;

                            var ip = _itemsPresenter;
                            if (ip != null)
                            {
                                var visible = ip.Bounds.Width > availableWidth;
                                _listView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, visible ?
                                    ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden);

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

        if (!isHorizontal)
        {
            if (_listView != null)
            {
                // If not in Horizontal, ensure we let the scrollviewer work correctly
                _listView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                _listView.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            }
            
            if (_tabContainerGrid != null)
            {
                var rows = _tabContainerGrid.RowDefinitions;
                // Calcuate the height of the rows without the TabView
                double height = 0;
                foreach (var item in _tabContainerGrid.Children)
                {
                    if (item is TabViewListView)
                        continue;

                    height += item.DesiredSize.Height;
                }    
                var maxSpace = _tabContainerGrid.Bounds.Height;
                
                if (_isItemDraggedOver)
                {
                    // Add the dragging space in vertical view by using the avg. item height
                    height += (height / _tabContainerGrid.Children.Count);
                }

                _scrollViewer?.MaxHeight = double.Clamp(maxSpace - height, 0, double.PositiveInfinity);
            }
        }
        else if (_scrollViewer != null)
        {
            _scrollViewer.MaxHeight = double.PositiveInfinity;
        }

        if (shouldUpdateWidths || TabWidthMode != TabViewWidthMode.Equal)
        {
            foreach (var item in TabItems)
            {
                var tvi = item as TabViewItem ?? ContainerFromItem(item) as TabViewItem;
                tvi?.Width = tabWidth;
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

    public Control ContainerFromItem(object item) =>
       _listView?.ContainerFromItem(item);

    public Control ContainerFromIndex(int index) =>
        _listView?.ContainerFromIndex(index);

    public int IndexFromContainer(Control container) =>
        _listView?.IndexFromContainer(container) ?? -1;

    public object ItemFromContainer(Control container) =>
        _listView?.ItemFromContainer(container);

    private void OnPaneResizeHandlePointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
            return;

        var pt = e.GetCurrentPoint(null);
        if (e.Properties.IsLeftButtonPressed)
        {
            _initDragPanePoint = pt.Position;
            _startingPaneSize = VerticalOpenPaneLength;
        }
    }

    private void OnPaneResizeHandlePointerMoved(object sender, PointerEventArgs e)
    {
        if (e.Handled)
            return;

        if (_initDragPanePoint.HasValue)
        {
            var point = e.GetCurrentPoint(null);
            var delta = (point.Position - _initDragPanePoint.Value).X;
            if (!_isDraggingPane)
            {
                FAUISettings.GetSystemDragSize(TopLevel.GetTopLevel(this).RenderScaling, out var cxDrag, out _);
                
                if (double.Abs(delta) < cxDrag)
                {
                    return;
                }

                _isDraggingPane = true;
            }

            var min = MinimumVerticalOpenPaneLength;
            var max = MaximumVerticalOpenPaneLength;

            if (TabStripLocation == TabViewTabStripLocation.Right)
                delta *= -1;

            var paneLength = _startingPaneSize;
            var length = double.Clamp(paneLength + delta, min, max);

            SetCurrentValue(VerticalOpenPaneLengthProperty, length);
        }
    }

    private void OnPaneResizeHandlePointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (e.Handled)
            return;

        if (_initDragPanePoint.HasValue)
        {
            var point = e.GetCurrentPoint(null);
            if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                _initDragPanePoint = null;
                _isDraggingPane = false;
            }
        }
    }

    private void OnPaneResizeHandlePointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        if (e.Handled)
            return;

        _initDragPanePoint = null;
        _isDraggingPane = false;
    }

    private int GetItemCount()
    {
        var src = TabItemsSource;
        if (src != null)
        {
            return src.Count();
        }
        else
        {
            return TabItems.Count;
        }
    }

    internal bool MoveFocus(bool moveForward)
    {
        // TODO: v3
        return false;
    }

    private bool MoveSelection(bool moveForward)
    {
        // TODO: v3
        return false;
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

    protected virtual void OnKeyboardAcceleratorInvoked(object parameter)
    {
        switch ((TabViewCommandType)parameter)
        {
            case TabViewCommandType.CtrlF4:
                RequestCloseCurrentTab();
                break;

            case TabViewCommandType.CtrlTab:
                MoveSelection(true);
                break;

            case TabViewCommandType.CtrlShftTab:
                MoveSelection(false);
                break;
        }
    }

    private void OnAddButtonKeyDown(object sender, KeyEventArgs args)
    {
        var ab = _addButton;
        if (args.Key == Key.Right)
        {
            args.Handled = MoveFocus(ab.FlowDirection == Avalonia.Media.FlowDirection.LeftToRight);
        }
        else if (args.Key == Key.Left)
        {
            args.Handled = MoveFocus(ab.FlowDirection == Avalonia.Media.FlowDirection.RightToLeft);
        }
    }

    // Note that the parameter is a DependencyObject for convenience to allow us to call this on the return value of ContainerFromIndex.
    // There are some non-control elements that can take focus - e.g. a hyperlink in a RichTextBlock - but those aren't relevant for our purposes here.
    private bool IsFocusable(InputElement obj, bool checkTabStop)
    {
        if (obj == null)
            return false;

        if (obj is Control c)
        {
            return c.IsEffectivelyVisible &&
                (c.IsEffectivelyEnabled) && // AllowFocusWhenDisabled (TODO v3)
                (c.IsTabStop || !checkTabStop);
        }

        return false;
    }

    private void UpdateIsItemDraggedOver(bool isItemDraggedOver)
    {
        if (_isItemDraggedOver != isItemDraggedOver)
        {
            _isItemDraggedOver = isItemDraggedOver;
            UpdateTabWidths();
        }
    }

    // ----------- TABVIEW TEAROUT - The following is left while I investigate adding this

    private void UpdateTabViewWithTearOutList()
    {
        //var list = GetTabViewWithTearOutList();
    }

    private void AttachMoveSizeLoopEvents() { }

    private void OnEnteringMoveSize() { }

    private void OnEnteredMoveSize() { }

    private void OnWindowRectChanging() { }

    private void DragTabWithinTabView() { }

    private void UpdateTabIndex() { }

    private void TearOutTab() { }

    private void DragTornOutTab() { }

    private int GetTabInsertionIndex() => -1;

    private void OnExitedMoveSize() { }

    private TabViewItem GetTabAtPoint(Point point) => null;

    private void PopulateTabViewList() { }

    // MutexLockedResource

    // GetInputNonClientPointerSource

    // GetAppWindowCoordinateConverter

    private void UpdateNonClientRegion() { }

    private nint GetAppWindowId() => 0;

    // ---------------- END TABVIEW TEAROUT

    private void UnhookEventsAndClearFields()
    {
        if (_tabContainerGrid != null)
        {
            _tabContainerGrid.PointerEntered -= OnTabStripPointerEnter;
            _tabContainerGrid.PointerExited -= OnTabStripPointerLeave;
        }

        if (_listView != null)
        {
            _listView.Loaded -= OnListViewLoaded;
            LogicalChildren.Remove(_listView);
            _listView.SelectionChanged -= OnListViewSelectionChanged;
            _listView.GettingFocus -= OnListViewGettingFocus;

            _listView.DragItemsStarting -= OnListViewDragItemsStarting;
            _listView.DragItemsCompleted -= OnListViewDragItemsCompleted;
            _listView.DragOver -= OnListViewDragOver;
            _listView.Drop -= OnListViewDrop;
            _listView.DragEnter -= OnListViewDragEnter;
            _listView.DragLeave -= OnListViewDragLeave;

            _listViewAllowDropPropertyChangedRevoker?.Dispose();
            _listViewCanReorderItemsPropertyChangedRevoker?.Dispose();
        }

        _addButton?.Click -= OnAddButtonClick;
        _addButton?.KeyDown -= OnAddButtonKeyDown;

        _itemsPresenter?.SizeChanged -= OnItemsPresenterSizeChanged;

        _scrollDecreaseButton?.Click -= OnScrollDecreaseClick;

        _scrollIncreaseButton?.Click -= OnScrollIncreaseClick;

        _scrollViewerViewChangedRevoker?.Dispose();

        if (_verticalPaneResizeHandle != null) // Null in Top/Bottom modes
        {
            _verticalPaneResizeHandle.PointerPressed -= OnPaneResizeHandlePointerPressed;
            _verticalPaneResizeHandle.PointerMoved -= OnPaneResizeHandlePointerMoved;
            _verticalPaneResizeHandle.PointerReleased -= OnPaneResizeHandlePointerReleased;
            _verticalPaneResizeHandle.PointerCaptureLost -= OnPaneResizeHandlePointerCaptureLost;
        }

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

    internal static string GetClassForStripLocation(TabViewTabStripLocation loc)
    {
        return loc switch
        {
            TabViewTabStripLocation.Left => s_pcLeft,
            TabViewTabStripLocation.Bottom => s_pcBottom,
            TabViewTabStripLocation.Right => s_pcRight,
            _ => s_pcTop
        };
    }

    internal string GetTabCloseButtonTooltipText() =>
       _tabCloseButtonTooltipText;


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
    private Border _verticalPaneResizeHandle;
    private SplitView _splitView;

    private bool _isDraggingPane;
    private Point? _initDragPanePoint;
    private double _startingPaneSize;

    private bool _isSwitchingTabLocation;
    private int _selectedIndexBeforeTabSwitch = -1;

    // A bunch of event revokers
    private IDisposable _scrollViewerViewChangedRevoker;
    private IDisposable _itemsPresenterSizeChangedRevoker;
    private IDisposable _listViewCanReorderItemsPropertyChangedRevoker;
    private IDisposable _listViewAllowDropPropertyChangedRevoker;
    private string _tabCloseButtonTooltipText;
    private Size _previousAvailableSize;

    private bool _isDragging = false;
    private bool _isItemBeingDragged;
    private bool _isItemDraggedOver;
    private double? _expandedWidthForDragOver;
    private bool _isInTabTearOutLoop;

    private Size _lastItemsPresenterSize;

    private static double c_tabMinimumWidth = 48d;
    private static double c_tabMaximumWidth = 200d;

    // (WinUI) TODO: what is the right number and should this be customizable?
    private static double c_scrollAmount = 50d;


    class TabViewCommand : ICommand
    {
        public TabViewCommand(Action<object> execute)
        {
            ExecuteHandler = execute;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

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
