using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages
{
    public partial class RangeControlsPage : UserControl
    {
        public RangeControlsPage()
        {
            InitializeComponent();

            this.FindControl<ButtonSpinner>("TargetButtonSpinner").Spin += (s, e) =>
            {
                (s as ButtonSpinner).Content = values[++_currentIndex];
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private readonly string[] values = { "Item1", "Item2", "Item3", "Item4", "Item5" };
        private int _currentIndex = 0;
    }
}
