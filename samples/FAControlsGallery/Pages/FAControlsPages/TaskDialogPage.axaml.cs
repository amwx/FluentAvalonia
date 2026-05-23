using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class TaskDialogPage : ControlsPageBase
{
    public TaskDialogPage()
    {
        InitializeComponent();
        _apiInActionTD = this.FindControl<FATaskDialog>("TaskDialog1");
        TargetType = typeof(FATaskDialog);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void LaunchAPIInActionTaskDialog(object sender, RoutedEventArgs args)
    {
        var td = new FATaskDialog
        {
            Title = _apiInActionTD.Title,
            Header = _apiInActionTD.Header,
            SubHeader = _apiInActionTD.SubHeader,
            Content = _apiInActionTD.Content,
            IconSource = _apiInActionTD.IconSource,
            ShowProgressBar = _apiInActionTD.ShowProgressBar,
            FooterVisibility = _apiInActionTD.FooterVisibility,
            IsFooterExpanded = _apiInActionTD.IsFooterExpanded,
            Footer = new CheckBox { Content = "Never show me this again" }
        };

        if (_apiInActionTD.ShowProgressBar)
        {
            td.SetProgressBarState(50, _progressFlags);
        }

        td.Commands.Add(_apiInActionTD.Commands[0]);

        for (int i = 0; i < _apiInActionTD.Buttons.Count; i++)
        {
            td.Buttons.Add(_apiInActionTD.Buttons[i]);
        }

        td.XamlRoot = TopLevel.GetTopLevel(this);
        var result = await td.ShowAsync(this.FindControl<CheckBox>("ShowWindowedCheck").IsChecked == false);

        this.FindControl<TextBlock>("LastResultText").Text = $"Last Dialog Result: {result}";
    }

    private void TaskDialog1ShowIconChecked(object sender, RoutedEventArgs args)
    {
        if (sender is CheckBox b)
        {
            _apiInActionTD.IconSource = b.IsChecked == true ?
                new FAPathIconSource { Data = StreamGeometry.Parse("M 24,4 A 1.8865337,1.8865337 0 0 0 22.351562,4.9707031 L 2.2363281,41.197266 A 1.8865337,1.8865337 0 0 0 3.8867188,44 H 44.113281 A 1.8865337,1.8865337 0 0 0 45.763672,41.197266 L 25.648438,4.9707031 A 1.8865337,1.8865337 0 0 0 24,4 Z M 22.28125,11.119141 H 25.621094 L 25.269531,31.236328 H 22.632812 Z M 24,35.025391 C 24.638021,35.025391 25.171875,35.240235 25.601562,35.669922 26.044271,36.099609 26.265625,36.626953 26.265625,37.251953 26.265625,37.876953 26.044271,38.404296 25.601562,38.833984 25.171875,39.263672 24.638021,39.478516 24,39.478516 23.361979,39.478516 22.821615,39.263672 22.378906,38.833984 21.949219,38.404296 21.734375,37.876953 21.734375,37.251953 21.734375,36.626953 21.949219,36.099609 22.378906,35.669922 22.821615,35.240235 23.361979,35.025391 24,35.025391 Z") } :
                null;
        }
    }

    private void TaskDialog1CommandShowIconChecked(object sender, RoutedEventArgs args)
    {
        if (sender is CheckBox b)
        {
            _apiInActionTD.Commands[0].IconSource = b.IsChecked == true ?
                new FASymbolIconSource { Symbol = FASymbol.MapPinFilled } :
                null;
        }
           
    }

    private void TaskDialog1ProgressIndetChecked(object sender, RoutedEventArgs args)
    {
        if (sender is CheckBox rb)
        {
            if (rb.IsChecked == true)
            {
                _progressFlags |= FATaskDialogProgressState.Indeterminate;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
            else
            {
                _progressFlags &= ~FATaskDialogProgressState.Indeterminate;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
        }
        
    }


    private void TaskDialog1ProgressNormalChecked(object sender, RoutedEventArgs args)
    {
        if (sender is RadioButton rb)
        {
            if (rb.IsChecked == true)
            {
                _progressFlags |= FATaskDialogProgressState.Normal;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
            else
            {
                _progressFlags &= ~FATaskDialogProgressState.Normal;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
        }
    }

    private void TaskDialog1ProgressErrorChecked(object sender, RoutedEventArgs args)
    {
        if (sender is RadioButton rb)
        {
            if (rb.IsChecked == true)
            {
                _progressFlags |= FATaskDialogProgressState.Error;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
            else
            {
                _progressFlags &= ~FATaskDialogProgressState.Error;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
        }
    }

    private void TaskDialog1ProgressSuspendedChecked(object sender, RoutedEventArgs args)
    {
        if (sender is RadioButton rb)
        {
            if (rb.IsChecked == true)
            {
                _progressFlags |= FATaskDialogProgressState.Suspended;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
            else
            {
                _progressFlags &= ~FATaskDialogProgressState.Suspended;
                _apiInActionTD.SetProgressBarState(50, _progressFlags);
            }
        }
    }

    private void TaskDialog1UpdateButtons(object sender, RoutedEventArgs args)
    {
        var l = new List<FATaskDialogButton>();
        if (this.FindControl<CheckBox>("OKCheck") is CheckBox b && b.IsChecked == true)
        {
            l.Add(FATaskDialogButton.OKButton);
        }
        if (this.FindControl<CheckBox>("CancelCheck") is CheckBox b2 && b2.IsChecked == true)
        {
            l.Add(FATaskDialogButton.CancelButton);
        }
        if (this.FindControl<CheckBox>("YesCheck") is CheckBox b3 && b3.IsChecked == true)
        {
            l.Add(FATaskDialogButton.YesButton);
        }
        if (this.FindControl<CheckBox>("NoCheck") is CheckBox b4 && b4.IsChecked == true)
        {
            l.Add(FATaskDialogButton.NoButton);
        }
        if (this.FindControl<CheckBox>("RetryCheck") is CheckBox b5 && b5.IsChecked == true)
        {
            l.Add(FATaskDialogButton.RetryButton);
        }
        if (this.FindControl<CheckBox>("CloseCheck") is CheckBox b6 && b6.IsChecked == true)
        {
            l.Add(FATaskDialogButton.CloseButton);
        }
        if (this.FindControl<CheckBox>("CustomCheck") is CheckBox b7 && b7.IsChecked == true)
        {
            l.Add(new FATaskDialogButton("Custom", "CustomButtonResult"));
        }

        _apiInActionTD.Buttons.Clear();
        for (int i = 0; i < l.Count; i++)
        {
            _apiInActionTD.Buttons.Add(l[i]);
        }

        typeof(FATaskDialog).GetMethod("SetButtons", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(_apiInActionTD, null);

    }


    private async void LaunchProgressDialog(object sender, RoutedEventArgs args)
    {
        var td = new FATaskDialog
        {
            Title = "FluentAvalonia",
            ShowProgressBar = true,
            IconSource = new FASymbolIconSource { Symbol = FASymbol.Download },
            SubHeader = "Downloading",
            Content = "Please wait while your file downloads",
            Buttons =
            {
                FATaskDialogButton.CancelButton
            }
        };

        td.Opened += async (s, e) =>
        {
            // We immediately begin the progress task as soon as the dialog opens
            await Task.Run(async () =>
            {
                int progress = 0;
                FATaskDialogProgressState state = FATaskDialogProgressState.Normal;
                int delay = 100;
                while (progress < 100)
                {
                    await Task.Delay(delay);
                    progress++;

                    if (progress == 50)
                    {
                        state = FATaskDialogProgressState.Indeterminate | FATaskDialogProgressState.Suspended;

                        // If you update the TaskDialog - remember we're not on the UI Thread, post via dispatcher
                        Dispatcher.UIThread.Post(() => { td.Content = "Experiencing network connectivitiy issues. Download speed reduced"; });
                        delay = 1000;
                    }
                    else if (progress == 60)
                    {
                        state = FATaskDialogProgressState.Normal;

                        Dispatcher.UIThread.Post(() => { td.Content = "All good. Resuming normal downloading."; });
                        delay = 100;
                    }

                    // This automatically does UIThread invoking, so just call this normally
                    td.SetProgressBarState(progress, state);
                }
                Debug.WriteLine("FINISHED");
                // All done, auto close the dialog here
                Dispatcher.UIThread.Post(() => { td.Hide(FATaskDialogStandardResult.OK); });
            });
        };

        // Don't forget to set the XamlRoot!!
        td.XamlRoot = TopLevel.GetTopLevel(this);
        var result = await td.ShowAsync();

        this.FindControl<TextBlock>("ProgressTaskResultText").Text = $"File Download Status: {result}";
    }

    private async void LaunchDeferralDialog(object sender, RoutedEventArgs args)
    {
        var td = new FATaskDialog
        {
            Title = "FluentAvalonia",
            ShowProgressBar = false,
            Content = "Are you sure you want to delete the file?\n" +
            @"../Directory/One/file.txt",
            Buttons =
            {
                FATaskDialogButton.YesButton,
                FATaskDialogButton.NoButton
            }
        };

        // Use the closing event to grab a deferral
        // You can also cancel closing here if you like
        td.Closing += (s, e) =>
        {
            // We only want to use the deferral on the 'Yes' Button
            if ((FATaskDialogStandardResult)e.Result == FATaskDialogStandardResult.Yes)
            {
                var deferral = e.GetDeferral();

                td.ShowProgressBar = true;
                int value = 0;
                DispatcherTimer timer = null;
                void Tick(object s, EventArgs e)
                {
                    td.SetProgressBarState(++value, FATaskDialogProgressState.Normal);
                    if (value == 100)
                    {
                        timer.Stop();

                        // Call this when you're done. It will signal the dialog to resume closing
                        deferral.Complete();
                    }
                }
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(75), DispatcherPriority.Normal, Tick);

                timer.Start();
            }
        };

        // Don't forget to set the XamlRoot!!
        td.XamlRoot = TopLevel.GetTopLevel(this);
        _ = await td.ShowAsync();
    }

    private FATaskDialogProgressState _progressFlags = FATaskDialogProgressState.Normal;
    private FATaskDialog _apiInActionTD;
}
