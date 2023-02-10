using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages;

public partial class RangeControlsPage : UserControl
{
    public RangeControlsPage()
    {
        InitializeComponent();

        this.FindControl<ButtonSpinner>("TargetButtonSpinner").Spin += (s, e) =>
        {
            _currentIndex = _currentIndex + (e.Direction == SpinDirection.Increase ? 1 : -1);
            if(_currentIndex < 0)
            {
                _currentIndex = values.Length - 1;
            }
            else if (_currentIndex >= values.Length)
            {
                _currentIndex = 0;
            }
            (s as ButtonSpinner).Content = values[_currentIndex];
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private readonly string[] values = { "Item1", "Item2", "Item3", "Item4", "Item5" };
    private int _currentIndex = 0;
}
