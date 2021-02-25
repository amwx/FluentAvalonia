using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public class IconsPage : UserControl
    {
        public IconsPage()
        {
            this.InitializeComponent();
            DataContext = new IconsPageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
