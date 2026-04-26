using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Input;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a command in a <see cref="FAMenuFlyout"/> control.
/// </summary>
public partial class MenuFlyoutItem : MenuFlyoutItemBase, ICommandSource
{
    /// <summary>
    /// Create instance of <see cref="MenuFlyoutItem"/>.
    /// </summary>
    public MenuFlyoutItem()
    {
        TemplateSettings = new MenuFlyoutItemTemplateSettings();
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_hotkey != null)
        {
            HotKey = _hotkey;
        }

        base.OnAttachedToLogicalTree(e);

        if (Command != null)
        {
            Command.CanExecuteChanged += CanExecuteChanged;
            CanExecuteChanged(this, null);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (HotKey != null)
        {
            _hotkey = HotKey;
            HotKey = null;
        }

        base.OnDetachedFromLogicalTree(e);

        if (Command != null)
        {
            Command.CanExecuteChanged -= CanExecuteChanged;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // For this, we make assumption that if you set Hotkey or assign a XamlUICommand,
        // you won't also set the InputGesture to something different, so setting HotKey
        // or using a XamlUICommand will automatically set the InputGesture property

        if (change.Property == CommandProperty)
        {
            var oldCommand = change.GetOldValue<ICommand>();
            var newCommand = change.GetNewValue<ICommand>();

            if (oldCommand is XamlUICommand oldXaml)
            {
                if (Text == oldXaml.Label)
                {
                    Text = null;
                }

                if (InputGesture == oldXaml.HotKey)
                {
                    HotKey = null;
                }
            }

            if (newCommand is XamlUICommand newXaml)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    Text = newXaml.Label;
                }

                if (IconSource == null)
                {
                    IconSource = newXaml.IconSource;
                }

                if (InputGesture == null)
                {
                    HotKey = newXaml.HotKey;
                }
            }

            if (((ILogical)this).IsAttachedToLogicalTree)
            {
                if (oldCommand != null)
                {
                    oldCommand.CanExecuteChanged -= CanExecuteChanged;
                }

                if (newCommand != null)
                {
                    newCommand.CanExecuteChanged += CanExecuteChanged;
                }
            }

            CanExecuteChanged(this, null);
        }
        else if (change.Property == CommandParameterProperty)
        {
            CanExecuteChanged(this, null);
        }
        else if (change.Property == InputGestureProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcHotkey, change.NewValue != null);
        }
        else if (change.Property == HotKeyProperty)
        {
            var kg = change.GetNewValue<KeyGesture>();
            InputGesture = kg;
        }
        else if (change.Property == IconSourceProperty)
        {
            TemplateSettings.Icon = IconHelpers.CreateFromUnknown(change.GetNewValue<IconSource>());
        }
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, true);
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcPressed, false);
        }
    }

    /// <summary>
    /// Raise <see cref="MenuItem.ClickEvent"/> and invke <see cref="Command"/> if ti is set.
    /// </summary>
    protected virtual void OnClick()
    {
        RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, this));

        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    internal void RaiseClick()
    {
        OnClick();
    }

    private void CanExecuteChanged(object sender, EventArgs e)
    {
        var canExec = Command == null || Command.CanExecute(CommandParameter);

        if (canExec != _canExecute)
        {
            _canExecute = canExec;
            UpdateIsEffectivelyEnabled();
        }
    }

    void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged(sender, e);

    private bool _canExecute = true;
    private KeyGesture _hotkey;
}
