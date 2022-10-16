using Avalonia;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using System;

namespace FluentAvalonia.UI.Controls;

public partial class SettingsExpanderItem : ContentControl
{
    public static readonly StyledProperty<string> DescriptionProperty = 
        SettingsExpander.DescriptionProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<IconSource> IconSourceProperty = 
        SettingsExpander.IconSourceProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<object> FooterProperty = 
        SettingsExpander.FooterProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty = 
        SettingsExpander.FooterTemplateProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<IconSource> ActionIconSourceProperty = 
        SettingsExpander.ActionIconSourceProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<bool> IsClickEnabledProperty = 
        SettingsExpander.IsClickEnabledProperty.AddOwner<SettingsExpanderItem>();
        
    public static readonly DirectProperty<SettingsExpanderItem, ICommand> CommandProperty = 
        Button.CommandProperty.AddOwner<SettingsExpanderItem>(x => x.Command,
            (x, v) => x.Command = v);

    public static readonly StyledProperty<object> CommandParameterProperty = 
        Button.CommandParameterProperty.AddOwner<SettingsExpanderItem>();

    public static readonly StyledProperty<SettingsExpanderTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<SettingsExpanderItem, SettingsExpanderTemplateSettings>(nameof(TemplateSettings));


    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        Button.ClickEvent;

    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
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

    public SettingsExpanderTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    private ICommand _command;
}
