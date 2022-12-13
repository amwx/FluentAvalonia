using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls.Metadata;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(SharedPseudoclasses.s_pcHotkey)]
[PseudoClasses(SharedPseudoclasses.s_pcPressed)]
public partial class MenuFlyoutItem
{
    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, string>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, IconSource>(nameof(IconSource));

    /// <summary>
    /// Defines the <see cref="Command"/> property
    /// </summary>
    public static readonly DirectProperty<MenuFlyoutItem, ICommand> CommandProperty =
        Button.CommandProperty.AddOwner<MenuFlyoutItem>(x => x.Command,
            (x, v) => x.Command = v);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<MenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="HotKey"/> property
    /// </summary>
    public static readonly StyledProperty<KeyGesture> HotKeyProperty =
        Button.HotKeyProperty.AddOwner<MenuFlyoutItem>();

    /// <summary>
    /// Defines the <see cref="InputGesture"/> property
    /// </summary>
    public static readonly StyledProperty<KeyGesture> InputGestureProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, KeyGesture>(nameof(InputGesture));

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<MenuFlyoutItemTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, MenuFlyoutItemTemplateSettings>(nameof(TemplateSettings));

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
    public IconSource IconSource
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
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
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
    public MenuFlyoutItemTemplateSettings TemplateSettings
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

    bool IMenuItem.HasSubMenu => false;

    bool IMenuItem.IsPointerOverSubMenu => false;

    bool IMenuItem.IsSubMenuOpen { get => false; set { } }

    public bool IsTopLevel => false;

    IMenuItem IMenuElement.SelectedItem { get => null; set { } }

    IEnumerable<IMenuItem> IMenuElement.SubItems => null;

    IMenuElement IMenuItem.Parent
    {
        get
        {
            if (this.FindLogicalAncestorOfType<MenuFlyoutSubItem>() is MenuFlyoutSubItem mfsi)
            {
                return mfsi;
            }

            return Parent as IMenuElement;
        }
    }

    bool IMenuItem.StaysOpenOnClick { get => false; set { } }

    private ICommand _command;
}
