using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using System.Globalization;

namespace FluentAvalonia.UI.Controls;

// NumberBox source is up to date with WinUI as of 5/9/26

/// <summary>
/// Represents a control that can be used to display and edit numbers.
/// </summary>
public partial class FANumberBox : TemplatedControl
{
    public FANumberBox()
    {
        AddHandler(PointerPressedEvent, OnPointerPressedPreview, RoutingStrategies.Tunnel);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UnhookEvents();

        base.OnApplyTemplate(e);

        var spinDownName = FALocalizationHelper.Instance.GetLocalizedStringResource(SR_NumberBoxDownSpinButtonName);
        var spinUpName = FALocalizationHelper.Instance.GetLocalizedStringResource(SR_NumberBoxUpSpinButtonName);

        _spinDown = e.NameScope.Find<RepeatButton>(s_tpDownSpinButton);
        _popupDownButton = e.NameScope.Find<RepeatButton>(s_tpPopupDownSpinButton);

        if (_spinDown != null)
        {
            _spinDown.Click += OnSpinDownClick;

            if (AutomationProperties.GetName(_spinDown) == null)
            {
                AutomationProperties.SetName(_spinDown, spinDownName);
            }
        }
        _popupDownButton?.Click += OnSpinDownClick;
      

        _spinUp = e.NameScope.Find<RepeatButton>(s_tpUpSpinButton);
        _popupUpButton = e.NameScope.Find<RepeatButton>(s_tpPopupUpSpinButton);

        if (_spinUp != null)
        {
            _spinUp.Click += OnSpinUpClick;

            if (AutomationProperties.GetName(_spinUp) == null)
            {
                AutomationProperties.SetName(_spinUp, spinUpName);
            }
        }

        _popupUpButton?.Click += OnSpinUpClick;
        
        _textBox = e.NameScope.Find<TextBox>(s_tpInputBox);
        if (_textBox != null)
        {
            _textBox.AddHandler(KeyDownEvent, OnNumberBoxKeyDown, RoutingStrategies.Tunnel);
            _textBox.KeyUp += OnNumberBoxKeyUp;
            _textBox.Loaded += OnTextBoxLoaded;
        }

        _popup = e.NameScope.Find<Popup>(s_tpUpDownPopup);
        _popup?.OverlayInputPassThroughElement = this;

        UpdateSpinButtonPlacement();
        UpdateSpinButtonEnabled();

        //UpdateVisualStateForIsEnabledChange();
        ReevaluateForwardedUIAProperties();

        if (double.IsNaN(Value) &&
            !string.IsNullOrEmpty(Text))
        {
            // If Text has been set, but Value hasn't, update Value based on Text.
            UpdateValueToText();
        }
        else
        {
            UpdateTextToValue();
        }
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception error)
    {
        base.UpdateDataValidation(property, state, error);

        if (property == ValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            OnValueChanged(change.GetOldValue<double>(), change.GetNewValue<double>());
        }
        else if (change.Property == TextProperty)
        {
            if (!_textUpdating)
            {
                UpdateValueToText();
            }
        }
        else if (change.Property == IsWrapEnabledProperty)
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
        else if (change.Property == ValidationModeProperty)
        {
            ValidateInput();
            UpdateSpinButtonEnabled();
        }
        else if (change.Property == SimpleNumberFormatProperty)
        {
            if (NumberFormatter != null)
                throw new InvalidOperationException("NumberFormatter must be null");

            UpdateTextToValue();
        }
        else if (change.Property == MinimumProperty || change.Property == MaximumProperty)
        {
            UpdateSpinButtonEnabled();
            ReevaluateForwardedUIAProperties();
        }
        else if (change.Property == AutomationProperties.NameProperty)
        {
            OnAutomationPropertiesNamePropertyChanged();
        }
        else if (change.Property == AutomationProperties.LabeledByProperty)
        {
            OnAutomationPropertiesLabeledByPropertyChanged();
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        _textBox?.SelectAll();

        if (SpinButtonPlacementMode == FANumberBoxSpinButtonPlacementMode.Compact)
        {
            _popup?.IsOpen = true;
        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);

        ValidateInput();

        _popup?.IsOpen = false;
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

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FANumberBoxAutomationPeer(this);
    }

    private void OnTextBoxLoaded(object sender, RoutedEventArgs args)
    {
        // Updating again once TextBox is loaded so its visual states are set properly.
        UpdateSpinButtonPlacement();
    }

    private void OnPointerPressedPreview(object sender, PointerPressedEventArgs args)
    {
        // Hack: B/c we make popup lightdismissable, we need to ensure we can reopen the popup if focus
        // never leaves the control, but we click back on it
        // Do this in Preview b/c TextBox will handle pointer event
        if (SpinButtonPlacementMode == FANumberBoxSpinButtonPlacementMode.Compact &&
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

                if (newValue != oldValue && !(double.IsNaN(newValue) && double.IsNaN(oldValue)))
                {
                    // Fire ValueChanged event
                    var ea = new FANumberBoxValueChangedEventArgs(oldValue, newValue);

                    ValueChanged?.Invoke(this, ea);

                    var peer = ControlAutomationPeer.FromElement(this) as FANumberBoxAutomationPeer;
                    peer?.RaiseValueChangedEvent(oldValue, newValue);
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

    private void OnAutomationPropertiesNamePropertyChanged()
    {
        ReevaluateForwardedUIAProperties();
    }

    private void OnAutomationPropertiesLabeledByPropertyChanged()
    {
        ReevaluateForwardedUIAProperties();
    }

    private void ReevaluateForwardedUIAProperties()
    {
        if (_textBox == null)
            return;

        var name = AutomationProperties.GetName(this);
        var minimum = Minimum == -double.MinValue ? string.Empty :
            $" {FALocalizationHelper.Instance.GetLocalizedStringResource(SR_NumberBoxMinimumValueStatus)} {Minimum}";
        var maximum = Maximum == double.MaxValue ? string.Empty :
            $" {FALocalizationHelper.Instance.GetLocalizedStringResource(SR_NumberBoxMaximumValueStatus)} {Maximum}";

        if (!string.IsNullOrEmpty(name))
        {
            AutomationProperties.SetName(_textBox, name + minimum + maximum);
        }
        else
        {
            var header = Header;
            if (header is string s)
            {
                AutomationProperties.SetName(_textBox, s + minimum + maximum);
            }
        }

        var labeledBy = AutomationProperties.GetLabeledBy(this);
        if (labeledBy != null)
        {
            AutomationProperties.SetLabeledBy(_textBox, labeledBy);
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

        var text = _textBox.Text?.Trim();

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
                if (ValidationMode == FANumberBoxValidationMode.InvalidInputOverwritten)
                {
                    // Override text to current value
                    UpdateTextToValue();
                }
            }
            else
            {
                if (value.Value == Value)
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

    private void OnSpinDownClick(object sender, RoutedEventArgs e)
    {
        StepValue(-SmallChange);
    }

    private void OnSpinUpClick(object sender, RoutedEventArgs e)
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

        var newVal = Value;
        var max = Maximum;
        var min = Minimum;
        if (!double.IsNaN(newVal))
        {
            newVal += change;

            if (IsWrapEnabled)
            {
                if (newVal > max)
                    newVal = min;
                else if (newVal < min)
                    newVal = max;
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
        var value = Value;

        if (!double.IsNaN(value))
        {
            // Round to 12 digits (standard .net rounding per WinUI in the NumberBox source)
            // We do this to prevent weirdness from floating point imprecision
            var newValue = Math.Round(value, 12);
            if (SimpleNumberFormat != null)
            {
                newText = newValue.ToString(SimpleNumberFormat);
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

            // GH 389: only move caret if we're focused, otherwise it triggers a bring into view event
            if (IsKeyboardFocusWithin)
                MoveCaretToTextEnd(); // Add this
        }
    }

    private void UpdateSpinButtonPlacement()
    {
        // v2: 
        // Control themes don't support nested /template/ which was previously used here
        // So we set :spinvisible and :spinpopup on the TextBox too since the TextBox
        // is already a custom style for the NumberBox. Will keep the pseudoclasses on
        // the NumberBox too for external styles that may want this info.
        // :spincollapsed will not be set on TextBox
        var sbm = SpinButtonPlacementMode;

        if (sbm == FANumberBoxSpinButtonPlacementMode.Inline)
        {
            PseudoClasses.Set(s_pcSpinVisible, true);
            PseudoClasses.Set(s_pcSpinPopup, false);
            PseudoClasses.Set(s_pcSpinCollapsed, false);

            if (_textBox != null)
            {
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinVisible, true);
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinPopup, false);
            }
        }
        else if (sbm == FANumberBoxSpinButtonPlacementMode.Compact)
        {
            PseudoClasses.Set(s_pcSpinVisible, false);
            PseudoClasses.Set(s_pcSpinPopup, true);
            PseudoClasses.Set(s_pcSpinCollapsed, false);

            if (_textBox != null)
            {
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinVisible, false);
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinPopup, true);
            }
        }
        else
        {
            PseudoClasses.Set(s_pcSpinVisible, false);
            PseudoClasses.Set(s_pcSpinPopup, false);
            PseudoClasses.Set(s_pcSpinCollapsed, true);

            if (_textBox != null)
            {
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinVisible, false);
                ((IPseudoClasses)_textBox.Classes).Set(s_pcSpinPopup, false);
            }
        }
    }

    private void UpdateSpinButtonEnabled()
    {
        bool isUpEnabled = false;
        bool isDownEnabled = false;

        var value = Value;
        if (!double.IsNaN(value))
        {
            if (IsWrapEnabled || ValidationMode != FANumberBoxValidationMode.InvalidInputOverwritten)
            {
                // If wrapping is enabled, or invalid values are allowed, then the buttons should be enabled
                isUpEnabled = true;
                isDownEnabled = true;
            }
            else
            {
                if (value < Maximum)
                    isUpEnabled = true;

                if (value > Minimum)
                    isDownEnabled = true;
            }
        }

        PseudoClasses.Set(s_pcUpDisabled, !isUpEnabled);
        PseudoClasses.Set(s_pcDownDisabled, !isDownEnabled);
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
        PseudoClasses.Set(FASharedPseudoclasses.s_pcHeader, showHeader);
    }

    private void MoveCaretToTextEnd()
    {
        _textBox?.SelectionStart = _textBox.SelectionEnd = _textBox.CaretIndex = _textBox.Text.Length;
    }

    private void CoerceValueIfNeeded(double min, double max)
    {
        var value = Value;
        if (double.IsNaN(value))
            return;

        if (value < min)
            Value = min;
        else if (value > max)
            Value = max;
    }

    private double CoerceValueToRange(double val)
    {
        var maximum = Maximum;
        var minimum = Minimum;
        if (!double.IsNaN(val) && (val > maximum || val < minimum) && ValidationMode == FANumberBoxValidationMode.InvalidInputOverwritten)
        {
            if (val > maximum)
                return maximum;

            if (val < minimum)
                return minimum;
        }

        return val;
    }

    private void UnhookEvents()
    {
        _spinDown?.Click -= OnSpinDownClick;
        _spinUp?.Click -= OnSpinUpClick;
        _popupDownButton?.Click -= OnSpinDownClick;
        _popupUpButton?.Click -= OnSpinUpClick;
        _textBox?.RemoveHandler(KeyDownEvent, OnNumberBoxKeyDown);
        _textBox?.KeyUp -= OnNumberBoxKeyUp;
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
