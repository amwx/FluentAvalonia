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
using Avalonia.Shared.PlatformSupport;
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
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/NavigationView/NavigationView.axaml")));

            // SplitView, ScrollViewer/ScrollBar, and Button styles are required too
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/SplitViewStyle.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollBarStyle.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollViewer.axaml")));
            UnitTestApplication.Current.Styles.Add(
               (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ButtonStyle.axaml")));


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
            using var app = UnitTestApplication.Start();
            
            bool opening = false;
            bool opened = false;
            bool closing = false;
            bool closed = false;

            var navView = Context.NavigationView;

            // Ensure the Pane is open to start, as that *should* be the default state
            // For some reason it's closed, but that's a thing for another day
            if (!navView.IsPaneOpen)
                navView.IsPaneOpen = true;

            navView.PaneOpening += (s, e) =>
            {
                Assert.False(opened);
                opening = true;
            };

            navView.PaneOpened += (s, e) =>
            {
                Assert.True(opening);
                opened = true;
            };

            navView.PaneClosing += (s, e) =>
            {
                Assert.False(closed);
                closing = true;
            };

            navView.PaneClosed += (s, e) =>
            {
                Assert.True(closing);
                closed = true;
            };

            navView.IsPaneOpen = false;

            Assert.True(closing);
            Assert.True(closed);


            navView.IsPaneOpen = true;
            Assert.True(opening);
            Assert.True(opened);            
        }

        [Fact]
        public void NavigationViewItemInvokedEventFiresAndSelectsItem()
        {
            using var app = UnitTestApplication.Start();
            var navView = Context.NavigationView;

            // Ensure the pane is open so Items are realized
            navView.IsPaneOpen = true;

            var firstItem = navView.MenuItems.ElementAt(0) as NavigationViewItem;

            bool eventFired = false;
            navView.ItemInvoked += (s, e) =>
            {
                eventFired = true;
                Assert.Equal(firstItem, e.InvokedItemContainer);
            };
                                   
            firstItem.RaiseEvent(new RoutedEventArgs(InputElement.TappedEvent));

            Assert.True(eventFired);
            Assert.True(firstItem.IsSelected);
        }

        [Fact]
        public void InvokingItemWithFalseSelectsOnInvokedDoesNotSelect()
        {
            using var app = UnitTestApplication.Start();
            var navView = Context.NavigationView;

            // Ensure the pane is open so Items are realized
            navView.IsPaneOpen = true;

            var firstItem = navView.MenuItems.ElementAt(0) as NavigationViewItem;
            firstItem.SelectsOnInvoked = false;

            bool eventFired = false;
            navView.ItemInvoked += (s, e) =>
            {
                eventFired = true;
                Assert.Equal(firstItem, e.InvokedItemContainer);
            };

            firstItem.RaiseEvent(new RoutedEventArgs(InputElement.TappedEvent));

            Assert.True(eventFired);
            Assert.False(firstItem.IsSelected);
        }

        
    }
}
