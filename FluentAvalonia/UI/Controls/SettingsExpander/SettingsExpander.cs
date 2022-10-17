using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Control used to display or group settings options within an app, like in
/// the Windows 11 Settings app
/// </summary>
public partial class SettingsExpander : HeaderedItemsControl, ICommandSource
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _expander = e.NameScope.Get<Expander>("Expander");
        // The Expander's template hasn't been loaded yet, so defer until later when it has
        // so we can load the ToggleButton within the template
        _expander.Loaded += ExpanderLoaded;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsClickEnabledProperty)
        {
            var newVal = change.GetNewValue<bool>();
            if (ItemCount > 0 && newVal)
            {
                throw new InvalidOperationException("Cannot set Items and mark IsClickEnabled to true on a SettingsExpander");
            }

            // Disable pointerover/pressed styles if we aren't clickable (empty or !IsClickEnabled)
            ((IPseudoClasses)_expanderToggleButton.Classes).Set(":allowClick", newVal || ItemCount > 0);

            // When IsClickEnabled == true, we don't allow items but we may want to show an ActionIcon
            // ControlThemes don't let is drill into sub-templates so we have to do this manually here
            // Set a style on the ToggleButton to indicate we want to hide the expand/collapse chevron
            if (_expanderToggleButton != null)
                ((IPseudoClasses)_expanderToggleButton.Classes).Set(":empty", newVal);
        }
    }

    protected override IItemContainerGenerator CreateItemContainerGenerator() =>
        new SettingsExpanderItemContainerGenerator(this, ContentControl.ContentProperty,
            ContentControl.ContentTemplateProperty);

    protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.ItemsChanged(e);

        if (IsClickEnabled && e.NewValue != null)
            throw new InvalidOperationException("Cannot set Items and mark IsClickEnabled to true on a SettingsExpander");
    }

    /// <summary>
    /// Invoked when the SettingsExpander is clicked when IsClickEnabled = true
    /// </summary>
    protected internal virtual void OnClick()
    {
        var args = new RoutedEventArgs(ClickEvent);
        RaiseEvent(args);

        var @param = CommandParameter;
        if (!args.Handled && _command?.CanExecute(@param) == true)
        {
            _command.Execute(@param);
        }
    }
       
    private void ExpanderLoaded(object sender, RoutedEventArgs e)
    {
        // Don't need this anymore, clear it
        _expander.Loaded -= ExpanderLoaded;

        if (_expanderToggleButton != null)
            _expanderToggleButton.Click -= ExpanderToggleButtonClick;

        var header = _expander.GetTemplateChildren().OfType<ToggleButton>().FirstOrDefault();
        if (header == null)
            throw new InvalidOperationException("Invalid template for SettingsExpander. Unable to find ToggleButton inside Expander");

        _expanderToggleButton = header;
        _expanderToggleButton.Click += ExpanderToggleButtonClick;

        bool allowClick = IsClickEnabled;

        // Disable pointerover/pressed styles if we aren't clickable (empty or !IsClickEnabled)
        ((IPseudoClasses)_expanderToggleButton.Classes).Set(":allowClick", IsClickEnabled || ItemCount > 0);

        // When IsClickEnabled == true, we don't allow items but we may want to show an ActionIcon
        // ControlThemes don't let is drill into sub-templates so we have to do this manually here
        // Set a style on the ToggleButton to indicate we want to hide the expand/collapse chevron
        ((IPseudoClasses)_expanderToggleButton.Classes).Set(":empty", IsClickEnabled || ItemCount == 0);
    }

    private void ExpanderToggleButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        OnClick();
    }

    private void CanExecuteChanged(object sender, EventArgs e)
    {
        var canExecute = _command == null || _command.CanExecute(CommandProperty);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }

    void ICommandSource.CanExecuteChanged(object sender, EventArgs e) =>
       CanExecuteChanged(sender, e);

    private bool _commandCanExecute = true;
    private Expander _expander;
    private ToggleButton _expanderToggleButton;
}
