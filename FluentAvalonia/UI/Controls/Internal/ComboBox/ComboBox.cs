using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.Core.Attributes;
using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// NOTE: This control is currently not receiving support. A better implementation is needed
    /// to support a fully editable combobox which is under consideration. It may be better to
    /// place that work in Avalonia rather than here
    /// </summary>
    public class ComboBox : HeaderedSelectingItemsControl
    {
        static ComboBox()
        {
            FocusableProperty.OverrideDefaultValue<ComboBox>(true);
        }

        public static readonly DirectProperty<ComboBox, bool> IsDropDownOpenProperty =
            AvaloniaProperty.RegisterDirect<ComboBox, bool>(nameof(IsDropDownOpen),
                o => o.IsDropDownOpen, (o, v) => o.IsDropDownOpen = v);

        public static readonly StyledProperty<double> MaxDropDownHeightProperty =
            AvaloniaProperty.Register<ComboBox, double>(nameof(MaxDropDownHeight), 200, validate: IsValidMaxDropDownHeight);

        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<ComboBox, IDataTemplate>("HeaderTemplate");

        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<ComboBox, string>(nameof(PlaceholderText));

        public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
            AvaloniaProperty.Register<ComboBox, bool>("IsLightDismissEnabled", true);

        public static readonly DirectProperty<ComboBox, ComboBoxSelectionChangedTrigger> SelectionChangedTriggerProperty =
            AvaloniaProperty.RegisterDirect<ComboBox, ComboBoxSelectionChangedTrigger>("SelectionChangedTrigger",
                x => x.SelectionChangedTrigger, (x, v) => x.SelectionChangedTrigger = v);

        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<ComboBox, string>("Description");

        public static readonly StyledProperty<bool> IsEditableProperty =
            AvaloniaProperty.Register<ComboBox, bool>("IsEditable");

        public static readonly DirectProperty<ComboBox, string> TextProperty =
            AvaloniaProperty.RegisterDirect<ComboBox, string>("Text",
                x => x.Text, (x, v) => x.Text = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        //PlaceholderBrush Property removed, just use DynamicResource

        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            ContentControl.HorizontalContentAlignmentProperty.AddOwner<ComboBox>();

        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<ComboBox>();

        private static bool IsValidMaxDropDownHeight(double val) => val >= 0;

        public double MaxDropDownHeight
        {
            get => GetValue(MaxDropDownHeightProperty);
            set => SetValue(MaxDropDownHeightProperty, value);
        }

        public bool IsEditable
        {
            get => GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => _isDropDownOpen;
            set => SetAndRaise(IsDropDownOpenProperty, ref _isDropDownOpen, value);
        }

        [NotImplemented]
        public bool IsSelectionBoxHighlighted
        {
            get => _isSelectionBoxHighlighted;
            protected set => _isSelectionBoxHighlighted = value;
        }

        public object SelectionBoxItem
        {
            get => _selectionBoxItem;
            protected set => _selectionBoxItem = value;
        }

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public IDataTemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public bool IsLightDismissEnabled
        {
            get => GetValue(IsLightDismissEnabledProperty);
            set => SetValue(IsLightDismissEnabledProperty, value);
        }

        [NotImplemented]
        public ComboBoxSelectionChangedTrigger SelectionChangedTrigger
        {
            get => _changeTrigger;
            set => SetAndRaise(SelectionChangedTriggerProperty, ref _changeTrigger, value);
        }

        public string Text
        {
            get => _text;
            set => SetAndRaise(TextProperty, ref _text, value);
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }        

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public IBinding ValueMemberBinding
        {
            get => _valueBindingEvaluator?.ValueBinding;
            set
            {
                if (ValueMemberBinding != value)
                {
                    _valueBindingEvaluator = new AutoCompleteBox.BindingEvaluator<string>(value);
                    OnValueMemberBindingChanged(value);
                }
            }
        }

        public int TempSelectedIndex
        {
            get => _tempSelectedIndex;
            set
            {
                // This is a hack to get around the way SelectingItemsControl works
                // When navigating via keyboard, to match the behavior in WinUI, we
                // have to physically move the selection, not just the focus. But we dont
                // want to constantly change the selected item if navigating via keyboard
                // so we need to get around it. If we just move focus and set the focused
                // styles to reflect this, then it will look like 2 items are selected in 
                // the dropdown, instead of 1. Instead of reimplementing SelectedItemsControl,
                // which requires a lot, given complex nature of selection & INCC, we hold onto 
                // the focused index and just turn off the :selected Psuedoclass of the actual
                // selected item to give a visual indicator of selection traversal. When we're done
                // everything should return to normal. This does NOT affect use by mouse.

                if (_tempSelectedIndex != -1)
                {
                    var cont = ItemContainerGenerator.ContainerFromIndex(_tempSelectedIndex);
                    if (cont is ComboBoxItem cbi)
                    {
                        ((IPseudoClasses)cont.Classes).Set(":selected", false);
                    }
                }
                _tempSelectedIndex = value;
                if (value != -1)
                {
                    var cont = ItemContainerGenerator.ContainerFromIndex(value);
                    if (cont is ComboBoxItem cbi)
                    {
                        ((IPseudoClasses)cont.Classes).Set(":selected", true);
                    }
                }

                if (SelectedIndex != -1)
                {
                    var cont = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
                    if (cont is ComboBoxItem cbi)
                    {
                        ((IPseudoClasses)cont.Classes).Set(":selected", value == SelectedIndex || value == -1);
                    }
                }
            }
        }


        ///////////////////
        //HELPER PROPERTIES
        //////////////////

        private int TextBoxSelectionStart
        {
            get => _textBox != null ? Math.Min(_textBox.SelectionStart, _textBox.SelectionEnd) : 0;
        }

        private int TextBoxSelectionLength
        {
            get => _textBox != null ? Math.Abs(_textBox.SelectionEnd - _textBox.SelectionStart) : 0;
        }


        ////////////
        //EVENTS
        ////////////
        
        /// <summary>
        /// Occurs when the user submits some text that does not correspond to an item in the ComboBox dropdown list.
        /// </summary>
        public event TypedEventHandler<ComboBox, ComboBoxTextSubmittedEventArgs> TextSubmitted;
        public event EventHandler<object> DropDownClosed;
        public event EventHandler<object> DropDownOpened;
        //public event SelectionChangedEventHandler SelectionChanged;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _textBoxSubs?.Dispose();
            _textBoxSubs = null;

            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpened;
                _popup.Closed -= OnPopupClosed;
            }

            if (_overlayBorder != null)
            {
                _overlayBorder.PointerPressed -= OnDropDownOverlayPointerPressed;
                _overlayBorder.PointerReleased -= OnDropDownOverlayPointerReleased;
                _overlayBorder.PointerCaptureLost -= OnDropDownOverlayPointerCaptureLost;
                _overlaySubscription?.Dispose();
                _overlaySubscription = null;
            }
            
            base.OnApplyTemplate(e);

            _popup = e.NameScope.Find<Popup>("Popup");
            if (_popup != null)
            {
                _popup.Opened += OnPopupOpened;
                _popup.Closed += OnPopupClosed;
                _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom;
                _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Bottom;
                _popup.PlacementMode = PlacementMode.AnchorAndGravity;
            }

            _textBox = e.NameScope.Find<TextBox>("EditableText");
            if (_textBox != null)
            {
                _textBoxSubs = _textBox.GetObservable(TextBox.TextProperty)
                    .Skip(1)
                    .Subscribe(_ => OnTextBoxTextChanged());
            }

            _selectionBoxPresenter = e.NameScope.Find<ContentPresenter>("ContentPresenter");

            _overlayBorder = e.NameScope.Get<Border>("DropDownOverlay");
            if (_overlayBorder != null)
            {
                _overlayBorder.PointerPressed += OnDropDownOverlayPointerPressed;
                _overlayBorder.PointerReleased += OnDropDownOverlayPointerReleased;
                _overlayBorder.PointerCaptureLost += OnDropDownOverlayPointerCaptureLost;
                _overlaySubscription = _overlayBorder.GetObservable(IsPointerOverProperty).Subscribe(OnDropDownOverlayPointerOverChanged);
            }   

            if (IsEditable && !string.IsNullOrEmpty(Text))
            {
                UpdateSelectionBoxItem(Text);
            }
            else
            {
                UpdateSelectionBoxItem(SelectedItem);
            }
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                OnTextChanged(change.GetNewValue<string>());
            }
            else if (change.Property == SelectedItemProperty)
            {
                OnSelectedItemChanged(change);
            }
            else if (change.Property == ItemTemplateProperty)
            {
                if (!_settingItemTemplateFromValueMemberBinding)
                    _itemTemplateIsFromValueMemberBinding = false;
            }
            else if (change.Property == ItemsProperty)
            {
                OnItemsChanged();
            }
            else if (change.Property == IsTextSearchEnabledProperty)
            {
                OnIsTextSearchEnabledChanged();
            }
            else if (change.Property == IsEditableProperty)
            {
                OnIsEditableChanged();
            }
        }

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<ComboBoxItem>(this, ComboBoxItem.ContentProperty,
                ComboBoxItem.ContentTemplateProperty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled)
                return;


            if (e.Key == Key.F4 ||
                ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt)))
            {
                _wasDropDownOpenedViaKeyboard = true;
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
            }
            else if (IsDropDownOpen && e.Key == Key.Escape)
            {
                _wasDropDownOpenedViaKeyboard = true;
                IsDropDownOpen = false;

                //Because we update the SelectionBox value when navigating via keyboard when the dropdown
                //is open, we need to make sure to revert the value if we cancel the dropdown
                UpdateSelectionBoxItem(SelectedItem);
                e.Handled = true;
            }
            else if (IsDropDownOpen && e.Key == Key.Enter)
            {
                if (IsEditable && _textBox.IsFocused)
                {
                    OnTextSubmitted();
                }
                else
                {
                    SelectFocusedItem();                    
                }
                IsDropDownOpen = false;
                e.Handled = true;
            }
            else if (!IsDropDownOpen)
            {
                if (e.Key == Key.Down)
                {
                    SelectNext();
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    SelectPrev();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter && IsEditable)
                {
                    OnTextSubmitted();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape && IsEditable)
                {
                    UpdateSelectionBoxItem(SelectedItem);
                    HighlightTextBoxText();
                    e.Handled = true;
                }
            }
            else if (IsDropDownOpen && SelectedIndex < 0 && ItemCount > 0 && 
                (e.Key == Key.Up || e.Key == Key.Down))
            {
                var firstChild = Presenter?.Panel?.Children.FirstOrDefault(c => CanFocus(c));
                if (firstChild != null)
                {
                    FocusManager.Instance?.Focus(firstChild, NavigationMethod.Directional);
                    e.Handled = true;
                }
            }
            else if (IsDropDownOpen && IsEditable && _textBox.IsFocused)
            {
                if (e.Key == Key.Up)
                {
                    var selIndex = SelectedIndex - 1;
                    var cont = ItemContainerGenerator.ContainerFromIndex(selIndex);
                    if (cont == null)
                    {
                        ScrollIntoView(SelectedIndex);
                        cont = ItemContainerGenerator.ContainerFromIndex(selIndex);
                    }

                    if (cont != null)
                    {
                        FocusManager.Instance?.Focus(cont, NavigationMethod.Directional);
                        e.Handled = true;
                    }                    
                }
                else if (e.Key == Key.Down)
                {
                    var selIndex = SelectedIndex + 1;
                    var cont = ItemContainerGenerator.ContainerFromIndex(selIndex);
                    if (cont == null)
                    {
                        ScrollIntoView(SelectedIndex);
                        cont = ItemContainerGenerator.ContainerFromIndex(selIndex);
                    }

                    if (cont != null)
                    {
                        FocusManager.Instance?.Focus(cont, NavigationMethod.Directional);
                        e.Handled = true;
                    }
                }
               
            }
        }

        private bool CanFocus(IControl control) => control.Focusable && control.IsEffectivelyEnabled && control.IsVisible;

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            if (IsEditable && !IsDropDownOpen)
            {
                FocusManager.Instance?.Focus(_textBox, e.NavigationMethod);
                if (!_isFocused)
                {
                    _isFocused = true;
                    HighlightTextBoxText();
                }                
            }
        }

        protected override void OnLostFocus(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (IsEditable && !HasImplicitFocus())
            {
                _isFocused = false;

                if (_hasUnsubmittedText)
                {
                    OnTextSubmitted();
                }

                if (!IsLightDismissEnabled && IsDropDownOpen)
                {
                    IsDropDownOpen = false;
                }
                ClearTextBoxSelection();
            }
        }

        protected bool HasImplicitFocus()
        {
            IVisual focused = FocusManager.Instance?.Current;

            while (focused != null)
            {
                if (object.ReferenceEquals(focused, this))
                {
                    return true;
                }

                IVisual parent = focused.GetVisualParent();
                if (parent == null)
                {
                    IControl element = focused as IControl;
                    if (element != null)
                    {
                        parent = element.Parent;
                    }
                }
                focused = parent;
            }

            return false;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                if (_popup?.IsInsidePopup((IVisual)e.Source) == true)
                {
                    if (UpdateSelectionFromEventSource(e.Source)) 
                    {
                        IsDropDownOpen = false;
                        e.Handled = true;
                    }
                }
                else if (!IsEditable)
                {
                    IsDropDownOpen = !IsDropDownOpen;
                    e.Handled = true;
                }
                else
                {

                }
            }

            base.OnPointerReleased(e);
        }



        private void OnDropDownOverlayPointerOverChanged(bool x)
        {
            PseudoClasses.Set(":overlaypointerover", x);
        }
        private void OnDropDownOverlayPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                PseudoClasses.Set(":overlaypressed", true);
            }
        }
        private void OnDropDownOverlayPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                PseudoClasses.Set(":overlaypressed", false);
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
            }
        }
        private void OnDropDownOverlayPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            PseudoClasses.Set(":overlaypressed", false);
        }


        private void OnPopupClosed(object sender, EventArgs e)
        {
            FocusManager.Instance?.Focus(this, _wasDropDownOpenedViaKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
            _wasDropDownOpenedViaKeyboard = false;

            TempSelectedIndex = -1;
            if (IsEditable)
            {
                HighlightTextBoxText();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (!IsEditable)
            {
                TryFocusSelectedItem();
            }
            else
            {
                FocusManager.Instance?.Focus(_textBox, _wasDropDownOpenedViaKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
                _wasDropDownOpenedViaKeyboard = false;
            }
            
            HighlightTextBoxText();
            TempSelectedIndex = SelectedIndex;

            //Logic to center popup & selected item over ComboBox
            //Do NOT do if in edit mode (display below)
            if (!IsEditable && _popup.Host != null)
            {
                bool handled = false;
                if (SelectedIndex != -1)
                {
                    var cont = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

                    if (cont != null)
                    {
                        var dy = cont.PointToScreen(new Point(0, 0)).Y - _popup.PointToScreen(new Point(0, 0)).Y;

                        _popup.Host.ConfigurePosition(_popup.PlacementTarget,
                            PlacementMode.AnchorAndGravity, new Point(0, -dy),
                             Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom,
                              Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Bottom,
                               Avalonia.Controls.Primitives.PopupPositioning.PopupPositionerConstraintAdjustment.All);

                        handled = true;
                    }
                }

                if (!handled)
                {
                    //No selected item or container, center Popup instead
                    var dy = _popup.Child.Bounds.Height / 2 + this.Bounds.Height / 2;

                    _popup.Host.ConfigurePosition(_popup.PlacementTarget,
                        PlacementMode.AnchorAndGravity, new Point(0, -dy),
                         Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom,
                          Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Bottom,
                           Avalonia.Controls.Primitives.PopupPositioning.PopupPositionerConstraintAdjustment.All);
                }

            }
        }

        protected virtual void OnDropDownOpened()
        {
            DropDownOpened?.Invoke(this, null);
        }

        protected virtual void OnDropDownClosed()
        {
            DropDownClosed?.Invoke(this, null);
        }


        private void OnTextChanged(string newValue) 
        {
            TextUpdated(newValue, false);
        }

        private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs args) 
        {
            if (_ignoreSelectedItemChangedOnTextSubmission)
                return;

            UpdateSelectionBoxItem(args.NewValue);

            //Fail safe...
            _ignoreTextPropertyChange++;
            Text = FormatValue(args.NewValue);
            _ignoreTextPropertyChange--;
        }
        
        private void OnItemsChanged() 
        {

        }

        private void OnIsTextSearchEnabledChanged() { }

        private void OnIsEditableChanged()
        {
            PseudoClasses.Set(":editable", IsEditable);
        }

        internal void OnItemFocused(ComboBoxItem item, GotFocusEventArgs args)
        {
            if (IsDropDownOpen && item.IsFocused && item.IsArrangeValid)
            {
                item.BringIntoView();

                //If navigating via keyboard in dropdown, we want to temporarily select
                //the current item to hold on to & update the SelectionBoxItem. This 
                //particularly applies to when IsEditable = true
                if (args.NavigationMethod == NavigationMethod.Directional || 
                    args.NavigationMethod == NavigationMethod.Tab)
                {
                    TempSelectedIndex = ItemContainerGenerator.IndexFromContainer(item);
                    UpdateSelectionBoxItem(item.Content);
                }
            }
        }

        private void UpdateSelectionBoxItem(object item)
        {            
            if (IsEditable && item != null)
            {
                UpdateTextValue(FormatValue(item), false);
            }
            

            var cc = item as IContentControl;
            if (cc != null)
            {
                item = cc.Content;
            }

            var ctrl = item as IControl;

            if (ctrl != null)
            {
                if (VisualRoot is object)
                {
                    ctrl.Measure(Size.Infinity);

                    SelectionBoxItem = new Rectangle
                    {
                        Width = ctrl.DesiredSize.Width,
                        Height = ctrl.DesiredSize.Height,
                        Fill = new VisualBrush
                        {
                            Visual = ctrl,
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Left
                        }
                    };
                }
            }
            else
            {
                if (item == null)
                {
                    SelectionBoxItem = Text;
                }
                else if (item != null && ValueMemberBinding != null)
                {
                    SelectionBoxItem = FormatValue(item);
                }
                else
                {
                    SelectionBoxItem = item;
                }                
            }

            if (_selectionBoxPresenter != null)
            {
                _selectionBoxPresenter.Content = SelectionBoxItem;
            }
        }

        private void TryFocusSelectedItem()
        {
            if (IsDropDownOpen)
            {
                var selIndex = SelectedIndex;
                if (selIndex != -1)
                {
                    var container = ItemContainerGenerator.ContainerFromIndex(selIndex);

                    if (container == null && SelectedIndex != -1)
                    {
                        ScrollIntoView(Selection.SelectedIndex);
                        container = ItemContainerGenerator.ContainerFromIndex(selIndex);
                    }

                    if (container != null && CanFocus(container))
                    {
                        FocusManager.Instance?.Focus(container, _wasDropDownOpenedViaKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
                        _wasDropDownOpenedViaKeyboard = false;
                    }
                }
            }
        }

        private void SelectFocusedItem()
        {
            foreach (ItemContainerInfo dropdownItem in ItemContainerGenerator.Containers)
            {
                if (dropdownItem.ContainerControl.IsFocused)
                {
                    SelectedIndex = dropdownItem.Index;
                    break;
                }
            }
        }


        private void SelectNext()
        {
            int next = SelectedIndex + 1;

            if (next >= ItemCount)
                next = 0;

            SelectedIndex = next;
        }

        private void SelectPrev()
        {
            int prev = SelectedIndex - 1;

            if (prev < 0)
                prev = ItemCount - 1;

            SelectedIndex = prev;
        }




        private string FormatValue(object value, bool clearDC)
        {
            string res = FormatValue(value);
            if (clearDC && _valueBindingEvaluator != null)
            {
                _valueBindingEvaluator.ClearDataContext();
            }

            return res;
        }

        protected virtual string FormatValue(object value)
        {
            if (_valueBindingEvaluator != null)
            {
                return _valueBindingEvaluator.GetDynamicValue(value) ?? string.Empty;
            }

            return value == null ? string.Empty : value.ToString();
        }


        private void OnTextBoxTextChanged()
        {
            Dispatcher.UIThread.Post(() => TextUpdated(_textBox.Text, true));
        }

        private void UpdateTextValue(string value)
        {
            UpdateTextValue(value, null);
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

            if (newText == null)
            {
                newText = string.Empty;
            }

            if(IsTextSearchEnabled && _textBox!= null && TextBoxSelectionLength > 0 && TextBoxSelectionStart != _textBox.Text.Length)
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
                    int selStart = TextBoxSelectionStart;

                    if (selStart == _text.Length && selStart > _textSelectionStart)
                    {
                        object top = TryGetMatch(text, Items, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith));
                        
                        if (top != null)
                        {
                            
                            string topString = FormatValue(top, true);

                            int minLength = Math.Min(topString.Length, text.Length);
                            if (AutoCompleteSearch.Equals(text.Substring(0, minLength), topString.Substring(0, minLength)))
                            {
                                UpdateTextValue(topString);

                                _textBox.SelectionStart = curLen;
                                _textBox.SelectionEnd = topString.Length;
                                _temporarySelectedItem = top;

                                if (_isDropDownOpen)
                                {
                                    var index = IndexOf(Items, _temporarySelectedItem);
                                    if (index != -1)
                                    {
                                        TempSelectedIndex = index;
                                    }
                                }                                
                            }
                        }
                    }
                    else
                    {
                        //When textsearch isn't enabled, still do a look up & focus the item in the popup
                        //But don't edit the text...
                        //TODO
                    }
                }
            }


            if (_ignoreTextSelectionChange)
            {
                _ignoreTextSelectionChange = false;
                if (_textBox != null)
                {
                    _textSelectionStart = TextBoxSelectionStart;
                }
            }
        }

        private object TryGetMatch(string searchText, IEnumerable items, AutoCompleteFilterPredicate<string> predicate)
        {
            if (items != null && ItemCount > 0)
            {
                foreach (object o in items)
                {
                    if (predicate(searchText, FormatValue(o)))
                    {
                        return o;
                    }
                }
            }

            return null;
        }

        private void HighlightTextBoxText()
        {
            _textBox.SelectionStart = 0;
            _textBox.SelectionEnd = _textBox.Text?.Length ?? 0;
        }

        private void ClearTextBoxSelection()
        {
            if (_textBox != null)
            {
                int len = _textBox.Text?.Length ?? 0;
                _textBox.SelectionStart = len;
                _textBox.SelectionEnd = len;
            }
        }

        private void OnValueMemberBindingChanged(IBinding value)
        {
            if (_itemTemplateIsFromValueMemberBinding)
            {
                var temp = new FuncDataTemplate<object>((x, _) =>
                {
                    return new ContentControl
                    {
                        [!ContentControl.ContentProperty] = value
                    };
                });

                _settingItemTemplateFromValueMemberBinding = true;
                ItemTemplate = temp;
                _settingItemTemplateFromValueMemberBinding = false;
            }
        }

        private void OnTextSubmitted()
        {
            var args = new ComboBoxTextSubmittedEventArgs(Text);
            OnTextSubmitted(args);
            TextSubmitted?.Invoke(this, args);

            //In WinUI, if args comes back unhandled, the SelectedItem is replaced with 
            //the text, even if it does not occur within the Items source. This isn't possible
            //here b/c current impl of SelectingItemsControl/SelectionModel requires the selection
            //to be present in the Items source, and if its not, sets it to null. So for now
            //this will just have to be as it is...

            UpdateSelectionBoxItem(_temporarySelectedItem);

            if (!args.Handled)
            {
                _ignoreSelectedItemChangedOnTextSubmission = true;
                SelectedItem = _temporarySelectedItem;
                _ignoreSelectedItemChangedOnTextSubmission = false;
            }
            _hasUnsubmittedText = false;
            ClearTextBoxSelection();
        }

        protected virtual void OnTextSubmitted(ComboBoxTextSubmittedEventArgs args) { }



        //TEMPLATE PARTS
        private Popup _popup;
        private TextBox _textBox;
        private Border _overlayBorder;
        private ContentPresenter _selectionBoxPresenter;
        private IDisposable _overlaySubscription;
        private IDisposable _textBoxSubs;


        //SELECTION
        private int _tempSelectedIndex = -1;

        private object _temporarySelectedItem;

        private bool _wasDropDownOpenedViaKeyboard;
        private bool _isFocused;
        private bool _hasUnsubmittedText;
        
        private AutoCompleteBox.BindingEvaluator<string> _valueBindingEvaluator;
        private int _ignoreTextPropertyChange;
        private bool _ignoreTextSelectionChange;
        private int _textSelectionStart;
        private bool _itemTemplateIsFromValueMemberBinding = true;
        private bool _settingItemTemplateFromValueMemberBinding;
        private bool _ignoreSelectedItemChangedOnTextSubmission;


        private bool _isDropDownOpen;
        private bool _isSelectionBoxHighlighted = true;
        
        
        private object _selectionBoxItem;
        //private IDisposable _subscriptionsOnOpen;
        private string _text;
        private ComboBoxSelectionChangedTrigger _changeTrigger;



        //Taken from AutoCompleteBox
        /// <summary>
        /// A predefined set of filter functions for the known, built-in
        /// AutoCompleteFilterMode enumeration values.
        /// </summary>
        private static class AutoCompleteSearch
        {
            /// <summary>
            /// Index function that retrieves the filter for the provided
            /// AutoCompleteFilterMode.
            /// </summary>
            /// <param name="FilterMode">The built-in search mode.</param>
            /// <returns>Returns the string-based comparison function.</returns>
            public static AutoCompleteFilterPredicate<string> GetFilter(AutoCompleteFilterMode FilterMode)
            {
                switch (FilterMode)
                {
                    case AutoCompleteFilterMode.Contains:
                        return Contains;

                    case AutoCompleteFilterMode.ContainsCaseSensitive:
                        return ContainsCaseSensitive;

                    case AutoCompleteFilterMode.ContainsOrdinal:
                        return ContainsOrdinal;

                    case AutoCompleteFilterMode.ContainsOrdinalCaseSensitive:
                        return ContainsOrdinalCaseSensitive;

                    case AutoCompleteFilterMode.Equals:
                        return Equals;

                    case AutoCompleteFilterMode.EqualsCaseSensitive:
                        return EqualsCaseSensitive;

                    case AutoCompleteFilterMode.EqualsOrdinal:
                        return EqualsOrdinal;

                    case AutoCompleteFilterMode.EqualsOrdinalCaseSensitive:
                        return EqualsOrdinalCaseSensitive;

                    case AutoCompleteFilterMode.StartsWith:
                        return StartsWith;

                    case AutoCompleteFilterMode.StartsWithCaseSensitive:
                        return StartsWithCaseSensitive;

                    case AutoCompleteFilterMode.StartsWithOrdinal:
                        return StartsWithOrdinal;

                    case AutoCompleteFilterMode.StartsWithOrdinalCaseSensitive:
                        return StartsWithOrdinalCaseSensitive;

                    case AutoCompleteFilterMode.None:
                    case AutoCompleteFilterMode.Custom:
                    default:
                        return null;
                }
            }

            /// <summary>
            /// An implementation of the Contains member of string that takes in a
            /// string comparison. The traditional .NET string Contains member uses
            /// StringComparison.Ordinal.
            /// </summary>
            /// <param name="s">The string.</param>
            /// <param name="value">The string value to search for.</param>
            /// <param name="comparison">The string comparison type.</param>
            /// <returns>Returns true when the substring is found.</returns>
            private static bool Contains(string s, string value, StringComparison comparison)
            {
                return s.IndexOf(value, comparison) >= 0;
            }

            /// <summary>
            /// Check if the string value begins with the text.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool StartsWith(string text, string value)
            {
                return value.StartsWith(text, StringComparison.CurrentCultureIgnoreCase);
            }

            /// <summary>
            /// Check if the string value begins with the text.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool StartsWithCaseSensitive(string text, string value)
            {
                return value.StartsWith(text, StringComparison.CurrentCulture);
            }

            /// <summary>
            /// Check if the string value begins with the text.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool StartsWithOrdinal(string text, string value)
            {
                return value.StartsWith(text, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Check if the string value begins with the text.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool StartsWithOrdinalCaseSensitive(string text, string value)
            {
                return value.StartsWith(text, StringComparison.Ordinal);
            }

            /// <summary>
            /// Check if the prefix is contained in the string value. The current
            /// culture's case insensitive string comparison operator is used.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool Contains(string text, string value)
            {
                return Contains(value, text, StringComparison.CurrentCultureIgnoreCase);
            }

            /// <summary>
            /// Check if the prefix is contained in the string value.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool ContainsCaseSensitive(string text, string value)
            {
                return Contains(value, text, StringComparison.CurrentCulture);
            }

            /// <summary>
            /// Check if the prefix is contained in the string value.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool ContainsOrdinal(string text, string value)
            {
                return Contains(value, text, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Check if the prefix is contained in the string value.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool ContainsOrdinalCaseSensitive(string text, string value)
            {
                return Contains(value, text, StringComparison.Ordinal);
            }

            /// <summary>
            /// Check if the string values are equal.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool Equals(string text, string value)
            {
                return value.Equals(text, StringComparison.CurrentCultureIgnoreCase);
            }

            /// <summary>
            /// Check if the string values are equal.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool EqualsCaseSensitive(string text, string value)
            {
                return value.Equals(text, StringComparison.CurrentCulture);
            }

            /// <summary>
            /// Check if the string values are equal.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool EqualsOrdinal(string text, string value)
            {
                return value.Equals(text, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Check if the string values are equal.
            /// </summary>
            /// <param name="text">The AutoCompleteBox prefix text.</param>
            /// <param name="value">The item's string value.</param>
            /// <returns>Returns true if the condition is met.</returns>
            public static bool EqualsOrdinalCaseSensitive(string text, string value)
            {
                return value.Equals(text, StringComparison.Ordinal);
            }
        }
    }

    
}
