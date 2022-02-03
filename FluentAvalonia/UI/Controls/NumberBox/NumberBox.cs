using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Globalization;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a control that can be used to display and edit numbers.
    /// </summary>
    public partial class NumberBox : TemplatedControl
    {
        public NumberBox()
        {
            AddHandler(PointerPressedEvent, OnPointerPressedPreview, RoutingStrategies.Tunnel);
        }

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

        protected override void UpdateDataValidation<T>(AvaloniaProperty<T> property, BindingValue<T> value)
        {
            if (property == ValueProperty)
            {
                DataValidationErrors.SetError(this, value.Error);
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
            else if (change.Property == LargeChangeProperty || change.Property == SmallChangeProperty)
			{
                UpdateSpinButtonEnabled();
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

            ValidateInput();

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
            // Hack: B/c we make popup lightdismissable, we need to ensure we can reopen the popup if focus
            // never leaves the control, but we click back on it
            // Do this in Preview b/c TextBox will handle pointer event
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

        private void OnNumberBoxKeyUp(object sender, KeyEventArgs e)
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
                MoveCaretToTextEnd(); // Add this
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

        private bool _textUpdating;
        private bool _valueUpdating;
    }
}
