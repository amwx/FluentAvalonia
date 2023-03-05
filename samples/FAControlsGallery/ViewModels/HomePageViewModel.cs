using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FAControlsGallery.Services;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;

namespace FAControlsGallery.ViewModels;

public class HomePageViewModel : MainPageViewModelBase
{
    public HomePageViewModel()
    {
        Pages = new List<HomeNavPageViewModel>
        {
            new HomeNavPageViewModel("Documentation", new Uri("https://amwx.github.io/FluentAvaloniaDocs/"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/Documentation.png"
            },
            new HomeNavPageViewModel("Github Repo", new Uri("https://www.github.com/amwx/FluentAvalonia"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/Github.png"
            },
            new HomeNavPageViewModel("Avalonia Repo", new Uri("https://www.github.com/AvaloniaUI/Avalonia"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/AvGithub.png"
            },
            new HomeNavPageViewModel("Fluent Design", new Uri("https://learn.microsoft.com/en-us/windows/apps/design/"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/FluentDesign.png"
            }
        };
    }

    public List<HomeNavPageViewModel> Pages { get; set; }
}

public class HomeNavPageViewModel
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
            Process.Start(new ProcessStartInfo(NavigateURI.ToString())
            { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(NavigateURI);
        }
    }
}
