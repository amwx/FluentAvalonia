using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        var dc = DataContext as SettingsPageViewModel;
        dc.CurrentAppTheme = Application.Current.ActualThemeVariant;

        if (TryGetResource("SystemAccentColor", null, out var value))
        {
            var color = Unsafe.Unbox<Color>(value);
            dc.CustomAccentColor = color;
            dc.ListBoxColor = color;
        }
    }
}
