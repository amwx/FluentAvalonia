using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class NavigationViewTests 
{
    [AvaloniaFact]
    public void PaneEventOrderIsCorrect()
    {
        bool opening = false;
        bool opened = false;
        bool closing = false;
        bool closed = false;
        void Opening(object sender, EventArgs e)
        {
            Assert.False(opened);
            opening = true;
        }
        void Opened(object sender, EventArgs e)
        {
            Assert.True(opening);
            opened = true;
        }
        void Closing(object sender, EventArgs e)
        {
            Assert.False(closed);
            closing = true;
        }
        void Closed(object sender, EventArgs e)
        {
            Assert.True(closing);
            closed = true;
        }

        using var Context = GetContext();

        var navView = Context.NavigationView;

        // Ensure the Pane is open to start, as that *should* be the default state
        // For some reason it's closed, but that's a thing for another day
        if (!navView.IsPaneOpen)
            navView.IsPaneOpen = true;

        navView.PaneOpening += Opening;
        navView.PaneOpened += Opened;
        navView.PaneClosing += Closing;
        navView.PaneClosed += Closed;

        navView.IsPaneOpen = false;

        Assert.True(closing);
        Assert.True(closed);


        navView.IsPaneOpen = true;
        Assert.True(opening);
        Assert.True(opened);

        navView.PaneOpening -= Opening;
        navView.PaneOpened -= Opened;
        navView.PaneClosing -= Closing;
        navView.PaneClosed -= Closed;
    }

    [AvaloniaFact]
    public void NavigationViewItemInvokedEventFiresAndSelectsItem()
    {
        using var Context = GetContext();
        var navView = Context.NavigationView;
        // Ensure the pane is open so Items are realized
        navView.IsPaneOpen = true;

        var firstItem = navView.MenuItems.ElementAt(0) as FANavigationViewItem;

        bool eventFired = false;
        void ItemInvoked(object sender, FANavigationViewItemInvokedEventArgs e)
        {
            eventFired = true;
            Assert.Equal(firstItem, e.InvokedItemContainer);
        }

        navView.ItemInvoked += ItemInvoked;
        firstItem.RaiseEvent(new TappedEventArgs(InputElement.TappedEvent, null));

        Assert.True(eventFired);
        Assert.True(firstItem.IsSelected);

        navView.ItemInvoked -= ItemInvoked;
    }

    [AvaloniaFact]
    public void InvokingItemWithFalseSelectsOnInvokedDoesNotSelect()
    {
        using var Context = GetContext();
        var navView = Context.NavigationView;

        // Ensure the pane is open so Items are realized
        navView.IsPaneOpen = true;

        var firstItem = navView.MenuItems.ElementAt(0) as FANavigationViewItem;
        firstItem.IsSelected = false; // Previous test may set this, ensure it's false to start
        firstItem.SelectsOnInvoked = false;

        bool eventFired = false;
        void ItemInvoked(object sender, FANavigationViewItemInvokedEventArgs e)
        {
            eventFired = true;
            Assert.Equal(firstItem, e.InvokedItemContainer);
        }

        navView.ItemInvoked += ItemInvoked;

        firstItem.RaiseEvent(new TappedEventArgs(InputElement.TappedEvent, null));

        Assert.True(eventFired);
        Assert.False(firstItem.IsSelected);

        navView.ItemInvoked -= ItemInvoked;
    }
    
    private static TestContext GetContext() => new TestContext();

    class TestContext : IDisposable
    {
        public TestContext()
        {
            NavigationView = new FANavigationView
            {
                MenuItems =
                {
                    new FANavigationViewItem { Content = "MenuItem 1", IconSource = new FASymbolIconSource { Symbol = FASymbol.Cut }},
                    new FANavigationViewItem { Content = "MenuItem 2", IconSource = new FASymbolIconSource { Symbol = FASymbol.Copy }},
                }
            };

            Window = new Window
            {
                Content = NavigationView
            };

            Window.Show();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public Window Window { get; }

        public FANavigationView NavigationView { get; }

        public void Dispose()
        {
            Window.Close();
        }
    }
}
