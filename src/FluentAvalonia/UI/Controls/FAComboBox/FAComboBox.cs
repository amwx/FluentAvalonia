using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

public partial class FAComboBox : HeaderedSelectingItemsControl
{
    static FAComboBox()
    {
        FocusableProperty.OverrideDefaultValue<FAComboBox>(true);
        IsTextSearchEnabledProperty.OverrideDefaultValue<FAComboBox>(true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        if (_textBox != null)
        {
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }

        if (_dropDownOverlay != null)
        {
            _dropDownOverlay.PointerPressed -= OnDropDownOverlayPointerPressed;
            _dropDownOverlay.PointerReleased -= OnDropDownOverlayPointerReleased;
            _dropDownOverlay.PointerCaptureLost -= OnDropDownOverlayPointerCaptureLost;
        }

        base.OnApplyTemplate(e);

        _popup = e.NameScope.Get<Popup>(s_tpPopup);
        _popup.Opened += OnPopupOpened;
        _popup.Closed += OnPopupClosed;

        _textBox = e.NameScope.Find<TextBox>(s_tpEditableText);
        if (_textBox != null)
        {
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.KeyDown += OnTextBoxKeyDown;
        }

        _dropDownOverlay = e.NameScope.Find<Border>(s_tpDropDownOverlay);
        if (_dropDownOverlay != null)
        {
            // Pointerover can be handled in xaml automatically, so we only need to worry
            // about the pressed state & clicking
            _dropDownOverlay.PointerPressed += OnDropDownOverlayPointerPressed;
            _dropDownOverlay.PointerReleased += OnDropDownOverlayPointerReleased;
            _dropDownOverlay.PointerCaptureLost += OnDropDownOverlayPointerCaptureLost;
        }

        var text = Text;
        if (IsEditable && !string.IsNullOrEmpty(text))
        {
            UpdateSelectionBoxItem(text);
        }
        else
        {
            UpdateSelectionBoxItem(SelectedItem);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            OnSelectedItemChanged(change);
        }
        else if (change.Property == DisplayMemberBindingProperty)
        {
            var temp = change.GetNewValue<IBinding>();
            if (temp != null)
            {
                _displayMemberTemplate = new FuncDataTemplate<object>((_, _) =>
                {
                    return new TextBlock
                    {
                        [!TextBlock.TextProperty] = temp
                    };
                });
            }
            else
            {
                _displayMemberTemplate = null;
            }
        }
        else if (change.Property == ItemTemplateProperty)
        {
            var temp = change.GetNewValue<IDataTemplate>();
            SelectionBoxItemTemplate = temp;
        }
        else if (change.Property == IsEditableProperty)
        {
            PseudoClasses.Set(s_pcEditable, change.GetNewValue<bool>());
            UpdateSelectionBoxItem(SelectedItem ?? Text);
        }
        else if (change.Property == TextProperty)
        {
            if (_textBox != null && IsEditable)
            {
                OnTextChanged(change.GetNewValue<string>());
            }
        }
        else if (change.Property == HeaderProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcHeader, change.NewValue is not null);
        }
    }

    protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
    {
        return new FAComboBoxItem();
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        bool isContainer = item is FAComboBoxItem;
        recycleKey = isContainer ? null : nameof(FAComboBoxItem);
        return !isContainer;
    }

    protected override void InvalidateMirrorTransform()
    {
        base.InvalidateMirrorTransform();
        UpdateFlowDirection();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        bool isOpen = IsDropDownOpen;
        bool isEditable = IsEditable;

        if ((e.Key == Key.F4 && e.KeyModifiers.HasFlag(KeyModifiers.Alt) == false) ||
                ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasFlag(KeyModifiers.Alt)))
        {
            IsDropDownOpen = !isOpen;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if (isOpen)
            {
                IsDropDownOpen = false;
            }

            // Two cases for this:
            // 1- If isOpen, since we change the selection box based on keyboard navigaton without changing
            //    the SelectedItem, we need to revert to the actual selection if its cancelled
            // 2- If !isOpen, and editable, user can revert their text change by pressing escape
            UpdateSelectionBoxItem(SelectedItem);
            if (isEditable && SelectedItem is null)
            {
                // UWP behavior - even if text is set, hitting escape will clear the text if no
                // item is selected
                Text = null;
            }
            e.Handled = true;
        }
        else if (!isOpen && !isEditable && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            IsDropDownOpen = true;
            e.Handled = true;
        }
        else if (isEditable && e.Key == Key.Enter)
        {
            if (_textBox is not null && _textBox.IsFocused)
            {
                OnTextSubmittedCore();
            }
            else
            {
                SelectFocusedItem();
            }
            IsDropDownOpen = false;
            e.Handled = true;
        }
        else if (isOpen && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            SelectFocusedItem();
            IsDropDownOpen = false;
            e.Handled = true;
        }
        else if (!isOpen)
        {
            if (e.Key == Key.Down)
            {
                if (!isEditable)
                {
                    SelectNext();
                }
                else
                {
                    IsDropDownOpen = true;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (!isEditable)
                {
                    SelectPrevious();
                }
                else
                {
                    IsDropDownOpen = true;
                }
                e.Handled = true;
            }
        }
        // This part of code is needed just to acquire initial focus, subsequent focus navigation will be done by ItemsControl.
        else if (isOpen && SelectedIndex < 0 && ItemCount > 0 &&
                 (e.Key == Key.Up || e.Key == Key.Down) && IsFocused == true)
        {
            var firstChild = Presenter?.Panel?.Children.FirstOrDefault(c => CanFocus(c));
            if (firstChild != null)
            {
                firstChild.Focus(NavigationMethod.Directional);
                e.Handled = true;
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (!e.Handled)
        {
            if (!IsDropDownOpen)
            {
                if (IsFocused)
                {
                    if (e.Delta.Y < 0)
                        SelectNext();
                    else
                        SelectPrevious();

                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.Handled && e.Source is Visual src)
        {
            if (_popup?.IsInsidePopup(src) == true)
                return;
        }
        PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual src)
        {
            if (_popup?.IsInsidePopup(src) == true)
            {
                if (UpdateSelectionFromEventSource(e.Source))
                {
                    _popup.Close();
                    e.Handled = true;
                }
            }
            else
            {
                if (!IsEditable)
                {
                    // Only open the dropdown here if we're not editable
                    // Editable CB requires clicking on the DropDownOverlay
                    bool open = IsDropDownOpen;
                    IsDropDownOpen = !open;
                }

                e.Handled = true;
            }
        }
        PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, false);
        base.OnPointerReleased(e);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        if (IsEditable && _textBox != null)
        {
            if (!IsDropDownOpen)
            {
                _textBox.Focus(e.NavigationMethod);
                HighlightTextBoxText();
            }
            else
            {
                if (!_textBox.IsFocused)
                {
                    // If focus moves to the dropdown, keep the textbox style looking
                    // like its focused
                    ((IPseudoClasses)_textBox.Classes).Set(s_pcFocus, true);
                }
            }
        }

        UpdateIsSelectionBoxHighlighted();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (IsDropDownOpen && e.Source is FAComboBoxItem)
        {
            return;
        }

        if (IsEditable && !HasImplicitFocus())
        {
            if (_hasUnsubmittedText)
            {
                OnTextSubmittedCore();
            }

            if (IsDropDownOpen && _popup.IsLightDismissEnabled)
            {
                IsDropDownOpen = false;
            }

            ClearTextBoxSelection();
        }

        bool HasImplicitFocus()
        {
            var c = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            // FAComboBoxItem is the only container we allow, so if it has focus
            // we know we're in this ComboBox's dropdown and have implicit focus
            if (c is FAComboBoxItem)
                return true;

            return c != null && c.FindAncestorOfType<FAComboBox>(true) == this;
        }
    }

    protected override void PrepareContainerForItemOverride(Control element, object item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);

        if (IsEditable)
        {
            // When the ComboBox is editable, we don't use the FocusAdorner in the ComboBox dropdown
            // and use the "selected" visual state as the indicator
            element.FocusAdorner = _noFocusAdornerTemplate;
        }
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);

        if (IsEditable)
        {
            element.ClearValue(FocusAdornerProperty);
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FAComboBoxAutomationPeer(this);
    }

    internal void ItemFocused(FAComboBoxItem item)
    {
        if (IsDropDownOpen && item.IsFocused && item.IsArrangeValid)
        {
            item.BringIntoView();

            var oldContainer = ContainerFromIndex(_dropDownSelectedIndex);
            if (oldContainer != null)
            {
                ((IPseudoClasses)oldContainer.Classes).Set(s_pcSelected, false);
            }

            var changeType = SelectionChangedTrigger;
            if (changeType == FAComboBoxSelectionChangedTrigger.Always)
            {
                SelectedIndex = IndexFromContainer(item);
            }
            else
            {
                _dropDownSelectedIndex = IndexFromContainer(item);
                ((IPseudoClasses)item.Classes).Set(s_pcSelected, true);
                UpdateSelectionBoxItem(ItemsView[_dropDownSelectedIndex]);
            }
        }
    }

    protected virtual void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateSelectionBoxItem(args.NewValue ?? Text);
        TryFocusSelectedItem();
    }

    protected virtual void OnTextSubmitted(FAComboBoxTextSubmittedEventArgs args)
    {

    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
        TryFocusSelectedItem();

        if (SelectionChangedTrigger == FAComboBoxSelectionChangedTrigger.Committed)
            _dropDownSelectedIndex = SelectedIndex;

        _subscriptionsOnOpen.Clear();

        var toplevel = this.GetVisualRoot() as TopLevel;
        if (toplevel != null)
        {
            _subscriptionsOnOpen.Add(
                toplevel.AddDisposableHandler(PointerWheelChangedEvent, (s, ev) =>
                {
                    if (IsDropDownOpen && (ev.Source as Visual)?.GetVisualRoot() == toplevel)
                        ev.Handled = true;
                }, RoutingStrategies.Tunnel));
        }

        _subscriptionsOnOpen.Add(
            this.GetObservable(IsVisibleProperty).Subscribe(
                new SimpleObserver<bool>(IsVisibleChanged)));

        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            _subscriptionsOnOpen.Add(
                parent.GetObservable(IsVisibleProperty).Subscribe(
                    new SimpleObserver<bool>(IsVisibleChanged)));
        }

        UpdateFlowDirection();

        DropDownOpened?.Invoke(this, EventArgs.Empty);
        PseudoClasses.Set(s_pcDropDownOpen, true);

        if (!IsEditable)
        {
            var si = SelectedIndex;
            double dropDownDelta = 0;
            if (si == -1)
            {
                // If nothing's selected, we'll center the popup over the ComboBox
                dropDownDelta = _popup.Child.DesiredSize.Height / 2;
            }
            else
            {
                // Find the selected item container, and center it over the ComboBox
                // TryFocusSelectedItem above should handle materializing the item if
                // it isn't already (it calls ScrollIntoView)
                var container = ContainerFromIndex(si);
                if (container is null) // If this happens for any reason, just bail out
                    return;

                var root = container.GetVisualRoot() as Visual;
                var transform = container.TransformToVisual(root);
                if (!transform.HasValue) // Also bail out if this fails for any reason
                    return;

                var y = new Point(0, 0).Transform(transform.Value).Y; // Top of container
                // Popup is normally positioned below ComboBox, so also move up container height
                dropDownDelta = y + container.DesiredSize.Height;
            }

            _popup.VerticalOffset = -dropDownDelta;

            var contentRoot = _popup.Child.GetVisualRoot();
            if (contentRoot is PopupRoot)
            {
                // HACK: Windowed popups appear to be +1 offset on x-axis for some reason
                // which makes the popup look off center. Overlay popups are fine
                _popup.HorizontalOffset = -1;
            }
        }
        else
        {
            UpdateCornerRadius();
        }
    }

    private void UpdateCornerRadius()
    {
        var child = _popup.Child as Visual;
        var thisRoot = this.GetVisualRoot();
        var popupRoot = child.GetVisualRoot();

        bool isPopupAbove = false;
        if (popupRoot is OverlayPopupHost oph)
        {
            Debug.Assert(thisRoot == popupRoot);
            // Overlay popups are in use, this is the easiest
            var pt = new Point(0, 0).Transform(this.TransformToVisual(thisRoot as Visual).Value);
            var pt2 = new Point(0, 0).Transform(child.TransformToVisual(thisRoot as Visual).Value);

            isPopupAbove = pt2.Y < pt.Y;
        }
        else
        {
            // v2-p6.1, PlatformImpl was hidden and they're stupidly doing it without providing
            // access to common APIs that are only accessible in the PlatformImpl, like the 
            // Window position, so popup will always have the unrounded top, rounded bottoms

            //var popupPosition = (_popup?.Host as PopupRoot)?.PlatformImpl?.Position;

            //// If we can't get the screen position of the popup, cancel now, the result will
            //// be the default behavior of unrounded bottom ComboBox and unrounded top popup
            //if (!popupPosition.HasValue)
            //    return;

            //var pt = child.PointToScreen(new Point(0, 0));
            //var thisInScreenSpace = this.PointToScreen(new Point(0, 0));

            //isPopupAbove = pt.Y < thisInScreenSpace.Y;

            //// HACK: Windowed popups appear to be +1 offset on x-axis for some reason
            //// which makes the popup look off center. Overlay popups are fine
            //_popup.HorizontalOffset = -1;
        }

        PseudoClasses.Set(s_pcPopupAbove, isPopupAbove);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();

        if (CanFocus(this))
        {
            if (IsEditable && _textBox != null)
            {
                _textBox.Focus();
            }
            else
            {
                Focus();
            }
        }

        DropDownClosed?.Invoke(this, EventArgs.Empty);

        UpdateSelectionBoxItem(SelectedItem ?? Text);

        if (_dropDownSelectedIndex != SelectedIndex)
        {
            var container = ContainerFromIndex(_dropDownSelectedIndex);
            if (container != null)
            {
                ((IPseudoClasses)container.Classes).Set(s_pcSelected, false);
            }
        }
        _dropDownSelectedIndex = -1;
        PseudoClasses.Set(s_pcDropDownOpen, false);
        PseudoClasses.Set(s_pcPopupAbove, false);
    }

    private void IsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
            IsDropDownOpen = false;
    }

    private void TryFocusSelectedItem()
    {
        var sIdx = SelectedIndex;
        if (IsDropDownOpen && sIdx != -1)
        {
            var container = ContainerFromIndex(sIdx);

            if (container == null && sIdx != -1)
            {
                ScrollIntoView(Selection.SelectedIndex);
                container = ContainerFromIndex(sIdx);
            }

            if (container != null && CanFocus(container))
                container.Focus();
        }
    }

    private bool CanFocus(Control control) =>
        control.Focusable && control.IsEffectivelyEnabled && control.IsVisible;

    private void UpdateSelectionBoxItem(object item)
    {
        if (!this.IsAttachedToVisualTree())
        {
            return;
        }

        if (IsEditable && item != null)
        {
            UpdateTextValue(FormatValue(item), false);
        }

        var contentControl = item as ContentControl;

        if (contentControl != null)
        {
            item = contentControl.Content;
        }

        var control = item as Control;

        if (control != null)
        {
            control.Measure(Size.Infinity);

            var sbi = SelectionBoxItem;
            var displayItem = (sbi as Rectangle) ?? new Rectangle();

            displayItem.Width = control.DesiredSize.Width;
            displayItem.Height = control.DesiredSize.Height;

            if (displayItem.Fill is VisualBrush vb)
            {
                vb.Visual = control;
            }
            else
            {
                displayItem.Fill = new VisualBrush
                {
                    Visual = control,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                };
            }

            if (sbi != displayItem)
            {
                SelectionBoxItem = displayItem;
            }

            UpdateFlowDirection();
        }
        else
        {
            if (item is string)
            {
                // If the item is a raw string, don't use the template or nothing will show
                SelectionBoxItemTemplate = null;
                SelectionBoxItem = item;
            }
            else
            {
                var template = _displayMemberTemplate ?? ItemTemplate;
                if (template is not null)
                    SelectionBoxItemTemplate = template;

                SelectionBoxItem = item;
            }
        }
    }

    private void UpdateFlowDirection()
    {
        if (SelectionBoxItem is Rectangle rectangle)
        {
            if ((rectangle.Fill as VisualBrush)?.Visual is Visual content)
            {
                var flowDirection = (content.GetVisualParent() as Control)?.FlowDirection ?? FlowDirection.LeftToRight;
                rectangle.FlowDirection = flowDirection;
            }
        }
    }

    private void SelectNext() => MoveSelection(SelectedIndex, 1, WrapSelection);
    private void SelectPrevious() => MoveSelection(SelectedIndex, -1, WrapSelection);

    private void MoveSelection(int startIndex, int step, bool wrap)
    {
        static bool IsSelectable(object o) => (o as AvaloniaObject)?.GetValue(IsEnabledProperty) ?? true;

        var count = ItemCount;
        for (int i = startIndex + step; i != startIndex; i += step)
        {
            if (i < 0 || i >= count)
            {
                if (wrap)
                {
                    if (i < 0)
                        i += count;
                    else if (i >= count)
                        i %= count;
                }
                else
                {
                    return;
                }
            }

            var item = ItemsView[i];
            var container = ContainerFromIndex(i);

            if (IsSelectable(item) && IsSelectable(container))
            {
                SelectedIndex = i;
                break;
            }
        }
    }

    private void SelectFocusedItem()
    {
        var change = SelectionChangedTrigger;
        if (change == FAComboBoxSelectionChangedTrigger.Committed)
        {
            SelectedIndex = _dropDownSelectedIndex;
            _dropDownSelectedIndex = -1;
        }
        else
        {
            foreach (var item in GetRealizedContainers())
            {
                if (item.IsFocused)
                {
                    SelectedIndex = IndexFromContainer(item);
                    break;
                }
            }
        }
    }

    private void UpdateIsSelectionBoxHighlighted()
    {
        // Per WPF:
        // !comboBox.IsDropDownOpen && comboBox.IsKeyboardFocusWithin ||
        // comboBox.HighlightedElement != null && highlightedElement.Content == comboBox._clonedElement
        // Not sure how to handle that second clause, as this is different implementation
        // so we'll just use the first
        IsSelectionBoxHighlighted = !IsDropDownOpen && IsKeyboardFocusWithin;
    }



    //////////////////////////////////////////////////////////////
    // Editable ComboBox related
    //////////////////////////////////////////////////////////////


    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (IsDropDownOpen && (e.Key == Key.Up || e.Key == Key.Down))
        {
            var first = ContainerFromIndex(0);
            if (first != null)
            {
                first.Focus(NavigationMethod.Directional);
                e.Handled = true;
            }
        }
    }

    private void OnDropDownOverlayPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_dropDownOverlay).Properties.IsLeftButtonPressed)
        {
            ((IPseudoClasses)_dropDownOverlay.Classes).Set(SharedPseudoclasses.s_pcPressed, true);
            e.Handled = true;
        }
    }

    private void OnDropDownOverlayPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (e.GetCurrentPoint(_dropDownOverlay).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            ((IPseudoClasses)_dropDownOverlay.Classes).Set(SharedPseudoclasses.s_pcPressed, false);
            IsDropDownOpen = !IsDropDownOpen;
            UpdateSelectionBoxItem(SelectedItem);
            e.Handled = true;
        }
    }

    private void OnDropDownOverlayPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        ((IPseudoClasses)_dropDownOverlay.Classes).Set(SharedPseudoclasses.s_pcPressed, false);
    }

    private string FormatValue(object item)
    {
        if (item is ContentControl cc)
        {
            return cc.Content.ToString();
        }
        else if (item is string s)
        {
            return s;
        }
        else
        {
            return _bindingHelper.Evaluate(DisplayMemberBinding, item).ToString();
        }
    }

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        // Per AutoComplete, post to dispatcher so textbox selection updates before
        // we process this...TODO: do we need to do that?
        Dispatcher.UIThread.Post(() => TextUpdated(_textBox.Text, true));
    }

    private void OnTextChanged(string newValue)
    {
        TextUpdated(newValue, false);
    }

    private void UpdateTextValue(string value, bool? user)
    {
        if ((user ?? true) && Text != value)
        {
            _ignoreTextPropertyChange++;
            Text = value;
        }

        if ((user == null || user == false) && _textBox != null && _textBox.Text != value)
        {
            _ignoreTextPropertyChange++;
            _textBox.Text = value ?? string.Empty;
        }
    }

    private void TextUpdated(string newText, bool user)
    {
        if (_ignoreTextPropertyChange > 0)
        {
            _ignoreTextPropertyChange--;
            return;
        }

        if (newText is null)
            newText = string.Empty;

        var start = _textBox.SelectionStart;
        var end = _textBox.SelectionEnd;

        if (IsTextSearchEnabled && _textBox != null && (end - start) > 0 &&
            start != _textBox.Text.Length)
        {
            return;
        }

        _hasUnsubmittedText = true;

        UpdateTextValue(newText, user);

        if (!string.IsNullOrEmpty(newText) && newText.Length > 0)
        {
            _ignoreTextSelectionChange = true;
            UpdateTextCompletion(true);
        }
    }

    private void UpdateTextCompletion(bool user)
    {
        string text = Text;
        if (ItemCount > 0)
        {
            if (IsTextSearchEnabled && _textBox != null && user)
            {
                int curLen = _textBox.Text.Length;
                int selStart = _textBox.SelectionStart;

                if (selStart == text.Length && selStart > _currentTextSelectionStart)
                {
                    var firstMatch = TryGetMatch(text, out int index);

                    if (firstMatch is not null)
                    {
                        var value = FormatValue(firstMatch);
                        int minLength = Math.Min(value.Length, text.Length);
                        if (Compare(text, value, minLength))
                        {
                            UpdateTextValue(value, null);

                            _textBox.SelectionStart = curLen;
                            _textBox.SelectionEnd = value.Length;

                            if (IsDropDownOpen)
                            {
                                _dropDownSelectedIndex = index;
                            }
                        }
                    }
                }
            }
        }

        if (_ignoreTextSelectionChange)
        {
            _ignoreTextSelectionChange = false;
            if (_textBox != null)
            {
                _currentTextSelectionStart = _textBox.SelectionStart;
            }
        }

        static bool Compare(string text1, string text2, int minLength)
        {
            var sp1 = text1.AsSpan().Slice(0, minLength);
            var sp2 = text2.AsSpan().Slice(0, minLength);
            return MemoryExtensions.Equals(sp1, sp2, StringComparison.OrdinalIgnoreCase);
        }
    }

    private object TryGetMatch(string currentText, out int index)
    {
        index = -1;
        var items = ItemsView;
        int count = items.Count;

        for (int i = 0; i < count; i++)
        {
            var item = items[i];
            var value = FormatValue(item);

            if (!string.IsNullOrEmpty(value) && value.StartsWith(currentText, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                return item;
            }
        }

        return null;
    }

    private void OnTextSubmittedCore()
    {
        var args = new FAComboBoxTextSubmittedEventArgs(Text);
        OnTextSubmitted(args);
        TextSubmitted?.Invoke(this, args);

        if (args.Handled)
        {
            _hasUnsubmittedText = false;
            ClearTextBoxSelection();
            return;
        }

        // In UWP, they will attempt to set the SelectedItem to the text, which may
        // produce an InvalidCastException and crashes the app
        // In WPF, this doesn't happen and the existing selection is cleared
        // We'll follow WPF b/c that's a much nicer behavior
        var idx = FindItemFromTextValue(args.Text);
        var oldIdx = SelectedIndex;
        SelectedIndex = idx;

        // However, UWP raises the SelectionChanged event for text submissions that aren't 
        // in the items collection
        if (oldIdx == -1 && idx == -1)
        {
            // However, if an item is selected previously, in UWP
            //   RemovedItems => Old Selected Item
            //   AddedItems => Text
            // Since we can't control the SelectionChangedEvent in Avalonia, here it will be 
            //   RemovedItems => Old Selected Item
            //   AddedItems => null

            // For consistency, we'll raise the event, but both Removed & AddedItems will be null
            RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, null, null));
            UpdateSelectionBoxItem(Text);
        }

        _hasUnsubmittedText = false;
        ClearTextBoxSelection();
    }

    private void HighlightTextBoxText()
    {
        _textBox.SelectAll();
    }

    private void ClearTextBoxSelection()
    {
        if (_textBox == null)
            return;

        var len = _textBox.Text?.Length ?? 0;
        _textBox.SelectionStart = _textBox.SelectionEnd = len;
    }

    private int FindItemFromTextValue(string text)
    {
        var items = ItemsView;
        var ct = items.Count;
        var binding = DisplayMemberBinding;

        for (int i = 0; i < ct; i++)
        {
            var item = items[i];

            if (item is ContentControl cc)
            {
                // If the item is a ContentControl (ComboBoxItem), directly search its content
                // we don't need to go through the binding path here since DisplayMemberBinding
                // isn't active when item = container
                if (cc.Content.ToString().Equals(text))
                {
                    return i;
                }
            }
            else
            {
                // Item is a ViewModel
                var value = binding is null ? item.ToString() : _bindingHelper.Evaluate(binding, item);

                if (value != null && value.Equals(text))
                {
                    return i;
                }
            }
        }

        return -1;
    }


    private Popup _popup;
    private TextBox _textBox;
    private Border _dropDownOverlay;
    private IDataTemplate _displayMemberTemplate;
    private int _dropDownSelectedIndex = -1;
    private int _ignoreTextPropertyChange = 0;
    private bool _ignoreTextSelectionChange = false;
    private bool _hasUnsubmittedText;
    private int _currentTextSelectionStart;
    private readonly ITemplate<Control> _noFocusAdornerTemplate = new FuncTemplate<Control>(() => new Decorator());

    private readonly BindingHelper _bindingHelper = new BindingHelper();
    private FACompositeDisposable _subscriptionsOnOpen = new FACompositeDisposable();

    private class BindingHelper : StyledElement
    {
        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<BindingHelper, object>("Value");

        public object Evaluate(IBinding binding, object dataContext)
        {
            dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            if (binding is null)
            {
                _lastBinding = null;
                return dataContext;
            }

            if (!dataContext.Equals(DataContext))
                DataContext = dataContext;

            if (_lastBinding != binding)
            {
                _lastBinding = binding;
                var ib = binding.Initiate(this, ValueProperty);
                BindingOperations.Apply(this, ValueProperty, ib, null);
            }

            return GetValue(ValueProperty);
        }

        private IBinding _lastBinding;
    }
}
