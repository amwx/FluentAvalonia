using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    public class FrameTestsContext : IDisposable
    {
        public FrameTestsContext()
        {
            _appDisposable = UnitTestApplication.Start();

            CreateFrame();
        }

        public Frame Frame { get; private set; }

        public TestRoot Root { get; private set; }

        public void Dispose()
        {
            _appDisposable.Dispose();
        }

        public void ResetFrame()
        {
            Frame = new Frame();

            Root.Child = Frame;

            Root.LayoutManager.ExecuteInitialLayoutPass();
            Root.LayoutManager.ExecuteLayoutPass();
        }

        private void CreateFrame()
        {
            Root = new TestRoot(new Size(1280, 720));
            Root.StylingParent = UnitTestApplication.Current;

            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/FrameStyles.axaml")));

            Frame = new Frame();

            Root.Child = Frame;

            Root.LayoutManager.ExecuteInitialLayoutPass();
            Root.LayoutManager.ExecuteLayoutPass();
        }

        private IDisposable _appDisposable;
    }

    public class FrameTests : IClassFixture<FrameTestsContext>
    {
        public FrameTests(FrameTestsContext ctx)
        {
            Context = ctx;
        }

        public FrameTestsContext Context { get; }

        [Fact]
        public void NavigateWorks()
        {
            Context.ResetFrame();
            Context.Frame.Navigate(typeof(TestPage1));

            Assert.IsType<TestPage1>(Context.Frame.Content);
        }

        [Fact]
        public void NavigatingIsCalledBeforeNavigated()
        {
            Context.ResetFrame();
            bool calledNavigating = false;
            bool calledNavigated = false;
            void OnNavigating(object sender, NavigatingCancelEventArgs args)
            {
                Assert.False(calledNavigated);
                calledNavigating = true;
            }

            void OnNavigated(object sender, NavigationEventArgs args)
            {
                Assert.True(calledNavigating);
                calledNavigated = true;
            }

            Context.Frame.Navigating += OnNavigating;
            Context.Frame.Navigated += OnNavigated;

            Context.Frame.Navigate(typeof(TestPage2));

            Assert.True(calledNavigating);
            Assert.True(calledNavigated);

            Context.Frame.Navigating -= OnNavigating;
            Context.Frame.Navigated -= OnNavigated;
        }

        [Fact]
        public void CancelingNavigatingPreventsNavigation()
        {
            Context.ResetFrame();
            Context.Frame.Navigate(typeof(TestPage1));

            bool calledNavigating = false;
            bool calledNavigated = false;
            void OnNavigating(object sender, NavigatingCancelEventArgs args)
            {
                calledNavigating = true;
                args.Cancel = true;
            }

            void OnNavigated(object sender, NavigationEventArgs args)
            {
                calledNavigated = true;
            }

            Context.Frame.Navigating += OnNavigating;
            Context.Frame.Navigated += OnNavigated;

            Context.Frame.Navigate(typeof(TestPage2));

            Assert.True(calledNavigating);
            Assert.False(calledNavigated);

            Assert.IsType<TestPage1>(Context.Frame.Content);

            Context.Frame.Navigating -= OnNavigating;
            Context.Frame.Navigated -= OnNavigated;
        }

        [Fact]
        public void NavigatingToInvalidTypeRaisesNavigationFailed()
        {
            Context.ResetFrame();
            bool failed = false;
            void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
            {
                failed = true;
            }

            Context.Frame.NavigationFailed += OnNavigationFailed;

            Context.Frame.Navigate(typeof(ThisPageShouldntLoad));

            Assert.True(failed);

            Context.Frame.NavigationFailed -= OnNavigationFailed;
        }

        [Fact]
        public void NavigationStackWorks()
        {
            Context.ResetFrame();

            Assert.True(Context.Frame.IsNavigationStackEnabled);

            Context.Frame.Navigate(typeof(TestPage1));
            Context.Frame.Navigate(typeof(TestPage2));
            Context.Frame.Navigate(typeof(TestPage3));

            Assert.Equal(2, Context.Frame.BackStackDepth);
            Assert.Empty(Context.Frame.ForwardStack);
            Assert.True(Context.Frame.CanGoBack);
            Assert.False(Context.Frame.CanGoForward);

            Context.Frame.GoBack();

            Assert.Equal(1, Context.Frame.BackStackDepth);
            Assert.True(Context.Frame.CanGoBack);
            Assert.True(Context.Frame.CanGoForward);

            Context.Frame.GoBack();
            Assert.Empty(Context.Frame.BackStack);
            Assert.False(Context.Frame.CanGoBack);
            Assert.True(Context.Frame.CanGoForward);

            Context.Frame.GoForward();

            Assert.Equal(1, Context.Frame.BackStackDepth);
            Assert.True(Context.Frame.CanGoBack);
            Assert.True(Context.Frame.CanGoForward);
        }

        [Fact]
        public void PagesAreCachedWhenNavigatingIfUsingNavigationStack()
        {
            Context.ResetFrame();

            Assert.True(Context.Frame.IsNavigationStackEnabled);

            Context.Frame.Navigate(typeof(TestPage1));
            Context.Frame.Navigate(typeof(TestPage2));
            Context.Frame.Navigate(typeof(TestPage3));

            Assert.Equal(2, Context.Frame.BackStackDepth);

            Assert.IsType<TestPage2>(Context.Frame.BackStack[1].Instance);
            Assert.IsType<TestPage1>(Context.Frame.BackStack[0].Instance);

            var prevPage = Context.Frame.BackStack[1].Instance;

            Context.Frame.GoBack();

            Assert.Equal(prevPage, Context.Frame.Content);
        }

        [Fact]
        public void DisablingNavigationStackPreventsPageCaching()
        {
            Context.ResetFrame();

            Context.Frame.IsNavigationStackEnabled = false;

            Context.Frame.Navigate(typeof(TestPage1));
            Context.Frame.Navigate(typeof(TestPage2));
            Context.Frame.Navigate(typeof(TestPage3));

            Assert.Empty(Context.Frame.BackStack);
            Assert.Equal(0, Context.Frame.BackStackDepth);
                    
            Context.Frame.GoBack();

            Assert.Empty(Context.Frame.ForwardStack);
        }
    }

    public class TestPage1 : UserControl { }

    public class TestPage2 : UserControl { }

    public class TestPage3 : UserControl { }

    public class ThisPageShouldntLoad  { }
}
