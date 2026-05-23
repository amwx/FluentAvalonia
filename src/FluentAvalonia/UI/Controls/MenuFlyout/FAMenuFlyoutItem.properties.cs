using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using System.Windows.Input;
using Avalonia.Controls.Metadata;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(FASharedPseudoclasses.s_pcHotkey)]
[PseudoClasses(FASharedPseudoclasses.s_pcPressed)]
public partial class FAMenuFlyoutItem
{
    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<FAMenuFlyoutItem, string>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FAMenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="Command"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> CommandProperty =
        Button.CommandProperty.AddOwner<FAMenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<FAMenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="HotKey"/> property
    /// </summary>
    public static readonly StyledProperty<KeyGesture> HotKeyProperty =
        Button.HotKeyProperty.AddOwner<FAMenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="InputGesture"/> property
    /// </summary>
    public static readonly StyledProperty<KeyGesture> InputGestureProperty =
        AvaloniaProperty.Register<FAMenuFlyoutItem, KeyGesture>(nameof(InputGesture));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FAMenuFlyoutItemTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<FAMenuFlyoutItem, FAMenuFlyoutItemTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Gets or sets the text content of a MenuFlyoutItem.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the graphic content of the menu flyout item.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the KeyGesture that should invoke this MenuFlyoutItem
    /// </summary>
    public KeyGesture HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the input gesture displayed by the MenuFlyoutItem
    /// </summary>
    /// <remarks>
    /// This property is equivalent to WinUI's KeyboardAcceleratorTextOverride
    /// property. It allows you to specify a key gesture without mapping to 
    /// a hotkey. This property takes priority over <see cref="HotKey"/>
    /// </remarks>
    public KeyGesture InputGesture
    {
        get => GetValue(InputGestureProperty);
        set => SetValue(InputGestureProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the item is pressed.
    /// </summary>
    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="Command"/> property.
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets the template settings for this MenuFlyoutItem
    /// </summary>
    public FAMenuFlyoutItemTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && _canExecute;

    /// <summary>
    /// Defines the <see cref="Click"/> event
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = MenuItem.ClickEvent;

    /// <summary>
    /// Raised when this MenuFlyoutItem is invoked
    /// </summary>
    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }
}
