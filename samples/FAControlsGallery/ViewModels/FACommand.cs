using System;
using System.Windows.Input;

namespace FAControlsGallery.ViewModels;

public class FACommand : ICommand
{
    public FACommand(Action<object> executeMethod)
    {
        _executeMethod = executeMethod;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter)
    {
        _executeMethod.Invoke(parameter);
    }

    public void Invalidate()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private Action<object> _executeMethod;
}
