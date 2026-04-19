using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;
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
                _isDragItemFocused = _dragItem.IsFocused;
                _isDragItemSelected = _dragItem.IsSelected;
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
        //FAUISettings.GetSystemDragSize(VisualRoot.RenderScaling, out _cxDrag, out _cyDrag);
    }

    private async void BeginDragReorder()
    {
        var package = new DataPackage();
        DragItemsStartingEventArgs dragArgs = null;
        object[] dragItems = null;
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

            _dragItemOpacitySub = _dragItem.SetValue(OpacityProperty, 0, BindingPriority.Animation);

            // Cache the locations of all containers before we start for reorder hints
            _isInReorder = true;
            _isDraggingOverSelf = true;
        }

        var effects = dragArgs?.Data.RequestedOperation ?? DragDropEffects.Move;

        var dropResult = await DragDrop.DoDragDropAsync(_initArgs, package, effects);

        SetPendingAutoPanVelocity(default);
        DestroyStartEdgeScrollTimer();

        if (_isInReorder)
        {
            _dragItemOpacitySub?.Dispose();
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

        if (_scrollTimer == null)
        {
            if (_isInReorder || isInReorderFromExternalSource)
            {
                _liveReorderHelper ??= new LiveReorderHelper(this);
                _liveReorderHelper.ProcessLiveReorder(e, _dragIndex);
            }
        }

        ComputeEdgeScrollVelocity(e.GetPosition(this), out var pVelocity);
        SetPendingAutoPanVelocity(pVelocity);

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
                SetPendingAutoPanVelocity(default);
                DestroyStartEdgeScrollTimer();
                _isDragWithinTabStrip = false;
                _isDraggingOverSelf = false;
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
        _isDragItemSelected = _isDragItemFocused = false;
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
        var dragIndex = _dragIndex;
        bool isDragItemFocused = _isDragItemFocused;
        bool isDragItemSelected = _isDragItemSelected;

        var insertIndex = _liveReorderHelper.GetInsertionIndexForLiveReorder();
        _liveReorderHelper.ResetAllItemsForLiveReorder();

        if (dragIndex == insertIndex)
            return;

        if (insertIndex == -1)
        {
            insertIndex = _liveReorderHelper.GetClosestElement(dropPoint, true);
        }

        // dragItem is the container, we need the actual data item here
        var data = ItemsView.GetAt(_dragIndex);// ItemFromContainer(dragItem);

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

    private void ComputeEdgeScrollVelocity(Point dragPoint, out Vector pVelocity)
    {
        bool isVerticalEnabled = false;
        bool isHorizontalEnabled = false;
        Size extent = default;
        Size viewport = default;
        Vector offset = default;

        if (Scroller != null)
        {
            var vertical = Scroller.VerticalScrollBarVisibility;
            var horizontal = Scroller.HorizontalScrollBarVisibility;
            extent = Scroller.Extent;
            viewport = Scroller.Viewport;
            offset = Scroller.Offset;

            isVerticalEnabled = vertical != ScrollBarVisibility.Disabled;
            isHorizontalEnabled = horizontal == ScrollBarVisibility.Visible;
        }

        double hVelocity = 0;
        double vVelocity = 0;
        if (isHorizontalEnabled)
        {
            // WinUI uses a hardcoded 100px as the threshold for where autoscroll starts, but that
            // doesn't seem great with small listviews. So I'm using 20% of the width
            var threshold = Bounds.Size.Width * 0.2;
            double bound = 0;

            // Try Scrolling Left
            hVelocity = -ComputeEdgeScrollVelocityFromEdgeDistance(dragPoint.X, threshold);
            if (hVelocity == 0)
            {
                // Try Scrolling Right
                var width = Bounds.Size.Width;
                hVelocity = ComputeEdgeScrollVelocityFromEdgeDistance(width - dragPoint.X, threshold);
                bound = extent.Width - viewport.Width;
            }

            // Disable if we're right up on the edge
            if (MathUtilities.AreClose(bound, offset.X, 0.05))
            {
                hVelocity = 0;
            }
        }

        if (isVerticalEnabled && hVelocity == 0)
        {
            var threshold = Bounds.Size.Height * 0.2;
            double bound = 0;

            vVelocity = -ComputeEdgeScrollVelocityFromEdgeDistance(dragPoint.Y, threshold);
            if (vVelocity == 0)
            {
                double height = Bounds.Size.Height;
                vVelocity = ComputeEdgeScrollVelocityFromEdgeDistance(height - dragPoint.Y, threshold);
                bound = extent.Height - viewport.Height;
            }

            // Disable if we're right up on the edge
            if (MathUtilities.AreClose(bound, offset.Y, 0.05))
            {
                vVelocity = 0;
            }
        }

        pVelocity = new Vector(hVelocity, vVelocity);
    }

    private static double ComputeEdgeScrollVelocityFromEdgeDistance(in double distFromEdge,
        double edgeDistanceThreshold = 100)
    {
        if (distFromEdge <= edgeDistanceThreshold)
        {
            return 200 - (distFromEdge / edgeDistanceThreshold) * (200 - 25);
        }

        return 0;
    }

    private void SetPendingAutoPanVelocity(Vector velocity)
    {
        if (!IsStationary(velocity))
        {
            if (!IsStationary(_currentAutoPanVelocity))
            {
                _currentAutoPanVelocity = velocity;
                EnsureStartEdgeScrollTimer();
            }
            else
            {
                _currentAutoPanVelocity = velocity;
            }

            // While AutoScrolling, be sure the live reorder manager isn't trying to 
            // do anything as container bounds are constantly changing and things
            // won't line up like it expects
            _liveReorderHelper?.ResetAllItemsForLiveReorder();
            // Also reset the opacity on the drag item so it doesn't affect
            // recycled containers
            _dragItemOpacitySub?.Dispose();
        }
        else
        {
            DestroyStartEdgeScrollTimer();
            _currentAutoPanVelocity = default;
            ScrollWithVelocity(default);

            _dragItemOpacitySub?.Dispose();
            var cont = ContainerFromIndex(_dragIndex);
            if (cont != null)
            {
                _dragItemOpacitySub = _dragItem.SetValue(OpacityProperty, 0, BindingPriority.Animation);
            }
        }
    }

    private void EnsureStartEdgeScrollTimer()
    {
        _scrollTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(50),
            DispatcherPriority.Normal, StartEdgeScrollTimerTick);

        _scrollTimer.Start();
    }

    private void DestroyStartEdgeScrollTimer()
    {
        _scrollTimer?.Stop();
        _scrollTimer = null;
    }

    private void StartEdgeScrollTimerTick(object sender, EventArgs args)
    {
        ScrollWithVelocity(_currentAutoPanVelocity);
    }

    private void ScrollWithVelocity(in Vector velocity)
    {
        var s = Scroller;
        var off = s.Offset;

        off += velocity;

        s.Offset = off;
    }

    private static bool IsStationary(Vector v) =>
        MathUtilities.IsZero(v.X) && MathUtilities.IsZero(v.Y);

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
    private bool _isDragItemFocused;
    private bool _isDragItemSelected;
    private bool _isInDrag = false;
    private bool _isInReorder = false;
    private IDisposable _dragItemOpacitySub;
    private bool _processReorder;
    private Point? _initialPoint;
    private double _cxDrag = double.NaN;
    private double _cyDrag = double.NaN;
    private Control _parent;
    private bool _isDragWithinTabStrip;
    // True if there is a drag drop operation started by this listview
    private bool _isDraggingOverSelf;

    private LiveReorderHelper _liveReorderHelper;
    //private LiveReorderIndices _liveReorderIndices = new LiveReorderIndices(-1,-1,-1);
    private DispatcherTimer _liveReorderTimer;
    //private readonly MovedItems _movedItems = new MovedItems();
    //private List<Rect> _cachedContainerBounds;
    private Point? _lastDragOverPoint;

    // For 12.0/v3 - Avalonia has decided to make the decision that the lowest common denominator
    // in the platform backends decides the entire public API. As part of this, DoDragDrop now
    // requires the initial pressed args, so we have to store them away so we can start DragDrop.
    // I tried to object, and failed (https://github.com/AvaloniaUI/Avalonia/pull/20988)
    // And you guessed it, freakin' Wayland
    private PointerPressedEventArgs _initArgs;
    
    private DispatcherTimer _scrollTimer;
    private Vector _currentAutoPanVelocity;

    private const string s_tpScrollViewer = "ScrollViewer";

    private const string s_pcReorder = ":reorder";
    private const string s_pcLeftShort = ":leftShort";
    private const string s_pcRightShort = ":rightShort";
}
