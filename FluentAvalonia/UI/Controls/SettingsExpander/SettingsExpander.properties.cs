using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls;

public partial class SettingsExpander
{
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = 
        AvaloniaProperty.Register<SettingsExpander, IDataTemplate>(nameof(HeaderTemplate));

    public static readonly StyledProperty<string> DescriptionProperty = 
        AvaloniaProperty.Register<SettingsExpander, string>(nameof(Description));

    public static readonly StyledProperty<IconSource> IconSourceProperty = 
        AvaloniaProperty.Register<SettingsExpander, IconSource>(nameof(IconSource));

    public static readonly StyledProperty<object> FooterProperty = 
        AvaloniaProperty.Register<SettingsExpander, object>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty = 
        AvaloniaProperty.Register<SettingsExpander, IDataTemplate>(nameof(FooterTemplate));

    public static readonly DirectProperty<SettingsExpander, bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<SettingsExpander>(x => x.IsExpanded,
            (x, v) => x.IsExpanded = v);

    public static readonly StyledProperty<IconSource> ActionIconSourceProperty = 
        AvaloniaProperty.Register<SettingsExpander, IconSource>(nameof(ActionIconSource));

    public static readonly StyledProperty<bool> IsClickEnabledProperty = 
        AvaloniaProperty.Register<SettingsExpander, bool>(nameof(IsClickEnabled));

    public static readonly StyledProperty<SettingsExpanderTemplateSettings> TemplateSettingsProperty = 
        AvaloniaProperty.Register<SettingsExpander, SettingsExpanderTemplateSettings>(nameof(TemplateSettings));

    public static readonly DirectProperty<SettingsExpander, ICommand> CommandProperty = 
        Button.CommandProperty.AddOwner<SettingsExpander>(x => x.Command,
            (x, v) => x.Command = v);

    public static readonly StyledProperty<object> CommandParameterProperty = 
        Button.CommandParameterProperty.AddOwner<SettingsExpander>();




    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    public IconSource ActionIconSource
    {
        get => GetValue(ActionIconSourceProperty);
        set => SetValue(ActionIconSourceProperty, value);
    }

    public bool IsClickEnabled
    {
        get => GetValue(IsClickEnabledProperty);
        set => SetValue(IsClickEnabledProperty, value);
    }

    public SettingsExpanderTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    public ICommand Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        Button.ClickEvent;

    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private ICommand _command;
    private bool _isExpanded;
}

public class SettingsExpanderTemplateSettings : AvaloniaObject
{
    internal SettingsExpanderTemplateSettings() { }

    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(Icon));

    public static readonly StyledProperty<FAIconElement> ActionIconProperty =
        AvaloniaProperty.Register<SettingsExpanderTemplateSettings, FAIconElement>(nameof(ActionIcon));

    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public FAIconElement ActionIcon
    {
        get => GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }
}
