using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia;
using FluentAvalonia.Core.Attributes;
using FluentAvalonia.Core;
using System;
using Avalonia.Controls.Metadata;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(s_pcSpinVisible, s_pcSpinPopup, s_pcSpinCollapsed)]
[PseudoClasses(s_pcUpDisabled, s_pcDownDisabled)]
[PseudoClasses(SharedPseudoclasses.s_pcHeader)]
[TemplatePart(s_tpDownSpinButton, typeof(RepeatButton))]
[TemplatePart(s_tpPopupDownSpinButton, typeof(RepeatButton))]
[TemplatePart(s_tpUpSpinButton, typeof(RepeatButton))]
[TemplatePart(s_tpPopupUpSpinButton, typeof(RepeatButton))]
[TemplatePart(s_tpInputBox, typeof(TextBox))]
[TemplatePart(s_tpUpDownPopup, typeof(Popup))]
public partial class NumberBox
{
    /// <summary>
    /// Defines the <see cref="AcceptsExpression"/> property
    /// </summary>
    public static readonly StyledProperty<bool> AcceptsExpressionProperty =
        AvaloniaProperty.Register<NumberBox, bool>(nameof(AcceptsExpression));

    /// <summary>
    /// Defines the <see cref="Description"/> property
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<NumberBox, string>(nameof(Description));

    /// <summary>
    /// Defines the <see cref="Header"/> property
    /// </summary>
    public static readonly StyledProperty<object> HeaderProperty =
        HeaderedContentControl.HeaderProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
        HeaderedContentControl.HeaderTemplateProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="IsWrapEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsWrapEnabledProperty =
        AvaloniaProperty.Register<NumberBox, bool>(nameof(IsWrapEnabled));

    /// <summary>
    /// Defines the <see cref="LargeChange"/> property
    /// </summary>
    public static readonly StyledProperty<double> LargeChangeProperty =
        RangeBase.LargeChangeProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="Minimum"/> property
    /// </summary>
    public static readonly DirectProperty<NumberBox, double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<NumberBox>(x => x.Minimum,
            (x, v) => x.Minimum = v);

    /// <summary>
    /// Defines the <see cref="Maximum"/> property
    /// </summary>
    public static readonly DirectProperty<NumberBox, double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<NumberBox>(x => x.Maximum,
            (x, v) => x.Maximum = v);

    //Skip NumberFormatter

    /// <summary>
    /// Defines the <see cref="PlaceholderText"/> property
    /// </summary>
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        TextBox.WatermarkProperty.AddOwner<NumberBox>();

    //Skip PreventKeyboardDisplayOnProgrammaticFocus

    /// <summary>
    /// Defines the <see cref="SelectionFlyout"/> property
    /// </summary>
    public static readonly StyledProperty<FlyoutBase> SelectionFlyoutProperty =
        AvaloniaProperty.Register<NumberBox, FlyoutBase>(nameof(SelectionFlyout));

    /// <summary>
    /// Defines the <see cref="SelectionHighlightColor"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> SelectionHighlightColorProperty =
        TextBox.SelectionBrushProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="SmallChange"/> property
    /// </summary>
    public static readonly StyledProperty<double> SmallChangeProperty =
        RangeBase.SmallChangeProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="SpinButtonPlacementMode"/> property
    /// </summary>
    public static readonly StyledProperty<NumberBoxSpinButtonPlacementMode> SpinButtonPlacementModeProperty =
        AvaloniaProperty.Register<NumberBox, NumberBoxSpinButtonPlacementMode>(nameof(SpinButtonPlacementMode), 
            NumberBoxSpinButtonPlacementMode.Hidden);

    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly DirectProperty<NumberBox, string> TextProperty =
        AvaloniaProperty.RegisterDirect<NumberBox, string>(nameof(Text),
            x => x.Text, (x, v) => x.Text = v, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="TextReadingOrder"/> property
    /// </summary>
    public static readonly StyledProperty<TextReadingOrder> TextReadingOrderProperty =
        AvaloniaProperty.Register<NumberBox, TextReadingOrder>(nameof(TextReadingOrder));

    /// <summary>
    /// Defines the <see cref="NumberBoxValidationMode"/> property
    /// </summary>
    public static readonly StyledProperty<NumberBoxValidationMode> ValidationModeProperty =
       AvaloniaProperty.Register<NumberBox, NumberBoxValidationMode>(nameof(ValidationMode));

    /// <summary>
    /// Defines the <see cref="Value"/> property
    /// </summary>
    public static readonly DirectProperty<NumberBox, double> ValueProperty =
         RangeBase.ValueProperty.AddOwnerWithDataValidation<NumberBox>(x => x.Value,
             (x, v) => x.Value = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    //Skip InputScope

    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        TextBlock.TextAlignmentProperty.AddOwner<NumberBox>();

    /// <summary>
    /// Defines the <see cref="SimpleNumberFormat"/> property
    /// </summary>
    public static readonly StyledProperty<string> SimpleNumberFormatProperty =
        AvaloniaProperty.Register<NumberBox, string>(nameof(SimpleNumberFormat));

    /// <summary>
    /// Toggles whether the control will accept and evaluate a basic formulaic expression entered as input.
    /// </summary>
    public bool AcceptsExpression
    {
        get => GetValue(AcceptsExpressionProperty);
        set => SetValue(AcceptsExpressionProperty, value);
    }

    /// <summary>
    /// Gets or sets content that is shown below the control. The content should provide guidance 
    /// about the input expected by the control.
    /// </summary>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the content for the control's header.
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the DataTemplate used to display the content of the control's header.
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Toggles whether line breaking occurs if a line of text extends beyond the available 
    /// width of the control.
    /// </summary>
    public bool IsWrapEnabled
    {
        get => GetValue(IsWrapEnabledProperty);
        set => SetValue(IsWrapEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that is added to or subtracted from Value when a large change is made,
    /// such as with the PageUP and PageDown keys.
    /// </summary>
    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    /// <summary>
    /// Gets or sets the numerical minimum for Value.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the numerical maximum for Value.
    /// </summary>
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
        get => GetValue(SimpleNumberFormatProperty);
        set => SetValue(SimpleNumberFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the text that is displayed in the control until the value is changed by a 
    /// user action or some other operation.
    /// </summary>
    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the flyout that is shown when text is selected, or null if no flyout is shown.
    /// NOTE: This property is not implemented
    /// </summary>
    [NotImplemented]
    public FlyoutBase SelectionFlyout
    {
        get => GetValue(SelectionFlyoutProperty);
        set => SetValue(SelectionFlyoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to highlight the selected text.
    /// </summary>
    public IBrush SelectionHighlightColor
    {
        get => GetValue(SelectionHighlightColorProperty);
        set => SetValue(SelectionHighlightColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that is added to or subtracted from Value when a small 
    /// change is made, such as with an arrow key or scrolling.
    /// </summary>
    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the placement of buttons used to increment 
    /// or decrement the Value property.
    /// </summary>
    public NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode
    {
        get => GetValue(SpinButtonPlacementModeProperty);
        set => SetValue(SpinButtonPlacementModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the string type representation of the Value property.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value that indicates how the reading order is determined for the NumberBox.
    /// NOTE This property is not implemented
    /// </summary>
    [NotImplemented]
    public TextReadingOrder TextReadingOrder
    {
        get => GetValue(TextReadingOrderProperty);
        set => SetValue(TextReadingOrderProperty, value);
    }

    /// <summary>
    /// Gets or sets the input validation behavior to invoke when invalid input is entered.
    /// </summary>
    public NumberBoxValidationMode ValidationMode
    {
        get => GetValue(ValidationModeProperty);
        set => SetValue(ValidationModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the numeric value of a NumberBox.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the TextAlignment of the text in the NumberBox
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    /// <summary>
    /// Occurs after the user triggers evaluation of new input by pressing the Enter key, 
    /// clicking a spin button, or by changing focus.
    /// </summary>
    public event TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs> ValueChanged;


    private double _minimum = double.MinValue;
    private double _maxmimum = double.MaxValue;
    public string _text = null;
    private double _value = double.NaN;

    private const string s_tpDownSpinButton = "DownSpinButton";
    private const string s_tpPopupDownSpinButton = "PopupDownSpinButton";
    private const string s_tpUpSpinButton = "UpSpinButton";
    private const string s_tpPopupUpSpinButton = "PopupUpSpinButton";
    private const string s_tpInputBox = "InputBox";
    private const string s_tpUpDownPopup = "UpDownPopup";

    private const string s_pcSpinVisible = ":spinvisible";
    private const string s_pcSpinPopup = ":spinpopup";
    private const string s_pcSpinCollapsed = ":spincollapsed";
    private const string s_pcUpDisabled = ":updisabled";
    private const string s_pcDownDisabled = ":downdisabled";
}
