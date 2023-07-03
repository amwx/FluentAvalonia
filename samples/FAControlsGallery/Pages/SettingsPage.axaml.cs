using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();

        LaunchRepoLinkItem.Click += LaunchRepoLinkItemClick;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var dc = DataContext as SettingsPageViewModel;
        dc.CurrentAppTheme = Application.Current.ActualThemeVariant;

        if (TryGetResource("SystemAccentColor", null, out var value))
        {
            var color = Unsafe.Unbox<Color>(value);
            dc.CustomAccentColor = color;
            dc.ListBoxColor = color;
        }
    }

    private async void LaunchRepoLinkItemClick(object sender, RoutedEventArgs e)
    {
        var uri = new Uri("https://github.com/amwx/FluentAvalonia");
        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString())
            { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(uri);
        }
    }
}
