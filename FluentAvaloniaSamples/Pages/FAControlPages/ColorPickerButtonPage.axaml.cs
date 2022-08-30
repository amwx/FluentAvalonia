using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages;

public partial class ColorPickerButtonPage : FAControlsPageBase
{
    public ColorPickerButtonPage()
    {
        InitializeComponent();

        TargetType = typeof(ColorPickerButton);
        Description = "A control that displays a ColorPicker in a flyout. This is useful in compact UIs where the ColorPicker is too large to comfortably fit.";


        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
