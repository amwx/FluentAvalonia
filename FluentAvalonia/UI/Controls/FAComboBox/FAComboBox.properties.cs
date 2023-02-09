using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public partial class FAComboBox : HeaderedSelectingItemsControl
{
    public static StyledProperty<double> MaxDropDownHeightProperty =
        ComboBox.MaxDropDownHeightProperty.AddOwner<FAComboBox>();

    public static StyledProperty<bool> IsEditableProperty =
        AvaloniaProperty.Register<FAComboBox, bool>(nameof(IsEditable));

    public static StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<FAComboBox, bool>(nameof(IsDropDownOpen));

    public static DirectProperty<FAComboBox, bool> IsSelectionBoxHighlightedProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, bool>(nameof(IsSelectionBoxHighlighted),
            x => x.IsSelectionBoxHighlighted);

    public static DirectProperty<FAComboBox, object> SelectionBoxItemProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, object>(nameof(SelectionBoxItem),
            x => x.SelectionBoxItem);

    public static DirectProperty<FAComboBox, IDataTemplate> SelectionBoxItemTemplateProperty =
        AvaloniaProperty.RegisterDirect<FAComboBox, IDataTemplate>(nameof(SelectionBoxItemTemplate),
            x => x.SelectionBoxItemTemplate);

    public static StyledProperty<string> PlaceholderTextProperty =
        ComboBox.PlaceholderTextProperty.AddOwner<FAComboBox>();

    public static StyledProperty<IDataTemplate> HeaderTemplateProperty =
        HeaderedContentControl.HeaderTemplateProperty.AddOwner<FAComboBox>();

    public static StyledProperty<FAComboBoxSelectionChangedTrigger> SelectionChangedTriggerProperty =
        AvaloniaProperty.Register<FAComboBox, FAComboBoxSelectionChangedTrigger>(nameof(SelectionChangedTrigger));

    public static StyledProperty<IBrush> PlaceholderForegroundProperty =
        ComboBox.PlaceholderForegroundProperty.AddOwner<FAComboBox>();

    public static StyledProperty<ControlTheme> TextBoxThemeProperty =
        AvaloniaProperty.Register<FAComboBox, ControlTheme>(nameof(TextBoxTheme));

    public static StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<FAComboBox, string>(nameof(Text));

    public static StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<FAComboBox>();

    public static StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<FAComboBox>();

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
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    // Per WPF:
    // !comboBox.IsDropDownOpen && comboBox.IsKeyboardFocusWithin ||
    // comboBox.HighlightedElement != null && highlightedElement.Content == comboBox._clonedElement
    public bool IsSelectionBoxHighlighted
    {
        get => _isSelectionBoxHighlighted;
        private set => SetAndRaise(IsSelectionBoxHighlightedProperty, ref _isSelectionBoxHighlighted, value);
    }

    public object SelectionBoxItem
    {
        get => _selectionBoxItem;
        private set => SetAndRaise(SelectionBoxItemProperty, ref _selectionBoxItem, value);
    }

    public IDataTemplate SelectionBoxItemTemplate
    {
        get => _selectionBoxItemTemplate;
        private set => SetAndRaise(SelectionBoxItemTemplateProperty, ref _selectionBoxItemTemplate, value);
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

    public FAComboBoxSelectionChangedTrigger SelectionChangedTrigger
    {
        get => GetValue(SelectionChangedTriggerProperty);
        set => SetValue(SelectionChangedTriggerProperty, value);
    }

    public IBrush PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public ControlTheme TextBoxTheme
    {
        get => GetValue(TextBoxThemeProperty);
        set => SetValue(TextBoxThemeProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }


    public event EventHandler<EventArgs> DropDownOpened;
    public event EventHandler<EventArgs> DropDownClosed;
    public event TypedEventHandler<FAComboBox, FAComboBoxTextSubmittedEventArgs> TextSubmitted;

    private bool _isSelectionBoxHighlighted;
    private object _selectionBoxItem;
    private IDataTemplate _selectionBoxItemTemplate;
}
