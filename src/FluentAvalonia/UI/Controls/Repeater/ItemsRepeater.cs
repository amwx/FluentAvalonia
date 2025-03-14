using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

public partial class ItemsRepeater : Panel
{
    public ItemsRepeater()
    {
        _viewportManager = new ViewportManager(this);
        _viewManager = new ViewManager(this);
        _transitionManager = new TransitionManager(this);

        SetCurrentValue(LayoutProperty, new StackLayout());

        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
        SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.Once);
        XYFocus.SetNavigationModes(this, XYFocusNavigationModes.Enabled);

        Loaded += OnRepeaterLoaded;
        Unloaded += OnRepeaterUnloaded;
        LayoutUpdated += OnLayoutUpdated;

        // TODO: Why is this here...
        //var layout = Layout as VirtualizingLayout;
        //OnLayoutChanged(null, layout);
        AddHandler(RequestBringIntoViewEvent, OnBringIntoViewRequested);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ItemsRepeaterAutomationPeer(this);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_isLayoutInProgress)
            throw new Exception("Reentrancy detected during layout");

        if (IsProcessingCollectionChange)
            throw new Exception("Cannot run layout in the middle of a collection change");

        var layout = GetEffectiveLayout();

        if (layout != null)
        {
            var stackLayout = layout as StackLayout;

            if (stackLayout != null && ++_stackLayoutMeasureCounter >= _maxStackLayoutIterations)
            {
#if DEBUG && REPEATER_TRACE
                //Log.Debug("MeasureOverride shortcut - {Counter}", _stackLayoutMeasureCounter);
#endif
                // Shortcut the apparent layout cycle by returning the previous desired size.
                // This can occur when children have variable sizes that prevent the ItemsPresenter's desired size from settling.
                Rect layoutExtent = _viewportManager.LayoutExtent;
                Size desiredSize = new Size(layoutExtent.Width - layoutExtent.X,
                    layoutExtent.Height - layoutExtent.Y );
                return desiredSize;
            }
        }

        _viewportManager.OnOwnerMeasuring();

        try
        {
            _isLayoutInProgress = true;
            _viewManager.PrunePinnedElements();
            Rect extent = default;
            Size desiredSize = default;

            if (layout != null)
            {
                var layoutContext = GetLayoutContext();

                // Expensive operation, do it only in debug builds.
#if DEBUG
                //(layoutContext as VirtualizingLayoutContext)?.Indent(Indent);
#endif

                // Checking if we have an data template and it is empty
                if (_isItemTemplateEmpty)
                {
                    // Has no content, so we will draw nothing and request zero space
                    extent = new Rect(_layoutOrigin.X, _layoutOrigin.Y, 0, 0);
                }
                else
                {
                    desiredSize = layout.Measure(layoutContext, availableSize);
                    extent = new Rect(_layoutOrigin.X, _layoutOrigin.Y, desiredSize.Width, desiredSize.Height);
                }

                // Clear auto recycle candidate elements that have not been kept alive by layout - i.e layout did not
                // call GetElementAt(index).
                var children = Children;
                for (int i = 0; i < children.Count; i++)
                {
                    var element = children[i];
                    var virtInfo = GetVirtualizationInfo(element);

                    if (virtInfo.Owner == VirtualizationInfo.ElementOwner.Layout &&
                        virtInfo.AutoRecycleCandidate &&
                        !virtInfo.KeepAlive)
                    {
#if DEBUG && REPEATER_TRACE
                        //Log.Debug("AutoClear - {Index}", virtInfo.Index);
#endif
                        ClearElementImpl(element);
                    }
                }
            }

            _viewportManager.SetLayoutExtent(extent);
            _lastAvailableSize = availableSize;

            return desiredSize;
        }
        finally
        {
            _isLayoutInProgress = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_isLayoutInProgress)
            throw new Exception("Reentrancy detected during layout");

        if (IsProcessingCollectionChange)
            throw new Exception("Cannot run layout in the middle of a collection change");

        try
        {
            _isLayoutInProgress = true;
            Size arrangeSize = default;

            if (GetEffectiveLayout() is Layout layout)
            {
                arrangeSize = layout.Arrange(GetLayoutContext(), finalSize);
            }

            // The view manager might clear elements during this call.
            // That's why we call it before arranging cleared elements
            // off screen.
            _viewManager.OnOwnerArranged();

            var children = Children;
            for (int i = 0; i < children.Count; i++)
            {
                var element = children[i];
                var vi = GetVirtualizationInfo(element);
                vi.KeepAlive = false;

                if (vi.Owner == VirtualizationInfo.ElementOwner.ElementFactory ||
                    vi.Owner == VirtualizationInfo.ElementOwner.PinnedPool)
                {
                    // Toss it away. And arrange it with size 0 so that XYFocus won't use it.
                    element.Arrange(new Rect(
                        ClearedElementsArrangePosition.X - element.DesiredSize.Width,
                        ClearedElementsArrangePosition.Y - element.DesiredSize.Height,
                        0, 0));
                }
                else
                {
                    var newBounds = element.Bounds;
                    if (vi.ArrangeBounds != ItemsRepeater.InvalidRect &&
                        newBounds != vi.ArrangeBounds)
                    {
                        _transitionManager.OnElementBoundsChanged(element, vi.ArrangeBounds, newBounds);
                    }

                    vi.ArrangeBounds = newBounds;
                }
            }

            _viewportManager.OnOwnerArranged();
            _transitionManager.OnOwnerArranged();

            return arrangeSize;
        }
        finally
        {
            _isLayoutInProgress = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        base.OnPropertyChanged(args);
        var property = args.Property;

        if (property == ItemsSourceProperty)
        {
            if (args.NewValue != args.OldValue)
            {
                var newValue = args.NewValue;
                var newDataSource = newValue as FAItemsSourceView;
                if (newValue != null && newDataSource == null)
                {
                    newDataSource = new FAItemsSourceView(newValue as IEnumerable);
                }

                OnDataSourcePropertyChanged(_itemsSourceView, newDataSource);
            }
        }
        else if (property == ItemTemplateProperty)
        {
            OnItemTemplateChanged(args.OldValue as IDataTemplate,
                args.NewValue as IDataTemplate);
        }
        else if (property == LayoutProperty)
        {
            OnLayoutChanged(args.GetOldValue<Layout>(), args.GetNewValue<Layout>());
        }
        else if (property == ItemTransitionProviderProperty)
        {
            OnTransitionProviderChanged(args.GetOldValue<ItemCollectionTransitionProvider>(), 
                args.GetNewValue<ItemCollectionTransitionProvider>());
        }
        else if (property == HorizontalCacheLengthProperty)
        {
            _viewportManager.HorizontalCacheLength = args.GetNewValue<double>();
        }
        else if (property == VerticalCacheLengthProperty)
        {
            _viewportManager.VerticalCacheLength = args.GetNewValue<double>();
        }
    }

    public int GetElementIndex(Control element) =>
        GetElementIndexImpl(element);

    public Control TryGetElement(int index) =>
        GetElementFromIndexImpl(index);

    public void PinElement(Control element) =>
        _viewManager.UpdatePin(element, true);

    public void UnpinElement(Control element) =>
        _viewManager.UpdatePin(element, false);

    public Control GetOrCreateElement(int index) =>
        GetOrCreateElementImpl(index);

    // Change from WinUI, to avoid an extra property read in ViewportManager,
    // we pass the VirtualizationInfo in here too since its called from 
    // ViewManager.GetElementFromElementFactory where we already have
    // a ref to the VirtualizationInfo
    internal void OnElementPrepared(Control element, int index, VirtualizationInfo vInfo)
    {
        _viewportManager.OnElementPrepared(element, vInfo);
        if (ElementPrepared != null)
        {
            if (_elementPreparedArgs == null)
            {
                _elementPreparedArgs = new ItemsRepeaterElementPreparedEventArgs(element, index);
            }
            else
            {
                _elementPreparedArgs.Update(element, index);
            }

            ElementPrepared.Invoke(this, _elementPreparedArgs);
        }
    }

    internal void OnElementClearing(Control element)
    {
        if (ElementClearing != null)
        {
            if (_elementClearingArgs == null)
            {
                _elementClearingArgs = new ItemsRepeaterElementClearingEventArgs(element);
            }
            else
            {
                _elementClearingArgs.Update(element);
            }

            ElementClearing.Invoke(this, _elementClearingArgs);
        }
    }

    internal void OnElementIndexChanged(Control element, int oldIndex, int newIndex)
    {
        if (ElementIndexChanged != null)
        {
            if (_elementIndexChangedArgs == null)
            {
                _elementIndexChangedArgs = new ItemsRepeaterElementIndexChangedEventArgs(element, oldIndex, newIndex);
            }
            else
            {
                _elementIndexChangedArgs.Update(element, oldIndex, newIndex);
            }

            ElementIndexChanged.Invoke(this, _elementIndexChangedArgs);
        }
    }

    internal Control GetElementImpl(int index, bool forceCreate, bool suppressAutoRecycle)
    {
        var element = _viewManager.GetElement(index, forceCreate, suppressAutoRecycle);
        return element;
    }

    internal void ClearElementImpl(Control element)
    {
        // Clearing an element due to a collection change
        // is more strict in that pinned elements will be forcibly
        // unpinned and sent back to the view generator.
        bool isClearedDueToCollectionChange =
            IsProcessingCollectionChange &&
            (_processingItemsSourceChange.Action == NotifyCollectionChangedAction.Remove ||
            _processingItemsSourceChange.Action == NotifyCollectionChangedAction.Replace ||
            _processingItemsSourceChange.Action == NotifyCollectionChangedAction.Reset);

        _viewManager.ClearElement(element, isClearedDueToCollectionChange);
        _viewportManager.OnElementCleared(element);
    }

    private int GetElementIndexImpl(Control element)
    {
        // Verify that element is actually a child of this ItemsRepeater
        var parent = element.GetVisualParent();
        if (parent == this)
        {
            var virtInfo = TryGetVirtualizationInfo(element);
            return _viewManager.GetElementIndex(virtInfo);
        }
        return -1;
    }

    private Control GetElementFromIndexImpl(int index)
    {
        Control result = null;

        var children = Children;
        for (int i = 0; i < children.Count && (result == null); ++i)
        {
            var element = children[i];
            var virtInfo = TryGetVirtualizationInfo(element);
            if (virtInfo != null && virtInfo.IsRealized && virtInfo.Index == index)
            {
                result = element;
            }
        }

        return result;
    }

    private Control GetOrCreateElementImpl(int index)
    {
        if (ItemsSourceView == null)
            throw new Exception("ItemsSource doesn't have a value");

        if (index >= 0 && index >= ItemsSourceView.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_isLayoutInProgress)
            throw new Exception("GetOrCreateElement invocation is not allowed during layout");

        var element = GetElementFromIndexImpl(index);
        bool isAnchorOutsideRealizedRange = element == null;

        if (isAnchorOutsideRealizedRange)
        {
            if (GetEffectiveLayout() == null)
                throw new Exception("Cannot make an anchor when there is no attached layout");

            element = GetLayoutContext().GetOrCreateElementAt(index);
            element.Measure(Size.Infinity);
        }

        _viewportManager.OnMakeAnchor(element, isAnchorOutsideRealizedRange);
        InvalidateMeasure();

        return element;
    }

    private int Indent()
    {
        // Debug thing...Ignore for now...
        return 4;
    }

    private IEnumerable<Control> GetChildrenInTabFocusOrder() =>
        CreateChildrenInTabFocusOrderIterable();

    private void OnBringIntoViewRequested(object sender, RequestBringIntoViewEventArgs args)
    {
        _viewportManager.OnBringIntoViewRequested(args);
    }

    private void OnRepeaterLoaded(object sender, RoutedEventArgs e)
    {
        // If we skipped an unload event, reset the scrollers now and invalidate measure so that we get a new
        // layout pass during which we will hookup new scrollers.
        if (_loadedCounter > _unloadedCounter)
        {
            InvalidateMeasure();
            _viewportManager.ResetScrollers();
        }
        ++_loadedCounter;
    }

    private void OnRepeaterUnloaded(object sender, RoutedEventArgs e)
    {
        _stackLayoutMeasureCounter = 0;
        ++_unloadedCounter;

        // Only reset the scrollers if this unload event is in-sync.
        if (_unloadedCounter == _loadedCounter)
        {
            _viewportManager.ResetScrollers();
        }
    }

    private void OnLayoutUpdated(object sender, EventArgs e)
    { 
        // Now that the layout has settled, reset the measure counter to detect the next potential StackLayout layout cycle.
        _stackLayoutMeasureCounter = 0;

        EnsureDefaultLayoutState();
    }

    private void OnDataSourcePropertyChanged(FAItemsSourceView oldValue, FAItemsSourceView newValue)
    {
        if (_isLayoutInProgress)
            throw new Exception();

        EnsureDefaultLayoutState();

        if (_itemsSourceView != null)
            _itemsSourceView.CollectionChanged -= OnItemsSourceViewChanged;

        _itemsSourceView = newValue;

        if (newValue != null)
            _itemsSourceView.CollectionChanged += OnItemsSourceViewChanged;

        if (GetEffectiveLayout() is Layout l)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            try
            {
                _processingItemsSourceChange = args;
                if (l is VirtualizingLayout vl)
                {
                    vl.OnItemsChangedCore(GetLayoutContext(), newValue, args);
                }
                else if (Layout is NonVirtualizingLayout nvl)
                {
                    // Walk through all the elements and make sure they are cleared for
                    // non-virtualizing layouts.
                    foreach (var item in Children)
                    {
                        if (GetVirtualizationInfo(item).IsRealized)
                            ClearElementImpl(item);
                    }

                    Children.Clear();
                }

                InvalidateMeasure();
            }
            finally
            {
                _processingItemsSourceChange = null;
            }
        }
    }

    // WinUI has type of IElementFactory here, but we need to use IDataTemplate
    // In WinUI, DataTemplate inherits from IElementFactory so that covers everything
    // For us, we need to work with what Avalonia gives us, easiest is IDataTemplate
    private void OnItemTemplateChanged(IDataTemplate oldValue, IDataTemplate newValue)
    {
        if (_isLayoutInProgress && oldValue != null)
            throw new InvalidOperationException("ItemTemplate cannot be changed during layout.");

        EnsureDefaultLayoutState();

        // Since the ItemTemplate has changed, we need to re-evaluate all the items that
        // have already been created and are now in the tree. The easiest way to do that
        // would be to do a reset.. Note that this has to be done before we change the template
        // so that the cleared elements go back into the old template.
        if (GetEffectiveLayout() is Layout layout)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            try
            {
                _processingItemsSourceChange = args;

                if (layout is VirtualizingLayout vl)
                {
                    vl.OnItemsChangedCore(GetLayoutContext(), newValue, args);
                }
                else if (layout is NonVirtualizingLayout nvl)
                {
                    // Walk through all the elements and make sure they are cleared for
                    // non-virtualizing layouts.
                    foreach (var child in Children)
                    {
                        if (GetVirtualizationInfo(child).IsRealized)
                        {
                            ClearElementImpl(child);
                        }
                    }
                }
            }
            finally
            {
                _processingItemsSourceChange = null;
            }
        }

        _isItemTemplateEmpty = false;
        _itemTemplateWrapper = newValue as IElementFactory;
        if (_itemTemplateWrapper == null)
        {
            // ItemTemplate set does not implement IElementFactoryShim. We also 
            // want to support DataTemplate and DataTemplateSelectors automagically.
            if (newValue is IDataTemplate template)
            {
                _itemTemplateWrapper = new ItemTemplateWrapper(template);
                //_isItemTemplateEmpty = template.Build(null) == null;
            }
            else if (newValue is DataTemplateSelector dts)
            {
                _itemTemplateWrapper = new ItemTemplateWrapper(dts);
            }
        }

        InvalidateMeasure();
    }

    private void OnLayoutChanged(Layout oldValue, Layout newValue)
    {
        bool isInitialSetup = !_wasLayoutChangedCalled;
        _wasLayoutChangedCalled = true;

        if (_isLayoutInProgress)
            throw new InvalidOperationException("Layout cannot be changed during layout.");

        _viewManager.OnLayoutChanging();
        _transitionManager.OnLayoutChanging();

        if (oldValue == null & !isInitialSetup)
        {
            oldValue = GetDefaultLayout();
        }
        if (newValue == null)
        {
            newValue = GetDefaultLayout();
        }    


        if (oldValue != null)
        {
            oldValue.UninitializeForContext(GetLayoutContext());
            newValue.MeasureInvalidated -= InvalidateMeasureForLayout;
            newValue.ArrangeInvalidated -= InvalidateArrangeForLayout;
            _stackLayoutMeasureCounter = 0;

            // Walk through all the elements and make sure they are cleared
            var children = Children;
            for (int i = 0; i < children.Count; ++i)
            {
                var element = children[i];
                if (GetVirtualizationInfo(element).IsRealized)
                {
                    ClearElementImpl(element);
                }
            }

            _layoutState = null;
        }

        if (newValue != null)
        {
            newValue.InitializeForContext(GetLayoutContext());
            newValue.MeasureInvalidated += InvalidateMeasureForLayout;
            newValue.ArrangeInvalidated += InvalidateArrangeForLayout;

            if (_ownsTransitionProvider)
            {
                _transitionManager.OnTransitionProviderChanged(newValue.CreateDefaultItemTransitionProvider());
            }
        }

        bool isVirtualizingLayout = newValue != null && newValue is VirtualizingLayout;
        _viewportManager.OnLayoutChanged(isVirtualizingLayout);
        InvalidateMeasure();
    }

    private void OnTransitionProviderChanged(ItemCollectionTransitionProvider _, ItemCollectionTransitionProvider newValue)
    {
        _ownsTransitionProvider = false;
        _transitionManager.OnTransitionProviderChanged(newValue);
    }

    private void OnItemsSourceViewChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (_isLayoutInProgress)
            throw new InvalidOperationException("Changes in data source are not allowed during layout.");

        if (IsProcessingCollectionChange)
            throw new InvalidOperationException("Changes in the data source are not allowed during another change in the data source.");

        try
        {
            _processingItemsSourceChange = args;

            _transitionManager.OnItemsSourceChanged(sender, args);
            _viewManager.OnItemsSourceChanged(sender, args);

            if (GetEffectiveLayout() is Layout layout)
            {
                if (layout is VirtualizingLayout vl)
                {
                    vl.OnItemsChangedCore(GetLayoutContext(), sender, args);
                }
                else
                {
                    InvalidateMeasure();
                }
            }
        }
        finally
        {
            _processingItemsSourceChange = null;
        }
    }

    private void InvalidateMeasureForLayout(Layout sender, EventArgs args)
    {
        InvalidateMeasure();
    }

    private void InvalidateArrangeForLayout(Layout sender, EventArgs args)
    {
        InvalidateArrange();
    }

    private void EnsureDefaultLayoutState()
    {
        if (!_wasLayoutChangedCalled)
        {
            // Initialize the cached layout to the default value
            // OnLayoutChanged has not been called yet for this ItemsRepeater.
            // This is the first call for the default VirtualizingLayout layout after the control's creation.
            var layout = GetEffectiveLayout() as VirtualizingLayout;
            OnLayoutChanged(null, layout);
        }
    }

    private VirtualizingLayoutContext GetLayoutContext()
    {
        _layoutContext ??= new RepeaterLayoutContext(this);

        return _layoutContext;
    }

    private IEnumerable<Control> CreateChildrenInTabFocusOrderIterable()
    {
        var children = Children;
        if (children.Count == 0)
        {
            return new ChildrenInTabFocusOrderIterable(this);
        }
        return null;
    }

    private Layout GetEffectiveLayout()
    {
        var l = Layout;
        if (l != null)
            return l;

        return GetDefaultLayout();
    }

    private Layout GetDefaultLayout()
    {
        // Default to StackLayout if the Layout property was not set.
        // We use thread_local here to get a unique instance per thread, since Layout objects
        // are not sharable across different xaml threads.
        //static thread_local winrt::Layout defaultLayout = winrt::StackLayout();
        //return defaultLayout;

        return new StackLayout();
    }


    // StackLayout measurements are shortcut when m_stackLayoutMeasureCounter reaches this value
    // to prevent a layout cycle exception.
    // The XAML Framework's iteration limit is 250, but that limit has been reached in practice
    // with this value as small as 61. It was never reached with 60. 
    internal const short _maxStackLayoutIterations = 60;
    internal static Point ClearedElementsArrangePosition = new Point(-10000, -10000);
    internal static Rect InvalidRect = new Rect(-1,-1,-1,-1);

    private readonly TransitionManager _transitionManager;
    private readonly ViewManager _viewManager;
    private readonly ViewportManager _viewportManager;

    private FAItemsSourceView _itemsSourceView;
    private IElementFactory _itemTemplateWrapper;
    private VirtualizingLayoutContext _layoutContext;
    private object _layoutState;
    private NotifyCollectionChangedEventArgs _processingItemsSourceChange;
    
    private Size _lastAvailableSize;
    private bool _isLayoutInProgress = false;
    // The value of _layoutOrigin is expected to be set by the layout
    // when it gets measured. It should not be used outside of measure.
    private Point _layoutOrigin;

    // Cached Event args to avoid creation cost every time
    private ItemsRepeaterElementPreparedEventArgs _elementPreparedArgs;
    private ItemsRepeaterElementClearingEventArgs _elementClearingArgs;
    private ItemsRepeaterElementIndexChangedEventArgs _elementIndexChangedArgs;

    // Loaded events fire on the first tick after an element is put into the tree 
    // while unloaded is posted on the UI tree and may be processed out of sync with subsequent loaded
    // events. We keep these counters to detect out-of-sync unloaded events and take action to rectify.
    private int _loadedCounter;
    private int _unloadedCounter;

    // Used to avoid layout cycles with StackLayout layouts where variable sized children prevent
    // the ItemsRepeater's layout to settle.
    private byte _stackLayoutMeasureCounter = 0;

    // Bug in framework's reference tracking causes crash during
    // UIAffinityQueue cleanup. To avoid that bug, take a strong ref
    private IElementFactory _itemTemplate;

    // Bug where DataTemplate with no content causes a crash.
    // See: https://github.com/microsoft/microsoft-ui-xaml/issues/776
    // Solution: Have flag that is only true when DataTemplate exists but it is empty.
    private bool _isItemTemplateEmpty = false;

    // If no ItemCollectionTransitionProvider is explicitly provided, we'll retrieve a default one
    // from the Layout object. In that case, we'll want to know that we own that object and can
    // overwrite it if the Layout object changes.
    private bool _ownsTransitionProvider = true;

    // Tracks whether OnLayoutChanged has already been called or not so that
    // EnsureDefaultLayoutState does not trigger a second call after the control's creation.
    private bool _wasLayoutChangedCalled;
}

// I think this is something special for WinRT/C++, we'll just use
// IElementFactory directly
//public interface IElementFactoryShim : IElementFactory
//{

//}
