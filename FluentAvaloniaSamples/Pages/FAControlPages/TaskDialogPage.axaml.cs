using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages
{
    public partial class TaskDialogPage : FAControlsPageBase
    {
        public TaskDialogPage()
        {
            InitializeComponent();
            TargetType = typeof(TaskDialog);
            Description = "TaskDialog provides an asynchronous dialog to display advanced content to the user";

            _apiInActionTD = this.FindControl<TaskDialog>("TaskDialog1");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void LaunchAPIInActionTaskDialog(object sender, RoutedEventArgs args)
        {
            var td = new TaskDialog
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
            
            td.XamlRoot = VisualRoot;
            var result = await td.ShowAsync(this.FindControl<CheckBox>("ShowWindowedCheck").IsChecked == false);

            this.FindControl<TextBlock>("LastResultText").Text = $"Last Dialog Result: {result}";
        }

        private void TaskDialog1ShowIconChecked(object sender, RoutedEventArgs args)
        {
            _apiInActionTD.IconSource =
                new PathIconSource { Data = StreamGeometry.Parse("M 24,4 A 1.8865337,1.8865337 0 0 0 22.351562,4.9707031 L 2.2363281,41.197266 A 1.8865337,1.8865337 0 0 0 3.8867188,44 H 44.113281 A 1.8865337,1.8865337 0 0 0 45.763672,41.197266 L 25.648438,4.9707031 A 1.8865337,1.8865337 0 0 0 24,4 Z M 22.28125,11.119141 H 25.621094 L 25.269531,31.236328 H 22.632812 Z M 24,35.025391 C 24.638021,35.025391 25.171875,35.240235 25.601562,35.669922 26.044271,36.099609 26.265625,36.626953 26.265625,37.251953 26.265625,37.876953 26.044271,38.404296 25.601562,38.833984 25.171875,39.263672 24.638021,39.478516 24,39.478516 23.361979,39.478516 22.821615,39.263672 22.378906,38.833984 21.949219,38.404296 21.734375,37.876953 21.734375,37.251953 21.734375,36.626953 21.949219,36.099609 22.378906,35.669922 22.821615,35.240235 23.361979,35.025391 24,35.025391 Z") };
        }

        private void TaskDialog1ShowIconUnchecked(object sender, RoutedEventArgs args)
        {
            _apiInActionTD.IconSource = null;
        }

        private void TaskDialog1CommandShowIconChecked(object sender, RoutedEventArgs args)
        {
            _apiInActionTD.Commands[0].IconSource =
                new SymbolIconSource { Symbol = Symbol.MapPinFilled };
        }

        private void TaskDialog1CommandShowIconUnchecked(object sender, RoutedEventArgs args)
        {
            _apiInActionTD.Commands[0].IconSource = null;
        }

        private void TaskDialog1ProgressIndetChecked(object sender, RoutedEventArgs args)
        {
            _progressFlags |= TaskDialogProgressState.Indeterminate;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressIndetUnchecked(object sender, RoutedEventArgs args)
        {
            _progressFlags &= ~TaskDialogProgressState.Indeterminate;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressNormalChecked(object sender, RoutedEventArgs args)
        {
            _progressFlags |= TaskDialogProgressState.Normal;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressNormalUnchecked(object sender, RoutedEventArgs args)
        {
            _progressFlags &= ~TaskDialogProgressState.Normal;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressErrorChecked(object sender, RoutedEventArgs args)
        {
            _progressFlags |= TaskDialogProgressState.Error;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressErrorUnchecked(object sender, RoutedEventArgs args)
        {
            _progressFlags &= ~TaskDialogProgressState.Error;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressSuspendedChecked(object sender, RoutedEventArgs args)
        {
            _progressFlags |= TaskDialogProgressState.Suspended;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1ProgressSuspendedUnchecked(object sender, RoutedEventArgs args)
        {
            _progressFlags &= ~TaskDialogProgressState.Suspended;
            _apiInActionTD.SetProgressBarState(50, _progressFlags);
        }

        private void TaskDialog1UpdateButtons(object sender, RoutedEventArgs args)
        {
            var l = new List<TaskDialogButton>();
            if (this.FindControl<CheckBox>("OKCheck") is CheckBox b && b.IsChecked == true)
            {
                l.Add(TaskDialogButton.OKButton);
            }
            if (this.FindControl<CheckBox>("CancelCheck") is CheckBox b2 && b2.IsChecked == true)
            {
                l.Add(TaskDialogButton.CancelButton);
            }
            if (this.FindControl<CheckBox>("YesCheck") is CheckBox b3 && b3.IsChecked == true)
            {
                l.Add(TaskDialogButton.YesButton);
            }
            if (this.FindControl<CheckBox>("NoCheck") is CheckBox b4 && b4.IsChecked == true)
            {
                l.Add(TaskDialogButton.NoButton);
            }
            if (this.FindControl<CheckBox>("RetryCheck") is CheckBox b5 && b5.IsChecked == true)
            {
                l.Add(TaskDialogButton.RetryButton);
            }
            if (this.FindControl<CheckBox>("CloseCheck") is CheckBox b6 && b6.IsChecked == true)
            {
                l.Add(TaskDialogButton.CloseButton);
            }
            if (this.FindControl<CheckBox>("CustomCheck") is CheckBox b7 && b7.IsChecked == true)
            {
                l.Add(new TaskDialogButton("Custom", "CustomButtonResult"));
            }

            _apiInActionTD.Buttons.Clear();
            for (int i = 0; i < l.Count; i++)
            {
                _apiInActionTD.Buttons.Add(l[i]);
            }

            typeof(TaskDialog).GetMethod("SetButtons", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(_apiInActionTD, null);

        }


        private async void LaunchProgressDialog(object sender, RoutedEventArgs args)
        {
            var td = new TaskDialog
            {
                Title = "FluentAvalonia",
                ShowProgressBar = true,
                IconSource = new SymbolIconSource { Symbol = Symbol.Download },
                SubHeader = "Downloading",
                Content = "Please wait while your file downloads",
                Buttons =
                {
                    TaskDialogButton.CancelButton
                }
            };

            td.Opened += async (s, e) =>
            {
                // We immediately begin the progress task as soon as the dialog opens
                await Task.Run(async () =>
                {
                    int progress = 0;
                    TaskDialogProgressState state = TaskDialogProgressState.Normal;
                    int delay = 100;
                    while (progress < 100)
                    {
                        await Task.Delay(delay);
                        progress++;

                        if (progress == 50)
                        {
                            state = TaskDialogProgressState.Indeterminate | TaskDialogProgressState.Suspended;

                            // If you update the TaskDialog - remember we're not on the UI Thread, post via dispatcher
                            Dispatcher.UIThread.Post(() => { td.Content = "Experiencing network connectivitiy issues. Download speed reduced"; });
                            delay = 1000;
                        }
                        else if (progress == 60)
                        {
                            state = TaskDialogProgressState.Normal;

                            Dispatcher.UIThread.Post(() => { td.Content = "All good. Resuming normal downloading."; });
                            delay = 100;
                        }

                        // This automatically does UIThread invoking, so just call this normally
                        td.SetProgressBarState(progress, state);
                    }
                    Debug.WriteLine("FINISHED");
                    // All done, auto close the dialog here
                    Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.OK); });
                });
            };

            // Don't forget to set the XamlRoot!!
            td.XamlRoot = VisualRoot;
            var result = await td.ShowAsync();

            this.FindControl<TextBlock>("ProgressTaskResultText").Text = $"File Download Status: {result}";
        }

        private async void LaunchDeferralDialog(object sender, RoutedEventArgs args)
        {
            var td = new TaskDialog
            {
                Title = "FluentAvalonia",
                ShowProgressBar = false,
                Content = "Are you sure you want to delete the file?\n" +
                @"../Directory/One/file.txt",
                Buttons =
                {
                    TaskDialogButton.YesButton,
                    TaskDialogButton.NoButton
                }
            };

            // Use the closing event to grab a deferral
            // You can also cancel closing here if you like
            td.Closing += (s, e) =>
            {
                // We only want to use the deferral on the 'Yes' Button
                if ((TaskDialogStandardResult)e.Result == TaskDialogStandardResult.Yes)
                {
                    var deferral = e.GetDeferral();

                    td.ShowProgressBar = true;
                    int value = 0;
                    DispatcherTimer timer = null;
                    void Tick(object s, EventArgs e)
                    {
                        td.SetProgressBarState(++value, TaskDialogProgressState.Normal);
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
            td.XamlRoot = VisualRoot;
            _ = await td.ShowAsync();
        }

        private TaskDialogProgressState _progressFlags = TaskDialogProgressState.Normal;
        private TaskDialog _apiInActionTD;
    }
}
