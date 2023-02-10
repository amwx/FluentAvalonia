using System;
using System.Windows.Input;
using Avalonia;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a button in <see cref="TaskDialog"/>
/// </summary>
/// <remarks>
/// This type is an abstraction of a button and is used to create the actual button
/// hosted in the TaskDialog.
/// </remarks>
public class TaskDialogButton : TaskDialogControl
{
    public TaskDialogButton() { }

    public TaskDialogButton(string text, object result)
        : base(text, result) { }

    private TaskDialogButton(TaskDialogStandardResult result)
        : this(result.ToString(), result)
    {
        _isStandard = true;
    }

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly DirectProperty<TaskDialogButton, IconSource> IconSourceProperty =
        AvaloniaProperty.RegisterDirect<TaskDialogButton, IconSource>(nameof(IconSource),
            x => x.IconSource, (x, v) => x.IconSource = v);

    /// <summary>
    /// Defines the <see cref="Command"/> property
    /// </summary>
    public static readonly DirectProperty<TaskDialogButton, ICommand> CommandProperty =
        AvaloniaProperty.RegisterDirect<TaskDialogButton, ICommand>(nameof(Command),
            x => x.Command, (x, v) => x.Command = v);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property
    /// </summary>
    public static readonly DirectProperty<TaskDialogButton, object> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<TaskDialogButton, object>(nameof(CommandParameter),
            x => x.CommandParameter, (x, v) => x.CommandParameter = v);

    /// <summary>
    /// Gets or sets the icon displayed in the button
    /// </summary>
    public IconSource IconSource
    {
        get => _iconSource;
        set
        {
            if (_isStandard)
                throw new InvalidOperationException("Cannot add icon to a predefined TaskDialogButton");

            SetAndRaise(IconSourceProperty, ref _iconSource, value);
        }
    }

    /// <summary>
    /// Gets or sets the command that is invoked when the button is clicked
    /// </summary>
    public ICommand Command
    {
        get => _command;
        set
        {
            if (_isStandard)
                throw new InvalidOperationException("Cannot add Command to a predefined TaskDialogButton");

            SetAndRaise(CommandProperty, ref _command, value);
        }
    }

    /// <summary>
    /// Gets or sets the command parameter for the <see cref="Command"/>
    /// </summary>
    public object CommandParameter
    {
        get => _commandParameter;
        set
        {
            if (_isStandard)
                throw new InvalidOperationException("Cannot add icon to a predefined TaskDialogButton");

            SetAndRaise(CommandParameterProperty, ref _commandParameter, value);
        }
    }

    /// <summary>
    /// Raised when the button is clicked
    /// </summary>
    public event TypedEventHandler<TaskDialogButton, EventArgs> Click;

    internal void RaiseClick()
    {
        // Can't really stop event handler attaching, so to prevent ugliness we just won't fire
        // the event here
        if (_isStandard)
            return;

        Click?.Invoke(this, EventArgs.Empty);
    }

    private IconSource _iconSource;
    private ICommand _command;
    private object _commandParameter;
    private bool _isStandard;

    /// <summary>
    /// Predefined button for 'OK'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton OKButton = new TaskDialogButton(TaskDialogStandardResult.OK);

    /// <summary>
    /// Predefined button for 'Cancel'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton CancelButton = new TaskDialogButton(TaskDialogStandardResult.Cancel);

    /// <summary>
    /// Predefined button for 'Yes'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton YesButton = new TaskDialogButton(TaskDialogStandardResult.Yes);

    /// <summary>
    /// Predefined button for 'No'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton NoButton = new TaskDialogButton(TaskDialogStandardResult.No);

    /// <summary>
    /// Predefined button for 'Retry'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton RetryButton = new TaskDialogButton(TaskDialogStandardResult.Retry);

    /// <summary>
    /// Predefined button for 'Close'. Note that predefined buttons cannot have command, icons,
    /// or click handlers attached - they are meant for simple purposes
    /// </summary>
    public static readonly TaskDialogButton CloseButton = new TaskDialogButton(TaskDialogStandardResult.Close);
}
