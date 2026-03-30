using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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

public class TabViewTests
{
    [AvaloniaFact]
    public void TabItemsChangedEventFires()
    {
        var (w, TabView) = GetTabView();
        bool eventFired = false;
        void ItemsChanged(TabView sender, EventArgs e)
        {
            eventFired = true;
        }

        TabView.TabItemsChanged += ItemsChanged;

        TabView.TabItems.Add(new TabViewItem());

        Assert.True(eventFired);

        TabView.TabItemsChanged -= ItemsChanged;
    }

    [AvaloniaFact]
    public void AddTabButtonFiresAddTabButtonClickEvent()
    {
        var (w, TabView) = GetTabView();
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
        var (w, TabView) = GetTabView();
        bool eventRaised = false;
        void TabClose(TabView sender, TabViewTabCloseRequestedEventArgs e)
        {
            eventRaised = true;
        }

        TabView.TabCloseRequested += TabClose;

        var firstItem = TabView.TabItems[0] as TabViewItem;

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
        var (w, TabView) = GetTabView();
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

    // Other TabViewWidthModes need to be tested manually - however
    // we can at least check that CompactTabWidth does what it supposed to
    [AvaloniaFact]
    public void CompactTabWidthModeHidesContentPresentersOfNonSelectedTabs()
    {
        // NOTE: Only the content presenter is hidden. WinUI does not hide the close button too
        // The only thing that should be visible is the Icon (if present)
        var (w, TabView) = GetTabView();
        TabView.TabWidthMode = TabViewWidthMode.Compact;
        TabView.SelectedIndex = 0;
        var items = TabView.TabItems;

        Assert.True(IsContentPresenterVisible(items[0] as TabViewItem));
        Assert.False(IsContentPresenterVisible(items[1] as TabViewItem));
        Assert.False(IsContentPresenterVisible(items[2] as TabViewItem));

        static bool IsContentPresenterVisible(TabViewItem item)
        {
            var pres = item.GetTemplateChildren()
                .FirstOrDefault(x => x is ContentPresenter);

            if (pres == null)
                Assert.False(true, "Unable to find ContentPresenter");

            return pres.IsVisible;
        }
    }

    [AvaloniaFact]
    public void SelectedIndexIsSetWhenItemsAreAddedBeforeInitialization()
    {
        // If items are attached before the first layout pass when everything is loaded,
        // the selected index should be implicitly set to 0 (the first item)
        // NOTE: It does not happen if items are set AFTER everything is loaded - which matches WinUI

        var (w, TabView) = GetTabView();
        Assert.Equal(0, TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void SettingSelectedIndexBeforeInitializationIsRespected()
    {
        var (w, TabView) = GetTabView(selIndex: 2);
        Assert.Equal(2, TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void TabContentIsSetWhenTabIsSelected()
    {
        var (w, TabView) = GetTabView();

        var presenter = TabView.GetTemplateChildren()
            .FirstOrDefault(x => x is ContentPresenter c && c.Name.Equals(FluentAvalonia.UI.Controls.TabView.s_tpTabContentPresenter)) as ContentPresenter;

        Assert.Equal((TabView.TabItems[0] as TabViewItem).Content, presenter.Content);

        TabView.SelectedIndex = 1;
        Assert.Equal((TabView.TabItems[1] as TabViewItem).Content, presenter.Content);
    }

    [AvaloniaFact]
    public void AddTabButtonRespectVisibleProperty()
    {
        var (w, TabView) = GetTabView();

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == TabView.s_tpAddButton);

        // NOTE: Use IsEffectivelyVisible b/c the Binding is on the Border that hosts the
        // button so IsVisible is not set
        Assert.Equal(TabView.IsAddTabButtonVisible, button.IsEffectivelyVisible);

        TabView.IsAddTabButtonVisible = false;
        Assert.Equal(TabView.IsAddTabButtonVisible, button.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void LeftClickingOnTabItemChangesSelctedItem()
    {
        var (w, TabView) = GetTabView();
        var item = TabView.TabItems[1] as TabViewItem;

        item.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(item, TabView.SelectedItem);
        Assert.Equal(1, TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void MiddleClickingOnTabItemRequestsCloseIfClosable()
    {
        bool raisedEvent = false;
        void RequestClose(object sender, TabViewTabCloseRequestedEventArgs args)
        {
            raisedEvent = true;
        }

        var (w, TabView) = GetTabView();
        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as TabViewItem;
        var item2 = TabView.TabItems[1] as TabViewItem;
        item.IsClosable = false;

        item.ClickControl(new Point(10, 10), MouseButton.Middle);
        Assert.False(raisedEvent);

        item2.ClickControl(new Point(10, 10), MouseButton.Middle);
        Assert.True(raisedEvent);
    }

    [AvaloniaFact]
    public void TabViewItemCloseButtonOverlayModeWorks()
    {
        var (w, TabView) = GetTabView();
        var firstItem = TabView.TabItems[0] as TabViewItem;

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
        var secondItem = TabView.TabItems[1] as TabViewItem;
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
        var (w, TabView) = GetTabView();
        var firstItem = TabView.TabItems[0] as TabViewItem;

        var vd = firstItem.GetVisualDescendants();
           var button = vd.OfType<Button>()
           .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        // Default state is for it to be visible
        Assert.NotNull(button);

        firstItem.IsClosable = false;
        var pc = firstItem.Classes;
        // Even the selected tab should hide it
        Assert.False(button.IsVisible);

        // Make sure it returns
        firstItem.IsClosable = true;
        Assert.True(button.IsVisible);

        // Switch to the second item, just to double check unselected items
        var secondItem = TabView.TabItems[1] as TabViewItem;
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        secondItem.IsClosable = false;
        Assert.False(button.IsVisible);

        // Make sure it returns
        secondItem.IsClosable = true;
        Assert.True(button.IsVisible);
    }

    [AvaloniaFact]
    public void TabViewAddButtonCommandWorks()
    {
        var (w, TabView) = GetTabView();
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
            .FirstOrDefault(x => x.Name == TabView.s_tpAddButton);

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
    public void ClickingAddTabButtonRaisesAddTabButtonClickEvent()
    {
        var (w, TabView) = GetTabView();
        bool eventRaised = false;
        void Handler(TabView tv, EventArgs args)
        {
            eventRaised = true;
        }
        TabView.AddTabButtonClick += Handler;

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == TabView.s_tpAddButton);
        button.ClickControl(new Point(10, 10), MouseButton.Left);
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void BindingItemsGeneratesTabItems()
    {
        var (w, TabView) = GetTabView(false);
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
        tv.TabItemsSource = items;

        Dispatcher.UIThread.RunJobs();

        for (int i = 0; i < items.Count; i++)
        {
            var container = tv.ContainerFromIndex(i);
            Assert.IsType<TabViewItem>(container);
        }
    }

    [AvaloniaFact]
    public void UsingTabItemsSourceSetsTabItemsToListViewItemCollection()
    {
        var (w, TabView) = GetTabView(false);
        var items = new List<TestTabItem>
        {
            new TestTabItem { Header = "This is tab 1", Content = "Tab1 Content" },
            new TestTabItem { Header = "This is tab 2", Content = "Tab2 Content" },
            new TestTabItem { Header = "This is tab 3", Content = "Tab2 Content" },
        };
        TabView.TabItemsSource = items;

        Assert.IsType<ItemCollection>(TabView.TabItems);
        Assert.Equal(items.Count, TabView.TabItems.Count);
    }

    [AvaloniaFact]
    public void SelectionChangeIsFiredWhenSelectedItemOrIndexChanges()
    {
        var (w, TabView) = GetTabView();

        SelectionChangedEventArgs args = null;
        void Handler(object sender, SelectionChangedEventArgs e)
        {
            args = e;
        }

        TabView.SelectionChanged += Handler;

        TabView.SelectedIndex = 1;

        Assert.NotNull(args);

        Assert.Single(args.AddedItems);
        Assert.Single(args.RemovedItems);

        Assert.Equal(TabView.TabItems[1], args.AddedItems[0]);
        Assert.Equal(TabView.TabItems[0], args.RemovedItems[0]);
    }

    [AvaloniaFact]
    public void TabCloseRequestedEventHasCorrectInfoWithTabItemsSet()
    {
        TabViewTabCloseRequestedEventArgs args = null;
        void RequestClose(object sender, TabViewTabCloseRequestedEventArgs e)
        {
            args = e;
        }

        var (w, TabView) = GetTabView();
        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as TabViewItem;

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.NotNull(args);
        Assert.Equal(item, args.Item);
        Assert.Equal(item, args.Tab);
    }

    [AvaloniaFact]
    public void TabCloseRequestedEventHasCorrectInfoWithTabItemsSourceSet()
    {
        TabViewTabCloseRequestedEventArgs args = null;
        void RequestClose(object sender, TabViewTabCloseRequestedEventArgs e)
        {
            args = e;
        }

        var (w, TabView) = GetTabView(false);
        var items = new List<TestTabItem>
        {
            new TestTabItem { Header = "This is tab 1", Content = "Tab1 Content" },
            new TestTabItem { Header = "This is tab 2", Content = "Tab2 Content" },
            new TestTabItem { Header = "This is tab 3", Content = "Tab2 Content" },
        };
        TabView.TabItemsSource = items;
        TabView.UpdateLayout();

        TabView.TabCloseRequested += RequestClose;
        var item = TabView.ContainerFromIndex(0) as TabViewItem;

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.NotNull(args);
        Assert.Equal(items[0], args.Item);
        Assert.Equal(item, args.Tab);
    }

    [AvaloniaFact]
    public void ClosingTabAttemptsToKeepSelection()
    {
        var (w, TabView) = GetTabView();

        void RequestClose(object sender, TabViewTabCloseRequestedEventArgs e)
        {
            // NOTE: Because TabItems is an IList, Remove throws an exception
            // Hoping this gets fixed, opened issue #21046
            TabView.TabItems.RemoveAt(TabView.TabItems.IndexOf(e.Tab));
        }
        
        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as TabViewItem;
        var item1 = TabView.TabItems[1];

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(0, TabView.SelectedIndex);
        Assert.Equal(item1, TabView.SelectedItem);
        w.Close();

        // Redo the test, but this time close the last item and it should set to the new last item
        (w, TabView) = GetTabView();
        TabView.SelectedIndex = 2;

        TabView.TabCloseRequested += RequestClose;
        item = TabView.TabItems[^1] as TabViewItem;
        item1 = TabView.TabItems[^2];

        vd = item.GetVisualDescendants();
        button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == TabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(TabView.TabItems.Count - 1, TabView.SelectedIndex);
        Assert.Equal(item1, TabView.SelectedItem);
        w.Close();
    }

    private (Window w, TabView tv) GetTabView(bool addTabs = true, int? selIndex = null)
    {
        var tv = new TabView();
        if (addTabs)
        {
            tv.TabItems.Add(new TabViewItem { Header = "Tab1", Content = "ContentTab1" });
            tv.TabItems.Add(new TabViewItem { Header = "Tab2", Content = "ContentTab2" });
            tv.TabItems.Add(new TabViewItem { Header = "Tab3", Content = "ContentTab3" });
        }

        if (selIndex.HasValue)
        {
            tv.SelectedIndex = selIndex.Value;
        }

        var w = new Window { Content = tv };
        w.Show();
        tv.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (w, tv);
    }
}

public class TestTabItem
{
    public string Header { get; set; }

    public string Content { get; set; }
}
