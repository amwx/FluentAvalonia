using Avalonia.Controls;

namespace FASandbox;

// This is set up for quick testing purposes.

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        PointerPressed += (s, e) =>
        {
            int x = 0;
        };
    }
}
