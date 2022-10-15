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
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class TabViewTestsContext : IDisposable
{
    public TabViewTestsContext()
    {
        _appDisposable = UnitTestApplication.Start();

        CreateTabView();
    }

    public TabView TabView { get; private set; }

    public TestRoot Root { get; private set; }

    public void Dispose()
    {
        _appDisposable.Dispose();
    }

    public void ResetTabView()
    {
        TabView = new TabView();
        AddTabItems();

        Root.Child = TabView;

        Root.LayoutManager.ExecuteInitialLayoutPass();
        Root.LayoutManager.ExecuteLayoutPass();
    }

    private void CreateTabView()
    {
        Root = new TestRoot(new Size(1280, 720));
        Root.StylingParent = UnitTestApplication.Current;

        //UnitTestApplication.Current.Styles.Add(
        //    (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/TabView/TabViewStyles.axaml")));

        TabView = new TabView();
        AddTabItems();

        Root.Child = TabView;

        Root.LayoutManager.ExecuteInitialLayoutPass();
        Root.LayoutManager.ExecuteLayoutPass();
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

    private IDisposable _appDisposable;
}

public class TabViewTests : IClassFixture<TabViewTestsContext>
{
    public TabViewTests(TabViewTestsContext ctx)
    {
        Context = ctx;
    }

    public TabViewTestsContext Context { get; }

    [Fact]
    public void TabItemsChangedEventFires()
    {
        bool eventFired = false;
        void ItemsChanged(TabView sender, EventArgs e)
        {
            eventFired = true;
        }

        Context.TabView.TabItemsChanged += ItemsChanged;

        (Context.TabView.TabItems as AvaloniaList<TabViewItem>).Add(new TabViewItem());

        Assert.True(eventFired);

        Context.TabView.TabItemsChanged -= ItemsChanged;
    }

    [Fact]
    public void AddTabButtonFiresAddTabButtonClickEvent()
    {
        bool eventRaised = false;
        void TabAdd(TabView sender, EventArgs e)
        {
            eventRaised = true;
        }

        Context.TabView.AddTabButtonClick += TabAdd;

        var button = Context.TabView.GetVisualDescendants()
            .OfType<Button>()
            .Where(x => x.Name == "AddButton")
            .FirstOrDefault();

        Assert.NotNull(button);

        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs
        {
            RoutedEvent = Button.ClickEvent,
        });

        Assert.True(eventRaised);

        Context.TabView.AddTabButtonClick -= TabAdd;
    }

    [Fact]
    public void TabCloseButtonFiresTabCloseRequestedEvent()
    {
        bool eventRaised = false;
        void TabClose(TabView sender, TabViewTabCloseRequestedEventArgs e)
        {
            eventRaised = true;
        }

        Context.TabView.TabCloseRequested += TabClose;

        var firstItem = (Context.TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
            .OfType<Button>()
            .Where(x => x.Name == "CloseButton")
            .FirstOrDefault();

        Assert.NotNull(button);

        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs
        {
            RoutedEvent = Button.ClickEvent,
        });

        Assert.True(eventRaised);

        Context.TabView.TabCloseRequested -= TabClose;
    }

    [Fact]
    public void EqualTabWidthModeProducesEqualWidthTabs()
    {
        // This is the default TabViewWidthMode, so we don't need to switch states
        if (Context.TabView.TabItems is AvaloniaList<TabViewItem> l)
        {
            var firstSize = l[0].Bounds.Width;

            for (int i = 1; i < l.Count; i++)
            {
                if (l[i].Bounds.Width != firstSize)
                {
                    Assert.False(true);
                }
            }
        }

        Assert.True(true);
    }

    // Other TabViewWidthModes need to be tested manually

    [Fact]
    public void TabViewItemCloseButtonOverlayModeWorks()
    {
        var firstItem = (Context.TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
           .OfType<Button>()
           .Where(x => x.Name == "CloseButton")
           .FirstOrDefault();

        // Default state is for it to be visible
        Assert.NotNull(button);
        Assert.True(button.IsVisible);

        // Currently using the first item, which is selected by default...this should still display its
        // close button even if not pointer over
        Context.TabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.OnPointerOver;
        Assert.True(button.IsVisible);
        
        // Switch to the second item, which is unselected and should hide its close button by default, if in
        // PointerOver mode
        var secondItem = (Context.TabView.TabItems as AvaloniaList<TabViewItem>)[1];
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .Where(x => x.Name == "CloseButton")
           .FirstOrDefault();

        Assert.False(button.IsVisible);

        secondItem.RaiseEvent(new PointerEventArgs(InputElement.PointerEnteredEvent, button, null, Context.Root, new Point(), 0, 
            new PointerPointProperties(), KeyModifiers.None));
        Assert.True(button.IsVisible);
        secondItem.RaiseEvent(new PointerEventArgs(InputElement.PointerExitedEvent, button, null, Context.Root, new Point(), 0, 
            new PointerPointProperties(), KeyModifiers.None));

        Context.TabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.Auto; //Auto is default state, same as Always
    }

    [Fact]
    public void TabViewItemIsClosableFalseHidesCloseButton()
    {
        var firstItem = (Context.TabView.TabItems as AvaloniaList<TabViewItem>)[0];

        var button = firstItem.GetVisualDescendants()
           .OfType<Button>()
           .Where(x => x.Name == "CloseButton")
           .FirstOrDefault();

        // Default state is for it to be visible
        Assert.NotNull(button);

        firstItem.IsClosable = false;
        // Even the selected tab should hide it
        Assert.False(button.IsVisible);

        // Make sure it returns
        firstItem.IsClosable = true;
        Assert.True(button.IsVisible);

        // Switch to the second item, just to double check unselected items
        var secondItem = (Context.TabView.TabItems as AvaloniaList<TabViewItem>)[1];
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .Where(x => x.Name == "CloseButton")
           .FirstOrDefault();

        secondItem.IsClosable = false;
        Assert.False(button.IsVisible);

        // Make sure it returns
        secondItem.IsClosable = true;
        Assert.True(button.IsVisible);
    }

    [Fact]
    public void TabViewAddButtonCommandWorks()
    {
        bool commandInvoked = false;
        void TabAdd(object parameter)
        {
            commandInvoked = true;

            Assert.Equal("Parameter", parameter);
        }
        var testCommand = new TestCommand(TabAdd);
        Context.TabView.AddTabButtonCommand = testCommand;
        Context.TabView.AddTabButtonCommandParameter = "Parameter";

        var button = Context.TabView.GetVisualDescendants()
            .OfType<Button>()
            .Where(x => x.Name == "AddButton")
            .FirstOrDefault();

        Assert.NotNull(button);

        Assert.True(button.IsEnabled);

        // Have to trigger pointer event to properly get click/command to work
        // But having trouble getting fake pointer events to pass the 
        // GetVisualsAt test Button does in PointerReleased, so will invoke via reflection
        var clickMethod = 
            button.GetType().GetMethod("OnClick", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        clickMethod?.Invoke(button, null);

        Assert.True(commandInvoked);


        testCommand.SetCanExecute(false);
        Assert.False(button.IsEffectivelyEnabled);

        Context.TabView.AddTabButtonCommandParameter = null;
        Context.TabView.AddTabButtonCommand = null;
    }

    [Fact]
    public void BindingItemsGeneratesTabItems()
    {
        var tv = Context.TabView;  
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

        for (int i = 0; i < items.Count; i++)
        {
            var container = tv.ContainerFromIndex(i);
            Assert.IsType<TabViewItem>(container);
        }

        Assert.Equal(0, tv.SelectedIndex);

        Context.ResetTabView();
    }
}

public class TestTabItem
{
    public string Header { get; set; }

    public string Content { get; set; }
}
