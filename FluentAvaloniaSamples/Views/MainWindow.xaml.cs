using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using FluentAvalonia.UI.Media;

namespace FluentAvaloniaSamples.Views
{
    public class MainWindow : Window, ISupportReveal
    {
        public MainWindow()
        {

            //var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            
           // var sty = assets.Open(new System.Uri("resm:FluentAvalonia.Styling.ControlStyles.xaml?assembly=FluentAvalonia"));

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        public bool IsRevealActive { get => true; set => _ = value; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
