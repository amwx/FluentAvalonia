using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class TabViewTests : IDisposable
{
    public TabViewTests()
    {
        _window = new Window();
        CreateTabView();
        _window.Show();
    }

    public TabView TabView { get; private set; }

    [AvaloniaFact]
    public void TabItemsChangedEventFires()
    {
        bool eventFired = false;
        void ItemsChanged(TabView sender, EventArgs e)
        {
            eventFired = true;
        }

        TabView.TabItemsChanged += ItemsChanged;

        (TabView.TabItems as AvaloniaList<TabViewItem>).Add(new TabViewItem());

        Assert.True(eventFired);

        TabView.TabItemsChanged -= ItemsChanged;
    }

    [AvaloniaFact]
    public void AddTabButtonFiresAddTabButtonClickEvent()
    {
        bool eventRaised = false;
        void TabAdd(TabView sender, EventArgs e)
        {
            eventRaised = true;
        }

        TabView.AddTabButtonClick += TabAdd;

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == "AddButton");

        Assert.NotNull(button);

        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs
        {
            RoutedEvent = Button.ClickEvent,
        });

        Assert.True(eventRaised);

        TabView.AddTabButtonClick -= TabAdd;
    }

    [AvaloniaFact]
    public void TabCloseButtonFiresTabCloseRequestedEvent()
    {
        bool eventRaised = false;
        void TabClose(TabView sender, TabViewTabCloseRequestedEventArgs e)
        {
            eventRaised = true;
        }

        TabView.TabCloseRequested += TabClose;

        var firstItem = (TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == "CloseButton");

        Assert.NotNull(button);

        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs
        {
            RoutedEvent = Button.ClickEvent,
        });

        Assert.True(eventRaised);

        TabView.TabCloseRequested -= TabClose;
    }

    [AvaloniaFact]
    public void EqualTabWidthModeProducesEqualWidthTabs()
    {
        // This is the default TabViewWidthMode, so we don't need to switch states
        if (TabView.TabItems is AvaloniaList<TabViewItem> l)
        {
            var firstSize = l[0].Bounds.Width;

            for (int i = 1; i < l.Count; i++)
            {
                Assert.True(l[i].Bounds.Width == firstSize);
            }
        }
    }

    // Other TabViewWidthModes need to be tested manually

    [AvaloniaFact]
    public void TabViewItemCloseButtonOverlayModeWorks()
    {
        var firstItem = (TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        // Default state is for it to be visible
        Assert.NotNull(button);
        Assert.True(button.IsVisible);

        // Currently using the first item, which is selected by default...this should still display its
        // close button even if not pointer over
        TabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.OnPointerOver;
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsVisible);

        // Switch to the second item, which is unselected and should hide its close button by default, if in
        // PointerOver mode
        var secondItem = (TabView.TabItems as AvaloniaList<TabViewItem>)[1];
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        Assert.False(button.IsVisible);

        secondItem.MoveMouseToControl(new Point(10, 10));
        Assert.True(button.IsVisible);
        secondItem.MoveMouseToControl(new Point(-100, -100));
        Assert.False(button.IsVisible);

        TabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.Auto; //Auto is default state, same as Always
    }

    [AvaloniaFact]
    public void TabViewItemIsClosableFalseHidesCloseButton()
    {
        var firstItem = (TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        // Default state is for it to be visible
        Assert.NotNull(button);

        firstItem.IsClosable = false;
        // Even the selected tab should hide it
        Assert.False(button.IsVisible);

        // Make sure it returns
        firstItem.IsClosable = true;
        Assert.True(button.IsVisible);

        // Switch to the second item, just to double check unselected items
        var secondItem = (TabView.TabItems as AvaloniaList<TabViewItem>)[1];
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        secondItem.IsClosable = false;
        Assert.False(button.IsVisible);

        // Make sure it returns
        secondItem.IsClosable = true;
        Assert.True(button.IsVisible);
    }

    [AvaloniaFact]
    public void TabViewAddButtonCommandWorks()
    {
        bool commandInvoked = false;
        void TabAdd(object parameter)
        {
            commandInvoked = true;

            Assert.Equal("Parameter", parameter);
        }

        bool active = true;
        var testCommand = new TestCommand(TabAdd, x => active);
        TabView.AddTabButtonCommand = testCommand;
        TabView.AddTabButtonCommandParameter = "Parameter";

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == "AddButton");

        Assert.NotNull(button);

        Assert.True(button.IsEnabled);

        button.ClickControl(new Point(10, 10), MouseButton.Left);
        Assert.True(commandInvoked);


        active = false;
        testCommand.Invalidate();
        Assert.False(button.IsEffectivelyEnabled);

        TabView.AddTabButtonCommandParameter = null;
        TabView.AddTabButtonCommand = null;
    }

    [AvaloniaFact]
    public void BindingItemsGeneratesTabItems()
    {
        CreateTabView(false);
        var tv = TabView;
        tv.TabItemTemplate = new FuncDataTemplate<TestTabItem>((x, ns) =>
        {
            return new TabViewItem
            {
                [!TabViewItem.HeaderProperty] = new Binding("Header"),
                [!TabViewItem.ContentProperty] = new Binding("Content")
            };
        });

        var items = new List<TestTabItem>
        {
            new TestTabItem { Header = "This is tab 1", Content = "Tab1 Content" },
            new TestTabItem { Header = "This is tab 2", Content = "Tab2 Content" },
            new TestTabItem { Header = "This is tab 3", Content = "Tab2 Content" },
        };
        tv.TabItems = items;

        Dispatcher.UIThread.RunJobs();

        for (int i = 0; i < items.Count; i++)
        {
            var container = tv.ContainerFromIndex(i);
            Assert.IsType<TabViewItem>(container);
        }

        Assert.Equal(0, tv.SelectedIndex);

        CreateTabView(); // Reset it
    }

    private void CreateTabView(bool addTabs = true)
    {
        TabView = new TabView();
        if (addTabs)
            AddTabItems();

        _window.Content = TabView;
    }

    private void AddTabItems()
    {
        TabView.TabItems = new AvaloniaList<TabViewItem>
        {
            new TabViewItem { Header = "Tab1" },
            new TabViewItem { Header = "Tab2" },
            new TabViewItem { Header = "Tab3" }
        };
    }

    public void Dispose()
    {
        _window.Close();
    }

    private Window _window;
}

public class TestTabItem
{
    public string Header { get; set; }

    public string Content { get; set; }
}
