using Avalonia.Controls;
using FAControlsGallery.Services;

namespace FAControlsGallery.ViewModels;

public sealed class HomeNavPageViewModel
{
    public HomeNavPageViewModel() { }

    public HomeNavPageViewModel(string title, Uri uri)
    {
        Title = title;
        NavigateURI = uri;
    }

    public string Title { get; set; }

    public string ImageUri { get; set; }

    public Uri NavigateURI { get; set; }

    public async void Navigate()
    {
        if (Design.IsDesignMode)
            return;

        try
        {
            var result = await NavigationService.Instance.LaunchURI(NavigateURI);

            if (!result)
                RaiseError();
        }
        catch
        {
            RaiseError();
        }

        async void RaiseError()
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(NavigateURI);
        }
    }
}
