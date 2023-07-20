using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class ContentDialogTests : IDisposable
{
    public ContentDialogTests()
    {
        _window = new Window();
        _window.Show();
    }

    [AvaloniaFact]
    public void NotSettingButtonTextShowsNoButtons()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;
        dlg.ApplyTemplate();
        dlg.SetupDialog();

        var items = dlg.GetVisualDescendants().OfType<Button>();

        foreach (var item in items)
        {
            Assert.False(item.IsVisible);
        }
        _window.Content = null;
    }

    [AvaloniaFact]
    public void SettingPrimaryButtonTextShowsPrimaryButton()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;

        dlg.PrimaryButtonText = "Primary";
        dlg.ApplyTemplate();
        dlg.SetupDialog();

        var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
        Assert.NotNull(button);

        Assert.True(button.IsVisible);
        Assert.Equal("Primary", button.Content);
        _window.Content = null;
    }

    [AvaloniaFact]
    public void SettingSecondaryButtonTextShowsSecondaryButton()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;

        dlg.SecondaryButtonText = "Secondary";
        dlg.ApplyTemplate();
        dlg.SetupDialog();

        var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "SecondaryButton").FirstOrDefault();
        Assert.NotNull(button);

        Assert.True(button.IsVisible);
        Assert.Equal("Secondary", button.Content);
        _window.Content = null;
    }

    [AvaloniaFact]
    public void SettingCloseButtonTextShowsCloseButton()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;

        dlg.CloseButtonText = "Close";
        dlg.ApplyTemplate();
        dlg.SetupDialog();

        var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "CloseButton").FirstOrDefault();
        Assert.NotNull(button);

        Assert.True(button.IsVisible);
        Assert.Equal("Close", button.Content);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void ContentDialogInCodeShows()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;

        bool shown = false;
        dlg.Opened += (_, __) =>
        {
            shown = true;
            Assert.Equal(_window, TopLevel.GetTopLevel(dlg));
            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);

        Assert.True(shown);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void AlreadyParentedContentDialogShows()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;

        bool shown = false;
        dlg.Opened += (_, __) =>
        {
            shown = true;
            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);

        Assert.True(shown);
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void ContentDialogShowsAtCorrectWindow()
    {
        var dlg = new ContentDialog();
        _window.Content = dlg;
        var wnd2 = new Window();
        wnd2.Show();

        TopLevel topLevel = null;
        dlg.Opened += (_, __) =>
        {
            topLevel = TopLevel.GetTopLevel(dlg);
            dlg.Hide();
        };

        var res = await dlg.ShowAsync(wnd2);

        Assert.Equal(wnd2, topLevel);
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void PrimaryButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };

        bool clicked = false;
        bool commandInvoked = false;
        dlg.PrimaryButtonClick += (s, e) =>
        {
            // Command should fire AFTER button event
            Assert.False(commandInvoked);
            clicked = true;
        };

        var com = new TestCommand(_ =>
        {
            commandInvoked = true;
        });
        dlg.PrimaryButtonCommand = com;

        var res = await dlg.ShowAsync(window);

        Assert.True(clicked);
        Assert.True(commandInvoked);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void SecondaryButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.SecondaryButtonText = "Secondary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "SecondaryButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };

        bool clicked = false;
        bool commandInvoked = false;
        dlg.SecondaryButtonClick += (s, e) =>
        {
            // Command should fire AFTER button event
            Assert.False(commandInvoked);
            clicked = true;
        };

        var com = new TestCommand(_ =>
        {
            commandInvoked = true;
        });
        dlg.SecondaryButtonCommand = com;

        var res = await dlg.ShowAsync(window);

        Assert.True(clicked);
        Assert.True(commandInvoked);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void CloseButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.CloseButtonText = "Close";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "CloseButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };

        bool clicked = false;
        bool commandInvoked = false;
        dlg.CloseButtonClick += (s, e) =>
        {
            // Command should fire AFTER button event
            Assert.False(commandInvoked);
            clicked = true;
        };

        var com = new TestCommand(_ =>
        {
            commandInvoked = true;
        });
        dlg.CloseButtonCommand = com;

        var res = await dlg.ShowAsync(window);

        Assert.True(clicked);
        Assert.True(commandInvoked);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void FocusIsMovedIntoContentDialogUponOpening()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary"
        };
        _window.Content = dlg;

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);

        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void OldFocusIsPreservedThroughContentDialog()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary"
        };

        var mainButton = new Button { Content = "hello" };
        _window.Content = new Panel
        {
            Children =
            {
                mainButton
            }
        };

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            dlg.Hide();
        };

        mainButton.Focus();
        Assert.True(mainButton.IsFocused);

        var res = await dlg.ShowAsync(_window);

        Assert.True(mainButton.IsFocused);

        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void PrimaryDefaultButtonGetsInitialFocus()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary
        };

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void SecondaryDefaultButtonGetsInitialFocus()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Secondary
        };

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "SecondaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void CloseDefaultButtonGetsInitialFocus()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close
        };

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "CloseButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void UserCanOverrideInitialFocus()
    {
        var tb = new TextBox();
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = tb
        };

        dlg.Opened += (_, __) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsFocused);

            tb.Focus();

            Assert.True(tb.IsFocused);

            dlg.Hide();
        };

        var res = await dlg.ShowAsync(_window);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void EnterKeyInvokesDefaultButtonIfSet()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary
        };

        dlg.Opened += (_, __) =>
        {
            _window.KeyPress(Key.Enter, RawInputModifiers.None);
            _window.KeyRelease(Key.Enter, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(ContentDialogResult.Primary, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void EscapeKeyClosesDialogWithNoResult()
    {
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary
        };

        dlg.Opened += (_, __) =>
        {
            _window.KeyPress(Key.Escape, RawInputModifiers.None);
            _window.KeyRelease(Key.Escape, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(ContentDialogResult.None, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void UnhandledEnterKeyInUserContentInvokesDefaultButton()
    {
        var tb = new TextBox();
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = tb
        };

        dlg.Opened += (_, __) =>
        {
            tb.Focus();

            _window.KeyPress(Key.Enter, RawInputModifiers.None);
            _window.KeyRelease(Key.Enter, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(ContentDialogResult.Primary, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void HandledEnterKeyInUserContentDoesNotInvokesDefaultButton()
    {
        var tb = new TextBox();
        var dlg = new ContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = tb
        };

        tb.KeyDown += (_, e) =>
        {
            e.Handled = true;
        };

        dlg.Opened += (_, __) =>
        {
            tb.Focus();

            _window.KeyPress(Key.Enter, RawInputModifiers.None);
            _window.KeyRelease(Key.Enter, RawInputModifiers.None);
            dlg.Hide(ContentDialogResult.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(ContentDialogResult.None, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public void UsingEnterKeyToLaunchDialogDoesNotImmediatelyCloseDialogIfDefaultButtonIsSet()
    {

        var mainButton = new Button { Content = "hello" };
        _window.Content = new Panel
        {
            Children =
            {
                mainButton
            }
        };

        mainButton.Click += async (s, e) =>
        {
            var dlg = new ContentDialog
            {
                PrimaryButtonText = "Primary",
                SecondaryButtonText = "Secondary",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary
            };

            dlg.Opened += (_, __) =>
            {
                dlg.Hide(ContentDialogResult.None);
            };

            var res = await dlg.ShowAsync(_window);
            Assert.Equal(ContentDialogResult.None, res);
        };

        mainButton.Focus();

        _window.KeyPress(Key.Enter, RawInputModifiers.None);
        _window.KeyRelease(Key.Enter, RawInputModifiers.None);

        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void CancellingClosingInButtonClickCancelsDialogClosing()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };


        dlg.PrimaryButtonClick += (s, e) =>
        {
            e.Cancel = true;

            Dispatcher.UIThread.Post(() => dlg.Hide(), DispatcherPriority.Background);
        };

        var res = await dlg.ShowAsync(window);

        Assert.Equal(ContentDialogResult.None, res);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void ButtonDeferralWorks()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };

        bool deferralRun = false;

        dlg.PrimaryButtonClick += (s, e) =>
        {
            var def = e.GetDeferral();

            Dispatcher.UIThread.Post(() =>
            {
                deferralRun = true;
                def.Complete();
            }, DispatcherPriority.Background);
        };

        var res = await dlg.ShowAsync(window);
        Assert.True(deferralRun);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void CanCancelWithButtonDeferral()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            var button = dlg.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);

            var transform = button.TransformToVisual(window).Value;
            var bnds = new Rect(button.Bounds.Size).TransformToAABB(transform);
            var pt = bnds.Center;

            window.MouseDown(pt, MouseButton.Left);
            window.MouseUp(pt, MouseButton.Left);
        };

        bool deferralRun = false;

        dlg.PrimaryButtonClick += (s, e) =>
        {
            var def = e.GetDeferral();

            Dispatcher.UIThread.Post(() =>
            {
                deferralRun = true;
                def.Complete();

                e.Cancel = true;

                Dispatcher.UIThread.Post(() =>
                {
                    dlg.Hide();
                }, DispatcherPriority.Background);
            }, DispatcherPriority.Background);
        };

        var res = await dlg.ShowAsync(window);
        Assert.True(deferralRun);
        Assert.Equal(ContentDialogResult.None, res);

        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void ClosingDeferralWorks()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            dlg.Hide();
        };

        bool deferralRun = false;
        dlg.Closing += (s, e) =>
        {
            var def = e.GetDeferral();

            Dispatcher.UIThread.Post(() =>
            {
                deferralRun = true;
                def.Complete();
            }, DispatcherPriority.Background);
        };

        var res = await dlg.ShowAsync(window);
        Assert.True(deferralRun);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void CanCancelWithClosingDeferral()
    {
        var window = new Window();
        var dlg = new ContentDialog();
        dlg.PrimaryButtonText = "Primary";
        window.Show();

        dlg.Opened += (s, e) =>
        {
            dlg.Hide();
        };

        bool ignore = false;
        bool deferralRun = false;
        dlg.Closing += (s, e) =>
        {
            if (ignore)
                return;

            ignore = true;
            var def = e.GetDeferral();

            Dispatcher.UIThread.Post(() =>
            {
                deferralRun = true;
                def.Complete();

                e.Cancel = true;

                Dispatcher.UIThread.Post(() =>
                {
                    dlg.Hide();
                }, DispatcherPriority.Background);
            }, DispatcherPriority.Background);
        };

        var res = await dlg.ShowAsync(window);
        Assert.True(deferralRun);
        window.Close();
    }

    public void Dispose()
    {
        _window.Close();
    }

    private Window _window;
}
