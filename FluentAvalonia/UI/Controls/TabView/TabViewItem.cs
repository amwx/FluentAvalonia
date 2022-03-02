using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls
{
    [PseudoClasses(":icon", ":compact", ":closecollapsed", ":borderleft", ":borderright", ":noborder", ":foreground")]
    public partial class TabViewItem : ListBoxItem
    {
        public TabViewItem()
        {
            TabViewTemplateSettings = new TabViewItemTemplateSettings();

            // Use AttachedToVisualTree override for Loaded event

            // Will use OnPropertyChanged for SizeChanged, IsSelectedProperty changed, and Foreground changed

            // ListBoxItem uses the PressedMixin...ugh... which uses tunnel events to set :pressed
            // The problem is that doesn't respect if you've clicked on another control
            // So, when we click the close button, the :pressed state is activated too, we don't want that
            // So we have to undo what the :pressed mixin does
            // This is the easy solution, since the other involves not deriving from ListBoxItem
            AddHandler(PointerPressedEvent, (s, e) =>
            {
                var hasButton = (e.Source as IVisual).GetVisualAncestors()
                    .Where(x => x == _closeButton).Any();

                if (hasButton)
                {
                    PseudoClasses.Set(":pressed", false);
                }
            }, RoutingStrategies.Tunnel);
        }

        static TabViewItem()
        {
            FocusableProperty.OverrideDefaultValue<TabViewItem>(true);
        }

        protected internal TabView ParentTabView
        {
            get
            {
                // AGH Stupid Mac build fails here because
                // UsE oF uNaSsIgNeD lOcAl VaRiAbLe 'target'
                // No Mac compiler, its fine, 'target' isn't used without assignment

                TabView target = null;
                if (_parentTabView?.TryGetTarget(out target) == true)
                    return target;

                return null;
            }
            set => _parentTabView = new WeakReference<TabView>(value);
        }

        public IVisual TabSeparator { get; private set; }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty)
            {
                OnSizeChanged(change);
            }
            else if (change.Property == IsSelectedProperty)
            {
                OnIsSelectedPropertyChanged(change);
            }
            else if (change.Property == TextBlock.ForegroundProperty)
            {
                OnForegroundPropertyChanged(change);
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
            base.OnApplyTemplate(e);

            TabSeparator = e.NameScope.Find<IVisual>("TabSeparator");

            _headerContentPresenter = e.NameScope.Find<ContentPresenter>("ContentPresenter");

            var tabView = this.FindAncestorOfType<TabView>();

            _closeButton = e.NameScope.Find<Button>("CloseButton");
            if (_closeButton != null)
            {
                // Skip Automation

                if (tabView != null)
                {
                    ToolTip.SetTip(_closeButton, tabView.GetTabCloseButtonTooltipText());
                }

                _closeButton.Click += OnCloseButtonClick;
            }

            OnHeaderChanged();
            OnIconSourceChanged();

            if (tabView != null)
            {
                // ignore shadow

                _tabDragRevoker = new CompositeDisposable(
                    Disposable.Create(() => tabView.TabDragStarting -= OnTabDragStarting),
                    Disposable.Create(() => tabView.TabDragCompleted -= OnTabDragCompleted));

                tabView.TabDragStarting += OnTabDragStarting;
                tabView.TabDragCompleted += OnTabDragCompleted;
            }

            // Add this to fix a bug that's clearly in WinUI, adding a new TabViewItem doesn't check
            // the CloseButtonOverlay mode, thus new tabs ALWAYS initialize with 'Auto' even if the 
            // TabView's CloseButtonOverlayMode is not Auto
            var tv = ParentTabView;

            if (tv != null)
            {
                _closeButtonOverlayMode = tv.CloseButtonOverlayMode;
            }

            UpdateCloseButton();
            UpdateForeground();
            UpdateWidthModeVisualState();
            UpdateTabGeometry();

            // Handle TabViewItem::Loaded
            if (tv != null)
            {
                tv.SetTabSeparatorOpacity(tv.IndexFromContainer(this));
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (IsSelected && e.Pointer.Type == PointerType.Mouse)
            {
                var pointerPoint = e.GetCurrentPoint(this);
                if (pointerPoint.Properties.IsLeftButtonPressed)
                {
                    // TODO: Ctrl + cross-platform??
                    var isCtrlDown = (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control;
                    if (isCtrlDown)
                    {
                        // Return here so the base class will not pick it up, but let it remain unhandled so someone else could handle it.
                        return;
                    }
                }
            }

            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed)
            {
                // Pointer capture is implicit in Avalonia
                _hasPointerCapture = true;
                _isMiddlePointerButtonPressed = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

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

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            _isPointerOver = true;

            if (_hasPointerCapture)
            {
                _isMiddlePointerButtonPressed = true;
            }

            UpdateCloseButton();
            HideLeftAdjacentTabSeparator();
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);

            _isPointerOver = false;
            _isMiddlePointerButtonPressed = false;

            UpdateCloseButton();
            UpdateForeground();
            RestoreLeftAdjacentTabSeparatorVisibility();
        }

        // Don't have PointerCanceled

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            _hasPointerCapture = false;
            _isMiddlePointerButtonPressed = false;
            RestoreLeftAdjacentTabSeparatorVisibility();
        }

        private void UpdateTabGeometry()
        {
            var height = Bounds.Height;
            var popupRadius = this.TryFindResource(c_overlayCornerRadiusKey, out var value) ? (CornerRadius)value : default;
            var leftCorner = popupRadius.TopLeft;
            var rightCorner = popupRadius.TopRight;

            const string data = "F1 M0,{0}  a 4,4 0 0 0 4,-4  L 4,{1}  a {2},{3} 0 0 1 {4},-{5}  l {6},0  a {7},{8} 0 0 1 {9},{10}  l 0,{11}  a 4,4 0 0 0 4,4 Z";

            var builder = new StringBuilder();
            // WinUI 6644
            builder.AppendFormat(data, height,
                    leftCorner, leftCorner, leftCorner, leftCorner, leftCorner,
                    Bounds.Width - (leftCorner + rightCorner),
                    rightCorner, rightCorner, rightCorner, rightCorner,
                    height - (4 + rightCorner));

            TabViewTemplateSettings.TabGeometry = StreamGeometry.Parse(builder.ToString());
        }

        private void OnSizeChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            // WinUI #6748
            Dispatcher.UIThread.Post(() => UpdateTabGeometry());
        }

        private void OnIsSelectedPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            // Ignore AutomationPeer

            if (change.NewValue.GetValueOrDefault<bool>())
            {
                SetValue(ZIndexProperty, 20);

                StartBringTabIntoView();
            }
            else
            {
                SetValue(ZIndexProperty, 0);
            }

            // UpdateShadow();
            UpdateWidthModeVisualState();

            UpdateCloseButton();
            UpdateForeground();
        }

        private void OnForegroundPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            UpdateForeground();
        }

        private void UpdateForeground()
        {
            // We shouldn't have to do this here because Foreground is automatically inherited

            bool isForegroundSet = this.GetDiagnostic(ForegroundProperty).Priority != Avalonia.Data.BindingPriority.Unset;
            bool set = isForegroundSet && !IsSelected && !_isPointerOver;

            PseudoClasses.Set(":foreground", set);

            // We only need to set the foreground state when the TabViewItem is in rest state and not selected.
           // if (!IsSelected && !_isPointerOver)
           // {
                // If Foreground is set, then change icon and header foreground to match.
                //winrt::VisualStateManager::GoToState(
                //   *this,
                //   ReadLocalValue(winrt::Control::ForegroundProperty()) == winrt::DependencyProperty::UnsetValue() ? L"ForegroundNotSet" : L"ForegroundSet",
                //   false /*useTransitions*/);
            //}
        }

        private void OnTabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        {
            _isDragging = true;
            //UpdateShadow();
        }

        private void OnTabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        {
            _isDragging = false;
            //UpdateShadow();
            UpdateForeground();
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

            PseudoClasses.Set(":closecollapsed", isCollapsed);
        }

        private void UpdateWidthModeVisualState()
        {
            // Visual States
            // Compact
            // StandardWidth
            // => :compact

            // Handling compact/non compact width mode
            PseudoClasses.Set(":compact", !IsSelected && _tabViewWidthMode == TabViewWidthMode.Compact);
        }

        private void RequestClose()
        {
            if (this.FindAncestorOfType<TabView>() is TabView tabView)
            {
                tabView.RequestCloseTab(this, false);
            }
        }

        internal void RaiseRequestClose(TabViewTabCloseRequestedEventArgs args)
        {
            // This should only be called from TabView, to ensure that both this event and the TabView TabRequestedClose event are raised
            CloseRequested?.Invoke(this, args);
        }

        private void OnCloseButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            RequestClose();
        }

        private void OnIsClosablePropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> args)
        {
            UpdateCloseButton();
        }

        private void OnHeaderPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> args)
        {
            OnHeaderChanged();
        }

        private void OnHeaderChanged()
        {
            if (_headerContentPresenter != null)
            {
                _headerContentPresenter.Content = Header;
            }

            if (_firstTimeSettingToolTip)
            {
                _firstTimeSettingToolTip = false;

                var tip = ToolTip.GetTip(this);
                if (tip == null)
                {
                    // App author has not specified a tooltip; use our own

                    // WinUI assigns an empty ToolTip here, but since tooltips work differently
                    // we'll just mark this not null
                    _toolTip = string.Empty;
                }
            }

            if (_toolTip != null)
            {
                // Update tooltip text to new header text
                var headerContent = Header;

                if (headerContent != null)
                {
                    _toolTip = headerContent;
                    ToolTip.SetTip(this, _toolTip);
                }
            }
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

        private void OnIconSourcePropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> args)
        {
            OnIconSourceChanged();
        }

        private void OnIconSourceChanged()
        {
            if (IconSource != null)
            {
                TabViewTemplateSettings.IconElement = IconHelpers.CreateFromUnknown(IconSource);
                PseudoClasses.Set(":icon", true);
            }
            else
            {
                TabViewTemplateSettings.IconElement = null;
                PseudoClasses.Set(":icon", false);
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

        

        private Button _closeButton;
        private object _toolTip;
        private ContentPresenter _headerContentPresenter;
        private TabViewWidthMode _tabViewWidthMode = TabViewWidthMode.Equal;
        private TabViewCloseButtonOverlayMode _closeButtonOverlayMode = TabViewCloseButtonOverlayMode.Auto;
        private bool _firstTimeSettingToolTip = true;
        // Close Button click revoker
        //TabDragStarting revoker
        //TabDragCompleted revoker
        private CompositeDisposable _tabDragRevoker;

        private bool _hasPointerCapture = false;
        private bool _isMiddlePointerButtonPressed = false;
        private bool _isDragging = false;
        private bool _isPointerOver = false;

        private WeakReference<TabView> _parentTabView;

        const string c_overlayCornerRadiusKey = "OverlayCornerRadius";
        const int c_targetRectWidthIncrement = 2;
    }
}
