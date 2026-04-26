using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

// Note: this source should come from ViewportManagerWithPlatformFeatures.cpp

internal class ViewportManager
{
    public ViewportManager(ItemsRepeater owner)
    {
        _owner = owner;
    }

    public double HorizontalCacheLength
    {
        get => _maximumHorizontalCacheLength;
        set
        {
            ValidateCacheLength(value);
            _maximumHorizontalCacheLength = value;
            ResetCacheBuffer();
        }
    }

    public double VerticalCacheLength
    {
        get => _maximumVerticalCacheLength;
        set
        {
            ValidateCacheLength(value);
            _maximumVerticalCacheLength = value;
            ResetCacheBuffer();
        }
    }

    public Control SuggestedAnchor
    {
        get
        {
            // The element generated during the ItemsRepeater.MakeAnchor call has precedence over the next tick.
            Control suggestedAnchor = _makeAnchorElement;
            Control owner = _owner;

            if (suggestedAnchor == null)
            {
                var anchorElement = _scroller?.CurrentAnchor;

                if (anchorElement != null)
                {
                    // We can't simply return anchorElement because, in case of nested Repeaters, it may not
                    // be a direct child of ours, or even an indirect child. We need to walk up the tree starting
                    // from anchorElement to figure out what child of ours (if any) to use as the suggested element.
                    var child = anchorElement;
                    var parent = child.GetVisualParent();
                    while (parent != null)
                    {
                        if (parent == owner)
                        {
                            suggestedAnchor = child;
                            break;
                        }

                        child = (Control)parent;
                        parent = parent.GetVisualParent();
                    }
                }
            }

            return suggestedAnchor;
        }
    }

    public Rect LayoutExtent => _layoutExtent;

    public Control MadeAnchor => _makeAnchorElement;

    private bool HasScroller => _scroller != null;
        
    public Rect GetLayoutVisibleWindowDiscardAnchor()
    {
        var visibleWindow = _visibleWindow;

        if (HasScroller)
        {
            visibleWindow = visibleWindow.WithX(
                visibleWindow.X + _layoutExtent.X + _expectedViewportShift.X + _unshiftableShift.X)
                .WithY(
                visibleWindow.Y + _layoutExtent.Y + _expectedViewportShift.Y + _unshiftableShift.Y);
        }

        return visibleWindow;
    }

    public Rect GetLayoutVisibleWindow()
    {
        var visibleWindow = _visibleWindow;

        if (_makeAnchorElement != null && _isAnchorOutsideRealizedRange)
        {
            // The anchor is not necessarily laid out yet. Its position should default
            // to zero and the layout origin is expected to change once layout is done.
            // Until then, we need a window that's going to protect the anchor from
            // getting recycled.

            // Also, we only want to mess with the realization rect iff the anchor is not inside it.
            // If we fiddle with an anchor that is already inside the realization rect,
            // shifting the realization rect results in repeater, layout and scroller thinking that it needs to act upon StartBringIntoView.
            // We do NOT want that!

            visibleWindow = new Rect(default, visibleWindow.Size);
        }
        else if (HasScroller)
        {
            visibleWindow = visibleWindow.WithX(
                visibleWindow.X + _layoutExtent.X + _expectedViewportShift.X + _unshiftableShift.X)
                .WithY(
                visibleWindow.Y + _layoutExtent.Y + _expectedViewportShift.Y + _unshiftableShift.Y);
        }

        return visibleWindow;
    }

    public Rect GetLayoutRealizationWindow()
    {
        var realizationWindow = GetLayoutVisibleWindow();

        if (HasScroller)
        {
            realizationWindow = new Rect(
                realizationWindow.X - _horizontalCacheBufferPerSide,
                realizationWindow.Y - _verticalCacheBufferPerSide,
                realizationWindow.Width + _horizontalCacheBufferPerSide * 2,
                realizationWindow.Height + _verticalCacheBufferPerSide * 2);
        }

        return realizationWindow;
    }

    public void SetLayoutExtent(Rect extent)
    {
        _expectedViewportShift = new Point(
            _expectedViewportShift.X + _layoutExtent.X - extent.X,
            _expectedViewportShift.Y + _layoutExtent.Y - extent.Y);

        // We tolerate viewport imprecisions up to 1 pixel to avoid invaliding layout too much.
        if (Math.Abs(_expectedViewportShift.X) > 1 || Math.Abs(_expectedViewportShift.Y) > 1)
        {
#if DEBUG && REPEATER_TRACE
            Log.Debug("{Layout}: Expecting viewport shift of {shift}",
                GetLayoutId(), _expectedViewportShift);
#endif

            // There are cases where we might be expecting a shift but not get it. We will
            // be waiting for the effective viewport event but if the scroll viewer is not able
            // to perform the shift (perhaps because it cannot scroll in negative offset),
            // then we will end up not realizing elements in the visible 
            // window. To avoid this, we register to layout updated for this layout pass. If we 
            // get an effective viewport, we know we have a new viewport and we unregister from
            // layout updated. If we get the layout updated handler, then we know that the 
            // scroller was unable to perform the shift and we invalidate measure and unregister
            // from the layout updated event.
            if (!_layoutUpdatedRevoker)
            {
                _layoutUpdatedRevoker = true;
                _owner.LayoutUpdated += OnLayoutUpdated;
            }
        }

        _layoutExtent = extent;
        _pendingViewportShift = _expectedViewportShift;

        // We just finished a measure pass and have a new extent.
        // Let's make sure the scrollers will run its arrange so that they track the anchor.
        if (_scroller != null)
        {
            (_scroller as Control).InvalidateArrange();
        }
    }

    public void OnLayoutChanged(bool isVirtualizing)
    {
        _managingViewportDisabled = !isVirtualizing;

        _layoutExtent = default;
        _expectedViewportShift = default;
        _pendingViewportShift = default;

        if (_managingViewportDisabled)
        {
            _owner.EffectiveViewportChanged -= OnEffectiveViewportChanged;
            _effectiveViewportChangedRevoker = false;
        }
        else if (!_effectiveViewportChangedRevoker)
        {
            _effectiveViewportChangedRevoker = true;
            _owner.EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        _unshiftableShift = default;
        ResetCacheBuffer();
    }

    public void OnElementPrepared(Control element, VirtualizationInfo vInfo)
    {
        // If we have an anchor element, we do not want the
        // scroll anchor provider to start anchoring some other element.
        vInfo.CanBeScrollAnchor = true;
        _scroller?.RegisterAnchorCandidate(element);
    }

    public void OnElementCleared(Control element)
    {
        // Unlike OnElementPrepared, we can't make the change and add the virt info
        // as the caller of this (ItemsRepeater.ClearElementImpl) doesn't have a ref
        // and that method has multiple refs which don't have a the virt info either
        ItemsRepeater.GetVirtualizationInfo(element).CanBeScrollAnchor = false;
        _scroller?.UnregisterAnchorCandidate(element);
    }

    public void OnOwnerMeasuring()
    {
        // This is because of a bug that causes effective viewport to not 
        // fire if you register during arrange.
        // Bug 17411076: EffectiveViewport: registering for effective viewport in arrange should invalidate viewport
        EnsureScroller();
    }

    public void OnOwnerArranged()
    {
        _expectedViewportShift = default;

        if (!_managingViewportDisabled)
        {
            // This is because of a bug that causes effective viewport to not 
            // fire if you register during arrange.
            // Bug 17411076: EffectiveViewport: registering for effective viewport in arrange should invalidate viewport
            // EnsureScroller();

            if (HasScroller)
            {
                double maximumHorizontalCacheBufferPerSide = _maximumHorizontalCacheLength * _visibleWindow.Width / 2;
                double maximumVerticalCacheBufferPerSide = _maximumVerticalCacheLength * _visibleWindow.Height / 2;

                bool continueBuildingCache =
                    _horizontalCacheBufferPerSide < maximumHorizontalCacheBufferPerSide ||
                    _verticalCacheBufferPerSide < maximumVerticalCacheBufferPerSide;

                if (continueBuildingCache)
                {
                    _horizontalCacheBufferPerSide += CacheBufferPerSideInflationPixelDelta;
                    _verticalCacheBufferPerSide += CacheBufferPerSideInflationPixelDelta;

                    _horizontalCacheBufferPerSide = Math.Min(_horizontalCacheBufferPerSide, maximumHorizontalCacheBufferPerSide);
                    _verticalCacheBufferPerSide = Math.Min(_verticalCacheBufferPerSide, maximumVerticalCacheBufferPerSide);

                    // Since we grow the cache buffer at the end of the arrange pass,
                    // we need to register work even if we just reached cache potential.
                    RegisterCacheBuildWork();
                }
            }
        }
    }

    private void OnLayoutUpdated(object sender, EventArgs e)
    {
        _layoutUpdatedRevoker = false;
        _owner.LayoutUpdated -= OnLayoutUpdated;

        if (_managingViewportDisabled)
            return;

        // We were expecting a viewport shift but we never got one and we are not going to in this
        // layout pass. We likely will never get this shift, so lets assume that we are never going to get it and
        // adjust our expected shift to track that. One case where this can happen is when there is no scrollviewer
        // that can scroll in the direction where the shift is expected.

        if (_pendingViewportShift.X != 0 && _pendingViewportShift.Y != 0)
        {
#if DEBUG && REPEATER_TRACE
            Log.Debug("{Layout}: Layout updated with pending shift {Shift} - invalidating measure",
                GetLayoutId(), _pendingViewportShift);
#endif

            _unshiftableShift = new Point(
                _unshiftableShift.X + _pendingViewportShift.X,
                _unshiftableShift.Y + _pendingViewportShift.Y);
            _pendingViewportShift = default;
            _expectedViewportShift = default;

            TryInvalidateMeasure();
        }
    }

    public void OnMakeAnchor(Control anchor, bool isAnchorOutsideRealizedRange)
    {
        _makeAnchorElement = anchor;
        _isAnchorOutsideRealizedRange = isAnchorOutsideRealizedRange;
    }

    public void OnBringIntoViewRequested(RequestBringIntoViewEventArgs args)
    {
        if (_managingViewportDisabled)
            return;

        // We do not animate bring-into-view operations where the anchor is disconnected because
        // it doesn't look good (the blank space is obvious because the layout can't keep track
        // of two realized ranges while the animation is going on).
        if (_isAnchorOutsideRealizedRange)
        {
            //args.AnimationDesired = false;
        }

        // During the time between a bring into view request and the element coming into view we do not
        // want the anchor provider to pick some anchor and jump to it. Instead we want to anchor on the
        // element that is being brought into view. We can do this by making just that element as a potential
        // anchor candidate and ensure no other element of this repeater is an anchor candidate.
        // Once the layout pass is done and we render the frame, the element will be in frame and we can
        // switch back to letting the anchor provider pick a suitable anchor.

        // get the targetChild - i.e the immediate child of this repeater that is being brought into view.
        // Note that the element being brought into view could be a descendant.
        var targetChild = GetImmediateChildOfRepeater((Control)args.TargetObject);

        // Make sure that only the target child can be the anchor during the bring into view operation.
        foreach (var child in _owner.Children)
        {
            var vInfo = ItemsRepeater.GetVirtualizationInfo(child);
            if (vInfo.CanBeScrollAnchor && child != targetChild)
            {
                // In WinUI, CanBeScrollAnchor is used to automatically set the scroll
                // anchor on the IScrollAnchorProvider - here we have to do that manually
                _scroller?.UnregisterAnchorCandidate(child);
                vInfo.CanBeScrollAnchor = false;
            }
        }

        // Register to rendering event to go back to how things were before where any child can be the anchor.
        _isBringIntoViewInProgress = true;
        if (!_renderingToken)
        {
            _renderingToken = true;
            // Note from Avalonia implementation
            // Register action to go back to how things were before where any child can be the anchor. Here,
            // WinUI uses CompositionTarget.Rendering but we don't currently have that, so post an action to
            // run *after* rendering has completed (priority needs to be lower than Render as Transformed
            // bounds must have been set in order for OnEffectiveViewportChanged to trigger).
            Dispatcher.UIThread.Post(OnCompositionTargetRendering, DispatcherPriority.Loaded);
            //CompositionTarget.Rendering += OnCompositionTargetRendering;
        }
    }

    private Control GetImmediateChildOfRepeater(Control descendant)
    {
        Control targetChild = descendant;
        Control parent = (Control)descendant.GetVisualParent();
        while (parent != null && parent != _owner)
        {
            targetChild = parent;
            parent = (Control)parent.GetVisualParent();
        }

        // This is what WinUI does, but this can be hit in Avalonia...
        // My repro: ItemsRepeater -> Control w/ Popup -> ListBox in Popup
        // Select item in the ListBox, we hit this
        // Looking at Avalonia's port of ItemsRepeater, they just return
        // null if parent is null, so that's what I'll do
        //if (parent == null)
        //{
        //    throw new InvalidOperationException("OnBringIntoViewRequested called with args.target element not under the ItemsRepeater that recieved the call");
        //}
        if (parent == null)
            return null;

        return targetChild;
    }

    private void OnCompositionTargetRendering()
    {
        _renderingToken = false;
        //CompositionTarget.Rendering -= OnCompositionTargetRendering;

        _isBringIntoViewInProgress = false;
        _makeAnchorElement = null;

        // Now that the item has been brought into view, we can let the anchor provider pick a new anchor.
        foreach (var child in _owner.Children)
        {
            // Change from WinUI - unfortunately need the property read here regardless since
            // we need to store CanBeScrollAnchor on the Virt Info
            // Fortunately this only happens after a BringIntoView request, which shouldn't
            // be a common case
            var info = ItemsRepeater.GetVirtualizationInfo(child);
            if (!info.CanBeScrollAnchor && info.IsRealized && info.IsHeldByLayout)
            {
                _scroller?.RegisterAnchorCandidate(child);
                info.CanBeScrollAnchor = true;
            }
        }
    }

    internal void ResetScrollers()
    {
        _scroller = null;
        _owner.EffectiveViewportChanged -= OnEffectiveViewportChanged;
        _effectiveViewportChangedRevoker = false;
        _ensuredScroller = false;
    }

    private void OnCacheBuildActionCompleted()
    {
        _cacheBuildAction = null;
        if (!_managingViewportDisabled)
            _owner.InvalidateMeasure();
    }

    private void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs args)
    {
#if DEBUG && REPEATER_TRACE
        Debug.Assert(!_managingViewportDisabled);
        Log.Debug("{Layout}: EffectiveViewportChanged event callback", GetLayoutId());
#endif 

        UpdateViewport(args.EffectiveViewport);

        _pendingViewportShift = default;
        _unshiftableShift = default;
        if (_visibleWindow == default)
        {
            // We got cleared
            _layoutExtent = default;
        }

        // We got a new viewport, we dont need to wait for layout updated anymore to 
        // see if our request for a pending shift was handled.
        if (_layoutUpdatedRevoker)
        {
            _layoutUpdatedRevoker = false;
            _owner.LayoutUpdated -= OnLayoutUpdated;
        }
    }

    private void EnsureScroller()
    {
        if (!_ensuredScroller)
        {
            ResetScrollers();

            _scroller = _owner.FindAncestorOfType<IScrollAnchorProvider>();

            if (!_managingViewportDisabled)
            {
                if (_scroller == null)
                {
                    // We usually update the viewport in the post arrange handler. 
                    // But, since we don't have a scroller, let's do it now.
                    UpdateViewport(default);
                }
                else
                {
                    _effectiveViewportChangedRevoker = true;
                    _owner.EffectiveViewportChanged += OnEffectiveViewportChanged;
                }
            }

            _ensuredScroller = true;
        }
    }

    private void UpdateViewport(Rect viewport)
    {
#if DEBUG && REPEATER_TRACE
        Debug.Assert(!_managingViewportDisabled);
        var previousVisibleWindow = _visibleWindow;
        Log.Debug("{Layout}: Effective Viewport: {Old}->{New}",
            GetLayoutId(),
            previousVisibleWindow, viewport);
#endif

        var currentVisibleWindow = viewport;

        if (-currentVisibleWindow.X <= ItemsRepeater.ClearedElementsArrangePosition.X &&
            -currentVisibleWindow.Y <= ItemsRepeater.ClearedElementsArrangePosition.Y)
        {
#if DEBUG && REPEATER_TRACE
            Log.Debug("{Layout}: Viewport is invalid. visible window cleared", GetLayoutId());
#endif
            // We got cleared.
            _visibleWindow = default;
        }
        else
        {
#if DEBUG && REPEATER_TRACE
            Log.Debug("{Layout}: Used viewport {Old} -> {New}",
                GetLayoutId(), previousVisibleWindow, currentVisibleWindow);
#endif
            _visibleWindow = currentVisibleWindow;
        }

        TryInvalidateMeasure();
    }

    private void ResetCacheBuffer()
    {
        _horizontalCacheBufferPerSide = 0;
        _verticalCacheBufferPerSide = 0;

        if (!_managingViewportDisabled)
        {
            // We need to start building the realization buffer again.
            RegisterCacheBuildWork();
        }
    }

    private void ValidateCacheLength(double cacheLength)
    {
        if (cacheLength < 0 || double.IsInfinity(cacheLength) || double.IsNaN(cacheLength))
            throw new Exception("The maximum cache length must be equal or superior to zero.");
    }

    private void RegisterCacheBuildWork()
    {
        if (_owner.Layout != null && _cacheBuildAction == null)
        {
            // We capture 'owner' (a strong refernce on ItemsRepeater) to make sure ItemsRepeater is still around
            // when the async action completes. By protecting ItemsRepeater, we also ensure that this instance
            // of ViewportManager (referenced by 'this' pointer) is valid because the lifetime of ItemsRepeater
            // and ViewportManager is the same (see ItemsRepeater::m_viewportManager).
            // We can't simply hold a strong reference on ViewportManager because it's not a COM object.
            _cacheBuildAction = () => Dispatcher.UIThread.Post(OnCacheBuildActionCompleted,
                DispatcherPriority.Background);

            _cacheBuildAction.Invoke();
        }
    }

    private void TryInvalidateMeasure()
    {
        // Don't invalidate measure if we have an invalid window.
        if (_visibleWindow != default)
        {
            // We invalidate measure instead of just invalidating arrange because
            // we don't invalidate measure in UpdateViewport if the view is changing to
            // avoid layout cycles.
#if DEBUG && REPEATER_TRACE
            Log.Debug("{Layout}: Invalidating measure due to viewport change", GetLayoutId());
#endif
            _owner.InvalidateMeasure();
        }
    }

    string GetLayoutId() => _owner?.Layout?.LayoutId ?? string.Empty;

    private ItemsRepeater _owner;
    private bool _ensuredScroller;
    private IScrollAnchorProvider _scroller;
    private Control _makeAnchorElement;
    private bool _isAnchorOutsideRealizedRange;

    private Action _cacheBuildAction;

    private Rect _visibleWindow;
    private Rect _layoutExtent;
    // This is the expected shift by the layout.
    private Point _expectedViewportShift;
    // This is what is pending and not been accounted for. 
    // Sometimes the scrolling surface cannot service a shift (for example
    // it is already at the top and cannot shift anymore.)
    private Point _pendingViewportShift;
    // Unshiftable shift amount that this view manager can
    // handle on its own to fake it to the layout as if the shift
    // actually happened. This can happen in cases where no scrollviewer
    // in the parent chain can scroll in the shift direction.
    private Point _unshiftableShift;

    private double _maximumHorizontalCacheLength = 2;
    private double _maximumVerticalCacheLength = 2;
    private double _horizontalCacheBufferPerSide;
    private double _verticalCacheBufferPerSide;

    private bool _isBringIntoViewInProgress = false;
    // For non-virtualizing layouts, we do not need to keep
    // updating viewports and invalidating measure often. So when
    // a non virtualizing layout is used, we stop doing all that work.
    private bool _managingViewportDisabled;

    private bool _layoutUpdatedRevoker;
    private bool _effectiveViewportChangedRevoker;
    private bool _renderingToken;

    // Pixel delta by which to inflate the cache buffer on each side.  Rather than fill the entire
    // cache buffer all at once, we chunk the work to make the UI thread more responsive.  We inflate
    // the cache buffer from 0 to a max value determined by the Maximum[Horizontal,Vertical]CacheLength
    // properties.
    private const double CacheBufferPerSideInflationPixelDelta = 40.0;
}
