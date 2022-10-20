using System;
using System.Windows.Input;

namespace FluentAvaloniaTests.Helpers;

public class TestCommand : ICommand
{
    public TestCommand(Action<object> execute)
    {
        _execute = execute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) =>
        _canExecute;

    public void Execute(object parameter)
    {
        _execute.Invoke(parameter);
    }

    public void SetCanExecute(bool canExecute)
    {
        _canExecute = canExecute;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool _canExecute = true;
    private Action<object> _execute;
}
