using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvaloniaTests.Helpers;
using Xunit;
using Button = Avalonia.Controls.Button;

namespace FluentAvaloniaTests.ControlTests;

public class TaskDialogTestContext : IDisposable
{
    public TaskDialogTestContext()
    {
        _appDisposable = UnitTestApplication.Start();

        Setup();
        Root.LayoutManager.ExecuteInitialLayoutPass();
        Root.LayoutManager.ExecuteLayoutPass();
    }

    //public TaskDialog Dialog { get; }

    public TestRoot Root { get; private set; }

    public IVisual VisualRoot { get; private set; }

    private void Setup()
    {
        Root = new TestRoot(new Avalonia.Size(1280, 720));
        var vlm = new VisualLayerManager();
        Root.Child = vlm;

        VisualRoot = new Decorator();
        vlm.Child = VisualRoot as IControl;
        
        Root.StylingParent = UnitTestApplication.Current;
        
        //UnitTestApplication.Current.Styles.Add(
        //   (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/OverlayPopupHostStyles.axaml")));
        //UnitTestApplication.Current.Styles.Add(
        //   (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/TaskDialog/TaskDialogStyles.axaml")));
        //UnitTestApplication.Current.Styles.Add(
        //    (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollBarStyles.axaml")));
        //UnitTestApplication.Current.Styles.Add(
        //    (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollViewerStyles.axaml")));
        //UnitTestApplication.Current.Styles.Add(
        //   (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ButtonStyles.axaml")));
    }

    public void Dispose()
    {
        _appDisposable.Dispose();
    }

    private IDisposable _appDisposable;
}
public class TaskDialogTests : IClassFixture<TaskDialogTestContext>
{
    public TaskDialogTests(TaskDialogTestContext ctx)
    {
        Context = ctx;
    }

    public TaskDialogTestContext Context { get; }

    [Fact(Timeout = 5000)]
    public async void OpeningEventsFire()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        bool opening = false;
        bool opened = false;

        void OnOpening(object sender, EventArgs e)
        {
            opening = true;
        }

        void OnOpened(object sender, EventArgs e)
        {
            opened = true;
            td.Hide();
        }

        td.Opening += OnOpening;
        td.Opened += OnOpened;

        await td.ShowAsync(true);

        Assert.True(opening);
        Assert.True(opened);

        td.Opening -= OnOpening;
        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void ClosingEventsFire()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        bool closing = false;
        bool closed = false;

        void OnClosing(object sender, TaskDialogClosingEventArgs e)
        {
            closing = true;
        }

        void OnClosed(object sender, EventArgs e)
        {
            closed = true;
            td.Hide();
        }

        void OnOpened(object sender, EventArgs e)
        {
            td.Hide();
        }

        td.Opened += OnOpened;
        td.Closing += OnClosing;
        td.Closed += OnClosed;

        await td.ShowAsync(true);

        Assert.True(closing);
        Assert.True(closed);

        td.Opened -= OnOpened;
        td.Closing -= OnClosing;
        td.Closed -= OnClosed;
    }

    [Fact(Timeout = 5000)]
    public async void CustomButtonResultWorks()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        var button = new TaskDialogButton("Button", "CustomResult");

        void OnOpened(object sender, EventArgs e)
        {
            Context.Root.LayoutManager.ExecuteLayoutPass();

            var actualButton = td.FindDescendantOfType<TaskDialogButtonHost>();

            actualButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, actualButton));
        }

        td.Opened += OnOpened;

        td.Buttons.Add(button);

        var result = await td.ShowAsync(true);

        Assert.Equal("CustomResult", result);

        td.Buttons.Clear();

        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void CustomButtonClickAndCommandHandlersWork()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        var button = new TaskDialogButton("Button", "CustomResult");

        bool clickFired = false;
        button.Click += (s, e) =>
        {
            clickFired = true;
        };

        bool commandFired = false;
        button.Command = new TestCommand((x) =>
        {
            commandFired = true;
        });

        void OnOpened(object sender, EventArgs e)
        {
            Context.Root.LayoutManager.ExecuteLayoutPass();

            var actualButton = td.FindDescendantOfType<TaskDialogButtonHost>();

            //actualButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, actualButton));
            // Have to trigger pointer event to properly get click/command to work
            // But having trouble getting fake pointer events to pass the 
            // GetVisualsAt test Button does in PointerReleased, so will invoke via reflection
            var clickMethod =
                actualButton.GetType().GetMethod("OnClick",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            clickMethod?.Invoke(actualButton, null);
        }

        td.Opened += OnOpened;

        td.Buttons.Add(button);

        _ = await td.ShowAsync(true);

        Assert.True(clickFired);
        Assert.True(commandFired);

        td.Buttons.Clear();

        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void CommandReturnsItsDialogResult()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        var button = new TaskDialogCommand
        {
            Text = "Command",
            DialogResult = "CommandResult"
        };

        void OnOpened(object sender, EventArgs e)
        {
            Context.Root.LayoutManager.ExecuteInitialLayoutPass();
            Context.Root.LayoutManager.ExecuteLayoutPass();

            var actualButton = td.FindDescendantOfType<TaskDialogCommandHost>();

            actualButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, actualButton));
        }

        td.Opened += OnOpened;

        td.Commands.Add(button);

        var result = await td.ShowAsync(true);

        Assert.Equal("CommandResult", result);

        td.Commands.Clear();

        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void CommandClickAndCommandHandlersWork()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        var button = new TaskDialogCommand
        {
            Text = "Command",
            DialogResult = "CommandResult"
        };

        bool clickFired = false;
        button.Click += (s, e) =>
        {
            clickFired = true;
        };

        bool commandFired = false;
        button.Command = new TestCommand((x) =>
        {
            commandFired = true;
        });

        void OnOpened(object sender, EventArgs e)
        {
            Context.Root.LayoutManager.ExecuteLayoutPass();

            var actualButton = td.FindDescendantOfType<TaskDialogCommandHost>();

            //actualButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, actualButton));
            // Have to trigger pointer event to properly get click/command to work
            // But having trouble getting fake pointer events to pass the 
            // GetVisualsAt test Button does in PointerReleased, so will invoke via reflection
            var clickMethod =
                actualButton.GetType().GetMethod("OnClick",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            clickMethod?.Invoke(actualButton, null);
        }

        td.Opened += OnOpened;

        td.Commands.Add(button);

        _ = await td.ShowAsync(true);

        Assert.True(clickFired);
        Assert.True(commandFired);

        td.Commands.Clear();

        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void EscapeKeyCancelsDialogWithNoneResult()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        void OnOpened(object sender, EventArgs e)
        {
            td.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent, 
                Route = RoutingStrategies.Tunnel,
                Key = Key.Escape
            });
        }

        td.Opened += OnOpened;

        var result = await td.ShowAsync(true);

        Assert.Equal(TaskDialogStandardResult.None, result);

        td.Opened -= OnOpened;
    }

    [Fact(Timeout = 5000)]
    public async void EnterKeyClosesDialogWithDefaultButtonResult()
    {
        var td = new TaskDialog { XamlRoot = Context.VisualRoot };

        td.Buttons.Add(new TaskDialogButton("Button", "result")
        {
            IsDefault = true
        });
        td.Buttons.Add(new TaskDialogButton("Cancel", TaskDialogStandardResult.Cancel));

        bool clickFired = false;
        td.Buttons[0].Click += (s, e) =>
        {
            clickFired = true;
        };

        bool commandFired = false;
        td.Buttons[0].Command = new TestCommand(x =>
        {
            commandFired = true;
        });

        void OnOpened(object sender, EventArgs e)
        {
            // Force apply template - otherwise it isn't done yet and buttons aren't
            // created yet
            td.ApplyTemplate();

            td.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Route = RoutingStrategies.Tunnel,
                Key = Key.Enter
            });
        }

        td.Opened += OnOpened;

        var result = await td.ShowAsync(true);

        Assert.Equal("result", result);
        Assert.True(commandFired);
        Assert.True(clickFired);

        td.Opened -= OnOpened;
    }
}
