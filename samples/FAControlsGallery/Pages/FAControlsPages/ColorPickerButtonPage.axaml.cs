using Avalonia.Markup.Xaml;

namespace FAControlsGallery.Pages;

public partial class ColorPickerButtonPage : FAControlsPageBase
{
    public ColorPickerButtonPage()
    {
        InitializeComponent();

        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
