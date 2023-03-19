using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ColorPickerButtonPage : ControlsPageBase
{
    public ColorPickerButtonPage()
    {
        InitializeComponent();

        DataContext = this;
        TargetType = typeof(ColorPickerButton);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
