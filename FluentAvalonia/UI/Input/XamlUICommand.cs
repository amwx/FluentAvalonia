using Avalonia;
using System.Windows.Input;

namespace FluentAvalonia.UI.Input;

/// <summary>
/// Provides a base class for defining the command behavior of an interactive UI element that 
/// performs an action when invoked (such as sending an email, deleting an item, or submitting a form).
/// </summary>
public partial class XamlUICommand : AvaloniaObject, ICommand
{
    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, null);

    public bool CanExecute(object param)
    {
        bool canExec = false;

        var args = new CanExecuteRequestedEventArgs(param);

        CanExecuteRequested?.Invoke(this, args);

        canExec = args.CanExecute;

        var command = Command;
        if (command != null)
        {
            bool canExecCommand = command.CanExecute(param);
            canExec = canExec && canExecCommand;
        }

        return canExec;
    }

    public void Execute(object param)
    {
        var args = new ExecuteRequestedEventArgs(param);

        ExecuteRequested?.Invoke(this, args);

        Command?.Execute(param);
    }
}
