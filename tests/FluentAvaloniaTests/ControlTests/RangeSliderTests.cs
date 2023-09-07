using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;
public class RangeSliderTests : IDisposable
{
    public RangeSliderTests()
    {
        _window = new Window();
        _window.Show();
    }

    [AvaloniaTheory]
    // User range values are not changed
    [InlineData(0, 100, 0, 100)]
    [InlineData(10, 90, 10, 90)]
    // Ensure RangeStart can't be bigger than RangeEnd
    [InlineData(90, 10, 10, 10)]
    [InlineData(110, 10, 10, 10)]
    // Ensure Range values are constrained by Min/Max
    [InlineData(-50, -50, 0, 0)]
    [InlineData(-90, 90, 0, 90)]
    [InlineData(-90, -10, 0, 0)]
    [InlineData(-10, -90, 0, 0)]
    [InlineData(10, -90, 0, 0)]
    [InlineData(150, 150, 100, 100)]
    [InlineData(10, 190, 10, 100)]
    [InlineData(110, 190, 100, 100)]
    [InlineData(190, 110, 100, 100)]
    public void RangeValuesAreConstrainedProperly(double rStart, double rEnd, double expectedRStart, double expectedREnd)
    {
        var rs = new RangeSlider
        {
            RangeStart = rStart,
            RangeEnd = rEnd,
            Minimum = 0,
            Maximum = 100
        };

        _window.Content = rs;

        _window.UpdateLayout();

        Assert.Equal(expectedRStart, rs.RangeStart);
        Assert.Equal(expectedREnd, rs.RangeEnd);

        _window.Content = null;
    }

    [AvaloniaFact]
    public void DraggingRangeStartThumbWorks()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(10, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        var pxPerStep = rs.DragWidth / 100; // 100 = Maximum - Minimum = 100 - 0
        var expectedNewValue = Math.Round(delta.X /*dragAmount*/ / pxPerStep);

        Assert.Equal(expectedNewValue, rs.RangeStart);

        // Reset to zero so math is easier
        rs.RangeStart = 0;

        downPoint = TransformToHost(minThumb);
        delta = new Point(rs.Bounds.Width / 2, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        expectedNewValue = Math.Round(delta.X /*dragAmount*/ / pxPerStep);

        Assert.Equal(expectedNewValue, rs.RangeStart);
    }

    [AvaloniaFact]
    public void DraggingRangeEndThumbWorks()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var maxThumb = rs.GetTemplateChildren().Where(x => x.Name == "MaxThumb").First();

        var downPoint = TransformToHost(maxThumb);
        var delta = new Point(-10, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        var pxPerStep = rs.DragWidth / 100; // 100 = Maximum - Minimum = 100 - 0
        var expectedNewValue = 100 - Math.Round(-delta.X /*dragAmount*/ / pxPerStep);

        Assert.Equal(expectedNewValue, rs.RangeEnd);

        // Reset so math is easier
        rs.RangeEnd = 100;

        downPoint = TransformToHost(maxThumb);
        delta = new Point(-rs.DragWidth / 2, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        expectedNewValue = 100 - Math.Round(-delta.X /*dragAmount*/ / pxPerStep);

        // Expected value is eV - 1 here b/c we're dragging from the middle of the MaxThumb which is beyond
        // the calculated drag extent (which ends at the start of the MaxThumb if it's all the way to the right)
        Assert.Equal(expectedNewValue, rs.RangeEnd);
    }

    [AvaloniaFact]
    public void DraggingRangeStartThumbWorksWithStepFrequency()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        double stepFreq = 5;
        rs.StepFrequency = stepFreq;

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(50, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        var pxPerStep = rs.DragWidth / 100; // 100 = Maximum - Minimum = 100 - 0
        var expectedNewValue = Math.Round((delta.X /*dragAmount*/ / pxPerStep) / stepFreq) * stepFreq;

        Assert.Equal(expectedNewValue, rs.RangeStart);

        //// Reset to zero so math is easier
        rs.RangeStart = 0;

        downPoint = TransformToHost(minThumb);
        delta = new Point(rs.DragWidth / 2, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        expectedNewValue = Math.Round((delta.X /*dragAmount*/ / pxPerStep) / stepFreq) * stepFreq;

        Assert.Equal(expectedNewValue, rs.RangeStart);
    }

    [AvaloniaFact]
    public void DraggingRangeEndThumbWorksWithStepFrequency()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MaxThumb").First();

        double stepFreq = 5;
        rs.StepFrequency = stepFreq;

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(-50, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        var pxPerStep = rs.DragWidth / 100; // 100 = Maximum - Minimum = 100 - 0
        var expectedNewValue = 100 - Math.Round((-delta.X /*dragAmount*/ / pxPerStep) / stepFreq) * stepFreq;

        Assert.Equal(expectedNewValue, rs.RangeEnd);

        //// Reset to zero so math is easier
        rs.RangeEnd = 100;

        downPoint = TransformToHost(minThumb);
        delta = new Point(-rs.DragWidth / 2, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left);

        expectedNewValue = 100 - Math.Round((-delta.X /*dragAmount*/ / pxPerStep) / stepFreq) * stepFreq;

        Assert.Equal(expectedNewValue, rs.RangeEnd);
    }

    [AvaloniaFact]
    public void LeftAndRightKeysMovesRangeStartThumb()
    {
        var rs = new RangeSlider()
        {
            RangeStart = 50
        };
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        minThumb.Focus();

        _window.KeyPress(Key.Right, RawInputModifiers.None);
        Assert.Equal(51, rs.RangeStart);

        _window.KeyPress(Key.Left, RawInputModifiers.None);
        Assert.Equal(50, rs.RangeStart);
    }

    [AvaloniaFact]
    public void LeftAndRightKeyMovesRangeEndThumb()
    {
        var rs = new RangeSlider()
        {
            RangeEnd = 50
        };
        _window.Content = rs;
        _window.UpdateLayout();
        var maxThumb = rs.GetTemplateChildren().Where(x => x.Name == "MaxThumb").First();

        maxThumb.Focus();

        _window.KeyPress(Key.Right, RawInputModifiers.None);
        Assert.Equal(51, rs.RangeEnd);

        _window.KeyPress(Key.Left, RawInputModifiers.None);
        Assert.Equal(50, rs.RangeEnd);
    }

    [AvaloniaFact]
    public void DraggingRangeStartThumbShowsToolTip()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(10, 0);
        _window.MouseDown(downPoint, MouseButton.Left);

        Assert.True(ToolTip.GetIsOpen(minThumb));
    }

    [AvaloniaFact]
    public void DraggingRangeEndThumbShowsToolTip()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var maxThumb = rs.GetTemplateChildren().Where(x => x.Name == "MaxThumb").First();

        var downPoint = TransformToHost(maxThumb);
        var delta = new Point(10, 0);
        _window.MouseDown(downPoint, MouseButton.Left);

        Assert.True(ToolTip.GetIsOpen(maxThumb));
    }

    [AvaloniaFact]
    public async Task UsingLeftRightShowsToolTipAndAutoHides()
    {
        var rs = new RangeSlider()
        {
            RangeStart = 50
        };
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        minThumb.Focus();

        _window.KeyPress(Key.Right, RawInputModifiers.None);
        Assert.True(ToolTip.GetIsOpen(minThumb));
        _window.KeyRelease(Key.Right, RawInputModifiers.None);

        // Tooltip dismisses automatically after 1 second, hold the thread here to wait for that
        await Task.Delay(2000);

        // Now force the Dispatcher to run, which should invoke the timer to hide the tip
        Dispatcher.UIThread.RunJobs();
        Assert.False(ToolTip.GetIsOpen(minThumb));
    }

    [AvaloniaFact]
    public void CanDragRange()
    {
        var rs = new RangeSlider()
        {
            RangeStart = 25,
            RangeEnd = 75
        };
        _window.Content = rs;
        _window.UpdateLayout();

        var downPoint = TransformToHost(rs);
        var delta = new Point(15, 0);
        _window.MouseDown(downPoint, Avalonia.Input.MouseButton.Left, RawInputModifiers.Control);
        _window.MouseMove(downPoint + delta, RawInputModifiers.Control);
        _window.MouseUp(downPoint + delta, Avalonia.Input.MouseButton.Left, RawInputModifiers.Control);

        var pxPerStep = rs.DragWidth / 100; // 100 = Maximum - Minimum = 100 - 0
        var expectedNewStart = 25 + Math.Round(delta.X /*dragAmount*/ / pxPerStep);
        var expectedNewEnd = 75 + Math.Round(delta.X /*dragAmount*/ / pxPerStep);

        Assert.Equal(expectedNewStart, rs.RangeStart);
        Assert.Equal(expectedNewEnd, rs.RangeEnd);
    }

    [AvaloniaFact]
    public void CanHideToolTip()
    {
        var rs = new RangeSlider()
        {
            ShowValueToolTip = false
        };
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(10, 0);
        _window.MouseDown(downPoint, MouseButton.Left);

        Assert.False(ToolTip.GetIsOpen(minThumb));
    }

    [AvaloniaFact]
    public void ValueChangedOnlyFiresOnceWhenDragging()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        int count = 0;
        rs.ValueChanged += (s, e) =>
        {
            count++;
        };

        var downPoint = TransformToHost(minThumb);
        var delta = new Point(50, 0);
        _window.MouseDown(downPoint, MouseButton.Left);
        _window.MouseMove(downPoint + delta);
        _window.MouseUp(downPoint + delta, MouseButton.Left);

        Assert.Equal(1, count);
    }

    [AvaloniaFact]
    public void ValueChangedFiresWhenProgrammaticallySet()
    {
        var rs = new RangeSlider();
        _window.Content = rs;
        _window.UpdateLayout();
        var minThumb = rs.GetTemplateChildren().Where(x => x.Name == "MinThumb").First();

        int count = 0;
        rs.ValueChanged += (s, e) =>
        {
            count++;
        };

        rs.RangeStart = 20;
        Assert.Equal(1, count);

        count = 0;
        rs.RangeEnd = 70;
        Assert.Equal(1, count);
    }

    private Point TransformToHost(Control c)
    {
        var x = c.Bounds.Width * 0.5;
        var y = c.Bounds.Height * 0.5;

        return c.TransformToVisual(_window).Value.Transform(new Point(x, y));
    }

    public void Dispose()
    {
        _window.Close();
    }

    private Window _window;
}
