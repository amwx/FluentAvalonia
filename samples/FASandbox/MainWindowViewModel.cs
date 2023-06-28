using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FASandbox;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        
    }

    public Command Commands { get; } = new Command();


    public event PropertyChangedEventHandler PropertyChanged;

    public void Test(object param)
    {
        Debug.WriteLine("FIRED");
    }

    private bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName]string propName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            return true;
        }

        return false;
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

