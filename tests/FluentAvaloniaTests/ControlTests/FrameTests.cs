using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;


public class FrameTests 
{
    [AvaloniaFact]
    public void NavigateWorks()
    {
        var Context = GetTestContext();
        Context.Frame.Navigate(typeof(TestPage1));

        Assert.IsType<TestPage1>(Context.Frame.Content);
    }

    [AvaloniaFact]
    public void NavigatingIsCalledBeforeNavigated()
    {
        var Context = GetTestContext();
        bool calledNavigating = false;
        bool calledNavigated = false;
        void OnNavigating(object sender, FANavigatingCancelEventArgs args)
        {
            Assert.False(calledNavigated);
            calledNavigating = true;
        }

        void OnNavigated(object sender, FANavigationEventArgs args)
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

    [AvaloniaFact]
    public void CancelingNavigatingPreventsNavigation()
    {
        var Context = GetTestContext();
        Context.Frame.Navigate(typeof(TestPage1));

        bool calledNavigating = false;
        bool calledNavigated = false;
        void OnNavigating(object sender, FANavigatingCancelEventArgs args)
        {
            calledNavigating = true;
            args.Cancel = true;
        }

        void OnNavigated(object sender, FANavigationEventArgs args)
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

    [AvaloniaFact]
    public void NavigatingToInvalidTypeRaisesNavigationFailed()
    {
        var Context = GetTestContext();
        bool failed = false;
        void OnNavigationFailed(object sender, FANavigationFailedEventArgs e)
        {
            failed = true;
        }

        Context.Frame.NavigationFailed += OnNavigationFailed;

        Context.Frame.Navigate(typeof(ThisPageShouldntLoad));

        Assert.True(failed);

        Context.Frame.NavigationFailed -= OnNavigationFailed;
    }

    [AvaloniaFact]
    public void NavigationStackWorks()
    {
        var Context = GetTestContext();

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

    [AvaloniaFact]
    public void PagesAreCachedWhenNavigatingIfUsingNavigationStack()
    {
        var Context = GetTestContext();

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

    [AvaloniaFact]
    public void DisablingNavigationStackPreventsPageCaching()
    {
        var Context = GetTestContext();

        Context.Frame.IsNavigationStackEnabled = false;

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3));

        Assert.Empty(Context.Frame.BackStack);
        Assert.Equal(0, Context.Frame.BackStackDepth);

        Context.Frame.GoBack();

        Assert.Empty(Context.Frame.ForwardStack);
    }

    [AvaloniaFact]
    public void NavigationPageEventsFire()
    {
        var Context = GetTestContext();

        Context.Frame.Navigate(typeof(TestPage4), "parameter");
        Dispatcher.UIThread.RunJobs();

        var page4 = Context.Frame.Content as TestPage4;
        Assert.NotNull(page4);

        // NavTo should've fired
        Assert.True(page4.NavigatedToFired);
        // And the parameter should've been passed
        Assert.True(page4.ParameterPassed);


        // Now navigate to TestPage5

        Context.Frame.Navigate(typeof(TestPage5), "parameter2");
        Dispatcher.UIThread.RunJobs();

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

    [AvaloniaFact]
    public void NavigationStateSavesInCorrectFormat()
    {
        var Context = GetTestContext();

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

    [AvaloniaFact]
    public void NavigationStateIsRestoredCorrectly()
    {
        var Context = GetTestContext();

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3), "param");
        Context.Frame.Navigate(null);

        Context.Frame.GoBack();

        var str = Context.Frame.GetNavigationState();
        Context.Dispose();

        Context = GetTestContext();

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

    [AvaloniaFact]
    public void NavigationStateIsRestoredCorrectlyWithSuppressNavigation()
    {
        var Context = GetTestContext();

        Context.Frame.Navigate(typeof(TestPage1));
        Context.Frame.Navigate(typeof(TestPage2));
        Context.Frame.Navigate(typeof(TestPage3), "param");
        Context.Frame.Navigate(null);

        Context.Frame.GoBack();

        var str = Context.Frame.GetNavigationState();

        Context.Dispose();
        Context = GetTestContext();

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

    private static TestContext GetTestContext() => new TestContext();

    class TestContext : IDisposable
    {
        public TestContext()
        {
            Frame = new FAFrame();
            Window = new Window
            {
                Content = Frame
            };

            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public Window Window { get; }

        public FAFrame Frame { get; }

        public void Dispose()
        {
            Window.Close();
        }
    }
}

public class TestPage1 : UserControl { }

public class TestPage2 : UserControl { }

public class TestPage3 : UserControl { }

public class ThisPageShouldntLoad { }

public class TestPage4 : UserControl
{
    public TestPage4()
    {
        AddHandler(FAFrame.NavigatingFromEvent, (s, e) =>
        {
            NavigatingFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);

        AddHandler(FAFrame.NavigatedFromEvent, (s, e) =>
        {
            NavigatedFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, Avalonia.Interactivity.RoutingStrategies.Direct);

        AddHandler(FAFrame.NavigatedToEvent, (s, e) =>
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
        AddHandler(FAFrame.NavigatedToEvent, (s, e) =>
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
