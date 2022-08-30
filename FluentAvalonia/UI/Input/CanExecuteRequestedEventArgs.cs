using System;

namespace FluentAvalonia.UI.Input;

/// <summary>
/// Provides event data for the CanExecuteRequested event.
/// </summary>
public class CanExecuteRequestedEventArgs : EventArgs
{
    internal CanExecuteRequestedEventArgs(object param)
    {
        Parameter = param;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ICommand that raised this event is able to execute.
    /// </summary>
    public bool CanExecute { get; set; } = true;

    /// <summary>
    /// Gets the command parameter passed into the CanExecute method that raised this event.
    /// </summary>
    public object Parameter { get; }
}
