using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a selection control that combines a non-editable text box and a drop-down list box that 
/// allows users to select an item from a list.
/// </summary>
[TemplatePart(s_tpPopup, typeof(Popup))]
[TemplatePart(s_tpEditableText, typeof(TextBox))]
[TemplatePart(s_tpDropDownOverlay, typeof(Border))]
[PseudoClasses(s_pcSelected, s_pcFocus, SharedPseudoclasses.s_pcPressed)]
[PseudoClasses(s_pcEditable, s_pcDropDownOpen, s_pcPopupAbove, SharedPseudoclasses.s_pcHeader)]
public partial class FAComboBox : HeaderedSelectingItemsControl
{
    /// <summary>
    /// Defines the <see cref="MaxDropDownHeight"/> property
    /// </summary>
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        ComboBox.MaxDropDownHeightProperty.AddOwner<FAComboBox>();

    /// <summary>
    /// Defines the <see cref="IsEditable"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsEditableProperty =
        AvaloniaProperty.Register<FAComboBox, bool>(nameof(IsEditable));

    /// <summary>
    /// Defines the <see cref="IsDropDownOpen"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<FAComboBox, bool>(nameof(IsDropDownOpen));

    /// <summary>
    /// Defines the <see cref="IsSelectionBoxHighlighted"/> property
    /// </summary>
    public static readonly DirectProperty<FAComboBox, bool> IsSelectionBoxHighlightedProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, bool>(nameof(IsSelectionBoxHighlighted),
            x => x.IsSelectionBoxHighlighted);

    /// <summary>
    /// Defines the <see cref="SelectionBoxItem"/> property
    /// </summary>
    public static readonly DirectProperty<FAComboBox, object> SelectionBoxItemProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, object>(nameof(SelectionBoxItem),
            x => x.SelectionBoxItem);

    /// <summary>
    /// Defines the <see cref="SelectionBoxItemTemplate"/> property
    /// </summary>
    public static readonly DirectProperty<FAComboBox, IDataTemplate> SelectionBoxItemTemplateProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, IDataTemplate>(nameof(SelectionBoxItemTemplate),
            x => x.SelectionBoxItemTemplate);

    /// <summary>
    /// Defines the <see cref="PlaceholderText"/> property
    /// </summary>
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        ComboBox.PlaceholderTextProperty.AddOwner<FAComboBox>();

    /// <summary>
    /// Defines the <see cref="SelectionChangedTrigger"/> property
    /// </summary>
    public static readonly StyledProperty<FAComboBoxSelectionChangedTrigger> SelectionChangedTriggerProperty =
        AvaloniaProperty.Register<FAComboBox, FAComboBoxSelectionChangedTrigger>(nameof(SelectionChangedTrigger));

    /// <summary>
    /// Defines the <see cref="PlaceholderForeground"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> PlaceholderForegroundProperty =
        ComboBox.PlaceholderForegroundProperty.AddOwner<FAComboBox>();

    /// <summary>
    /// Defines the <see cref="TextBoxTheme"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> TextBoxThemeProperty =
        AvaloniaProperty.Register<FAComboBox, ControlTheme>(nameof(TextBoxTheme));

    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<FAComboBox, string>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<FAComboBox>();

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<FAComboBox>();

    /// <summary>
    /// Gets or sets the <see cref="HorizontalAlignment"/> of the content in the ComboBox
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="VerticalAlignment"/> of the content in the ComboBox
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed height of the dropdown
    /// </summary>
    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this ComboBox is editable
    /// </summary>
    public bool IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this ComboBox's dropdown is open
    /// </summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
        
    /// <summary>
    /// Gets whether the SelectionBox is hightlighted
    /// </summary>
    public bool IsSelectionBoxHighlighted
    {
        get => _isSelectionBoxHighlighted;
        private set => SetAndRaise(IsSelectionBoxHighlightedProperty, ref _isSelectionBoxHighlighted, value);
    }

    /// <summary>
    /// Gets the item shown whne the ComboBox is closed
    /// </summary>
    public object SelectionBoxItem
    {
        get => _selectionBoxItem;
        private set => SetAndRaise(SelectionBoxItemProperty, ref _selectionBoxItem, value);
    }

    /// <summary>
    /// Gets the template applied to the selection box content.
    /// </summary>
    public IDataTemplate SelectionBoxItemTemplate
    {
        get => _selectionBoxItemTemplate;
        private set => SetAndRaise(SelectionBoxItemTemplateProperty, ref _selectionBoxItemTemplate, value);
    }

    /// <summary>
    /// Gets or sets the text that is displayed in the control until the value is changed by a user action 
    /// or some other operation.
    /// </summary>
    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates what action causes a SelectionChanged event to occur.
    /// </summary>
    public FAComboBoxSelectionChangedTrigger SelectionChangedTrigger
    {
        get => GetValue(SelectionChangedTriggerProperty);
        set => SetValue(SelectionChangedTriggerProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush that describes the color of placeholder text.
    /// </summary>
    public IBrush PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ControlTheme"/> used for the TextBox part of the ComboBox
    /// </summary>
    public ControlTheme TextBoxTheme
    {
        get => GetValue(TextBoxThemeProperty);
        set => SetValue(TextBoxThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the text in the ComboBox.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Occurs when the drop-down portion of the ComboBox opens.
    /// </summary>
    public event EventHandler<EventArgs> DropDownOpened;

    /// <summary>
    /// Occurs when the drop-down portion of the ComboBox closes.
    /// </summary>
    public event EventHandler<EventArgs> DropDownClosed;

    /// <summary>
    /// Occurs when the user submits some text that does not correspond to an item in the ComboBox dropdown list.
    /// </summary>
    public event TypedEventHandler<FAComboBox, FAComboBoxTextSubmittedEventArgs> TextSubmitted;

    private bool _isSelectionBoxHighlighted;
    private object _selectionBoxItem;
    private IDataTemplate _selectionBoxItemTemplate;

    private const string s_tpPopup = "Popup";
    private const string s_tpEditableText = "EditableText";
    private const string s_tpDropDownOverlay = "DropDownOverlay";

    private const string s_pcEditable = ":editable";
    private const string s_pcFocus = ":focus";
    private const string s_pcSelected = ":selected";
    private const string s_pcDropDownOpen = ":dropdownopen";
    private const string s_pcPopupAbove = ":popupAbove";
}
