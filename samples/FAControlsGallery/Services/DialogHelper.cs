using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls.Primitives;

namespace FAControlsGallery.Services;

public class DialogHelper
{
    public static async Task ShowUnableToOpenLinkDialog(Uri uri)
    {
        var copyLinkButton = new FATaskDialogCommand
        {
            Text = "Copy Link",
            IconSource = new FASymbolIconSource { Symbol = FASymbol.Link },
            Description = uri.ToString(),
            ClosesOnInvoked = false
        };

        var td = new FATaskDialog
        {
            Content = "It looks like your platform doesn't support Process.Start " +
            "and we are unable to open a link.",
            SubHeader = "Oh No!",
            Commands =
            {
                copyLinkButton
            },
            Buttons =
            {
                FATaskDialogButton.OKButton
            },
            IconSource = new FASymbolIconSource { Symbol = FASymbol.ImportantFilled }
        };

        copyLinkButton.Click += async (s, __) =>
        {
            await ClipboardService.SetTextAsync(uri.ToString());

            var flyout = new Flyout
            {
                Content = "Copied!"
            };

            var comHost = td.FindDescendantOfType<FATaskDialogCommandHost>();

            FlyoutBase.SetAttachedFlyout(comHost, flyout);
            FlyoutBase.ShowAttachedFlyout(comHost);

            DispatcherTimer.RunOnce(() => flyout.Hide(), TimeSpan.FromSeconds(1));
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
