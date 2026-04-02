using System.Diagnostics;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Internal;
using Path = Avalonia.Controls.Shapes.Path;

namespace FluentAvalonia.UI.Controls;

public partial class TabViewItem : SelectorItem
{
    public TabViewItem()
    {
        TabViewTemplateSettings = new TabViewItemTemplateSettings();

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }
    
    protected internal TabView ParentTabView
    {
        get
        {
            if (_parentTabView?.TryGetTarget(out var target) == true)
                return target;

            return null;
        }
        set => _parentTabView = new WeakReference<TabView>(value);
    }

    public Visual TabSeparator { get; private set; }

    internal bool IsBeingDragged { get; set; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            OnIsSelectedPropertyChanged(change);
        }
        else if (change.Property == HeaderProperty)
        {
            OnHeaderPropertyChanged(change);
        }
        else if (change.Property == IconSourceProperty)
        {
            OnIconSourcePropertyChanged(change);
        }
        else if (change.Property == IsClosableProperty)
        {
            OnIsClosablePropertyChanged(change);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _tabDragRevoker?.Dispose();
        _selectedBackgroundPath?.SizeChanged -= OnSelectedBackgroundPathSizeChanged;
        _closeButton?.Click -= OnCloseButtonClick;

        base.OnApplyTemplate(e);

        _selectedBackgroundPath = e.NameScope.Find<Path>(s_selectedBackgroundPathName);
        _selectedBackgroundPath?.SizeChanged += OnSelectedBackgroundPathSizeChanged;

        TabSeparator = e.NameScope.Find<Visual>(s_tpTabSeparator);

        _headerContentPresenter = e.NameScope.Find<ContentPresenter>(s_tpContentPresenter);

        var tabView = Parent as TabView ?? this.FindAncestorOfType<TabView>();

        _closeButton = e.NameScope.Get<Button>(s_tpCloseButton);

        if (string.IsNullOrEmpty(AutomationProperties.GetName(_closeButton)))
        {
            // TODO: I need to remember how I made my json file and update it to include this
            //var name = FALocalizationHelper.Instance.GetLocalizedStringResource(s_TabViewCloseButtonName);
            //AutomationProperties.SetName(_closeButton, name);
        }

        if (tabView != null)
        {
            ToolTip.SetTip(_closeButton, tabView.GetTabCloseButtonTooltipText());
        }

        _closeButton?.Click += OnCloseButtonClick;

        OnHeaderChanged();
        OnIconSourceChanged();

        if (tabView != null)
        {
            // ignore shadow

            // GH #260 - Using strong events here leaves a ref to this TVI from the TabView if its removed
            //           leading to a memory leak - so we need to use WeakEvents here to stop that
            //           Not 100% sure this is done correctly (there's no docs for this as I think this is
            //           meant to be an Avalonia internal specific thing), but at least according to VS's memory
            //           snapshot, no TVIs remained after removing & forcing a GC.Collect()
            _startingDragSub = new TargetWeakEventSubscriber<TabView, TabViewTabDragStartingEventArgs>(
                tabView, static (target, _, _, e) =>
                {
                    e.Tab?.OnTabDragStarting(target, e);
                });

            TabView.TabDragStartingWeakEvent.Subscribe(tabView, _startingDragSub);

            _completedDragSub = new TargetWeakEventSubscriber<TabView, TabViewTabDragCompletedEventArgs>(
                tabView, static (target, _, _, e) =>
                {
                    e.Tab?.OnTabDragCompleted(target, e);
                });

            TabView.TabDragCompletedWeakEvent.Subscribe(tabView, _completedDragSub);

            _tabDragRevoker = new FACompositeDisposable(
                new FADisposable(() => TabView.TabDragStartingWeakEvent.Unsubscribe(tabView, _startingDragSub)),
                new FADisposable(() => TabView.TabDragCompletedWeakEvent.Unsubscribe(tabView, _completedDragSub)));

            // Add this to fix a bug that's clearly in WinUI, adding a new TabViewItem doesn't check
            // the CloseButtonOverlay mode, thus new tabs ALWAYS initialize with 'Auto' even if the 
            // TabView's CloseButtonOverlayMode is not Auto
            _closeButtonOverlayMode = tabView.CloseButtonOverlayMode;
        }

        UpdateCloseButton();
        UpdateWidthModeVisualState();
        UpdateTabGeometry();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var pointer = e.Pointer;
        var devType = e.Pointer.Type;
        var point = e.GetCurrentPoint(this);

        if (devType == PointerType.Mouse || devType == PointerType.Pen)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                _lastPointerPressedPosition = point.Position;

                BeginCheckingForDrag(pointer.Id);

                var mod = TopLevel.GetTopLevel(this).PlatformSettings.HotkeyConfiguration.CommandModifiers;
                bool ctrlDown = (e.KeyModifiers & mod) == mod;

                if (ctrlDown)
                {
                    IsSelected = true;

                    // Return here so the base class will not pick it up, but let it remain unhandled so someone else could handle it.
                    return;
                }
            }
        }
        else if (devType == PointerType.Touch)
        {
            _lastPointerPressedPosition = point.Position;
            BeginCheckingForDrag(pointer.Id);
        }

        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed)
        {
            // Pointer capture is implicit in Avalonia
            _hasPointerCapture = true;
            _isMiddlePointerButtonPressed = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (ShouldStartDrag(e))
        {
            UpdateDragDropVisualState(true);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var pointer = e.Pointer;

        StopCheckingForDrag(e.Pointer.Id);
        UpdateDragDropVisualState(false);

        if (_hasPointerCapture)
        {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonReleased)
            {
                bool wasPressed = _isMiddlePointerButtonPressed;
                _isMiddlePointerButtonPressed = false;
                // Pointer capture release is implicit

                if (wasPressed)
                {
                    if (IsClosable)
                    {
                        RequestClose();
                    }
                }
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        _isPointerOver = true;

        if (_hasPointerCapture)
        {
            _isMiddlePointerButtonPressed = true;
        }

        UpdateCloseButton();
        HideLeftAdjacentTabSeparator();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        _isPointerOver = false;
        _isMiddlePointerButtonPressed = false;

        UpdateCloseButton();
        RestoreLeftAdjacentTabSeparatorVisibility();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        StopCheckingForDrag(e.Pointer.Id);

        if (_hasPointerCapture)
        {
            _hasPointerCapture = false;
            _isMiddlePointerButtonPressed = false;
        }
                
        RestoreLeftAdjacentTabSeparatorVisibility();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new TabViewItemAutomationPeer(this);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && (e.Key == Key.Left || e.Key == Key.Right))
        {
            // Alt+Shift+Arrow reorders tabs, so we don't want to handle that combination.
            // ListView also handles Alt+Arrow  (no Shift) by just doing regular XY focus,
            // same as how it handles Arrow without any modifier keys, so in that case
            // we do want to handle things so we get the improved keyboarding experience.
            bool isAltDown = (e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
            bool isShiftDown = (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;

            if (!isAltDown || !isShiftDown)
            {
                bool moveForward = FlowDirection == FlowDirection.LeftToRight && e.Key == Key.Right ||
                    FlowDirection == FlowDirection.RightToLeft && e.Key == Key.Left;

                e.Handled = ParentTabView?.MoveFocus(moveForward) ?? false;
            }
        }

        if (!e.Handled)
            base.OnKeyDown(e);
    }

    private bool IsOutsideDragRectangle(Point testPoint, Point dragRectangleCenter)
    {
        var dx = double.Abs(testPoint.X - dragRectangleCenter.X);
        var dy = double.Abs(testPoint.Y - dragRectangleCenter.Y);

        FAUISettings.GetSystemDragSize(TopLevel.GetTopLevel(this).RenderScaling, out var maxDx, out var maxDy);

        maxDx *= 2; //c_tabViewItemMouseDragThresholdMultiplier;
        maxDy *= 2; //c_tabViewItemMouseDragThresholdMultiplier;

        return (dx > maxDx || dy > maxDy);
    }

    private bool ShouldStartDrag(PointerEventArgs args)
    {
        return _isCheckingForDrag &&
            IsOutsideDragRectangle(args.GetCurrentPoint(this).Position, _lastPointerPressedPosition) &&
            _dragPointerId == args.Pointer.Id;
    }

    private void BeginCheckingForDrag(int pointerId)
    {
        _dragPointerId = pointerId;
        _isCheckingForDrag = true;
    }

    private void StopCheckingForDrag(int pointerId)
    {
        if (_isCheckingForDrag && _dragPointerId == pointerId)
        {
            _dragPointerId = 0;
            _isCheckingForDrag = false;
        }
    }

    private void UpdateTabGeometry()
    {
        if (_location == TabViewTabStripLocation.Left || _location == TabViewTabStripLocation.Right)
            return;

        bool isTop = _location == TabViewTabStripLocation.Top;
        var height = Bounds.Height;
        var popupRadius = this.TryFindResource(c_overlayCornerRadiusKey, out var value) ? (CornerRadius)value : default;
        var leftCorner = popupRadius.TopLeft;
        var rightCorner = popupRadius.TopRight;

        const string data = "F1 M0,{0}  a 4,4 0 0 0 4,-4  L 4,{1}  a {2},{3} 0 0 1 {4},-{5}  l {6},0  a {7},{8} 0 0 1 {9},{10}  l 0,{11}  a 4,4 0 0 0 4,4 Z";

        var builder = StringBuilderCache.Acquire(data.Length * 2);
        // WinUI 6644
        builder.AppendFormat(CultureInfo.InvariantCulture,
            data,
            height,
            leftCorner, leftCorner, leftCorner, leftCorner, leftCorner,
            Bounds.Width - (leftCorner + rightCorner),
            rightCorner, rightCorner, rightCorner, rightCorner,
            height - (4 + rightCorner));

        var geom = StreamGeometry.Parse(StringBuilderCache.GetStringAndRelease(builder));

        if (!isTop)
        {
            geom.Transform = new RotateTransform(180, geom.Bounds.Width * 0.5, geom.Bounds.Height * 0.5);
        }

        TabViewTemplateSettings.TabGeometry = geom;
    }

    private void OnIsSelectedPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // Not sure what Avalonia equivalent of this is
        //if (const auto peer = winrt::FrameworkElementAutomationPeer::CreatePeerForElement(*this))
        //{
        //    peer.RaiseAutomationEvent(winrt::AutomationEvents::SelectionItemPatternOnElementSelected);
        //}

        if (change.GetNewValue<bool>())
        {
            SetValue(ZIndexProperty, 20);

            StartBringTabIntoView();
        }
        else
        {
            SetValue(ZIndexProperty, 0);
        }

        UpdateWidthModeVisualState();
        UpdateCloseButton();
    }

    private void OnTabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
    {
        _isBeingDragged = true;
    }

    private void OnTabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
    {
        _isBeingDragged = false;

        StopCheckingForDrag(_dragPointerId);
        UpdateDragDropVisualState(false);
    }

    internal void OnCloseButtonOverlayModeChanged(TabViewCloseButtonOverlayMode mode)
    {
        _closeButtonOverlayMode = mode;
        UpdateCloseButton();
    }

    internal void OnTabViewWidthModeChanged(TabViewWidthMode mode)
    {
        _tabViewWidthMode = mode;
        UpdateWidthModeVisualState();
    }

    internal void HandleTabStripLocationChanged(TabViewTabStripLocation newLocation)
    {
        _location = newLocation;
        PseudoClasses.Set(TabView.s_pcTop, newLocation == TabViewTabStripLocation.Top);
        PseudoClasses.Set(TabView.s_pcBottom, newLocation == TabViewTabStripLocation.Bottom);
        PseudoClasses.Set(TabView.s_pcRight, newLocation == TabViewTabStripLocation.Right);
        PseudoClasses.Set(TabView.s_pcLeft, newLocation == TabViewTabStripLocation.Left);

        UpdateTabGeometry();
        if (_selectedBackgroundPath != null)
            OnSelectedBackgroundPathSizeChanged(null, null);
    }

    private void UpdateCloseButton()
    {
        // Visual States
        // CloseButtonCollapsed
        // CloseButtonVisible
        // => :closecollapsed

        bool isCollapsed;
        if (!IsClosable)
        {
            isCollapsed = true;
        }
        else
        {
            switch (_closeButtonOverlayMode)
            {
                case TabViewCloseButtonOverlayMode.OnPointerOver:
                    {    // If we only want to show the button on hover, we also show it when we are selected, otherwise hide it
                        if (IsSelected || _isPointerOver)
                        {
                            isCollapsed = false;
                        }
                        else
                        {
                            isCollapsed = true;
                        }
                        break;
                    }
                default:
                    {
                        // Default, use "Auto"
                        isCollapsed = false;
                        break;
                    }
            }
        }

        PseudoClasses.Set(s_pcCloseCollapsed, isCollapsed);
    }

    private void UpdateWidthModeVisualState()
    {
        // Visual States
        // Compact
        // StandardWidth
        // => :compact

        // Handling compact/non compact width mode
        PseudoClasses.Set(SharedPseudoclasses.s_pcCompact, !IsSelected && _tabViewWidthMode == TabViewWidthMode.Compact);
    }

    private void UpdateDragDropVisualState(bool isVisible)
    {
        PseudoClasses.Set(s_pcDragging, isVisible);
    }

    private void RequestClose()
    {
        var tabView = ParentTabView ?? this.FindAncestorOfType<TabView>();
        tabView.RequestCloseTab(this, false);
    }

    internal void RaiseRequestClose(TabViewTabCloseRequestedEventArgs args)
    {
        // This should only be called from TabView, to ensure that both this event and the TabView TabRequestedClose event are raised
        CloseRequested?.Invoke(this, args);
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        RequestClose();
    }

    private void OnIsClosablePropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateCloseButton();
    }

    private void OnHeaderPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        OnHeaderChanged();
    }

    private void OnHeaderChanged()
    {
        _headerContentPresenter?.Content = Header;

        //if (_firstTimeSettingToolTip)
        //{
        //    _firstTimeSettingToolTip = false;

        //    var tip = ToolTip.GetTip(this);
        //    if (tip == null)
        //    {
        //        // App author has not specified a tooltip; use our own

        //        // WinUI assigns an empty ToolTip here, but since tooltips work differently
        //        // we'll just mark this not null
        //        _toolTip = string.Empty;
        //    }
        //}
        
        //if (_toolTip != null)
        //{
        //    // Update tooltip text to new header text
        //    var headerContent = Header;

        //    if (headerContent is string s)
        //    {
        //        _toolTip = s;
        //        ToolTip.SetTip(this, _toolTip);
        //    }
        //}
    }

    private void HideLeftAdjacentTabSeparator()
    {
        if (ParentTabView is TabView tv)
        {
            var index = tv.IndexFromContainer(this);
            tv.SetTabSeparatorOpacity(index - 1, 0);
        }
    }

    private void RestoreLeftAdjacentTabSeparatorVisibility()
    {
        if (ParentTabView is TabView tv)
        {
            var index = tv.IndexFromContainer(this);
            tv.SetTabSeparatorOpacity(index - 1);
        }
    }

    private void OnIconSourcePropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        OnIconSourceChanged();
    }

    private void OnIconSourceChanged()
    {
        if (IconSource != null)
        {
            TabViewTemplateSettings.IconElement = IconHelpers.CreateFromUnknown(IconSource);
            PseudoClasses.Set(SharedPseudoclasses.s_pcIcon, true);
        }
        else
        {
            TabViewTemplateSettings.IconElement = null;
            PseudoClasses.Set(SharedPseudoclasses.s_pcIcon, false);
        }
    }

    internal void StartBringTabIntoView()
    {
        var targetRect = new Rect(0, 0, DesiredSize.Width + c_targetRectWidthIncrement, DesiredSize.Height);
        RaiseEvent(new RequestBringIntoViewEventArgs
        {
            RoutedEvent = RequestBringIntoViewEvent,
            TargetObject = this,
            TargetRect = targetRect,
            Source = this
        });
    }

    private void OnSelectedBackgroundPathSizeChanged(object sender, SizeChangedEventArgs e)
    {
        //if (_location == TabViewTabStripLocation.Left || _location == TabViewTabStripLocation.Right)
        //    return;

        //bool top = _location == TabViewTabStripLocation.Top;
        //var path = _selectedBackgroundPath;

        //var offset = path.Bounds.Y;
        //var actualOffset = double.Round(offset);

        //if (actualOffset > offset)
        //{
        //    // Move the SelectedBackgroundPath element down by a fraction of a pixel to avoid a faint gap line
        //    // between the selected TabViewItem and its content.
        //    var tt = new TranslateTransform(0, top ? actualOffset - offset : offset - actualOffset);
        //    path.RenderTransform = tt;
        //}
        //else if (path.RenderTransform != null)
        //{
        //    path.RenderTransform = null;
        //}
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // WinUI #6748
        Dispatcher.UIThread.Post(() => UpdateTabGeometry());
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ParentTabView is TabView tv)
        {
            tv.SetTabSeparatorOpacity(tv.IndexFromContainer(this));
        }
    }

    private Button _closeButton;
    private object _toolTip;
    private ContentPresenter _headerContentPresenter;
    private TabViewWidthMode _tabViewWidthMode = TabViewWidthMode.Equal;
    private TabViewCloseButtonOverlayMode _closeButtonOverlayMode = TabViewCloseButtonOverlayMode.Auto;
    private bool _firstTimeSettingToolTip = true;
    // Close Button click revoker
    //TabDragStarting revoker
    //TabDragCompleted revoker
    private FACompositeDisposable _tabDragRevoker;
    private Path _selectedBackgroundPath;
    private TabViewTabStripLocation _location;

    private bool _hasPointerCapture = false;
    private bool _isMiddlePointerButtonPressed = false;
    private bool _isBeingDragged = false;
    private bool _isPointerOver = false;
    private Point _lastPointerPressedPosition;
    private int _dragPointerId;
    private bool _isCheckingForDrag;

    private WeakReference<TabView> _parentTabView;

    private const string c_overlayCornerRadiusKey = "OverlayCornerRadius";
    private const int c_targetRectWidthIncrement = 2;
    private const string s_selectedBackgroundPathName = "SelectedBackgroundPath";
    private const string s_pcDragging = ":dragging";

    private TargetWeakEventSubscriber<TabView, TabViewTabDragStartingEventArgs> _startingDragSub;
    private TargetWeakEventSubscriber<TabView, TabViewTabDragCompletedEventArgs> _completedDragSub;
}

public class TabViewItemAutomationPeer : ListItemAutomationPeer
{
    public TabViewItemAutomationPeer(ContentControl owner) : base(owner)
    {
    }
}
