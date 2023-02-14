using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class ColorPickerPage : FAControlsPageBase
{
    public ColorPickerPage()
    {
        InitializeComponent();

        DataContext = new ColorPickerPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
