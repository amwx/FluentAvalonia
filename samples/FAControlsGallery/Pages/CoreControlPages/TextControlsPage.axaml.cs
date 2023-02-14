using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class TextControlsPage : UserControl
{
    public TextControlsPage()
    {
        InitializeComponent();

        DataContext = new TextControlsPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
