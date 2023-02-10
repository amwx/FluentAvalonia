using System;
using System.Diagnostics;
using System.Windows.Input;

namespace FASandbox;

public class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        
    }

    public Command Commands { get; } = new Command();

    public void Test(object param)
    {
        Debug.WriteLine("FIRED");
    }
}

public class Command : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        Debug.WriteLine("FIRED");
    }
}

