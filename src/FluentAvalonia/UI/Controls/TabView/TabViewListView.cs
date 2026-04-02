using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Collections;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Data;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents the ListView used in the TabStrip of a <see cref="TabView"/>
/// </summary>
/// <remarks>
/// This control should not be used outside of a TabView
/// </remarks>
[PseudoClasses(s_pcReorder)]
[TemplatePart(s_tpScrollViewer, typeof(ScrollViewer))]
public sealed class TabViewListView : ListBox
{
    public TabViewListView()
    {
        ItemsView.CollectionChanged += OnItemsChanged;


        Tapped += (s, e) =>
        {
            if (e.Source is Visual v && v.FindAncestorOfType<TabViewItem>(true) is TabViewItem tvi)
            {
                var index = IndexFromContainer(tvi);
                UpdateSelection(index, true);
                e.Handled = true;
            }
        };

        // Because of event differences in WinUI vs Avalonia, we need more control over the drag events
        // so we are translating them into CLR events for the TabView to subscribe to
        // See OnApplyTemplate for more info
        AddHandler(DragDrop.DragOverEvent, OnListViewDragOver);
        AddHandler(DragDrop.DropEvent, OnListViewDrop);
    }

    /// <summary>
    /// Defines the <see cref="CanReorderItems"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanReorderItemsProperty =
        AvaloniaProperty.Register<TabViewListView, bool>(nameof(CanReorderItems));

    /// <summary>
    /// Defines the <see cref="CanDragItems"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanDragItemsProperty =
        AvaloniaProperty.Register<TabViewListView, bool>(nameof(CanDragItems));

    /// <summary>
    /// Gets or sets whether this ListView can reorder items
    /// </summary>
    public bool CanReorderItems
    {
        get => GetValue(CanReorderItemsProperty);
        set => SetValue(CanReorderItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether dragging items is supported on this ListView
    /// </summary>
    public bool CanDragItems
    {
        get => GetValue(CanDragItemsProperty);
        set => SetValue(CanDragItemsProperty, value);
    }

    internal ScrollViewer Scroller { get; private set; }

    internal event EventHandler<DragEventArgs> DragEnter;
    internal event EventHandler<DragEventArgs> DragOver;
    internal event EventHandler<DragEventArgs> DragLeave;
    internal event EventHandler<DragEventArgs> Drop;

    /// <summary>
    /// Occurs when a drag operation that involves one of the items in the view is initiated.
    /// </summary>
    public event DragItemsStartingEventHandler DragItemsStarting;

    /// <summary>
    /// Occurs when a drag operation that involves one of the items in the view is ended.
    /// </summary>
    public event TypedEventHandler<TabViewListView, DragItemsCompletedEventArgs> DragItemsCompleted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Scroller = e.NameScope.Find<ScrollViewer>(s_tpScrollViewer);

        // HACK: DragDrop events work differently in Avalonia than WinUI. In WinUI, they aren't true
        // routed events, so the OriginalSource parameter returns TabView or TabViewItem, etc., not
        // template items. In Avalonia, we get template items, but only like 1 or 2. This means that
        // we can't actually detect if we've left the TabStrip during a drag operation. As of me writing
        // this, the only things we detect from the local Drag handlers is some template items in the
        // TabViewItems - ABSOLUTELY NOTHING FROM TABVIEWLISTVIEW, arghhhhh...That's annoying
        // So this seems to work - if we grab a drag enter handler on the TabView itself and do a bounds
        // check on this, we know if the pointer left the tab strip or not. 
        _parent = this.FindAncestorOfType<TabView>();
        _parent.AddHandler(DragDrop.DragLeaveEvent, OnParentDragEnter);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedIndexProperty)
        {
            UpdateBottomBorderVisualState();
        }
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        bool isItem = item is TabViewItem;
        recycleKey = isItem ? null : nameof(TabViewItem);
        return !isItem;
    }

    protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
    {
        var cont = this.FindDataTemplate(item, ItemTemplate)?.Build(item);
        
        if (cont is TabViewItem tvi)
        {
            tvi.IsContainerFromTemplate = true;
            return tvi;
        }

        return new TabViewItem();
    }

    protected override void ContainerForItemPreparedOverride(Control container, object item, int index)
    {
        // NOTE: BE CAREFUL HERE! Avalonia has two separate preparation events
        // PrepareContainerForItemOverride - does not have the container connected yet
        // This one does - I've raised an issue b/c this is dumb
        var tvi = container as TabViewItem;

        // WinUI: Due to virtualization, a TabViewItem might be recycled to display a different tab data item.
        //        In that case, there is no need to set the TabWidthMode of the TabViewItem or its parent TabView
        //        as they are already set correctly here.
        //
        //        We know we are currently looking at a TabViewItem being recycled if its parent TabView has
        //        already been set.
        var tabLocation = TabViewTabStripLocation.Top; // Default to top
        if (tvi.ParentTabView == null)
        {
            var parentTV = container.FindAncestorOfType<TabView>();
            if (parentTV != null)
            {
                tvi.OnTabViewWidthModeChanged(parentTV.TabWidthMode);
                tvi.ParentTabView = parentTV;
                tabLocation = parentTV.TabStripLocation;
            }
        }
        else
        {
            tabLocation = tvi.ParentTabView.TabStripLocation;
        }

        tvi.HandleTabStripLocationChanged(tabLocation);

        // Special b/c we use the header and not Content. Somehow this *just works* in
        // WinUI b/c they don't have to do this.
        if (container == item || tvi.IsContainerFromTemplate)
        {
            base.ContainerForItemPreparedOverride(container, item, index);
            return;
        }

        tvi.Header = item;

        var itemTemplate = this.FindDataTemplate(item, ItemTemplate);

        if (itemTemplate != null)
            tvi.HeaderTemplate = itemTemplate;

        base.ContainerForItemPreparedOverride(container, item, index);

        if (!tvi.IsSelected)
        {
            // Bug Fix: When containers are being virtualized, they may "come back online" with
            // old state left over.
            // This is also a bug in WinUI, but it materializes differently. In Avalonia, we can see
            // this very clearly because WinUI pins and does not recycle the SelectedItem container,
            // but Avalonia does. Thus, when the previously selected container is reused, it still
            // has the visual state we apply to selected items, specifically the :noborder pseudoclass
            // Because WinUI pins the container, we never see this issue
            // HOWEVER, we can see it in in WinUI with the LeftOfSelectedTab/RightOfSelectedTab states
            // If you select an item and then scroll away such that SelIndex-1 and Selindex+1 container
            // are recycled, inspect them, you'll see the margin still applied to the Border line indicating
            // the state was never cleared. If you scroll to those new containers you'll see a tiny little
            // gap in the bottom border because of this. 
            // Fix here by just ensuring this state get's updated. I don't want to call TabView.UpdateBottom...
            // because that iterates over containers and that isn't great in this path. 
            // Also added unit test to ensure this is fixed.

            var selIndex = SelectedIndex;
            int state = -1;
            if (selIndex != -1)
            {
                if (index == selIndex)
                {
                    state = 0;
                }
                else if (index == selIndex - 1)
                {
                    state = 1;
                }
                else if (index == selIndex + 1)
                {
                    state = 2;
                }
            }

            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcNoBorder, state == 0);
            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderLeft, state == 1);
            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderRight, state == 2);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs args)
    {
        if (args.Handled)
            return;

        if (CanDragItems || CanReorderItems)
        {
            var currentPoint = args.GetCurrentPoint(this);
            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                _initialPoint = currentPoint.Position;
                _dragItem = (args.Source as Visual).FindAncestorOfType<TabViewItem>(true);
                _dragIndex = IndexFromContainer(_dragItem);
                _initArgs = args;
                UpdateDragInfo();
                // args.Handled = true;
            }
        }

        base.OnPointerPressed(args);
    }

    protected override void OnPointerMoved(PointerEventArgs args)
    {
        if (args.Handled)
            return;

        if (_initialPoint.HasValue)
        {
            if (!_isInDrag || !_isInReorder)
            {
                var currentPoint = args.GetPosition(this);
                var delta = currentPoint - _initialPoint.Value;

                if (double.Abs(delta.X) > _cxDrag || double.Abs(delta.Y) > _cyDrag)
                {
                    BeginDragReorder();
                    //args.Handled = true;
                }
            }
        }

        base.OnPointerMoved(args);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs args)
    {
        base.OnPointerReleased(args);
        CancelDrag();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs args)
    {
        base.OnPointerCaptureLost(args);

        // TODO: We really need to handle this, but this fires in cases I don't think it should
        // 1- Mouse Button Release
        // 2- Start of DragDrop
    }
    

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _parent.RemoveHandler(DragDrop.DragLeaveEvent, OnParentDragEnter);
        _parent = null;
    }

    private async void BeginDragReorder()
    {
        var package = new DataPackage();
        DragItemsStartingEventArgs dragArgs = null;
        object[] dragItems = null;
        IDisposable opacity = null;
        bool canReorder = CanReorderItems;
        bool canDrag = CanDragItems;

        if (canDrag)
        {
            dragItems = new object[] { ItemsView.GetAt(_dragIndex) };
            dragArgs = new DragItemsStartingEventArgs { Items = dragItems, Data = package };
            DragItemsStarting?.Invoke(this, dragArgs);

            if (dragArgs.Cancel)
            {
                CancelDrag();
                return;
            }

            _isInDrag = true;
        }

        if (canReorder)
        {
            // This is reorder only. We will handle this case ourselves
            // WinUI will allow you to start the drag operation even if the underlying
            // source is not INCC, which to me doesn't seem right. So...
            // If TabItems are in use, we'll allow it b/c that is INCC
            // If TabItemsSource is in use, we'll check if its INCC and only allow it if it is
            // In addition, if user has disabled this as a DropTarget, reject

            if (!DragDrop.GetAllowDrop(this))
            {
                CancelDrag();
                Logger.TryGet(LogEventLevel.Debug, "TabView")?
                    .Log("TabView", "User disabled this TabView as as drop target - canceling drag");
                return;
            }

            // Note: That Avalonia also has the restriction that INCC collections must also implement
            // the non-generic IList, so we'll also check the IsReadOnly property
            var src = ItemsSource;
            if (src != null && (src is not INotifyCollectionChanged || (src is IList l && l.IsReadOnly)))
            {
                CancelDrag();
                Logger.TryGet(LogEventLevel.Debug, "TabView")?
                    .Log("TabView", "Attempted to initiate Drag/Reorder without INCC / mutable collection");
                return;
            }

            opacity = _dragItem.SetValue(OpacityProperty, 0, BindingPriority.Animation);

            // Cache the locations of all containers before we start for reorder hints
            _isInReorder = true;
            _isDraggingOverSelf = true;
        }

        var effects = dragArgs?.Data.RequestedOperation ?? DragDropEffects.Move;

        var dropResult = await DragDrop.DoDragDropAsync(_initArgs, package, effects);

        if (_isInReorder)
        {
            opacity?.Dispose();
            _isInReorder = false;
        }

        if (_isInDrag)
        {
            var completedArgs = new DragItemsCompletedEventArgs(dropResult, dragItems);
            DragItemsCompleted?.Invoke(this, completedArgs);
            _isInDrag = false;
        }

        CancelDrag();
        _liveReorderHelper?.ClearContainerBoundsCache(true);
    }

    private void OnListViewDragOver(object sender, DragEventArgs e)
    {
        // OLE DragDrop seems to fire on a Timer which means we get a constant stream
        // of DragOver events. WinRT DnD doesn't do this. The issue is in the live reorder
        // system where constant events constantly restart the timer so it never actually
        // triggers. So to prevent that, filter the pointer and only proceed with drag
        // if the pointer location changes
        if (_lastDragOverPoint == null)
        {
            _lastDragOverPoint = e.GetPosition(this);
        }
        else
        {
            var pt = e.GetPosition(this);
            if (double.Abs(pt.X - _lastDragOverPoint.Value.X) < 1e-5 &&
                double.Abs(pt.Y - _lastDragOverPoint.Value.Y) < 1e-5)
            {
                return;
            }
            _lastDragOverPoint = pt;
        }

        bool canReorder = CanReorderItems;
        bool isInReorderFromExternalSource = (!_isDraggingOverSelf && canReorder);

        if (!_isDragWithinTabStrip)
        {
            _isDragWithinTabStrip = true;
            DragEnter?.Invoke(this, e);
        }
        else
        {
            // According to WinUI, TabStripDragOver doesn't fire if reordering is active
            // even if CanDragTabs is true - so only raise if we're only dragging items
            if (!_isInReorder)
                DragOver?.Invoke(this, e);

            // If this ListView initiated drag drop, _dragItem will be set
            _isDraggingOverSelf = _dragItem != null;            
        }
                
        Process(_isInReorder, canReorder, e);
                
        if (_isInReorder || isInReorderFromExternalSource)
        {
            _liveReorderHelper ??= new LiveReorderHelper(this);
            _liveReorderHelper.ProcessLiveReorder(e, _dragIndex);
        }

        static void Process(bool isInReorder, bool canReorder, DragEventArgs args)
        {
            // Let the user handle & set drag effects first, if left unhandled,
            // set a move operation on the args if reordering
            // See ListViewBase::OnDragOver in WinUI
            if (!args.Handled)
            {
                // Reorder operations have this
                var effects = isInReorder || canReorder ? DragDropEffects.Move : DragDropEffects.None;
                args.DragEffects &= effects;
            }            
        }
    }

    // This is actually DragLeave for us
    private void OnParentDragEnter(object sender, DragEventArgs e)
    {
        if (_isDragWithinTabStrip)
        {
            var bnds = new Rect(Bounds.Size);
            var pt = e.GetPosition(this);
            if (!bnds.Contains(pt))
            {
                _isDragWithinTabStrip = false;
                _isDraggingOverSelf = false;
                _liveReorderIndices = default;
                _liveReorderHelper?.ResetAllItemsForLiveReorder();
                DragLeave?.Invoke(this, e);
            }
        }        
    }

    private void OnListViewDrop(object sender, DragEventArgs e)
    {
        if (e.Handled)
            return;

        if (DropCausesReorder())
        {
            var pt = e.GetPosition(this);
            OnReorderDrop(pt);

            e.DragEffects = DragDropEffects.Move;
            e.Handled = true;
        }
        else
        {
            _liveReorderHelper?.ResetAllItemsForLiveReorder();
        }

        Drop?.Invoke(this, e);
    }

    private void CancelDrag()
    {
        _initArgs = null;
        _initialPoint = null;
        _isInDrag = _isInReorder = false;
        _dragIndex = -1;
        _dragItem = null;
        _isDraggingOverSelf = false;
        _lastDragOverPoint = null;
        _isDragWithinTabStrip = false;
        _liveReorderHelper?.ResetAllItemsForLiveReorder();
    }

    private bool DropCausesReorder()
    {
        if (_isDraggingOverSelf)
        {
            return CanReorderItems && DragDrop.GetAllowDrop(this);
        }

        return false;
    }

    private void OnReorderDrop(Point dropPoint)
    {
        // Container & original index - TabView does not support multi-item drag
        // so we don't need anything else here
        var dragItem = _dragItem;
        var dragIndex = _dragIndex;
        bool isDragItemFocused = dragItem.IsFocused;
        bool isDragItemSelected = dragItem.IsSelected;

        var insertIndex = _liveReorderHelper.GetInsertionIndexForLiveReorder();
        _liveReorderHelper.ResetAllItemsForLiveReorder();

        if (dragIndex == insertIndex)
            return;

        if (insertIndex == -1)
        {
            insertIndex = _liveReorderHelper.GetClosestElement(dropPoint, true);
        }
        
        // dragItem is the container, we need the actual data item here
        var data = ItemFromContainer(dragItem);

        if (dragIndex < insertIndex)
        {
            insertIndex--;
        }

        var itemsSource = ItemsSource;
        // Avalonia enforces the constraint that INCC must be IList, so this is safe
        if (itemsSource is IList l)
        {
            try
            {
                // In the event the user has a list that isn't mutable and we got to this
                // point somehow (we check when reorder starts), don't crash the app
                // Just silently fail here

                l.RemoveAt(dragIndex);
                l.Insert(insertIndex, data);
            }
            catch { }
        }
        else if (itemsSource == null)
        {
            var items = Items;
            try
            {
                items.RemoveAt(dragIndex);
                items.Insert(insertIndex, data);
            }
            catch { }
        }

        // Note that _dragItem is no longer valid since the container
        // may have changed, grab the new container from insertIndex
        
        UpdateLayout(); // Force an update so ScrollIntoView works

        ScrollIntoView(insertIndex);

        if (isDragItemFocused)
        {
            // If the old drag item was focused, we should refocus it
            if (ContainerFromIndex(insertIndex) is Control c)
            {
                c.Focus();
            }
        }

        if (isDragItemSelected)
        {
            SelectedIndex = insertIndex;
        }
    }

    //private void CacheContainerBounds()
    //{
    //    Debug.WriteLine("CACHING CONTAINER BOUNDS");
    //    // Because reorder will arrange the containers in new places
    //    // the Bounds on the container may not actually reflect where
    //    // the item actually is. In WinUI, ModernCollectionBasePanel's 
    //    // ContainerManager caches arrange rects and are used for estimation
    //    // APIs. Here we are mimicing that
    //    // This must be called at the start of drag/drop, when the auto scroll
    //    // of the scrollviewer stops, or when we first drag onto the TabView
    //    // if that TabView didn't start the dragdrop operation

    //    var panel = ItemsPanelRoot;
    //    if (panel is VirtualizingStackPanel vsp)
    //    {
    //        var firstRealized = vsp.FirstRealizedIndex;
    //        var lastRealized = vsp.LastRealizedIndex;
    //        _cachedContainerBounds ??= new List<Rect>((lastRealized - firstRealized) + 1);

    //        for (int i = firstRealized; i <= lastRealized; i++)
    //        {
    //            var cont = ContainerFromIndex(i);
    //            _cachedContainerBounds.Add(cont.Bounds);
    //        }
    //    }
    //    else if (panel is StackPanel sp)
    //    {
    //        _cachedContainerBounds ??= new List<Rect>(ItemCount);
    //        // Stack Panels don't virtualize and arrange in order so this is safe
    //        for (int i = 0; i < ItemCount; i++)
    //        {
    //            _cachedContainerBounds.Add(panel.Children[i].Bounds);
    //        }
    //    }
    //}

    //private void ClearContainerBoundsCache(bool clearCompletely = false)
    //{
    //    // Clear container bounds
    //    _cachedContainerBounds?.Clear();

    //    // Don't keep this memory when not in use, so when drag drop operation
    //    // ceases completely, set it to null
    //    if (clearCompletely)
    //        _cachedContainerBounds = null;
    //}

    //private void ProcessLiveReorder(DragEventArgs args)
    //{
    //    _liveReorderHelper ??= new LiveReorderHelper(this);

    //    var orientation = GetLogicalOrientation();
    //    if (orientation == null)
    //        return; // We're in a panel we don't know how to deal with, exit out
        
    //    var helper = _liveReorderHelper;

    //    if (helper.ShouldCacheContainerBounds())
    //        helper.CacheContainerBounds();




    //    // OLD --------------------------------------------------------
    //    var orientation = GetLogicalOrientation();
    //    if (orientation == null)
    //        return; // We're in a panel we don't know how to deal with, exit out

    //    // If we don't have a cache of the current realized container bounds,
    //    // create it now before we start doing anything. This happens on first
    //    // startup, but can also be reset through the drag operation (autoscroll,
    //    // drag outside, etc). The cache is cleared on ResetAllItemsForLiveReorder()
    //    if (_cachedContainerBounds == null || _cachedContainerBounds.Count == 0)
    //    {
    //        CacheContainerBounds();
    //    }
        
    //    int draggedIndex = _dragIndex;
    //    int insertionIndex = -1;
    //    int dragOverIndex = GetClosestElement(args.GetPosition(this));// IndexFromContainer(currentItem); // The raw item index under the pointer
    //    var previousDragOverIndex = _liveReorderIndices.draggedOverIndex;
    //    int itemsCount = ItemCount;

    //    if (draggedIndex == -1)
    //        draggedIndex = itemsCount;

    //    if (previousDragOverIndex == -1)
    //    {
    //        previousDragOverIndex = draggedIndex;
    //    }

    //    // The estimated insertion index in the panel
    //    insertionIndex = GetClosestElement(args.GetPosition(this), true /*requestingInsertionIndex*/);

    //    if (draggedIndex == itemsCount && insertionIndex == itemsCount - 1)
    //    {
    //        // If we didn't start in this TabView, see if the index is actually the end or -1
    //        var spLastElement = ContainerFromIndex(insertionIndex);
    //        if (spLastElement is TabViewItem tvi)
    //        {
    //            if (IsInBottomHalf(args.GetPosition(spLastElement), new Rect(spLastElement.Bounds.Size), orientation.Value))
    //            {
    //                insertionIndex = itemsCount;
    //            }
    //        }
    //    }

    //    //var old = dragOverIndex; // Keep this here for debug purposes, if needed
    //    if (insertionIndex == itemsCount)
    //    {
    //        dragOverIndex = itemsCount;
    //    }
    //    else
    //    {
    //        // This adjusts the dragover index based on the direction of the drag
    //        dragOverIndex = GetDragOverIndex(dragOverIndex, insertionIndex, previousDragOverIndex);
    //    }

    //    // Debug.WriteLine($"\tLive Reorder DragOverIndex: {dragOverIndex} || DragOverBeforeAdj: {old} || InsertionIndex {insertionIndex} || PrevDragOverIndex {previousDragOverIndex}");

    //    _liveReorderIndices = new LiveReorderIndices(draggedIndex, dragOverIndex, itemsCount);

    //    if (previousDragOverIndex == draggedIndex || previousDragOverIndex != dragOverIndex)
    //    {
    //        StartLiveReorderTimer();
    //    }

    //    static bool IsInBottomHalf(Point pt, Rect rc, Orientation orientation)
    //    {
    //        if (orientation == Orientation.Horizontal)
    //        {
    //            return (pt.X - rc.Left) >= rc.Width * 0.5;
    //        }
    //        else
    //        {
    //            return (pt.Y - rc.Top) >= rc.Height * 0.5;
    //        }
    //    }

    //    static int GetDragOverIndex(int closestElementIndex, int insertionIndex, int previousDragOverIndex)
    //    {
    //        int dragOverIndex = closestElementIndex;

    //        if (insertionIndex == closestElementIndex)
    //        {
    //            if (previousDragOverIndex < insertionIndex)
    //            {
    //                --dragOverIndex;
    //            }
    //        }
    //        else
    //        {
    //            if (previousDragOverIndex >= insertionIndex)
    //            {
    //                ++dragOverIndex;
    //            }
    //        }

    //        return dragOverIndex;
    //    }
    //}

    //private int GetClosestElement(Point dragPoint, bool requestingInsertionIndex = false)
    //{
    //    // This estimates the container index given the current pointer position
    //    var panel = ItemsPanelRoot;
    //    if (panel is VirtualizingStackPanel vsp)
    //    {
    //        var firstRealized = vsp.FirstRealizedIndex;
    //        var lastRealized = vsp.LastRealizedIndex;
    //        var orientation = vsp.Orientation;
    //        var movedItems = _movedItems.AsSpan();
    //        int closestIndex = -1;
    //        double closestDist = double.PositiveInfinity;
    //        Rect closestItemRect = default;

    //        // Loop over the currently realized items to find the closest
    //        for (int i = firstRealized; i <= lastRealized; i++)
    //        {
    //            // If the item is currently in our MovedItems list, it may not be 
    //            // where it usually is, so we can't test the actual Bounds or we'll
    //            // estimate the wrong index, but we have the original bounds saved
    //            Rect rc = _cachedContainerBounds[i - firstRealized];
    //            double dist;

    //            if (orientation == Orientation.Horizontal)
    //            {
    //                double cx = double.Clamp(dragPoint.X, rc.X, rc.Right);
    //                dist = double.Abs(dragPoint.X - cx);
    //            }
    //            else
    //            {
    //                double cy = double.Clamp(dragPoint.Y, rc.Y, rc.Bottom);
    //                dist = double.Abs(dragPoint.Y - cy);
    //            }

    //            if (dist < closestDist)
    //            {
    //                closestDist = dist;
    //                closestIndex = i;
    //                closestItemRect = rc;
    //            }
    //        }

    //        if (requestingInsertionIndex)
    //        {
    //            if (orientation == Orientation.Horizontal)
    //            {
    //                if (dragPoint.X - closestItemRect.X >= closestItemRect.Width * 0.5)
    //                {
    //                    closestIndex++;
    //                }
    //            }
    //            else if (orientation == Orientation.Vertical)
    //            {
    //                if (dragPoint.Y - closestItemRect.Y >= closestItemRect.Height * 0.5)
    //                {
    //                    closestIndex++;
    //                }
    //            }
    //        }

    //        return closestIndex;
    //    }
    //    else if (panel is StackPanel sp)
    //    {
    //        //var children = sp.Children;
    //        //var orientation = sp.Orientation;
    //        //var movedItems = _movedItems.AsSpan();

    //        //for (int i = 0; i < children.Count; i++)
    //        //{
    //        //    // If the item is currently in our MovedItems list, it may not be 
    //        //    // where it usually is, so we can't test the actual Bounds or we'll
    //        //    // estimate the wrong index, but we have the original bounds saved
    //        //    if (IsInMovedItems(movedItems, i, out var rc))
    //        //    {
    //        //        if (rc.Contains(dragPoint))
    //        //        {
    //        //            return i;
    //        //        }
    //        //    }
    //        //    else
    //        //    {
    //        //        // The item is not in moved items, so it is safe to use the Bounds directly
    //        //        if (children[i].Bounds.Contains(dragPoint))
    //        //        {
    //        //            return i;
    //        //        }
    //        //    }
    //        //}
    //    }

    //    return -1;

    //    //static bool IsInMovedItems(ReadOnlySpan<MovedItem> items, int sourceIndex, out Rect srcRect)
    //    //{
    //    //    foreach (var item in items)
    //    //    {
    //    //        if (item.sourceIndex == sourceIndex)
    //    //        {
    //    //            srcRect = item.sourceRect;
    //    //            return true;
    //    //        }
    //    //    }

    //    //    srcRect = default;
    //    //    return false;
    //    //}
    //}

    //private void StartLiveReorderTimer()
    //{
    //    StopLiveReorderTimer();

    //    EnsureLiveReorderTimer();

    //    _liveReorderTimer.Interval = TimeSpan.FromMilliseconds(200);
    //    _liveReorderTimer.Start();
    //}

    //private void EnsureLiveReorderTimer()
    //{
    //    if (_liveReorderTimer == null)
    //    {
    //        _liveReorderTimer = new DispatcherTimer();
    //        _liveReorderTimer.Tick += LiveReorderTimerTickHandler;
    //    }
    //}

    //private void LiveReorderTimerTickHandler(object sender, EventArgs e)
    //{
    //    StopLiveReorderTimer();

    //    Debug.Assert(_liveReorderIndices.draggedItemIndex != -1);

    //    var orientation = GetLogicalOrientation().Value;

    //    using var newItems = new PooledList<MovedItem>();
    //    using var newItemsToMove = new PooledList<MovedItem>();
    //    using var oldItemsToMoveBack = new PooledList<MovedItem>();

    //    GetNewMovedItemsForLiveReorder(newItems);

    //    _movedItems.Update(orientation == Orientation.Vertical, newItems, newItemsToMove, oldItemsToMoveBack);

    //    MoveItemsForLiveReorder(false /*areNewItems*/, oldItemsToMoveBack);

    //    MoveItemsForLiveReorder(true, newItemsToMove);

    //    foreach (var item in _movedItems.AsSpan())
    //    {
    //        Debug.WriteLine($"\t MovedItems: {item.sourceIndex} -> {item.destinationIndex} || {item.sourceRect} -> {item.destinationRect}");
    //    }
    //}

    //private void GetNewMovedItemsForLiveReorder(IList<MovedItem> newItems)
    //{
    //    int startIndex = _liveReorderIndices.draggedItemIndex;
    //    int endIndex = _liveReorderIndices.draggedOverIndex;
    //    int increment = (startIndex < endIndex) ? 1 : -1;

    //    // Debug.WriteLine($"GetNewMovedItems: {startIndex} -> {endIndex}");

    //    newItems.Clear();
    //    for (int i = startIndex; i != endIndex; i += increment)
    //    {
    //        int targetIndex = i - increment;

    //        if (i == startIndex)
    //        {
    //            targetIndex = -1;
    //        }

    //        AddNewItemForLiveReorder(i, targetIndex, newItems, _liveReorderIndices.itemsCount, this);
    //    }

    //    AddNewItemForLiveReorder(endIndex, endIndex - increment, newItems, _liveReorderIndices.itemsCount, this);

    //   // Debug.WriteLine($"TotalNewItems: {newItems.Count}");

    //    static void AddNewItemForLiveReorder(int sourceIndex, int targetIndex, IList<MovedItem> newItems, 
    //        int itemsCount, TabViewListView host)
    //    {
    //        Rect src = default;
    //        Rect target = default;

    //        if (sourceIndex != targetIndex)
    //        {
    //            src = GetLayoutSlot(host, sourceIndex);
    //        }

    //        if (targetIndex != -1 && targetIndex != itemsCount)
    //        {
    //            target = GetLayoutSlot(host, targetIndex);
    //        }

    //        newItems.Add(new MovedItem(sourceIndex, targetIndex, src, target));
    //    }

    //    static Rect GetLayoutSlot(TabViewListView host, int index)
    //    {
    //        // make sure we grab the original bounds. If virtualizing, translate
    //        // to index in our container cache
    //        var adjIndex = host.ItemsPanelRoot is VirtualizingStackPanel vsp ?
    //            index - vsp.FirstRealizedIndex : index;

    //        if (adjIndex >= host._cachedContainerBounds.Count)
    //            return default;

    //        return host._cachedContainerBounds[adjIndex];
    //        //return host.ContainerFromIndex(index)?.Bounds ?? default;
    //    }
    //}

    //private void MoveItemsForLiveReorder(bool areNewItems, PooledList<MovedItem> newItemsToMove)
    //{
    //    Rect rc;
    //    foreach (var item in newItemsToMove.AsSpan())
    //    {
    //        var container = ContainerFromIndex(item.sourceIndex);

    //        if (container is Control c)
    //        {
    //            if (areNewItems)
    //            {
    //                rc = item.destinationRect;
    //            }
    //            else
    //            {
    //                rc = item.sourceRect;
    //            }

    //            c.Arrange(rc);
    //        }
    //    }
    //}

    //private void ResetAllItemsForLiveReorder()
    //{
    //    StopLiveReorderTimer();

    //    foreach (var item in _movedItems.AsSpan())
    //    {
    //        if (item.destinationIndex != -1)
    //        {
    //            var cont = ContainerFromIndex(item.sourceIndex);
    //            if (cont is Control c)
    //            {
    //                c.Arrange(item.sourceRect);
    //            }
    //        }
    //    }

    //    _movedItems.Clear();
    //    _liveReorderIndices = new LiveReorderIndices(-1,-1,-1);
    //    ClearContainerBoundsCache();
    //}

    //private void StopLiveReorderTimer()
    //{
    //    _liveReorderTimer?.Stop();
    //}

    internal Orientation? GetLogicalOrientation()
    {
        var panel = ItemsPanelRoot;
        if (panel is VirtualizingStackPanel vsp)
            return vsp.Orientation;
        else if (panel is StackPanel sp)
            return sp.Orientation;

        return null;
    }

    internal void HandleTabStripLocationChanged(TabViewTabStripLocation newLocation, string oldClass, string newClass)
    {
        if (oldClass != null)
            PseudoClasses.Set(oldClass, false);

        PseudoClasses.Set(newClass, true);

        if (Scroller != null)
        {
            if (oldClass != null)
                ((IPseudoClasses)Scroller.Classes).Set(oldClass, false);

            ((IPseudoClasses)Scroller.Classes).Set(newClass, true);
        }

        var panel = ItemsPanelRoot;
        if (panel != null)
        {
            foreach (var item in panel.Children)
            {
                if (item is TabViewItem tvi)
                {
                    tvi.HandleTabStripLocationChanged(newLocation);
                }
            }

            // If we have a Stacking Panel, adjust its orientation
            // If user uses any other type of panel, do nothing & log warning
            // User will need to monitor changes and adjust their panel accordingly
            if (panel is VirtualizingStackPanel vsp)
            {
                if (vsp.Orientation == Orientation.Vertical &&
                    (newLocation == TabViewTabStripLocation.Top || newLocation == TabViewTabStripLocation.Bottom))
                {
                    vsp.Orientation = Orientation.Horizontal;
                }
                else if (vsp.Orientation == Orientation.Horizontal &&
                    (newLocation == TabViewTabStripLocation.Left || newLocation == TabViewTabStripLocation.Right))
                {
                    vsp.Orientation = Orientation.Vertical;
                }
            }
            else if (panel is StackPanel sp)
            {
                if (sp.Orientation == Orientation.Vertical &&
                    (newLocation == TabViewTabStripLocation.Top || newLocation == TabViewTabStripLocation.Bottom))
                {
                    sp.Orientation = Orientation.Horizontal;
                }
                else if (sp.Orientation == Orientation.Horizontal &&
                    (newLocation == TabViewTabStripLocation.Left || newLocation == TabViewTabStripLocation.Right))
                {
                    sp.Orientation = Orientation.Vertical;
                }
            }
            else
            {
                Logger.Sink?.Log(LogEventLevel.Warning, "TabView", this,
                    "User has TabView with non-stacking panel, which may not be compatible with TabStripLocation changes");
            }
        }
    }

    private void UpdateBottomBorderVisualState()
    {
        PseudoClasses.Set(s_pcLeftShort, SelectedIndex == 0);
        PseudoClasses.Set(s_pcRightShort, SelectedIndex == ItemsView.Count - 1);
    }

    private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var tv = this.FindAncestorOfType<TabView>();
        tv?.OnItemsChanged(e);
    }

    private void UpdateDragInfo()
    {
        FAUISettings.GetSystemDragSize(VisualRoot.RenderScaling, out _cxDrag, out _cyDrag);
    }

    private TabViewItem _dragItem;
    private int _dragIndex = -1;
    private bool _isInDrag = false;
    private bool _isInReorder = false;
    private bool _processReorder;
    private Point? _initialPoint;
    private double _cxDrag = double.NaN;
    private double _cyDrag = double.NaN;
    private Control _parent;
    private bool _isDragWithinTabStrip;
    // True if there is a drag drop operation started by this listview
    private bool _isDraggingOverSelf;

    private LiveReorderHelper _liveReorderHelper;
    private LiveReorderIndices _liveReorderIndices = new LiveReorderIndices(-1,-1,-1);
    private DispatcherTimer _liveReorderTimer;
    private readonly MovedItems _movedItems = new MovedItems();
    private List<Rect> _cachedContainerBounds;
    private Point? _lastDragOverPoint;

    // For 12.0/v3 - Avalonia has decided to make the decision that the lowest common denominator
    // in the platform backends decides the entire public API. As part of this, DoDragDrop now
    // requires the initial pressed args, so we have to store them away so we can start DragDrop.
    // I tried to object, and failed (https://github.com/AvaloniaUI/Avalonia/pull/20988)
    // And you guessed it, freakin' Wayland
    private PointerPressedEventArgs _initArgs;
    
    private static Popup _dragReorderPopup;
    private Point _popupOffset;
    private object _dragItemToolTip;

    private DispatcherTimer _scrollTimer;
    private int _scrollDirection;
    private Rect _noAutoScrollRect;

    private const string s_tpScrollViewer = "ScrollViewer";

    private const string s_pcReorder = ":reorder";
    private const string s_pcLeftShort = ":leftShort";
    private const string s_pcRightShort = ":rightShort";

    private enum AutoScrollAction
    {
        NoAction,
        ScrollStarted,
        ScrollEnded,
    }


}
