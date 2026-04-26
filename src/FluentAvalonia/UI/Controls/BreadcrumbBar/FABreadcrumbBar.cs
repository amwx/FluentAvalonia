using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.Collections;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// The BreadcrumbBar control provides the direct path of pages or folders to the current location.
/// </summary>
[TemplatePart(Name = s_tpItemsRepeater, Type=typeof(FAItemsRepeater))]
public class FABreadcrumbBar : TemplatedControl
{
    public FABreadcrumbBar()
    {
        _itemsRepeaterElementFactory = new BreadcrumbElementFactory();
        _itemsRepeaterLayout = new BreadcrumbLayout(this);
        _itemsIterable = new BreadcrumbIterable(null);
        
        AddHandler(KeyDownEvent, OnChildPreviewKeyDown, RoutingStrategies.Tunnel);
        //AccessKeyInvoked
        GettingFocus += OnGettingFocus;
        // Ignore FlowDirection
    }

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<FABreadcrumbBar>();

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<FABreadcrumbBar>();

    /// <summary>
    /// Defines the <see cref="IsLastItemClickEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsLastItemClickEnabledProperty =
        AvaloniaProperty.Register<FABreadcrumbBar, bool>(nameof(IsLastItemClickEnabled));

    /// <summary>
    /// Gets or sets an object source used to generate the content of the BreadcrumbBar.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template for the BreadcrumbBarItem.
    /// </summary>
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the last item can be clicked
    /// </summary>
    public bool IsLastItemClickEnabled
    {
        get => GetValue(IsLastItemClickEnabledProperty);
        set => SetValue(IsLastItemClickEnabledProperty, value);
    }

    /// <summary>
    /// Occurs when an item is clicked in the BreadcrumbBar.
    /// </summary>
    public event TypedEventHandler<FABreadcrumbBar, FABreadcrumbBarItemClickedEventArgs> ItemClicked;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        RevokeListeners();

        base.OnApplyTemplate(e);

        var repeater = e.NameScope.Get<FAItemsRepeater>(s_tpItemsRepeater);
        repeater.Layout = _itemsRepeaterLayout;
        repeater.ItemTemplate = _itemsRepeaterElementFactory;

        repeater.ElementPrepared += OnElementPreparedEvent;
        repeater.ElementIndexChanged += OnElementIndexChangedEvent;
        repeater.ElementClearing += OnElementClearingEvent;

        repeater.Loaded += OnBreadcrumbBarItemsRepeaterLoaded;

        _itemsRepeater = repeater;

        UpdateItemsRepeaterItemsSource();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            UpdateItemsRepeaterItemsSource();
        }
        else if (change.Property == ItemTemplateProperty)
        {
            UpdateItemTemplate();
            UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate();
        }
        else if (change.Property == IsLastItemClickEnabledProperty)
        {
            ForceUpdateLastElement();
        }
    }

    private void OnBreadcrumbBarItemsRepeaterLoaded(object sender, RoutedEventArgs e)
    {
        if (_itemsRepeater != null)
            OnBreadcrumbBarItemsSourceCollectionChanged(null, null);
    }

    private void UpdateItemTemplate()
    {
        var template = ItemTemplate;
        _itemsRepeaterElementFactory.UserElementFactory(template);
    }

    private void UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate()
    {
        var template = ItemTemplate;

        _ellipsisBreadcrumBarItem?.SetEllipsisDropDownItemDataTemplate(template);
    }

    private void UpdateItemsRepeaterItemsSource()
    {
        if (_breadcrumbItemsSourceView != null)
        {
            _breadcrumbItemsSourceView.CollectionChanged -= OnBreadcrumbBarItemsSourceCollectionChanged;
        }

        _breadcrumbItemsSourceView = null;
        var src = ItemsSource;
        if (src != null)
        {
            _breadcrumbItemsSourceView = new FAItemsSourceView(src);
            if (_itemsRepeater != null)
            {
                _itemsIterable = new BreadcrumbIterable(src);
                _itemsRepeater.ItemsSource = _itemsIterable;
            }
            if (_breadcrumbItemsSourceView != null)
            {
                _breadcrumbItemsSourceView.CollectionChanged += OnBreadcrumbBarItemsSourceCollectionChanged;
            }
        }
    }

    private void OnBreadcrumbBarItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_itemsRepeater != null)
        {
            var src = ItemsSource;
            _itemsIterable = new BreadcrumbIterable(src);
            _itemsRepeater.ItemsSource = _itemsIterable;

            // For some reason, when interacting with keyboard, the last element doesn't raise the OnPrepared event
            ForceUpdateLastElement();
        }
    }

    private void ResetLastBreadcrumbBarItem()
    {
        _lastBreadcrumbBarItem?.ResetVisualProperties();
    }

    private void ForceUpdateLastElement()
    {
        var isv = _breadcrumbItemsSourceView;
        if (isv != null)
        {
            var itemCount = isv.Count;
            if (_itemsRepeater is FAItemsRepeater repeater)
            {
                var newLastItem = repeater.TryGetElement(itemCount);
                UpdateLastElement(newLastItem as FABreadcrumbBarItem);
            }

            // If the given collection is empty, then reset the last element visual properties
            if (itemCount == 0)
                ResetLastBreadcrumbBarItem();
        }
        else
        {
            // Or if the ItemsSource was null, also reset the last breadcrumb Item
            ResetLastBreadcrumbBarItem();
        }
    }

    private void UpdateLastElement(FABreadcrumbBarItem newLastItem)
    {
        // If the element is the last element in the array,
        // then we reset the visual properties for the previous
        // last element
        ResetLastBreadcrumbBarItem();

        newLastItem?.SetPropertiesForLastItem();
        _lastBreadcrumbBarItem = newLastItem;
    }

    private void OnElementPreparedEvent(FAItemsRepeater sender, FAItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FABreadcrumbBarItem item)
        {
            item.SetIsEllipsisDropDownItem(false);

            // Set the parent breadcrumb reference for raising click events
            item.SetParentBreadcrumb(this);

            // Set the item index to fill the Index parameter in the ClickedEventArgs
            var itemIndex = args.Index;
            item.SetIndex(itemIndex);

            // The first element is always the ellipsis item
            if (itemIndex == 0)
            {
                item.SetPropertiesForEllipsisItem();
                _ellipsisBreadcrumBarItem = item;
                UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate();

                var str = FALocalizationHelper.Instance
                    .GetLocalizedStringResource("AutomationNameEllipsisBreadcrumbBarItem");
                AutomationProperties.SetName(item, str);
            }
            else
            {
                if (_breadcrumbItemsSourceView is FAItemsSourceView isv)
                {
                    var itemCount = isv.Count;
                    if (itemIndex == itemCount)
                    {
                        UpdateLastElement(item);
                    }
                    else
                    {
                        // Any other element just resets the visual properties
                        item.ResetVisualProperties();
                    }
                }
            }
        }
    }

    private void OnElementIndexChangedEvent(FAItemsRepeater sender, FAItemsRepeaterElementIndexChangedEventArgs args)
    {
        if (_focusedIndex == args.OldIndex)
        {
            var newIndex = args.NewIndex;

            if (args.Element is FABreadcrumbBarItem item)
            {
                item.SetIndex(newIndex);
            }

            FocusElementAt(newIndex);
        }
    }

    private void OnElementClearingEvent(FAItemsRepeater sender, FAItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is FABreadcrumbBarItem item)
            item.ResetVisualProperties();
    }

    internal void RaiseItemClickedEvent(object content, in int index)
    {
        if (ItemClicked != null)
        {
            var args = new FABreadcrumbBarItemClickedEventArgs(index, content);

            ItemClicked(this, args);
        }
    }

    internal IEnumerable<object> GetHiddenElementsList(int firstShownElement)
    {
        var l = new PooledList<object>();
        if (_breadcrumbItemsSourceView != null)
        {
            for (int i = 0; i < firstShownElement - 1; i++)
            {
                l.Add(_breadcrumbItemsSourceView.GetAt(i));
            }
        }
        return l;
    }

    internal IEnumerable<object> HiddenElements()
    {
        if (_itemsRepeater != null && _itemsRepeaterLayout != null &&
            _itemsRepeaterLayout.EllipsisIsRendered)
        {
            return GetHiddenElementsList(_itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis);
        }

        return null;
    }

    internal void ReIndexVisibleElementsForAccessibility()
    {
        if (_itemsRepeater is FAItemsRepeater repeater)
        {
            var visibleCount = _itemsRepeaterLayout.GetVisibleItemsCount;
            bool isEllipsisRendered = _itemsRepeaterLayout.EllipsisIsRendered;
            int firstItemToIndex = 1;

            if (isEllipsisRendered)
                firstItemToIndex = _itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis;

            // In order to make the ellipsis inaccessible to accessbility tools when it's hidden,
            // we set the accessibilityView to raw and restore it to content when it becomes visible.
            if (_ellipsisBreadcrumBarItem is FABreadcrumbBarItem ellipsisItem)
            {
                var accView = isEllipsisRendered ? AccessibilityView.Content : AccessibilityView.Raw;
                ellipsisItem.SetValue(AutomationProperties.AccessibilityViewProperty, accView);
            }

            // For every BreadcrumbBar item we set the index (starting from 1 for the root/highest-level item)
            // accessibilityIndex is the index to be assigned to each item
            // itemToIndex is the real index and it may differ from accessibilityIndex as we must only index the visible items
            for (int accIdx = 1, itemToIndex = firstItemToIndex; accIdx <= visibleCount; accIdx++, itemToIndex++)
            {
                if (repeater.TryGetElement(itemToIndex) is Control c)
                {
                    c.SetValue(AutomationProperties.PositionInSetProperty, accIdx);
                    c.SetValue(AutomationProperties.SizeOfSetProperty, visibleCount);
                }
            }
        }
    }

    private void OnGettingFocus(object sender, FocusChangingEventArgs args)
    {
        if (_itemsRepeater is FAItemsRepeater repeater)
        {
            // WinUI checks args InputDevice for Keyboard
            if (args.NavigationMethod == NavigationMethod.Directional || args.NavigationMethod == NavigationMethod.Tab)
            {
                var ctrl = Application.Current.PlatformSettings.HotkeyConfiguration.CommandModifiers;
                var oldFocusedElement = args.OldFocusedElement;
                if (oldFocusedElement == null || repeater != (oldFocusedElement as Visual)?.GetVisualParent())
                {
                    if (_itemsRepeaterLayout != null)
                    {
                        if (_itemsRepeaterLayout.EllipsisIsRendered)
                        {
                            _focusedIndex = 0;
                        }
                        else
                        {
                            _focusedIndex = 1;
                        }

                        FocusElementAt(_focusedIndex);
                    }

                    if (repeater.TryGetElement(_focusedIndex) is Control selectedItem)
                    {
                        if (args.TrySetNewFocusedElement(selectedItem))
                        {
                            args.Handled = true;
                        }
                    }
                }
                // Focus was already in the repeater: in RS3+ Selection follows focus unless control is held down.
                else if ((ctrl & args.KeyModifiers) != KeyModifiers.Control)
                {
                    if (args.NewFocusedElement is Control newFocus)
                    {
                        FocusElementAt(repeater.GetElementIndex(newFocus));
                        args.Handled = true;
                    }
                }
            }
        }
    }

    private void FocusElementAt(int index)
    {
        if (index >= 0)
            _focusedIndex = index;
    }

    private bool MoveFocus(int indexIncrement)
    {
        var ir = _itemsRepeater;
        if (ir != null)
        {
            var focusedElem = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
            if (focusedElem is Control element)
            {
                var index = ir.GetElementIndex(element);

                if (index >= 0 && indexIncrement != 0)
                {
                    index += indexIncrement;
                    var itemCount = ir.ItemsSourceView.Count;
                    while (index >= 0 && index < itemCount)
                    {
                        var item = ir.TryGetElement(index);
                        if (item != null)
                        {
                            if (item.Focus())
                            {
                                FocusElementAt(index);
                                return true;
                            }
                        }
                        index += indexIncrement;
                    }
                }
            }
        }

        return false;
    }

    private bool MoveFocusPrevious()
    {
        var movementPrevious = -1;

        // If the focus is in the first visible item, then move to the ellipsis
        var ir = _itemsRepeater;
        if (ir != null)
        {
            var layout = ir.Layout as BreadcrumbLayout;
            if (layout != null)
            {
                if (_focusedIndex == 1)
                {
                    movementPrevious = 0;
                }
                else if (layout.EllipsisIsRendered &&
                    _focusedIndex == layout.FirstRenderedItemIndexAfterEllipsis)
                {
                    movementPrevious = -_focusedIndex;
                }
            }
        }

        return MoveFocus(movementPrevious);
    }

    private bool MoveFocusNext()
    {
        int movementNext = 1;
        
        if (_focusedIndex == 0)
        {
            var ir = _itemsRepeater;
            if (ir != null)
            {
                var layout = ir.Layout as BreadcrumbLayout;
                movementNext = layout.FirstRenderedItemIndexAfterEllipsis;
            }
        }

        return MoveFocus(movementNext);
    }

    private FindNextElementOptions GetFindNextElementOptions()
    {
        var options = new FindNextElementOptions
        {
            SearchRoot = this
        };
        return options;
    }

    private void OnChildPreviewKeyDown(object sender, KeyEventArgs args)
    {
        bool flowDirectionIsLtr = FlowDirection == FlowDirection.LeftToRight;
        bool keyIsLeft = args.Key == Key.Left;
        bool keyIsRight = args.Key == Key.Right;

        // Moving to the next element
        if ((flowDirectionIsLtr && keyIsRight) || (!flowDirectionIsLtr && keyIsLeft))
        {
            if (MoveFocusNext())
            {
                args.Handled = true;
                return;
            }
            // Gamepad
        }
        else if ((flowDirectionIsLtr && keyIsLeft) || (!flowDirectionIsLtr && keyIsRight))
        {
            if (MoveFocusPrevious())
            {
                args.Handled = true;
                return;
            }
            // Gamepad
        }
    }

    // AccessKeyInvoked



    private void RevokeListeners()
    {
        if (_itemsRepeater != null)
        {
            _itemsRepeater.Loaded -= OnBreadcrumbBarItemsRepeaterLoaded;
            _itemsRepeater.ElementPrepared -= OnElementPreparedEvent;
            _itemsRepeater.ElementIndexChanged -= OnElementIndexChangedEvent;
            _itemsRepeater.ElementClearing -= OnElementClearingEvent;
        }

        if (_breadcrumbItemsSourceView != null)
        {
            _breadcrumbItemsSourceView.CollectionChanged -= OnBreadcrumbBarItemsSourceCollectionChanged;
        }
    }

    private FAItemsSourceView _breadcrumbItemsSourceView;
    private BreadcrumbIterable _itemsIterable;

    private FAItemsRepeater _itemsRepeater;
    private BreadcrumbElementFactory _itemsRepeaterElementFactory;
    private BreadcrumbLayout _itemsRepeaterLayout;

    private FABreadcrumbBarItem _ellipsisBreadcrumBarItem;
    private FABreadcrumbBarItem _lastBreadcrumbBarItem;
    private int _focusedIndex;

    private const string s_tpItemsRepeater = "PART_ItemsRepeater";
}
