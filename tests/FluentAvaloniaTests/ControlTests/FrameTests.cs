using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

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

       // UnitTestApplication.Current.Styles.Add(
       //     (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/FrameStyles.axaml")));

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

    [Fact]
    public void NavigationPageEventsFire()
    {
        Context.ResetFrame();

        Context.Frame.Navigate(typeof(TestPage4), "parameter");

        var page4 = Context.Frame.Content as TestPage4;
        Assert.NotNull(page4);

        // NavTo should've fired
        Assert.True(page4.NavigatedToFired);
        // And the parameter should've been passed
        Assert.True(page4.ParameterPassed);


        // Now navigate to TestPage5

        Context.Frame.Navigate(typeof(TestPage5), "parameter2");
        // NavFrom should've fired on the old page
        Assert.True(page4.NavigatingFromFired);
        // Check param
        Assert.True(page4.ParameterPassed);
        // NavFrom should've fired on the old page
        Assert.True(page4.NavigatedFromFired);
        // Check param
        Assert.True(page4.ParameterPassed);

        var page5 = Context.Frame.Content as TestPage5;
        Assert.NotNull(page5);

        // NavTo should've fired
        Assert.True(page5.NavigatedToFired);
        // Check param
        Assert.True(page5.ParameterPassed);
    }

    [Fact]
    public void NavigationStateSavesInCorrectFormat()
    {
        Context.ResetFrame();

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3), "param");
        Context.Frame.Navigate(null);

        Context.Frame.GoBack();

        // NavigationState should be:
        // FluentAvaloniaTests.ControlTests.TestPage2|
        // 1
        // FluentAvaloniaTests.ControlTests.TestPage1|
        // 1
        // FluentAvaloniaTests.ControlTests.TestPage3|param
        // [Blank Line]

        var expected =
            $"{typeof(TestPage2).AssemblyQualifiedName}|" + Environment.NewLine +
            "1" + Environment.NewLine +
            $"{typeof(TestPage1).AssemblyQualifiedName}|" + Environment.NewLine +
            "1" + Environment.NewLine +
            $"{typeof(TestPage3).AssemblyQualifiedName}|param" + Environment.NewLine;

        var str = Context.Frame.GetNavigationState();

        Assert.Equal(expected, str);
    }

    [Fact]
    public void NavigationStateIsRestoredCorrectly()
    {
        Context.ResetFrame();

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3), "param");
        Context.Frame.Navigate(null);

        Context.Frame.GoBack();

        var str = Context.Frame.GetNavigationState();

        Context.ResetFrame();

        Context.Frame.SetNavigationState(str);

        Assert.Single(Context.Frame.BackStack);
        Assert.Single(Context.Frame.ForwardStack);

        // The actual controls aren't created yet, just the PageStackEntries, but 
        // verify they're there
        Assert.Equal(typeof(TestPage1), Context.Frame.BackStack[0].SourcePageType);
        Assert.Equal(typeof(TestPage3), Context.Frame.ForwardStack[0].SourcePageType);

        Assert.IsType<TestPage2>(Context.Frame.Content);

        Context.Frame.GoBack();

        // Going back should build the content now
        Assert.IsType<TestPage1>(Context.Frame.Content);
    }

    [Fact]
    public void NavigationStateIsRestoredCorrectlyWithSuppressNavigation()
    {
        Context.ResetFrame();

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3), "param");
        Context.Frame.Navigate(null);

        Context.Frame.GoBack();

        var str = Context.Frame.GetNavigationState();

        Context.ResetFrame();

        Context.Frame.SetNavigationState(str, true);

        Assert.Equal(2, Context.Frame.BackStack.Count);
        Assert.Single(Context.Frame.ForwardStack);

        // The actual controls aren't created yet, just the PageStackEntries, but 
        // verify they're there
        Assert.Equal(typeof(TestPage1), Context.Frame.BackStack[0].SourcePageType);

        // This is the current page, but we didn't want to navigate to it, so it's here
        Assert.Equal(typeof(TestPage2), Context.Frame.BackStack[1].SourcePageType);
        Assert.Equal(typeof(TestPage3), Context.Frame.ForwardStack[0].SourcePageType);

        Assert.Null(Context.Frame.Content);

        Context.Frame.GoBack();

        // Going back should load the current page
        Assert.IsType<TestPage2>(Context.Frame.Content);
    }
}

public class TestPage1 : UserControl { }

public class TestPage2 : UserControl { }

public class TestPage3 : UserControl { }

public class ThisPageShouldntLoad  { }

public class TestPage4 : UserControl
{
    public TestPage4()
    {
        AddHandler(Frame.NavigatingFromEvent, (s, e) =>
        {
            NavigatingFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);

        AddHandler(Frame.NavigatedFromEvent, (s, e) =>
        {
            NavigatedFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);

        AddHandler(Frame.NavigatedToEvent, (s, e) =>
        {
            NavigatedToFired = true;
            ParameterPassed = e.Parameter.Equals("parameter");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);
    }

    public bool NavigatingFromFired { get; set; }

    public bool NavigatedFromFired { get; set; }

    public bool NavigatedToFired { get; set; }

    public bool ParameterPassed { get; set; }
}

public class TestPage5 : UserControl
{
    public TestPage5()
    {
        AddHandler(Frame.NavigatedToEvent, (s, e) =>
        {
            NavigatedToFired = true;

            ParameterPassed = e.Parameter.Equals("parameter2");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);
    }

    public bool NavigatingFromFired { get; set; }

    public bool NavigatedFromFired { get; set; }

    public bool NavigatedToFired { get; set; }

    public bool ParameterPassed { get; set; }
}
