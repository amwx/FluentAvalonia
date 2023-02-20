using Avalonia.Markup.Xaml;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ColorPickerPage : FAControlsPageBase
{
    public ColorPickerPage()
    {
        InitializeComponent();

        DataContext = new ColorPickerPageViewModel();
        TargetType = typeof(FAColorPicker);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
