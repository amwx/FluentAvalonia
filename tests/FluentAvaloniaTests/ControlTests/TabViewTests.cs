using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

// TODO: Tests to Add
// Check Keyboard Accelerators



public class TabViewTests
{
    [AvaloniaFact]
    public void TabItemsChangedEventFires()
    {
        var (w, TabView) = GetTabView();
        bool eventFired = false;
        void ItemsChanged(FATabView sender, EventArgs e)
        {
            eventFired = true;
        }

        TabView.TabItemsChanged += ItemsChanged;

        TabView.TabItems.Add(new FATabViewItem());

        Assert.True(eventFired);

        TabView.TabItemsChanged -= ItemsChanged;
    }

    [AvaloniaFact]
    public void AddTabButtonFiresAddTabButtonClickEvent()
    {
        var (w, TabView) = GetTabView();
        bool eventRaised = false;
        void TabAdd(FATabView sender, EventArgs e)
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
        void TabClose(FATabView sender, FATabViewTabCloseRequestedEventArgs e)
        {
            eventRaised = true;
        }

        TabView.TabCloseRequested += TabClose;

        var firstItem = TabView.TabItems[0] as FATabViewItem;

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
        if (TabView.TabItems is AvaloniaList<FATabViewItem> l)
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
        TabView.TabWidthMode = FATabViewWidthMode.Compact;
        TabView.SelectedIndex = 0;
        var items = TabView.TabItems;

        Assert.True(IsContentPresenterVisible(items[0] as FATabViewItem));
        Assert.False(IsContentPresenterVisible(items[1] as FATabViewItem));
        Assert.False(IsContentPresenterVisible(items[2] as FATabViewItem));

        static bool IsContentPresenterVisible(FATabViewItem item)
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
            .FirstOrDefault(x => x is ContentPresenter c && c.Name.Equals(FATabView.s_tpTabContentPresenter)) as ContentPresenter;

        Assert.Equal((TabView.TabItems[0] as FATabViewItem).Content, presenter.Content);

        TabView.SelectedIndex = 1;
        Assert.Equal((TabView.TabItems[1] as FATabViewItem).Content, presenter.Content);
    }

    [AvaloniaFact]
    public void AddTabButtonRespectVisibleProperty()
    {
        var (w, TabView) = GetTabView();

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabView.s_tpAddButton);

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
        var item = TabView.TabItems[1] as FATabViewItem;

        item.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(item, TabView.SelectedItem);
        Assert.Equal(1, TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void MiddleClickingOnTabItemRequestsCloseIfClosable()
    {
        bool raisedEvent = false;
        void RequestClose(object sender, FATabViewTabCloseRequestedEventArgs args)
        {
            raisedEvent = true;
        }

        var (w, TabView) = GetTabView();
        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as FATabViewItem;
        var item2 = TabView.TabItems[1] as FATabViewItem;
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
        var firstItem = TabView.TabItems[0] as FATabViewItem;

        var button = firstItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        // Default state is for it to be visible
        Assert.NotNull(button);
        Assert.True(button.IsVisible);

        // Currently using the first item, which is selected by default...this should still display its
        // close button even if not pointer over
        TabView.CloseButtonOverlayMode = FATabViewCloseButtonOverlayMode.OnPointerOver;
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsVisible);

        // Switch to the second item, which is unselected and should hide its close button by default, if in
        // PointerOver mode
        var secondItem = TabView.TabItems[1] as FATabViewItem;
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == "CloseButton");

        Assert.False(button.IsVisible);

        secondItem.MoveMouseToControl(new Point(10, 10));
        Assert.True(button.IsVisible);
        secondItem.MoveMouseToControl(new Point(-100, -100));
        Assert.False(button.IsVisible);

        TabView.CloseButtonOverlayMode = FATabViewCloseButtonOverlayMode.Auto; //Auto is default state, same as Always
    }

    [AvaloniaFact]
    public void TabViewItemIsClosableFalseHidesCloseButton()
    {
        var (w, TabView) = GetTabView();
        var firstItem = TabView.TabItems[0] as FATabViewItem;

        var vd = firstItem.GetVisualDescendants();
        var button = vd.OfType<Button>()
        .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

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
        var secondItem = TabView.TabItems[1] as FATabViewItem;
        button = secondItem.GetVisualDescendants()
           .OfType<Button>()
           .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

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
            .FirstOrDefault(x => x.Name == FATabView.s_tpAddButton);

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
        void Handler(FATabView tv, EventArgs args)
        {
            eventRaised = true;
        }
        TabView.AddTabButtonClick += Handler;

        var button = TabView.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabView.s_tpAddButton);
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
            return new FATabViewItem
            {
                [!FATabViewItem.HeaderProperty] = new Binding("Header"),
                [!FATabViewItem.ContentProperty] = new Binding("Content")
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
            Assert.IsType<FATabViewItem>(container);
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
        FATabViewTabCloseRequestedEventArgs args = null;
        void RequestClose(object sender, FATabViewTabCloseRequestedEventArgs e)
        {
            args = e;
        }

        var (w, TabView) = GetTabView();
        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as FATabViewItem;

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.NotNull(args);
        Assert.Equal(item, args.Item);
        Assert.Equal(item, args.Tab);
    }

    [AvaloniaFact(Skip = "TODO")]
    public void TabCloseRequestedEventHasCorrectInfoWithTabItemsSourceSet()
    {
        FATabViewTabCloseRequestedEventArgs args = null;
        void RequestClose(object sender, FATabViewTabCloseRequestedEventArgs e)
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
        var item = TabView.ContainerFromIndex(0) as FATabViewItem;

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.NotNull(args);
        Assert.Equal(items[0], args.Item);
        Assert.Equal(item, args.Tab);
    }

    [AvaloniaFact]
    public void ClosingTabAttemptsToKeepSelection()
    {
        var (w, TabView) = GetTabView();

        void RequestClose(object sender, FATabViewTabCloseRequestedEventArgs e)
        {
            // NOTE: Because TabItems is an IList, Remove throws an exception
            // Hoping this gets fixed, opened issue #21046
            TabView.TabItems.RemoveAt(TabView.TabItems.IndexOf(e.Tab));
        }

        TabView.TabCloseRequested += RequestClose;
        var item = TabView.TabItems[0] as FATabViewItem;
        var item1 = TabView.TabItems[1];

        var vd = item.GetVisualDescendants();
        var button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(0, TabView.SelectedIndex);
        Assert.Equal(item1, TabView.SelectedItem);
        w.Close();

        // Redo the test, but this time close the last item and it should set to the new last item
        (w, TabView) = GetTabView();
        TabView.SelectedIndex = 2;

        TabView.TabCloseRequested += RequestClose;
        item = TabView.TabItems[^1] as FATabViewItem;
        item1 = TabView.TabItems[^2];

        vd = item.GetVisualDescendants();
        button = vd.OfType<Button>()
            .FirstOrDefault(x => x.Name == FATabViewItem.s_tpCloseButton);

        button.ClickControl(new Point(10, 10), MouseButton.Left);

        Assert.Equal(TabView.TabItems.Count - 1, TabView.SelectedIndex);
        Assert.Equal(item1, TabView.SelectedItem);
        w.Close();
    }

    [AvaloniaFact]
    public void TabViewDefaultsToTopMode()
    {
        var (w, TabView) = GetTabView();

        Assert.True(ClassesContains(TabView.Classes, FATabView.s_pcTop));

        Assert.True(ClassesContains(TabView.ListView.Classes, FATabView.s_pcTop));
        Assert.True(ClassesContains(TabView.ListView.Scroller.Classes, FATabView.s_pcTop));

        foreach (var item in TabView.TabItems)
            Assert.True(ClassesContains((item as FATabViewItem).Classes, FATabView.s_pcTop));
    }

    [AvaloniaFact]
    public void ChangingTabStripLocationUpdatesAllPseudoclasses()
    {
        var (w, TabView) = GetTabView();

        // LEFT TEST
        TabView.TabStripLocation = FATabViewTabStripLocation.Left;
        Assert.True(ClassesContains(TabView.Classes, FATabView.s_pcLeft));

        Assert.True(ClassesContains(TabView.ListView.Classes, FATabView.s_pcLeft));
        Assert.True(ClassesContains(TabView.ListView.Scroller.Classes, FATabView.s_pcLeft));

        foreach (var item in TabView.TabItems)
            Assert.True(ClassesContains((item as FATabViewItem).Classes, FATabView.s_pcLeft));

        // RIGHT TEST
        TabView.TabStripLocation = FATabViewTabStripLocation.Right;
        Assert.True(ClassesContains(TabView.Classes, FATabView.s_pcRight));

        Assert.True(ClassesContains(TabView.ListView.Classes, FATabView.s_pcRight));
        Assert.True(ClassesContains(TabView.ListView.Scroller.Classes, FATabView.s_pcRight));

        foreach (var item in TabView.TabItems)
            Assert.True(ClassesContains((item as FATabViewItem).Classes, FATabView.s_pcRight));

        // BOTTOM TEST
        TabView.TabStripLocation = FATabViewTabStripLocation.Bottom;
        Assert.True(ClassesContains(TabView.Classes, FATabView.s_pcBottom));

        Assert.True(ClassesContains(TabView.ListView.Classes, FATabView.s_pcBottom));
        Assert.True(ClassesContains(TabView.ListView.Scroller.Classes, FATabView.s_pcBottom));

        foreach (var item in TabView.TabItems)
            Assert.True(ClassesContains((item as FATabViewItem).Classes, FATabView.s_pcBottom));

        // BACK TO TOP TEST
        TabView.TabStripLocation = FATabViewTabStripLocation.Top;
        Assert.True(ClassesContains(TabView.Classes, FATabView.s_pcTop));

        Assert.True(ClassesContains(TabView.ListView.Classes, FATabView.s_pcTop));
        Assert.True(ClassesContains(TabView.ListView.Scroller.Classes, FATabView.s_pcTop));

        foreach (var item in TabView.TabItems)
            Assert.True(ClassesContains((item as FATabViewItem).Classes, FATabView.s_pcTop));        
    }

    [AvaloniaFact]
    public void AddingNewItemsSetsStripLocationOnTab()
    {
        // TODO: Turn this back on when we can do this
        //var (w, TabView) = GetTabView();
        //TabView.TabStripLocation = TabViewTabStripLocation.Left;

        //var newItem = new TabViewItem();
        //TabView.TabItems.Add(newItem);
        //TabView.UpdateLayout();
        //Assert.True(ClassesContains(newItem.Classes, TabView.s_pcLeft));
        //w.Close();

        // NOW CHECK WITH TABITEMSSOURCE
        var (w, TabView) = GetTabView(false);
        TabView.TabStripLocation = FATabViewTabStripLocation.Left;
        var items = new AvaloniaList<TestTabItem>
        {
            new TestTabItem { Header = "This is tab 1", Content = "Tab1 Content" },
            new TestTabItem { Header = "This is tab 2", Content = "Tab2 Content" },
            new TestTabItem { Header = "This is tab 3", Content = "Tab2 Content" },
        };
        TabView.TabItemsSource = items;
        TabView.UpdateLayout();

        items.Add(new TestTabItem());
        TabView.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var panel = TabView.ListView.ItemsPanelRoot;
        var item = TabView.ContainerFromIndex(3);
        Assert.True(ClassesContains(item.Classes, FATabView.s_pcLeft));
    }

    [AvaloniaFact]
    public void ChangingTabStripLocationPreservesSelection()
    {
        var (w, TabView) = GetTabViewWithItemsSource();
        TabView.SelectedIndex = 1;

        TabView.TabStripLocation = FATabViewTabStripLocation.Left;
        TabView.UpdateLayout();
        Assert.Equal(1, TabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void AddingTabsWhileVirtualizedInitializesCorrectly()
    {
        var (w, TabView) = GetTabView(false);
        var l = new AvaloniaList<string>();
        for (int i = 0; i < 100; i++)
        {
            l.Add($"New Tab {i + 1}");
        }
        TabView.TabItemsSource = l;
        TabView.UpdateLayout();
        TabView.SelectedIndex = 0;
        TabView.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        // Baseline check: First item is selected, it should have the :noborder class
        // All other items should not have it
        var item0 = TabView.ContainerFromIndex(0);
        Assert.True(ClassesContains(item0.Classes, FASharedPseudoclasses.s_pcNoBorder));

        int idx = 1;        
        while (idx < l.Count)
        {
            var item = TabView.ContainerFromIndex(idx);
            if (item == null)
                break; // End of realized containers

            Assert.False(ClassesContains(item.Classes, FASharedPseudoclasses.s_pcNoBorder));
            idx++;
        }

        // Now scroll and recheck
        var scroller = TabView.ListView.Scroller;
        var scrollableWidth = scroller.Extent.Width - scroller.Viewport.Width;
        scroller.Offset = new Vector(scrollableWidth * 0.5, 0);
        TabView.UpdateLayout();

        // Recheck - first item is still selected so all other containers should
        // not have the :noborder class
        // Skip Item0 b/c its the "focused" item in the Panel so it will always return a container in ContainerFromIndex
        idx = 1; 
        bool foundFirstRealized = false;
        while (idx < l.Count)
        {
            var cont = TabView.ContainerFromIndex(idx);
            if (!foundFirstRealized && cont == null)
            {
                idx++;
                continue;
            }

            foundFirstRealized = true;
            if (cont == null)
                break;

            Assert.False(ClassesContains(cont.Classes, FASharedPseudoclasses.s_pcNoBorder), $"Failed on container {idx}");
            Assert.False(ClassesContains(cont.Classes, FASharedPseudoclasses.s_pcBorderLeft), $"Failed on container {idx}");
            Assert.False(ClassesContains(cont.Classes, FASharedPseudoclasses.s_pcBorderRight), $"Failed on container {idx}");
            idx++;
        }

        // Now go back and ensure the first 2 containers (Sel + Sel+1) have their state set correctly
        scroller.Offset = new Vector(0, 0);
        TabView.UpdateLayout();

        // Should be :noborder only (selected)
        item0 = TabView.ContainerFromIndex(0);
        Assert.True(ClassesContains(item0.Classes, FASharedPseudoclasses.s_pcNoBorder));
        Assert.False(ClassesContains(item0.Classes, FASharedPseudoclasses.s_pcBorderLeft));
        Assert.False(ClassesContains(item0.Classes, FASharedPseudoclasses.s_pcBorderRight));
        
        // Should be :borderright (selected + 1)
        var item1 = TabView.ContainerFromIndex(1);
        Assert.False(ClassesContains(item1.Classes, FASharedPseudoclasses.s_pcNoBorder));
        Assert.False(ClassesContains(item1.Classes, FASharedPseudoclasses.s_pcBorderLeft));
        Assert.True(ClassesContains(item1.Classes, FASharedPseudoclasses.s_pcBorderRight));
    }

    [AvaloniaFact(Skip = "TODO")]
    public void TabWidthsAreEqualInEqualMode()
    {
        var (w, TabView) = GetTabView(false);
        w.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var l = new AvaloniaList<string>();
        for (int i = 0; i < 100; i++)
        {
            l.Add($"New Tab {i + 1}");
        }
        TabView.TabItemsSource = l;
        TabView.UpdateLayout();
        TabView.SelectedIndex = 0;
        TabView.UpdateLayout();

        var width0 = TabView.ContainerFromIndex(0).Bounds.Width;

        int idx = 1;
        while (idx < l.Count)
        {
            var cont = TabView.ContainerFromIndex(idx);
            if (cont == null)
                break;

            Assert.True(width0 == cont.Bounds.Width, $"Failed on container {idx}, {cont.Bounds.Width} expecting {width0}");
            idx++;
        }
    }

    [AvaloniaFact]
    public void DragItemsStartingDoesNotFireWithReorder()
    {
        var (w, TabView) = GetTabViewWithItemsSource();
        TabView.CanDragTabs = false;
        TabView.CanReorderTabs = true;
        DragDrop.SetAllowDrop(w, true); // Ensure the window is a drop target so we get events

        Dispatcher.UIThread.RunJobs();

        bool fired = false;
        TabView.TabDragStarting += (s, e) =>
        {
            fired = true;
        };

        var tabItem = TabView.ContainerFromIndex(0);

        tabItem.MouseDownControl(new Point(10, 10), MouseButton.Left);
        tabItem.MoveMouseToControl(new Point(100, 10));
        tabItem.MouseUpControl(new Point(110, 0), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.False(fired);
    }

    [AvaloniaFact]
    public void DragItemsStartingFiresWhenCanDragTabsIsTrue()
    {
        var (w, TabView) = GetTabViewWithItemsSource();
        TabView.CanDragTabs = true;
        TabView.CanReorderTabs = true;

        Dispatcher.UIThread.RunJobs();

        FATabViewTabDragStartingEventArgs args = null;
        TabView.TabDragStarting += (s, e) =>
        {
            args = e;
        };

        var tabItem = TabView.ContainerFromIndex(0);

        tabItem.MouseDownControl(new Point(10, 10), MouseButton.Left);
        tabItem.MoveMouseToControl(new Point(100, 10));
        tabItem.MouseUpControl(new Point(110, 0), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(args);
        Assert.Equal(tabItem, args.Tab);
        Assert.Equal(TabView.TabItemsSource.ElementAt(0), args.Item);
    }

    [AvaloniaFact]
    public void DragItemsCompletedFiresWhenDnDIsFinished()
    {
        var (w, TabView) = GetTabViewWithItemsSource();
        TabView.CanDragTabs = true;
        TabView.CanReorderTabs = true;

        Dispatcher.UIThread.RunJobs();

        FATabViewTabDragCompletedEventArgs args = null;
        TabView.TabDragCompleted += (s, e) =>
        {
            args = e;
        };

        var tabItem = TabView.ContainerFromIndex(0);

        tabItem.MouseDownControl(new Point(10, 10), MouseButton.Left);
        tabItem.MoveMouseToControl(new Point(100, 10));
        tabItem.MouseUpControl(new Point(110, 0), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(args);
        Assert.Equal(tabItem, args.Tab);
        Assert.Equal(TabView.TabItemsSource.ElementAt(0), args.Item);
    }

    [AvaloniaFact]
    public void TabContentSwitchesCorrectlyWhenClosingActiveTab()
    {
        // Fix for #714
        var (w, TabView) = GetTabViewWithItemsSource();
        var src = TabView.TabItemsSource as AvaloniaList<TestTabItem>;
        
        Dispatcher.UIThread.RunJobs();

        bool itemRemoved = false;
        TabView.TabCloseRequested += (s, e) =>
        {
            itemRemoved = src.Remove(e.Item as TestTabItem);
        };

        var tab = TabView.ContainerFromIndex(TabView.SelectedIndex) as FATabViewItem;
        tab.RequestClose();
        Dispatcher.UIThread.RunJobs();
        Assert.True(itemRemoved);

        var pres = TabView.TabContentPresenter;
        Assert.Equal(src[TabView.SelectedIndex], pres.Content);
    }



    private (Window w, FATabView tv) GetTabView(bool addTabs = true, int? selIndex = null)
    {
        var tv = new FATabView();
        if (addTabs)
        {
            tv.TabItems.Add(new FATabViewItem { Header = "Tab1", Content = "ContentTab1" });
            tv.TabItems.Add(new FATabViewItem { Header = "Tab2", Content = "ContentTab2" });
            tv.TabItems.Add(new FATabViewItem { Header = "Tab3", Content = "ContentTab3" });
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

    private (Window w, FATabView tv) GetTabViewWithItemsSource(bool addTabs = true, int? selIndex = null)
    {
        var tv = new FATabView();
        if (addTabs)
        {
            var l = new AvaloniaList<TestTabItem>
            {
                new TestTabItem { Header = "This is tab 1", Content = "Tab1 Content" },
                new TestTabItem { Header = "This is tab 2", Content = "Tab2 Content" },
                new TestTabItem { Header = "This is tab 3", Content = "Tab2 Content" },
            };
            tv.TabItemsSource = l;
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

    private static bool ClassesContains(Classes classes, string c) =>
            classes.Contains(c);
}

public class TestTabItem
{
    public string Header { get; set; }

    public string Content { get; set; }
}
