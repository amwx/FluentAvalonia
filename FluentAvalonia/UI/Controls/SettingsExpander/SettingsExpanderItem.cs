using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// An item displayed within a <see cref="SettingsExpander"/>
/// </summary>
public partial class SettingsExpanderItem : ContentControl, ICommandSource
{
    public SettingsExpanderItem()
    {
        TemplateSettings = new SettingsExpanderTemplateSettings();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // To handle cases where SettingsExpander is too small, we want to move the footer
        // content to the bottom so we can try to display everything nicely
        if (_hasFooter)
        {
            if (_isFooterAtBottom)
            {
                if (availableSize.Width > _adaptiveWidthTrigger)
                {
                    _isFooterAtBottom = false;
                    PseudoClasses.Set(":footerBottom", false);
                }
            }
            else if (availableSize.Width < _adaptiveWidthTrigger)
            {
                _isFooterAtBottom = true;
                PseudoClasses.Set(":footerBottom", true);
            }
        }

        return base.MeasureOverride(availableSize);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _adaptiveWidthDisposable = this.GetResourceObservable("SettingsExpanderItemAdaptiveWidthTrigger")
            .Subscribe(OnAdaptiveWidthValueChanged);

        // We only want to allow interaction on the SettingsExpanderItem IF we're a child item,
        // because we use this in the header of the expander for the SettingsExpander where the
        // ToggleButton does the heavy lifting for us
        _allowInteraction = IsClickEnabled && this.FindAncestorOfType<ToggleButton>() == null;
        PseudoClasses.Set(":allowClick", _allowInteraction);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            OnIconSourceChanged(change);
        }
        else if (change.Property == ActionIconSourceProperty)
        {
            OnActionIconSourceChanged(change);
        }
        else if (change.Property == IsClickEnabledProperty)
        {
            OnIsClickEnabledChanged(change);
        }
        else if (change.Property == FooterProperty)
        {
            OnFooterChanged(change);
        }
        else if (change.Property == ContentProperty)
        {
            OnContentChanged(change);
        }
        else if (change.Property == DescriptionProperty)
        {
            OnDescriptionChanged(change);
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        if (_command != null)
        {
            _command.CanExecuteChanged += CanExecuteChanged;
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        _adaptiveWidthDisposable?.Dispose();
        _adaptiveWidthDisposable = null;

        if (_command != null)
            _command.CanExecuteChanged -= CanExecuteChanged;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_allowInteraction && !e.Handled)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isPressed = true;
                PseudoClasses.Set(":pressed", true);
                e.Handled = true;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_allowInteraction && !e.Handled && e.Pointer.Captured != null)
        {
            // We do this because we don't get PointerExited events when the pointer
            // has a control captured - but to match normal behavior when moving the
            // pointer outside the control bounds - we want to keep track of it so
            // we can take the pressed state away if the pointer moves outside so we
            // don't trigger a click event if you release the pointer outside
            var pt = e.GetCurrentPoint(this);
            if (new Rect(Bounds.Size).Contains(pt.Position))
            {
                _isPressed = true;
                PseudoClasses.Set(":pressed", true);
            }
            else
            {
                _isPressed = false;
                PseudoClasses.Set(":pressed", false);
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isPressed && _allowInteraction)
        {
            _isPressed = false;
            PseudoClasses.Set(":pressed", false);

            if (!e.Handled)
            {
                e.Handled = true;

                OnClick();
            }
        }       
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isPressed = false;
        PseudoClasses.Set(":pressed", false);
    }

    /// <summary>
    /// Invoked when the SettingsExpanderItem is clicked when IsClickEnabled = true
    /// </summary>
    protected virtual void OnClick()
    {
        var args = new RoutedEventArgs(ClickEvent);
        RaiseEvent(args);

        var @param = CommandParameter;
        if (!args.Handled && _command?.CanExecute(@param) == true)
        {
            _command.Execute(@param);
        }
    }

    private void OnIconSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var newIcon = args.GetNewValue<IconSource>();
        PseudoClasses.Set(":icon", newIcon != null);

        TemplateSettings.Icon = IconHelpers.CreateFromUnknown(newIcon);
    }

    private void OnActionIconSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (IsClickEnabled)
        {
            // Only set the icon if IsClickEnabled
            var newIcon = args.GetNewValue<IconSource>();
            PseudoClasses.Set(":actionIcon", newIcon != null);

            TemplateSettings.ActionIcon = IconHelpers.CreateFromUnknown(newIcon);
        }
    }

    private void OnIsClickEnabledChanged(AvaloniaPropertyChangedEventArgs args)
    {
        bool enabled = args.GetNewValue<bool>();
        _allowInteraction = enabled && this.FindAncestorOfType<ToggleButton>() == null;
        PseudoClasses.Set(":allowClick", _allowInteraction);

        var actionIcon = ActionIconSource;
        if (!enabled && actionIcon != null)
        {
            // If not enabled, clear the ActionIcon in template settings
            TemplateSettings.ActionIcon = null;
            PseudoClasses.Set(":actionIcon", false);
        }
        else if (actionIcon != null)
        {
            TemplateSettings.ActionIcon = IconHelpers.CreateFromUnknown(actionIcon);
            PseudoClasses.Set(":actionIcon", true);
        }
    }

    private void OnFooterChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _hasFooter = args.NewValue != null;
        PseudoClasses.Set(":footer", _hasFooter);
    }

    private void OnContentChanged(AvaloniaPropertyChangedEventArgs args)
    {
        PseudoClasses.Set(":content", args.NewValue != null);
    }

    private void OnDescriptionChanged(AvaloniaPropertyChangedEventArgs args)
    {
        PseudoClasses.Set(":description", args.NewValue != null);
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

    private void OnAdaptiveWidthValueChanged(object value)
    {
        if (value == AvaloniaProperty.UnsetValue)
            return;

        _adaptiveWidthTrigger = Unsafe.Unbox<double>(value);
        InvalidateMeasure();
    }

    void ICommandSource.CanExecuteChanged(object sender, EventArgs e) =>
        CanExecuteChanged(sender, e);

    private bool _commandCanExecute = true;
    private bool _allowInteraction;
    private bool _isPressed;
    private bool _hasFooter;
    private bool _isFooterAtBottom;
    private IDisposable _adaptiveWidthDisposable;
    private double _adaptiveWidthTrigger = 460;
}
