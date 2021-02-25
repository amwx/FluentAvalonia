using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages
{
    public class ThemeManagerPage : UserControl
    {
        public ThemeManagerPage()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
