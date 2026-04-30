using System;
using System.Linq;
using System.Threading.Tasks;
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
        var dlg = new FAContentDialog();
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
        var dlg = new FAContentDialog();
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
        var dlg = new FAContentDialog();
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
        var dlg = new FAContentDialog();
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
    public async Task ContentDialogInCodeShows()
    {
        var dlg = new FAContentDialog();
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
    public async Task AlreadyParentedContentDialogShows()
    {
        var dlg = new FAContentDialog();
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
    public async Task ContentDialogShowsAtCorrectWindow()
    {
        var dlg = new FAContentDialog();
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
    public async Task PrimaryButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
    public async Task SecondaryButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
    public async Task CloseButtonInvokesEventAndCommand()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
    public async Task FocusIsMovedIntoContentDialogUponOpening()
    {
        var dlg = new FAContentDialog
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
    public async Task OldFocusIsPreservedThroughContentDialog()
    {
        var dlg = new FAContentDialog
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
    public async Task PrimaryDefaultButtonGetsInitialFocus()
    {
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary
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
    public async Task SecondaryDefaultButtonGetsInitialFocus()
    {
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Secondary
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
    public async Task CloseDefaultButtonGetsInitialFocus()
    {
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Close
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
    public async Task UserCanOverrideInitialFocus()
    {
        var tb = new TextBox();
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary,
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
    public async Task EnterKeyInvokesDefaultButtonIfSet()
    {
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary
        };

        dlg.Opened += (_, __) =>
        {
            _window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            _window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(FAContentDialogResult.Primary, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task EscapeKeyClosesDialogWithNoResult()
    {
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary
        };

        dlg.Opened += (_, __) =>
        {
            _window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            _window.KeyReleaseQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(FAContentDialogResult.None, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task UnhandledEnterKeyInUserContentInvokesDefaultButton()
    {
        var tb = new TextBox();
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary,
            Content = tb
        };

        dlg.Opened += (_, __) =>
        {
            tb.Focus();

            _window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            _window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(FAContentDialogResult.Primary, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task HandledEnterKeyInUserContentDoesNotInvokesDefaultButton()
    {
        var tb = new TextBox();
        var dlg = new FAContentDialog
        {
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close",
            DefaultButton = FAContentDialogButton.Primary,
            Content = tb
        };

        tb.KeyDown += (_, e) =>
        {
            e.Handled = true;
        };

        dlg.Opened += (_, __) =>
        {
            tb.Focus();

            _window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            _window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            dlg.Hide(FAContentDialogResult.None);
        };

        var res = await dlg.ShowAsync(_window);
        Assert.Equal(FAContentDialogResult.None, res);
        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async void UsingEnterKeyToLaunchDialogDoesNotImmediatelyCloseDialogIfDefaultButtonIsSet()
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
            var dlg = new FAContentDialog
            {
                PrimaryButtonText = "Primary",
                SecondaryButtonText = "Secondary",
                CloseButtonText = "Close",
                DefaultButton = FAContentDialogButton.Primary
            };

            dlg.Opened += (_, __) =>
            {
                dlg.Hide(FAContentDialogResult.None);
            };

            var res = await dlg.ShowAsync(_window);
            Assert.Equal(FAContentDialogResult.None, res);
        };

        mainButton.Focus();

        _window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        _window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        _window.Content = null;
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task CancellingClosingInButtonClickCancelsDialogClosing()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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

        Assert.Equal(FAContentDialogResult.None, res);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task ButtonDeferralWorks()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
    public async Task CanCancelWithButtonDeferral()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
        Assert.Equal(FAContentDialogResult.None, res);

        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task ClosingDeferralWorks()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
    public async Task CanCancelWithClosingDeferral()
    {
        var window = new Window();
        var dlg = new FAContentDialog();
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
