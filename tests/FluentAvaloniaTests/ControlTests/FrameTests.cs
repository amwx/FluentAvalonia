using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class FrameTests : IDisposable
{
    public FrameTests()
    {
        _window = new Window();
        _window.Show();
    }

    [AvaloniaFact]
    public void NavigateWorks()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage1));

        Assert.IsType<TestPage1>(frame.Content);
    }

    [AvaloniaFact]
    public void SettingSourcePageTypeNavigates()
    {
        var frame = CreateFrame();

        frame.SourcePageType = typeof(TestPage1);

        Assert.IsType<TestPage1>(frame.Content);
        Assert.Equal(typeof(TestPage1), frame.SourcePageType);
    }

    [AvaloniaFact]
    public void ClearingContentClearsCurrentEntry()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage1));

        Assert.NotNull(frame.CurrentEntry);

        frame.Content = null;

        Assert.Null(frame.CurrentEntry);
    }

    [AvaloniaFact]
    public void SettingSourcePageTypeToNullThrows()
    {
        var frame = CreateFrame();

        frame.SourcePageType = typeof(TestPage1);

        var ex = Assert.Throws<InvalidOperationException>(() => frame.SourcePageType = null);

        Assert.Equal("SourcePageType cannot be null. Use Content instead.", ex.Message);
    }

    [AvaloniaFact]
    public void NavigatingIsCalledBeforeNavigated()
    {
        var frame = CreateFrame();
        var calledNavigating = false;
        var calledNavigated = false;

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

        frame.Navigating += OnNavigating;
        frame.Navigated += OnNavigated;

        frame.Navigate(typeof(TestPage2));

        Assert.True(calledNavigating);
        Assert.True(calledNavigated);

        frame.Navigating -= OnNavigating;
        frame.Navigated -= OnNavigated;
    }

    [AvaloniaFact]
    public void CancelingNavigatingPreventsNavigation()
    {
        var frame = CreateFrame();
        frame.Navigate(typeof(TestPage1));

        var calledNavigating = false;
        var calledNavigated = false;

        void OnNavigating(object sender, FANavigatingCancelEventArgs args)
        {
            calledNavigating = true;
            args.Cancel = true;
        }

        void OnNavigated(object sender, FANavigationEventArgs args)
        {
            calledNavigated = true;
        }

        frame.Navigating += OnNavigating;
        frame.Navigated += OnNavigated;

        frame.Navigate(typeof(TestPage2));

        Assert.True(calledNavigating);
        Assert.False(calledNavigated);
        Assert.IsType<TestPage1>(frame.Content);

        frame.Navigating -= OnNavigating;
        frame.Navigated -= OnNavigated;
    }

    [AvaloniaFact]
    public void NavigatingToInvalidTypeRaisesNavigationFailed()
    {
        var frame = CreateFrame();
        var failed = false;

        void OnNavigationFailed(object sender, FANavigationFailedEventArgs e)
        {
            failed = true;
        }

        frame.NavigationFailed += OnNavigationFailed;

        frame.Navigate(typeof(ThisPageShouldntLoad));

        Assert.True(failed);

        frame.NavigationFailed -= OnNavigationFailed;
    }

    [AvaloniaFact]
    public void NavigationStackWorks()
    {
        var frame = CreateFrame();

        Assert.True(frame.IsNavigationStackEnabled);

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3));

        Assert.Equal(2, frame.BackStackDepth);
        Assert.Empty(frame.ForwardStack);
        Assert.True(frame.CanGoBack);
        Assert.False(frame.CanGoForward);

        frame.GoBack();

        Assert.Equal(1, frame.BackStackDepth);
        Assert.True(frame.CanGoBack);
        Assert.True(frame.CanGoForward);

        frame.GoBack();

        Assert.Empty(frame.BackStack);
        Assert.False(frame.CanGoBack);
        Assert.True(frame.CanGoForward);

        frame.GoForward();

        Assert.Equal(1, frame.BackStackDepth);
        Assert.True(frame.CanGoBack);
        Assert.True(frame.CanGoForward);
    }

    [AvaloniaFact]
    public void PagesAreCachedWhenNavigatingIfUsingNavigationStack()
    {
        var frame = CreateFrame();

        Assert.True(frame.IsNavigationStackEnabled);

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3));

        Assert.Equal(2, frame.BackStackDepth);
        Assert.IsType<TestPage2>(frame.BackStack[1].Instance);
        Assert.IsType<TestPage1>(frame.BackStack[0].Instance);

        var prevPage = frame.BackStack[1].Instance;

        frame.GoBack();

        Assert.Equal(prevPage, frame.Content);
    }

    [AvaloniaFact]
    public void DisablingNavigationStackPreventsPageCaching()
    {
        var frame = CreateFrame();

        frame.IsNavigationStackEnabled = false;

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3));

        Assert.Empty(frame.BackStack);
        Assert.Equal(0, frame.BackStackDepth);

        frame.GoBack();

        Assert.Empty(frame.ForwardStack);
    }

    [AvaloniaFact]
    public void NavigationPageEventsFire()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage4), "parameter");
        Dispatcher.UIThread.RunJobs();

        var page4 = Assert.IsType<TestPage4>(frame.Content);

        Assert.True(page4.NavigatedToFired);
        Assert.True(page4.ParameterPassed);

        frame.Navigate(typeof(TestPage5), "parameter2");
        Dispatcher.UIThread.RunJobs();

        Assert.True(page4.NavigatingFromFired);
        Assert.True(page4.ParameterPassed);
        Assert.True(page4.NavigatedFromFired);
        Assert.True(page4.ParameterPassed);

        var page5 = Assert.IsType<TestPage5>(frame.Content);

        Assert.True(page5.NavigatedToFired);
        Assert.True(page5.ParameterPassed);
    }

    [AvaloniaFact]
    public void NavigationStateSavesInCorrectFormat()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3), "param");
        frame.Navigate(null);

        frame.GoBack();

        var expected =
            $"{typeof(TestPage2).AssemblyQualifiedName}|" + Environment.NewLine +
            "1" + Environment.NewLine +
            $"{typeof(TestPage1).AssemblyQualifiedName}|" + Environment.NewLine +
            "1" + Environment.NewLine +
            $"{typeof(TestPage3).AssemblyQualifiedName}|param" + Environment.NewLine;

        var str = frame.GetNavigationState();

        Assert.Equal(expected, str);
    }

    [AvaloniaFact]
    public void NavigationStateIsRestoredCorrectly()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3), "param");
        frame.Navigate(null);

        frame.GoBack();

        var str = frame.GetNavigationState();

        frame = CreateFrame();

        frame.SetNavigationState(str);

        Assert.Single(frame.BackStack);
        Assert.Single(frame.ForwardStack);
        Assert.Equal(typeof(TestPage1), frame.BackStack[0].SourcePageType);
        Assert.Equal(typeof(TestPage3), frame.ForwardStack[0].SourcePageType);
        Assert.IsType<TestPage2>(frame.Content);

        frame.GoBack();

        Assert.IsType<TestPage1>(frame.Content);
    }

    [AvaloniaFact]
    public void NavigationStateIsRestoredCorrectlyWithSuppressNavigation()
    {
        var frame = CreateFrame();

        frame.Navigate(typeof(TestPage1));
        frame.Navigate(typeof(TestPage2));
        frame.Navigate(typeof(TestPage3), "param");
        frame.Navigate(null);

        frame.GoBack();

        var str = frame.GetNavigationState();

        frame = CreateFrame();

        frame.SetNavigationState(str, true);

        Assert.Equal(2, frame.BackStack.Count);
        Assert.Single(frame.ForwardStack);
        Assert.Equal(typeof(TestPage1), frame.BackStack[0].SourcePageType);
        Assert.Equal(typeof(TestPage2), frame.BackStack[1].SourcePageType);
        Assert.Equal(typeof(TestPage3), frame.ForwardStack[0].SourcePageType);
        Assert.Null(frame.Content);

        frame.GoBack();

        Assert.IsType<TestPage2>(frame.Content);
    }

    public void Dispose()
    {
        _window.Close();
    }

    private FAFrame CreateFrame()
    {
        var frame = new FAFrame();

        _window.Content = frame;
        _window.UpdateLayout();

        return frame;
    }

    private readonly Window _window;
}

public class TestPage1 : UserControl { }

public class TestPage2 : UserControl { }

public class TestPage3 : UserControl { }

public class ThisPageShouldntLoad { }

public class TestPage4 : UserControl
{
    public TestPage4()
    {
        AddHandler(FAFrame.NavigatingFromEvent, (_, e) =>
        {
            NavigatingFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, RoutingStrategies.Direct);

        AddHandler(FAFrame.NavigatedFromEvent, (_, e) =>
        {
            NavigatedFromFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, RoutingStrategies.Direct);

        AddHandler(FAFrame.NavigatedToEvent, (_, e) =>
        {
            NavigatedToFired = true;
            ParameterPassed = e.Parameter.Equals("parameter");
        }, RoutingStrategies.Direct);
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
        AddHandler(FAFrame.NavigatedToEvent, (_, e) =>
        {
            NavigatedToFired = true;
            ParameterPassed = e.Parameter.Equals("parameter2");
        }, RoutingStrategies.Direct);
    }

    public bool NavigatingFromFired { get; set; }

    public bool NavigatedFromFired { get; set; }

    public bool NavigatedToFired { get; set; }

    public bool ParameterPassed { get; set; }
}
