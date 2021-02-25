using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public class BasicControls : UserControl
    {
        public BasicControls()
        {
            this.InitializeComponent();

            DataContext = new BasicControlsViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
