using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.Core;
using System;
using System.Globalization;
using FluentAvalonia.Core.Attributes;
using Avalonia.Data;

namespace FluentAvalonia.UI.Controls
{
    public class NumberBox : TemplatedControl
    {
        public NumberBox()
        {
            AddHandler(PointerPressedEvent, OnPointerPressedPreview, RoutingStrategies.Tunnel);
        }

        public static readonly DirectProperty<NumberBox, bool> AcceptsExpressionProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, bool>(nameof(AcceptsExpression),
                x => x.AcceptsExpression, (x, v) => x.AcceptsExpression = v);

        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<NumberBox, string>(nameof(Description));

        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<NumberBox, object>(nameof(Header));

        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<NumberBox, IDataTemplate>(nameof(HeaderTemplate));

        public static readonly StyledProperty<bool> IsWrapEnabledProperty =
            AvaloniaProperty.Register<NumberBox, bool>(nameof(IsWrapEnabled));

        public static readonly DirectProperty<NumberBox, double> LargeChangeProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(LargeChangeProperty),
                x => x.LargeChange, (x, v) => x.LargeChange = v);

        public static readonly DirectProperty<NumberBox, double> MinimumProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(Minimum),
                x => x.Minimum, (x, v) => x.Minimum = v);

        public static readonly DirectProperty<NumberBox, double> MaximumProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(Maximum),
                x => x.Maximum, (x, v) => x.Maximum = v);

        //Skip NumberFormatter

        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<NumberBox, string>(nameof(PlaceholderText));

        //Skip PreventKeyboardDisplayOnProgrammaticFocus

        public static readonly DirectProperty<NumberBox, FlyoutBase> SelectionFlyoutProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, FlyoutBase>(nameof(SelectionFlyout),
                x => x.SelectionFlyout, (x, v) => x.SelectionFlyout = v);

        public static readonly StyledProperty<IBrush> SelectionHighlightColorProperty =
            AvaloniaProperty.Register<NumberBox, IBrush>(nameof(SelectionHighlightColor));

        public static readonly DirectProperty<NumberBox, double> SmallChangeProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(SmallChangeProperty),
                x => x.SmallChange, (x, v) => x.SmallChange = v);

        public static readonly StyledProperty<NumberBoxSpinButtonPlacementMode> SpinButtonPlacementModeProperty =
            AvaloniaProperty.Register<NumberBox, NumberBoxSpinButtonPlacementMode>(nameof(SpinButtonPlacementMode), NumberBoxSpinButtonPlacementMode.Hidden);

        public static readonly DirectProperty<NumberBox, string> TextProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, string>(nameof(Text),
                x => x.Text, (x, v) => x.Text = v, defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<NumberBox, TextReadingOrder> TextReadingOrderProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, TextReadingOrder>(nameof(TextReadingOrder),
                x => x.TextReadingOrder, (x, v) => x.TextReadingOrder = v);

        public static readonly DirectProperty<NumberBox, NumberBoxValidationMode> ValidationModeProperty =
           AvaloniaProperty.RegisterDirect<NumberBox, NumberBoxValidationMode>(nameof(ValidationMode),
               x => x.ValidationMode, (x, v) => x.ValidationMode = v);

        public static readonly DirectProperty<NumberBox, double> ValueProperty =
             AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(Value),
                 x => x.Value, (x, v) => x.Value = v, defaultBindingMode: BindingMode.TwoWay);

        //Skip InputScope

        public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
            TextBlock.TextAlignmentProperty.AddOwner<NumberBox>();

		public static readonly DirectProperty<NumberBox, string> SimpleNumberFormatProperty =
			AvaloniaProperty.RegisterDirect<NumberBox, string>(nameof(SimpleNumberFormat),
				x => x.SimpleNumberFormat, (x, v) => x.SimpleNumberFormat = v);


        public bool AcceptsExpression
        {
            get => _acceptsExpression;
            set => SetAndRaise(AcceptsExpressionProperty, ref _acceptsExpression, value);
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public IDataTemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public bool IsWrapEnabled
        {
            get => GetValue(IsWrapEnabledProperty);
            set => SetValue(IsWrapEnabledProperty, value);
        }

        public double LargeChange
        {
            get => _largeChange;
            set 
            {
                SetAndRaise(LargeChangeProperty, ref _largeChange, value);
                UpdateSpinButtonEnabled();
            }
        }

        public double Minimum
        {
            get => _minimum;
            set
            {
                if (value > _maxmimum)
                    value = _maxmimum;
                SetAndRaise(MinimumProperty, ref _minimum, value);
                CoerceValueIfNeeded(value, _maxmimum);
                UpdateSpinButtonEnabled();
            }
        }

        public double Maximum
        {
            get => _maxmimum;
            set
            {
                if (value < _minimum)
                    value = _minimum;
                SetAndRaise(MaximumProperty, ref _maxmimum, value);
                CoerceValueIfNeeded(_minimum, value);
                UpdateSpinButtonEnabled();
            }
        }

        /// <summary>
        /// A function for customizing the format of the NumberBox Value Text.
        /// </summary>
        /// <remarks>
        /// .NET doesn't have all of the formatting stuff from WinUI/WinRT, thus doing fancy things
        /// requires a bit more manual work, and I'm not about to attempt to replicate the NumberFormatters :D
		/// NOTE: This cannot be used if <see cref="SimpleNumberFormat"/> is in use
        /// </remarks>
        public Func<double, string> NumberFormatter { get; set; }

		/// <summary>
		/// Use this for simple number formatting using normal .net formatting. Resulting string must still
		/// be numeric in value, no special characters, as they are not removed when attempting to convert
		/// text to value
		/// </summary>
		/// <remarks>
		/// This property cannot be used if <see cref="NumberFormatter"/> is also in use
		/// </remarks>
		public string SimpleNumberFormat
		{
			get => _simpleFormat;
			set
			{
				if (NumberFormatter != null)
					throw new InvalidOperationException("NumberFormatter must be null");

				if (SetAndRaise(SimpleNumberFormatProperty, ref _simpleFormat, value))
				{
					UpdateTextToValue();
				}
			}
		}

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        [NotImplemented]
        public FlyoutBase SelectionFlyout
        {
            get => _selectionFlyout;
            set => SetAndRaise(SelectionFlyoutProperty, ref _selectionFlyout, value);
        }

        public IBrush SelectionHighlightColor
        {
            get => GetValue(SelectionHighlightColorProperty);
            set => SetValue(SelectionHighlightColorProperty, value);
        }

        public double SmallChange
        {
            get => _smallChange;
            set
            {
                SetAndRaise(SmallChangeProperty, ref _smallChange, value);
                UpdateSpinButtonEnabled();
            }
        }

        public NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode
        {
            get => GetValue(SpinButtonPlacementModeProperty);
            set => SetValue(SpinButtonPlacementModeProperty, value);
        }

        public string Text
        {
            get => _text;
            set
            {
                if (!_textUpdating && SetAndRaise(TextProperty, ref _text, value))
                {
                    UpdateValueToText();
                }
            }
        }

        [NotImplemented]
        public TextReadingOrder TextReadingOrder
        {
            get => _textReadingOrder;
            set
            {
                SetAndRaise(TextReadingOrderProperty, ref _textReadingOrder, value);
            }
        }

        public NumberBoxValidationMode ValidationMode
        {
            get => _validationMode;
            set
            {
                if (SetAndRaise(ValidationModeProperty, ref _validationMode, value))
                {
                    ValidateInput();
                    UpdateSpinButtonEnabled();
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
				if (!double.IsNaN(value) || !double.IsNaN(_value))
				{
					var old = _value;
					value = CoerceValueToRange(value);
					if (SetAndRaise(ValueProperty, ref _value, value))
					{
						OnValueChanged(old, value);
					}
				}
			}
        }

        public TextAlignment TextAlignment
        {
            get => GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public event TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs> ValueChanged;


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _spinDown = e.NameScope.Find<RepeatButton>("DownSpinButton");
            _popupDownButton = e.NameScope.Find<RepeatButton>("PopupDownSpinButton");
            if (_spinDown != null)
            {
                _spinDown.Click += OnSpinDownClick;
            }
            if (_popupDownButton != null)
            {
                _popupDownButton.Click += OnSpinDownClick;
            }

            _spinUp = e.NameScope.Find<RepeatButton>("UpSpinButton");
            _popupUpButton = e.NameScope.Find<RepeatButton>("PopupUpSpinButton");
            if (_spinUp != null)
            {
                _spinUp.Click += OnSpinUpClick;
            }
            if (_popupUpButton != null)
            {
                _popupUpButton.Click += OnSpinUpClick;
            }

            _textBox = e.NameScope.Find<TextBox>("InputBox");
            if (_textBox != null)
            {
                _textBox.AddHandler(KeyDownEvent, OnNumberBoxKeyDown, RoutingStrategies.Tunnel);

                _textBox.KeyUp += OnNumberBoxKeyUp;
            }

            _popup = e.NameScope.Find<Popup>("UpDownPopup");
			if (_popup != null)
			{
				_popup.OverlayInputPassThroughElement = this;
			}

			UpdateSpinButtonPlacement();
            UpdateSpinButtonEnabled();

            //UpdateVisualStateForIsEnabledChange();

            if (_value == double.NaN && 
                !string.IsNullOrEmpty(_text))
            {
                // If Text has been set, but Value hasn't, update Value based on Text.
                UpdateValueToText();
            }
            else
            {
                UpdateTextToValue();
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsWrapEnabledProperty)
            {
                UpdateSpinButtonEnabled();
            }
            else if (change.Property == SpinButtonPlacementModeProperty)
            {
                UpdateSpinButtonPlacement();
            }
            else if (change.Property == HeaderProperty || change.Property == HeaderTemplateProperty)
            {
                UpdateHeaderPresenterState();
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
						
            if (_textBox != null)
            {
				_textBox.SelectAll();
            }

            if (SpinButtonPlacementMode == NumberBoxSpinButtonPlacementMode.Compact)
            {
                if (_popup != null)
                {
                    _popup.IsOpen = true;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            if (_textBox != null && IsKeyboardFocusWithin)
            {
                //WHO DECIDED THIS NEEDS TO BE A VECTOR
                var delta = e.Delta.Y;
                if (delta > 0)
                {
                    StepValue(SmallChange);
                }
                else
                {
                    StepValue(-SmallChange);
                }
                e.Handled = true;
            }
        }

        private void OnPointerPressedPreview(object sender, PointerPressedEventArgs args)
        {
            //Hack: B/c we make popup lightdismissable, we need to ensure we can reopen the popup if focus
            //never leaves the control, but we click back on it
            //Do this in Preview b/c TextBox will handle pointer event
            if (SpinButtonPlacementMode == NumberBoxSpinButtonPlacementMode.Compact &&
                _popup != null && !_popup.IsOpen && IsKeyboardFocusWithin)
            {
                _popup.IsOpen = true;
            }
        }

        private void OnValueChanged(double oldValue, double newValue)
        {
            // This handler may change Value; don't send extra events in that case.
            if (!_valueUpdating)
            {
                try
                {
                    _valueUpdating = true;

                    if (!double.IsNaN(newValue) && !double.IsNaN(oldValue))
                    {
                        // Fire ValueChanged event
                        var ea = new NumberBoxValueChangedEventArgs(oldValue, newValue);

                        ValueChanged?.Invoke(this, ea);
                    }

                    UpdateTextToValue();
                    UpdateSpinButtonEnabled();

                }
                finally
                {
                    _valueUpdating = false;
                }
            }
        }

        private void UpdateValueToText()
        {
            if (_textBox != null)
            {
                _textBox.Text = Text;
                ValidateInput();
            }
        }

        private void ValidateInput()
        {
            // Validate the content of the inner textbox
            if (_textBox == null)
                return;

            var text = _textBox.Text.Trim();

            // Handles empty TextBox case, set text ot current value
            if (string.IsNullOrEmpty(text))
            {
                Value = double.NaN;
            }
            else
            {
                var value = AcceptsExpression ? NumberBoxParser.Compute(text) :
                    ParseDouble(text);

                if (value == null)
                {
                    if (ValidationMode == NumberBoxValidationMode.InvalidInputOverwritten)
                    {
                        // Override text to current value
                        UpdateTextToValue();
                    }
                }
                else
                {
                    if (value.Value == _value)
                    {
                        // Even if the value hasn't changed, we still want to update the text (e.g. Value is 3, user types 1 + 2, we want to replace the text with 3)
                        UpdateTextToValue();
                    }
                    else
                    {
                        Value = value.Value;
                    }
                }
            }
        }

        //Replaces INumberParser in winrt
        private double? ParseDouble(string txt)
        {
            if (double.TryParse(txt, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
            {
                return result;
            }

            return null;
        }

        private void OnSpinDownClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            StepValue(-SmallChange);
        }

        private void OnSpinUpClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            StepValue(SmallChange);
        }

        private void OnNumberBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    StepValue(SmallChange);
                    e.Handled = true;
                    break;

                case Key.Down:
                    StepValue(-SmallChange);
                    e.Handled = true;
                    break;

                case Key.PageUp:
                    StepValue(LargeChange);
                    e.Handled = true;
                    break;

                case Key.PageDown:
                    StepValue(-LargeChange);
                    e.Handled = true;
                    break;
            }
        }

        private void OnNumberBoxKeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.Enter:
                    ValidateInput();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    UpdateTextToValue();
                    e.Handled = true;
                    break;
            }
        }
        
        public void StepValue(double change)
        {
            // Before adjusting the value, validate the contents of the textbox so we don't override it.
            ValidateInput();

            var newVal = _value;
            if (!double.IsNaN(newVal))
            {
                newVal += change;
                
                if (IsWrapEnabled)
                {
                    if (newVal > _maxmimum)
                        newVal = _minimum;
                    else if (newVal < _minimum)
                        newVal = _maxmimum;                    
                }

                Value = newVal;

                // We don't want the caret to move to the front of the text for example when using the up/down arrows
                // to change the numberbox value.
                MoveCaretToTextEnd();
            }
        }

        // Updates TextBox.Text with the formatted Value
        private void UpdateTextToValue()
        {
            if (_textBox == null)
                return;

            string newText = "";

            if (!double.IsNaN(_value))
            {
                // Round to 12 digits (standard .net rounding per WinUI in the NumberBox source)
                // We do this to prevent weirdness from floating point imprecision
                var newValue = Math.Round(_value, 12);
				if (SimpleNumberFormat != null)
				{
					newText = newValue.ToString(_simpleFormat);
				}
                else if (NumberFormatter != null)
                {
                    newText = NumberFormatter(newValue);
                }
                else
                {
                    newText = newValue.ToString();
                }
            }

            _textBox.Text = newText;

            try
            {
                _textUpdating = true;
                Text = newText;
            }
            finally
            {
                _textUpdating = false;
                MoveCaretToTextEnd(); //Add this
            }
        }

        private void UpdateSpinButtonPlacement()
        {
            var sbm = SpinButtonPlacementMode;

            if (sbm == NumberBoxSpinButtonPlacementMode.Inline)
            {
                PseudoClasses.Set(":spinvisible", true);
                PseudoClasses.Set(":spinpopup", false);
                PseudoClasses.Set(":spincollapsed", false);
            }
            else if (sbm == NumberBoxSpinButtonPlacementMode.Compact)
            {
                PseudoClasses.Set(":spinvisible", false);
                PseudoClasses.Set(":spinpopup", true);
                PseudoClasses.Set(":spincollapsed", false);
            }
            else
            {
                PseudoClasses.Set(":spinvisible", false);
                PseudoClasses.Set(":spinpopup", false);
                PseudoClasses.Set(":spincollapsed", true);
            }
        }

        private void UpdateSpinButtonEnabled()
        {
            bool isUpEnabled = false;
            bool isDownEnabled = false;

            if (!double.IsNaN(_value))
            {
                if (IsWrapEnabled || ValidationMode != NumberBoxValidationMode.InvalidInputOverwritten)
                {
                    // If wrapping is enabled, or invalid values are allowed, then the buttons should be enabled
                    isUpEnabled = true;
                    isDownEnabled = true;
                }
                else
                {
                    if (_value < _maxmimum)
                        isUpEnabled = true;

                    if (_value > _minimum)
                        isDownEnabled = true;
                }
            }

            PseudoClasses.Set(":updisabled", !isUpEnabled);
            PseudoClasses.Set(":downdisabled", !isDownEnabled);
        }

        private void UpdateHeaderPresenterState()
        {
            bool showHeader = false;

            if (Header != null)
            {
                if (Header is string str)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        showHeader = true;
                    }
                }
                else
                {
                    showHeader = true;
                }
            }

            if (HeaderTemplate != null)
            {
                showHeader = true;
            }

            //Changed to Pseudoclass rather than keeping a ref to the ContentPresenter
            PseudoClasses.Set(":header", showHeader);
        }

        private void MoveCaretToTextEnd()
        {
            if (_textBox != null)
            {
                _textBox.SelectionStart = _textBox.SelectionEnd = _textBox.CaretIndex = _textBox.Text.Length;                
            }
        }


        private void CoerceValueIfNeeded(double min, double max)
        {
            if (double.IsNaN(_value))
                return;

            if (_value < min)
                Value = min;
            else if (_value > max)
                Value = max;
        }

        private double CoerceValueToRange(double val)
        {
            if (!double.IsNaN(val) && (val > _maxmimum || val < _minimum) && ValidationMode == NumberBoxValidationMode.InvalidInputOverwritten)
            {
                if (val > _maxmimum)
                    return _maxmimum;

                if (val < _minimum)
                    return _minimum;
            }

            return val;
        }

        


        //Template parts
        private RepeatButton _spinDown;
        private RepeatButton _spinUp;
        private TextBox _textBox;
        private Popup _popup;
        private RepeatButton _popupUpButton;
        private RepeatButton _popupDownButton;

        //Fields
        private bool _acceptsExpression;
        private double _largeChange = 10;
        private double _minimum = double.MinValue;
        private double _maxmimum = double.MaxValue;
        private FlyoutBase _selectionFlyout;
        private double _smallChange = 1;
        public string _text = null;
        private TextReadingOrder _textReadingOrder;
        private NumberBoxValidationMode _validationMode;
        private double _value = double.NaN;
		private string _simpleFormat;

        private bool _textUpdating;
        private bool _valueUpdating;
    }
}
