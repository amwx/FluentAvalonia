using System;
using System.Windows.Input;

namespace FluentAvaloniaTests.Helpers;

internal class TestCommand : ICommand
{
    public TestCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod = null)
    {
        _executeMethod = executeMethod;
        _canExecuteMethod = canExecuteMethod;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) =>
        _canExecuteMethod?.Invoke(parameter) ?? true;

    public void Execute(object parameter) =>
        _executeMethod.Invoke(parameter);

    public void Invalidate()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private Action<object> _executeMethod;
    private Func<object, bool> _canExecuteMethod;
}
