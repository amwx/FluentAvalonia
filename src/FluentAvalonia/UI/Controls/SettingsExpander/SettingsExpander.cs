using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Control used to display or group settings options within an app, like in
/// the Windows 11 Settings app
/// </summary>
public partial class SettingsExpander : HeaderedItemsControl, ICommandSource
{
    public SettingsExpander()
    {
        ItemsView.CollectionChanged += ItemsCollectionChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _expander = e.NameScope.Get<Expander>(s_tpExpander);
        // The Expander's template hasn't been loaded yet, so defer until later when it has
        // so we can load the ToggleButton within the template
        _expander.Loaded += ExpanderLoaded;
        _expander.Expanding += ExpanderExpanding;

        _contentHost = e.NameScope.Get<SettingsExpanderItem>(s_tpContentHost);
        _hasAppliedTemplate = true;

        SetIcons();
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
                ((IPseudoClasses)_expanderToggleButton.Classes).Set(SharedPseudoclasses.s_pcAllowClick, newVal || ItemCount > 0);

                // When IsClickEnabled == true, we don't allow items but we may want to show an ActionIcon
                // ControlThemes don't let is drill into sub-templates so we have to do this manually here
                // Set a style on the ToggleButton to indicate we want to hide the expand/collapse chevron
                ((IPseudoClasses)_expanderToggleButton.Classes).Set(s_pcEmpty, newVal);
            }                
        }
        else if (change.Property == IsExpandedProperty)
        {
            // Prevent going to expanded state if we don't have any child items
            // Use the IsAttachedToVisualTree flag here to prevent overwriting 'true' while control
            // is Initializing where IsExpanded may be set before Items
            if (ItemCount == 0 && change.GetNewValue<bool>() && this.IsAttachedToVisualTree())
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
        else if (change.Property == IconSourceProperty)
        {
            var oldVal = change.OldValue;
            if (oldVal != null)
                _iconCount--;

            var newVal = change.NewValue;
            if (newVal != null)
                _iconCount++;

            SetIcons();
        }
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // This fires for collection changes, whether they originate from Items or ItemsSource

        if (IsClickEnabled && ItemsView.Count > 0)
            throw new InvalidOperationException("Cannot set Items and mark IsClickEnabled to true on a SettingsExpander");

        if (_expanderToggleButton is not null)
        {
            // Disable the interaction states if items collection is cleared
            bool isInteractable = ItemCount > 0;
            ((IPseudoClasses)_expanderToggleButton.Classes).Set(SharedPseudoclasses.s_pcAllowClick, isInteractable);
            ((IPseudoClasses)_expanderToggleButton.Classes).Set(s_pcEmpty, !isInteractable); 
        }
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        bool isItem = item is SettingsExpanderItem;
        recycleKey = isItem ? null : nameof(SettingsExpanderItem);
        return !isItem;
    }

    protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
    {
        var cont = this.FindDataTemplate(item, ItemTemplate)?.Build(item);

        if (cont is SettingsExpanderItem sei)
        {
            sei.DataContext = item;
            sei.IsContainerFromTemplate = true;
            return sei;
        }

        return new SettingsExpanderItem();
    }

    protected override void PrepareContainerForItemOverride(Control container, object item, int index)
    {
        var sei = container as SettingsExpanderItem;

        // If the container was created from a DataTemplate, do NOT call PrepareContainer or it will
        // do another template lookup and then put a item within an item as it sets the normal
        // ContentControl properties. Items created from a DataTemplate are assumed to be
        // initialized, to be sure the DataContext is set in CreateContainer
        if (!sei.IsContainerFromTemplate)
            base.PrepareContainerForItemOverride(container, item, index);

        if (sei.IconSource != null)
            _iconCount++;
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);

        if (container is SettingsExpanderItem sei)
        {
            if (sei.IconSource != null)
                _iconCount--;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        SetIcons();
        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Invoked when the SettingsExpander is clicked when IsClickEnabled = true
    /// </summary>
    protected internal virtual void OnClick()
    {
        var args = new RoutedEventArgs(ClickEvent);
        RaiseEvent(args);

        var @param = CommandParameter;
        var command = Command;
        if (!args.Handled && command?.CanExecute(@param) == true)
        {
            command.Execute(@param);
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
        ((IPseudoClasses)_expanderToggleButton.Classes).Set(SharedPseudoclasses.s_pcAllowClick, IsClickEnabled || ItemCount > 0);

        // When IsClickEnabled == true, we don't allow items but we may want to show an ActionIcon
        // ControlThemes don't let is drill into sub-templates so we have to do this manually here
        // Set a style on the ToggleButton to indicate we want to hide the expand/collapse chevron
        ((IPseudoClasses)_expanderToggleButton.Classes).Set(s_pcEmpty, IsClickEnabled || ItemCount == 0);
    }
    
    private void ExpanderExpanding(object sender, CancelRoutedEventArgs e)
    {
        if (ItemCount == 0 && IsClickEnabled)
        {
            e.Cancel = true;
            e.Handled = true;
        }
    }

    private void ExpanderToggleButtonClick(object sender, RoutedEventArgs e)
    {
        if (!(e.Source == _expanderToggleButton))
            return;

        e.Handled = true;
        OnClick();
    }

    private void CanExecuteChanged(object sender, EventArgs e)
    {
        var command = Command;
        var canExecute = command == null || command.CanExecute(CommandParameter);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }

    void ICommandSource.CanExecuteChanged(object sender, EventArgs e) =>
       CanExecuteChanged(sender, e);

    private void SetIcons()
    {
        if (!_hasAppliedTemplate)
            return;

        // If the item count is 0, setting IconSource will automatically handle this
        // by the :icon in the SettingsExpanderItem
        if (ItemCount == 0)
            return;

        bool usePlaceholder = _iconCount > 0;
        ((IPseudoClasses)_contentHost.Classes).Set(s_pcIconPlaceholder, usePlaceholder);

        var rc = GetRealizedContainers();
        foreach (var item in GetRealizedContainers())
        {
            ((IPseudoClasses)item.Classes).Set(s_pcIconPlaceholder, usePlaceholder);
        }
    }

    internal void InvalidateIcons(SettingsExpanderItem item)
    {
        if (item == _contentHost)
            return;

        SetIcons();
    }

    private bool _commandCanExecute = true;
    private Expander _expander;
    private ToggleButton _expanderToggleButton;
    private SettingsExpanderItem _contentHost;
    private int _iconCount = 0;
    private bool _hasAppliedTemplate;
}
