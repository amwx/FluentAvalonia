using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;

namespace FluentAvaloniaSamples.Pages;

public partial class TabViewWindowingSample : AppWindow
{
    public TabViewWindowingSample()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        TabView = this.FindControl<TabView>("TabView");

        TabView.TabItemsChanged += TabView_TabItemsChanged;
    }

    public TabView TabView { get; }

    public static readonly string DataIdentifier = "MyTabItem";

    public static void LaunchRoot()
    {
        var tvws = new TabViewWindowingSample();
        // In order for Drag/Drop/Reordering to work, be sure to use an IList with
        // INotifyCollectionChanged, otherwise it may not work as expected
        tvws.TabView.TabItems = new AvaloniaList<TabViewItem>
        {
            new TabViewItem
            {
                Header = "TabItem 1",
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                Content = new TabViewWindowSampleContent("This is TabPage 1")
            },
            new TabViewItem
            {
                Header = "TabItem 2",
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                Content = new TabViewWindowSampleContent("This is TabPage 2")
            },
            new TabViewItem
            {
                Header = "TabItem 3",
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                Content = new TabViewWindowSampleContent("This is TabPage 3")
            },
        };

        tvws.Show();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (TitleBar != null)
        {
            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            var dragRegion = this.FindControl<Panel>("CustomDragRegion");
            dragRegion.MinWidth = FlowDirection == Avalonia.Media.FlowDirection.LeftToRight ?
                TitleBar.RightInset : TitleBar.LeftInset;
        }
    }

    private void TabView_TabItemsChanged(TabView sender, NotifyCollectionChangedEventArgs args)
    {
        // If TabItem count hits zero - close the window
        // Note that this event ONLY fires based on a INCC change action and not when changing the
        // items source. i.e., if you set the source to null, you won't get this event and you'll have
        // to then manually close the window (if that is applicable)
        // It also won't fire if the TabView is in dragging (an item from its collection is the current drag source)
        if (sender.TabItems.Count() == 0)
        {
            Close();
        }
    }

    private void AddTabButtonClick(TabView sender, EventArgs args)
    {
        (sender.TabItems as IList).Add(
            new TabViewItem
            {
                Header = "New Item",
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                Content = new TabViewWindowSampleContent("New item content")
            });
    }

    private void TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        (sender.TabItems as IList).Remove(args.Tab);
    }

    private void TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
    {
        // Set the data payload to the drag args
        args.Data.SetData(DataIdentifier, args.Tab);

        // Indicate we can move
        args.Data.RequestedOperation = DragDropEffects.Move;
    }

    private void TabStripDrop(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataIdentifier) && e.Data.Get(DataIdentifier) is TabViewItem tvi)
        {
            var destinationTabView = sender as TabView;

            // While the TabView's internal ListView handles placing an insertion point gap, it 
            // doesn't actually hold that position upon drop - meaning you now must calculate
            // the approximate position of where to insert the tab
            int index = -1;

            for (int i = 0; i < destinationTabView.TabItems.Count(); i++)
            {
                var item = destinationTabView.ContainerFromIndex(i) as TabViewItem;

                if (e.GetPosition(item).X - item.Bounds.Width < 0)
                {
                    index = i;
                    break;
                }
            }

            // Now remove the item from the source TabView
            var srcTabView = tvi.FindAncestorOfType<TabView>();
            var srcIndex = srcTabView.IndexFromContainer(tvi);
            (srcTabView.TabItems as IList).RemoveAt(srcIndex);

            // Now add it to the new TabView
            if (index < 0)
            {
                (destinationTabView.TabItems as IList).Add(tvi);
            }
            else if (index < destinationTabView.TabItems.Count())
            {
                (destinationTabView.TabItems as IList).Insert(index, tvi);
            }

            destinationTabView.SelectedItem = tvi;
            e.Handled = true;

            // Remember, TabItemsChanged won't fire during DragDrop so we need to check
            // here if we should close the window if TabItems.Count() == 0
            if (srcTabView.TabItems.Count() == 0)
            {
                var wnd = srcTabView.FindAncestorOfType<AppWindow>();
                wnd.Close();
            }
        }
    }

    private void TabStripDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataIdentifier))
        {
            // For dragover, use the standard DragEffects property
            e.DragEffects = DragDropEffects.Move;
        }
    }

    private void TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
    {
        // In this case, the tab was dropped outside of any tabstrip, let's move it to
        // a new window
        var s = new TabViewWindowingSample();

        // TabItems is by default initialized to an AvaloniaList<object>, so we can just
        // cast to IList and add
        // Be sure to remove the tab item from it's old TabView FIRST or else you'll get the
        // annoying "Item already has a Visual parent error"
        if (s.TabView.TabItems is IList l)
        {
            // If you're binding, args also as 'Item' where you can retrieve the data item instead
            (sender.TabItems as IList).Remove(args.Tab);

            // Preserving tab content state is easiest if you aren't binding. If you are, you will
            // need to manage preserving the state of the tabcontent across the different TabViews
            l.Add(args.Tab);
        }

        s.Show();

        // TabItemsChanged will fire here and will check if the window is closed, only because it
        // is raised after drag/drop completes, so we don't have to do that here
    }
}
