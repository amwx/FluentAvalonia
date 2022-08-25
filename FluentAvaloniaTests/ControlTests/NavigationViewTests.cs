using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    /// <summary>
    /// NavView & styles are expensive to load/create, so share this NavView instance
    /// among all tests to ease running them
    /// </summary>
    public class NavViewTestContext : IDisposable
    {
        public NavViewTestContext()
        {
            _appDisposable = UnitTestApplication.Start();

            NavigationView = GetNavView();
        }

        public NavigationView NavigationView { get; }

        public void Dispose()
        {
            _appDisposable.Dispose();
        }

        private NavigationView GetNavView()
        {
            var tr = new TestRoot(new Size(1280, 720));
            tr.StylingParent = UnitTestApplication.Current;

            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/NavigationView/NavigationViewStyles.axaml")));

            // SplitView, ScrollViewer/ScrollBar, and Button styles are required too
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/SplitViewStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollBarStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollViewerStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
               (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ButtonStyles.axaml")));


            var navView = new NavigationView()
            {
                MenuItems = new List<NavigationViewItemBase>()
                {
                    new NavigationViewItem { Content = "MenuItem 1", Icon = new SymbolIcon { Symbol = Symbol.Cut }},
                    new NavigationViewItem { Content = "MenuItem 2", Icon = new SymbolIcon { Symbol = Symbol.Copy }},
                }
            };

            tr.Child = navView;

            tr.LayoutManager.ExecuteInitialLayoutPass();
            tr.LayoutManager.ExecuteLayoutPass();
            return navView;
        }

        private IDisposable _appDisposable;
    }

    public class NavigationViewTests : IClassFixture<NavViewTestContext>
    {
        public NavigationViewTests(NavViewTestContext ctx)
        {
            Context = ctx;
        }

        public NavViewTestContext Context { get; }

        [Fact]
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

        [Fact]
        public void NavigationViewItemInvokedEventFiresAndSelectsItem()
        {
            var navView = Context.NavigationView;
            // Ensure the pane is open so Items are realized
            navView.IsPaneOpen = true;

            var firstItem = navView.MenuItems.ElementAt(0) as NavigationViewItem;

            bool eventFired = false;
            void ItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
            {
                eventFired = true;
                Assert.Equal(firstItem, e.InvokedItemContainer);
            }

            navView.ItemInvoked += ItemInvoked;
            firstItem.RaiseEvent(new RoutedEventArgs(InputElement.TappedEvent));

            Assert.True(eventFired);
            Assert.True(firstItem.IsSelected);

            navView.ItemInvoked -= ItemInvoked;
        }

        [Fact]
        public void InvokingItemWithFalseSelectsOnInvokedDoesNotSelect()
        {
            var navView = Context.NavigationView;

            // Ensure the pane is open so Items are realized
            navView.IsPaneOpen = true;

            var firstItem = navView.MenuItems.ElementAt(0) as NavigationViewItem;
            firstItem.IsSelected = false; // Previous test may set this, ensure it's false to start
            firstItem.SelectsOnInvoked = false;

            bool eventFired = false;
            void ItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
            {
                eventFired = true;
                Assert.Equal(firstItem, e.InvokedItemContainer);
            }

            navView.ItemInvoked += ItemInvoked;

            firstItem.RaiseEvent(new RoutedEventArgs(InputElement.TappedEvent));

            Assert.True(eventFired);
            Assert.False(firstItem.IsSelected);

            navView.ItemInvoked -= ItemInvoked;
        }

        
    }
}
