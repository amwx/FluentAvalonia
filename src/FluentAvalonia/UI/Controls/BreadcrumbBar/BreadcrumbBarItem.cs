using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.Collections;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

//https://github.com/microsoft/microsoft-ui-xaml/issues/7213

[TemplatePart(Name = "PART_LayoutRoot", Type = typeof(Grid))] // Required for Inline
[TemplatePart(Name = s_itemEllipsisFlyoutPartName, Type = typeof(Flyout))] // Required for Inline
[TemplatePart(Name = s_itemButtonPartName, Type = typeof(Button))] // Required for Inline
[TemplatePart(Name = "PART_LastItemContentPresenter", Type = typeof(ContentPresenter), IsRequired = false)]
[TemplatePart(Name = "PART_ChevronTextBlock", Type = typeof(TextBlock), IsRequired = false)]
[TemplatePart(Name = "PART_EllipsisDropDownItemContentPresenter", Type = typeof(ContentPresenter), IsRequired = false)]
[PseudoClasses(SharedPseudoclasses.s_pcPressed, s_pcInline, s_pcEllipsis, s_pcInline, s_pcEllipsisDropDown)]
public class BreadcrumbBarItem : ContentControl
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        RevokePartsListeners();

        if (_isEllipsisItem)
        {
            var rootGrid = e.NameScope.Get<Grid>("PART_LayoutRoot");
            _ellipsisFlyout = rootGrid.Resources[s_itemEllipsisFlyoutPartName] as Flyout;
            if (_ellipsisFlyout == null)
                throw new InvalidOperationException("PART_LayoutRoot on BreadcrumbBarItem is missing Flyout in resources");
        }

        _button = e.NameScope.Find<Button>(s_itemButtonPartName);
        if (_button != null)
        {
            _button.Loaded += OnButtonLoadedEvent;
        }

        UpdateButtonCommonVisualState();
        UpdateInlineItemTypeVisualState();

        UpdateItemTypeVisualState();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new BreadcrumbBarItemAutomationPeer(this);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_isEllipsisDropDownItem)
        {
            ProcessPointerOver(e);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isEllipsisDropDownItem)
        {
            ProcessPointerOver(e);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_isEllipsisDropDownItem)
        {
            ProcessPointerCanceled(e);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_isEllipsisDropDownItem)
        {
            if (IgnorePointerId(e.Pointer))
            {
                return;
            }

            if (e.Pointer.Type == PointerType.Mouse)
            {
                var props = e.GetCurrentPoint(this).Properties;
                _isPressed = props.IsLeftButtonPressed;
            }
            else
            {
                _isPressed = true;
            }

            if (_isPressed)
            {
                UpdateEllipsisDropDownItemCommonVisualState();
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isEllipsisDropDownItem)
        {
            if (IgnorePointerId(e.Pointer))
            {
                return;
            }

            if (_isPressed)
            {
                _isPressed = false;
                UpdateEllipsisDropDownItemCommonVisualState();
                OnClickEvent(null, null);
            }
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (_isEllipsisDropDownItem)
        {
            ProcessPointerCanceled(null, e.Pointer);
        }
    }

    private void ProcessPointerOver(PointerEventArgs args)
    {
        if (IgnorePointerId(args.Pointer))
        {
            return;
        }

        // Avalonia captures the pointer automatically and then does not fire
        // PointerExited if the pointer leaves the bounds of the control,
        // unlike WinUI (and what should happen) - so we work this in here
        // This makes it so that you can drag away from the item and if you drag
        // back the click is canceled

        bool pressed = _isPressed;

        if (pressed)
        {
            bool over = IsPointerOver;

            var bnds = new Rect(Bounds.Size);
            var pt = args.GetPosition(this);
            bool isInBounds = bnds.Contains(pt);

            if (!isInBounds)
                ProcessPointerCanceled(args);

            PseudoClasses.Set(":pointerover", over && isInBounds);
            PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, pressed && isInBounds);
        }
    }

    private void ProcessPointerCanceled(PointerEventArgs args, IPointer p = null)
    {
        if (IgnorePointerId(args?.Pointer ?? p))
        {
            return;
        }

        _isPressed = false;
        ResetTrackedPointerId();
        UpdateEllipsisDropDownItemCommonVisualState();
    }

    private void ResetTrackedPointerId()
    {
        _trackedPointerId = 0;
    }

    private void OnButtonLoadedEvent(object sender, RoutedEventArgs e)
    {
        _button.Loaded -= OnButtonLoadedEvent;

        if (_isEllipsisItem)
        {
            _button.Click += OnEllipsisItemClick;
        }
        else
        {
            _button.Click += OnBreadcrumbBarItemClick;
        }

        if (_isEllipsisItem)
        {
            SetPropertiesForEllipsisItem();
        }
        else if (_isLastItem)
        {
            SetPropertiesForLastItem();
        }
        else
        {
            ResetVisualProperties();
        }
    }

    internal void SetParentBreadcrumb(BreadcrumbBar parent)
    {
        _parentBreadcrumb = new WeakReference<BreadcrumbBar>(parent);
    }

    internal void SetEllipsisDropDownItemDataTemplate(object newDataTemplate)
    {
        if (newDataTemplate is IDataTemplate dataTemplate)
        {
            _ellipsisDropDownItemDataTemplate = dataTemplate;
        }
        else if (newDataTemplate == null)
        {
            _ellipsisDropDownItemDataTemplate = null;
        }
    }

    internal void SetIndex(int index) => _index = index;

    internal void SetIsEllipsisDropDownItem(bool isEllipsisDropDownItem)
    {
        _isEllipsisDropDownItem = isEllipsisDropDownItem;
        HookListeners(_isEllipsisDropDownItem);
        UpdateItemTypeVisualState();
    }

    internal void RaiseItemClickedEvent(object content, int index)
    {
        if (_parentBreadcrumb.TryGetTarget(out var target))
        {
            target.RaiseItemClickedEvent(content, index);
        }
    }

    private void OnBreadcrumbBarItemClick(object sender, RoutedEventArgs e)
    {
        RaiseItemClickedEvent(Content, _index - 1);
    }

    private void OnFlyoutElementPreparedEvent(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is BreadcrumbBarItem ellipsisItem)
        {
            ellipsisItem.SetIsEllipsisDropDownItem(true);
        }

        UpdateFlyoutIndex(args.Element, args.Index);
    }

    private void OnFlyoutElementIndexChangedEvent(ItemsRepeater repeater, ItemsRepeaterElementIndexChangedEventArgs args)
    {
        UpdateFlyoutIndex(args.Element, args.NewIndex);
    }

    private void OnChildPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (_isEllipsisDropDownItem)
        {
            if (args.Key == Key.Enter || args.Key == Key.Space)
            {
                OnClickEvent(sender, null);
                args.Handled = true;
            }
        }
        else if (args.Key == Key.Enter || args.Key == Key.Space)
        {
            if (_isEllipsisItem)
            {
                OnEllipsisItemClick(null, null);
            }
            else
            {
                OnBreadcrumbBarItemClick(null, null);
            }

            args.Handled = true;
        }
    }

    private void UpdateFlyoutIndex(Control element, int index)
    {
        if (_ellipsisItemsRepeater != null)
        {
            var isv = _ellipsisItemsRepeater.ItemsSourceView;
            var itemCount = isv.Count;

            if (element is BreadcrumbBarItem item)
            {
                item.SetEllipsisItem(this);
                item.SetIndex(itemCount - index);
            }

            element.SetValue(AutomationProperties.PositionInSetProperty, index + 1);
            element.SetValue(AutomationProperties.SizeOfSetProperty, itemCount);
        }
    }

    private IList<object> CloneEllipsisItemSource(IEnumerable<object> ellipsisItemsSource)
    {
        // The new list contains all the elements in reverse order
        int itemsSourceSize = ellipsisItemsSource.Count();
        
        // A copy of the hidden elements array in BreadcrumbLayout is created
        // to avoid getting a Layout cycle exception
        var newItemsSource = new List<object>(itemsSourceSize);

        if (itemsSourceSize > 0)
        {
            for (int i = itemsSourceSize - 1; i >= 0; i--)
            {
                newItemsSource.Add(ellipsisItemsSource.ElementAt(i));
            }
        }

        return newItemsSource;
    }

    private void OpenFlyout()
    {
        _ellipsisFlyout?.ShowAt(this);
    }

    private void CloseFlyout()
    {
        _ellipsisFlyout?.Hide();
    }

    private void UpdateItemTypeVisualState()
    {
        // Change the style based on whether the item is inline or in the dropdown
        PseudoClasses.Set(s_pcInline, !_isEllipsisDropDownItem);
        PseudoClasses.Set(s_pcEllipsisDropDown, _isEllipsisDropDownItem);
        //winrt::VisualStateManager::GoToState(*this, m_isEllipsisDropDownItem ? s_ellipsisDropDownStateName : s_inlineStateName, false /*useTransitions*/);
    }

    private void UpdateEllipsisDropDownItemCommonVisualState()
    {
        // PointerOver is already handled by core Avalonia

        PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, _isPressed);
    }

    private void UpdateInlineItemTypeVisualState()
    {
        PseudoClasses.Set(s_pcEllipsis, _isEllipsisItem);
        PseudoClasses.Set(s_pcLastItem, _isLastItem);

        PseudoClasses.Set(":allowClick", _allowClickOnLastItem);
    }

    private void UpdateButtonCommonVisualState()
    {
        if (_button == null)
            return;

        var pc = _button.Classes as IPseudoClasses;
        pc.Set(s_pcLastItem, _isLastItem && !_allowClickOnLastItem);
    }

    private void OnEllipsisItemClick(object sender, RoutedEventArgs e)
    {
        if (_parentBreadcrumb.TryGetTarget(out var target))
        {
            var targetHidden = target.HiddenElements();
            var hiddenElements = CloneEllipsisItemSource(targetHidden);
            if (targetHidden is PooledList<object> pl)
                pl.Dispose();

            if (_ellipsisDropDownItemDataTemplate != null)
            {
                _ellipsisElementFactory.UserElementFactory(_ellipsisDropDownItemDataTemplate);
            }

            if (_ellipsisItemsRepeater != null)
            {
                _ellipsisItemsRepeater.ItemsSource = hiddenElements;
            }

            OpenFlyout();
        }
    }

    internal void SetPropertiesForLastItem()
    {
        _isEllipsisItem = false;
        _isLastItem = true;

        if (_parentBreadcrumb.TryGetTarget(out var target))
        {
            _allowClickOnLastItem = target.IsLastItemClickEnabled;
        }

        UpdateButtonCommonVisualState();
        UpdateInlineItemTypeVisualState();
    }

    internal void ResetVisualProperties()
    {
        if (_isEllipsisDropDownItem)
        {
            UpdateEllipsisDropDownItemCommonVisualState();
        }
        else
        {
            _isEllipsisItem = false;
            _isLastItem = false;

            if (_button != null)
            {
                _button.Flyout = null;
            }
            _ellipsisFlyout = null;
            _ellipsisItemsRepeater = null;
            _ellipsisElementFactory = null;

            UpdateButtonCommonVisualState();
            UpdateInlineItemTypeVisualState();
        }
    }

    private void InstantiateFlyout()
    {
        // Only if the element has been created visually, instantiate the flyout
        if (_button != null && _ellipsisFlyout != null)
        {
            var repeater = new ItemsRepeater
            {
                Name = s_ellipsisItemsRepeaterPartName,
                [AutomationProperties.NameProperty] = s_ellipsisItemsRepeaterAutomationName,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Layout = new StackLayout()
            };

            _ellipsisElementFactory = new BreadcrumbElementFactory();
            repeater.ItemTemplate = _ellipsisElementFactory;

            if (_ellipsisDropDownItemDataTemplate != null)
            {
                _ellipsisElementFactory.UserElementFactory(_ellipsisDropDownItemDataTemplate);
            }

            repeater.ElementPrepared += OnFlyoutElementPreparedEvent;
            repeater.ElementIndexChanged += OnFlyoutElementIndexChangedEvent;

            _ellipsisItemsRepeater = repeater;

            // Can't set this as SetName requires a StyledElement, which Flyout is not
            //AutomationProperties.SetName(_ellipsisFlyout, s_ellipsisFlyoutAutomationName);
            _ellipsisFlyout.Content = repeater;
            _ellipsisFlyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
        }
    }

    internal void SetPropertiesForEllipsisItem()
    {
        _isEllipsisItem = true;
        _isLastItem = false;

        InstantiateFlyout();

        UpdateButtonCommonVisualState();
        UpdateInlineItemTypeVisualState();
    }

    private void SetEllipsisItem(BreadcrumbBarItem ellipsisItem)
    {
        _ellipsisItem = ellipsisItem;
    }

    internal void OnClickEvent(object sender, RoutedEventArgs args)
    {
        if (_isEllipsisDropDownItem)
        {
            var item = _ellipsisItem;
            if (item != null)
            {
                item.CloseFlyout();
                item.RaiseItemClickedEvent(Content, _index - 1);
            }
        }
        else if (_isEllipsisItem)
        {
            OnEllipsisItemClick(null, null);
        }
        else
        {
            OnBreadcrumbBarItemClick(null, null);
        }
    }

    private void HookListeners(bool forEllipsisDropDownItem)
    {
        if (!_childPreviewKeyDownToken)
        {
            AddHandler(KeyDownEvent, OnChildPreviewKeyDown, RoutingStrategies.Tunnel);
            _childPreviewKeyDownToken = true;
        }

        //if (forEllipsisDropDownItem)
        //{
        //    if (_isEnabledChangedRevoker == null)
        //    {
        //        _isEnabledChangedRevoker = IsEnabledProperty.Changed.Subscribe(
        //            new SimpleObserver<AvaloniaPropertyChangedEventArgs>(OnIsEnabledChanged));
        //    }
        //}
        //else if (_flowDirectionChangedToken == null)
        //{
        //    _flowDirectionChangedToken = FlowDirectionProperty.Changed.Subscribe(
        //        new SimpleObserver<AvaloniaPropertyChangedEventArgs>(OnFlowDirectionChanged));
        //}
    }

    private void RevokeListeners()
    {
        //if (_flowDirectionChangedToken != null)
        //{
        //    _flowDirectionChangedToken.Dispose();
        //    _flowDirectionChangedToken = null;
        //}

        if (_childPreviewKeyDownToken)
        {
            RemoveHandler(KeyDownEvent, OnChildPreviewKeyDown);
            _childPreviewKeyDownToken = false;
        }

        // KeyDown -= OnKeyDown;
    }

    private void RevokePartsListeners()
    {
        if (_button != null)
        {
            _button.Loaded -= OnButtonLoadedEvent;
            if (_isEllipsisItem)
            {
                _button.Click -= OnEllipsisItemClick;
            }
            else
            {
                _button.Click -= OnBreadcrumbBarItemClick;
            }
        }

        if (_ellipsisItemsRepeater != null)
        {
            _ellipsisItemsRepeater.ElementPrepared -= OnFlyoutElementPreparedEvent;
            _ellipsisItemsRepeater.ElementIndexChanged -= OnFlyoutElementIndexChangedEvent;
        }
    }

    internal bool IsEllipsisDropDownItem() => _isEllipsisDropDownItem;

    private bool IgnorePointerId(IPointer pointer)
    {
        var pointerId = pointer.Id;

        if (_trackedPointerId == 0)
        {
            _trackedPointerId = pointerId;
        }
        else if (_trackedPointerId != pointerId)
        {
            return true;
        }

        return false;
    }

    private bool _childPreviewKeyDownToken;
    private bool _isEllipsisDropDownItem;
    private bool _isEllipsisItem;
    private bool _isLastItem;
    private bool _allowClickOnLastItem;
    private Flyout _ellipsisFlyout;
    private Button _button;

    private WeakReference<BreadcrumbBar> _parentBreadcrumb;
    private ItemsRepeater _ellipsisItemsRepeater;
    private IDataTemplate _ellipsisDropDownItemDataTemplate;
    private BreadcrumbElementFactory _ellipsisElementFactory;

    private BreadcrumbBarItem _ellipsisItem;
    private int _index;

    private bool _isPressed;
    private int _trackedPointerId;

    // Template Parts
    private const string s_ellipsisItemsRepeaterPartName = "PART_EllipsisItemsRepeater";
    private const string s_itemButtonPartName = "PART_ItemButton";
    private const string s_itemEllipsisFlyoutPartName = "PART_EllipsisFlyout";

    //private const string s_ellipsisFlyoutAutomationName = "EllisisFlyout";
    private const string s_ellipsisItemsRepeaterAutomationName = "EllipsisItemsRepeater";

    private const string s_pcInline = ":inline";
    private const string s_pcEllipsis = ":ellipsis";
    private const string s_pcLastItem = ":lastItem";
    private const string s_pcEllipsisDropDown = ":ellipsisDropDown";
}
