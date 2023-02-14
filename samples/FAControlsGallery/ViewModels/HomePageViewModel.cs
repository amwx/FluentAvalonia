using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;

namespace FAControlsGallery.ViewModels;

public class HomePageViewModel : MainPageViewModelBase
{
    public HomePageViewModel()
    {
        Pages = new List<HomeNavPageViewModel>
        {
            new HomeNavPageViewModel("Documentation", new Uri("https://amwx.github.io/FluentAvaloniaDocs/")),
            new HomeNavPageViewModel("Github Repo", new Uri("https://www.github.com/amwx/FluentAvalonia")),
            new HomeNavPageViewModel("Avalonia Repo", new Uri("https://www.github.com/AvaloniaUI/Avalonia")),
            new HomeNavPageViewModel("Fluent Design", new Uri("https://learn.microsoft.com/en-us/windows/apps/design/"))
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

    public void Navigate()
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
            ShowErrorDialog(NavigateURI);
        }
    }

    private async void ShowErrorDialog(Uri uri)
    {
        var copyLinkButton = new TaskDialogCommand
        {
            Text = "Copy Link",
            IconSource = new SymbolIconSource { Symbol = Symbol.Link },
            Description = uri.ToString(),
            ClosesOnInvoked = false
        };
        
        var td = new TaskDialog
        {
            Content = "It looks like your platform doesn't support Process.Start " +
            "and we are unable to open a link.",
            SubHeader = "Oops!", 
            Commands =
            {
                copyLinkButton
            },
            Buttons =
            {
                TaskDialogButton.OKButton
            },
            IconSource = new SymbolIconSource { Symbol = Symbol.ImportantFilled }
        };

        copyLinkButton.Click += async (s, __) =>
        {
            await Application.Current.Clipboard.SetTextAsync(uri.ToString());

            var flyout = new Flyout
            {
                Content = "Copied!"
            };

            var comHost = td.FindDescendantOfType<TaskDialogCommandHost>();

            FlyoutBase.SetAttachedFlyout(comHost, flyout);
            FlyoutBase.ShowAttachedFlyout(comHost);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (_, __) =>
            {
                flyout.Hide();
            };
            timer.Start();
        };

        var app = Application.Current.ApplicationLifetime;
        if (app is IClassicDesktopStyleApplicationLifetime desktop)
        {
            td.XamlRoot = desktop.MainWindow;
        }
        else if (app is ISingleViewApplicationLifetime single)
        {
            td.XamlRoot = TopLevel.GetTopLevel(single.MainView);
        }

        await td.ShowAsync(true);
    }
}
