using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

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
                       
            if (_expanderToggleButton != null)
            {
                // Disable pointerover/pressed styles if we aren't clickable (empty or !IsClickEnabled)
                ((IPseudoClasses)_expanderToggleButton.Classes).Set(":allowClick", newVal || ItemCount > 0);

                // When IsClickEnabled == true, we don't allow items but we may want to show an ActionIcon
                // ControlThemes don't let is drill into sub-templates so we have to do this manually here
                // Set a style on the ToggleButton to indicate we want to hide the expand/collapse chevron
                ((IPseudoClasses)_expanderToggleButton.Classes).Set(":empty", newVal);
            }                
        }
        else if (change.Property == IsExpandedProperty)
        {
            // Prevent going to expanded state if we don't have any child items
            // Use the IsAttachedToVisualTree flag here to prevent overwriting 'true' while control
            // is Initializing where IsExpanded may be set before Items
            if (ItemCount == 0 && change.GetNewValue<bool>() && ((IVisual)this).IsAttachedToVisualTree)
            {
                // There seems to be an issue here where if we just set IsExpanded = false
                // the property does get set, but the :expanded pseudoclass is never cleared
                // from the Expander. So post to dispatcher to let this prop change notification
                // go through real quick, then change the value to false to get the correct state
                Dispatcher.UIThread.Post(() => IsExpanded = false, DispatcherPriority.Send);
            }
        }
        else if (change.Property == CommandProperty)
        {
            if (((ILogical)this).IsAttachedToLogicalTree)
            {
                var (oldValue, newValue) = change.GetOldAndNewValue<ICommand>();
                if (oldValue != null)
                {
                    oldValue.CanExecuteChanged -= CanExecuteChanged;
                }

                if (newValue != null)
                {
                    newValue.CanExecuteChanged += CanExecuteChanged;
                }
            }

            CanExecuteChanged(this, EventArgs.Empty);
        }
        else if (change.Property == CommandParameterProperty)
        {
            CanExecuteChanged(this, EventArgs.Empty);
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
        var canExecute = _command == null || _command.CanExecute(CommandParameter);

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
