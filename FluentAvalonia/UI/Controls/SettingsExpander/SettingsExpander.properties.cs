using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls;

public partial class SettingsExpander
{
    /// <summary>
    /// Defines the <see cref="HeaderTemplateProperty"/>
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = 
        AvaloniaProperty.Register<SettingsExpander, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Defines the <see cref="Description"/> property
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty = 
        AvaloniaProperty.Register<SettingsExpander, string>(nameof(Description));

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty = 
        AvaloniaProperty.Register<SettingsExpander, IconSource>(nameof(IconSource));

    /// <summary>
    /// Defines the <see cref="Footer"/> property
    /// </summary>
    public static readonly StyledProperty<object> FooterProperty = 
        AvaloniaProperty.Register<SettingsExpander, object>(nameof(Footer));

    /// <summary>
    /// Defines the <see cref="FooterTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty = 
        AvaloniaProperty.Register<SettingsExpander, IDataTemplate>(nameof(FooterTemplate));

    /// <summary>
    /// Defines the <see cref="IsExpanded"/> property
    /// </summary>
    public static readonly DirectProperty<SettingsExpander, bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<SettingsExpander>(x => x.IsExpanded,
            (x, v) => x.IsExpanded = v);

    /// <summary>
    /// Defines the <see cref="ActionIconSource"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> ActionIconSourceProperty = 
        AvaloniaProperty.Register<SettingsExpander, IconSource>(nameof(ActionIconSource));

    /// <summary>
    /// Defines the <see cref="IsClickEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsClickEnabledProperty = 
        AvaloniaProperty.Register<SettingsExpander, bool>(nameof(IsClickEnabled));

    /// <summary>
    /// Defines the <see cref="Command"/> property
    /// </summary>
    public static readonly DirectProperty<SettingsExpander, ICommand> CommandProperty = 
        Button.CommandProperty.AddOwner<SettingsExpander>(x => x.Command,
            (x, v) => x.Command = v);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty = 
        Button.CommandParameterProperty.AddOwner<SettingsExpander>();

    /// <summary>
    /// Defines the <see cref="Click"/> event
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        SettingsExpanderItem.ClickEvent;

    /// <summary>
    /// Gets or sets the Header template for the SettingsExpander
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the description text
    /// </summary>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the IconSource for the SettingsExpander
    /// </summary>
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the Footer content for the SettingsExpander
    /// </summary>
    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the Footer template for the SettingsExpander
    /// </summary>
    public IDataTemplate FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the SettingsExpander is currently expanded
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    /// <summary>
    /// Gets or sets the Action IconSource when <see cref="IsClickEnabled"/> is true
    /// </summary>
    public IconSource ActionIconSource
    {
        get => GetValue(ActionIconSourceProperty);
        set => SetValue(ActionIconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the item is clickable which can be used for navigation within an app
    /// </summary>
    /// <remarks>
    /// This property can only be set if no items are added to the SettingsExpander. Attempting to mark
    /// a settings expander clickable and adding child items will throw an exception
    /// </remarks>
    public bool IsClickEnabled
    {
        get => GetValue(IsClickEnabledProperty);
        set => SetValue(IsClickEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the Command that is invoked upon clicking the item
    /// </summary>
    public ICommand Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    /// <summary>
    /// Gets or sets the command parameter
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

    /// <summary>
    /// Event raised when the SettingsExpander is clicked and IsClickEnabled = true
    /// </summary>
    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private ICommand _command;
    private bool _isExpanded;
}
