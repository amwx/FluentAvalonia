using System.Runtime.CompilerServices;
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

        ClearFavoritesButton.Click += ClearFavoritesButtonClick;
        ClearRecentsButton.Click += ClearRecentsButtonClick;
    }

    private void ClearRecentsButtonClick(object sender, RoutedEventArgs e)
    {
        RecentFavoriteService.Instance.ClearRecentItems();
    }

    private void ClearFavoritesButtonClick(object sender, RoutedEventArgs e)
    {
        RecentFavoriteService.Instance.ClearFavorites();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var dc = DataContext as SettingsPageViewModel;

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
            await TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);            
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(uri);
        }
    }
}
