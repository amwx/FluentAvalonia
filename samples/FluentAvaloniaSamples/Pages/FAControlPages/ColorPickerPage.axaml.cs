using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages;

public partial class ColorPickerPage : FAControlsPageBase
{
    public ColorPickerPage()
    {
        InitializeComponent();

        TargetType = typeof(FAColorPicker);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.ColorPicker";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.colorpicker?view=winui-3.0");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/color-picker");
        Description = "A selectable color spectrum. This is a custom implementation based on the WinUI, WinUI toolkit, and other color pickers I've used.";


        DataContext = new ColorPickerPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
