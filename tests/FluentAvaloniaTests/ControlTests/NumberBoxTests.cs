using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class NumberBoxTests
{
    [AvaloniaFact]
    public void VerifyTextAlignmentProperties()
    {
        var c = GetNumberBox();
        var tb = c.nb.GetTemplateChildren().FirstOrDefault(x => x is TextBox tb && tb.Name == "InputBox") as TextBox;

        // Apparently Start is the default value, not left b/c we needed this...
        // TextAlignment shouuld be Left, Center, Right, or Justify, more unneeded crap
        Assert.Equal(TextAlignment.Start, tb.TextAlignment);

        c.nb.TextAlignment = TextAlignment.Right;
        c.nb.UpdateLayout();
        Assert.Equal(TextAlignment.Right, tb.TextAlignment);
    }

    [Theory]
    [InlineData("2+2", 4d)]
    [InlineData("2 + 2", 4d)]
    [InlineData("3*5", 15d)]
    [InlineData("5-4", 1d)]
    [InlineData("20/5", 4d)]
    [InlineData("2^3", 8d)]
    [InlineData("8+(6+7)", 21d)]
    [InlineData("2+", null)]
    [InlineData("2+(4+7", null)]
    [InlineData("2+-", null)]
    [InlineData("2*-7", -14d)]
    [InlineData("2", 2d)]
    [InlineData("3 + 4 * 2 / (1 - 5)^2", 3.5)]
    [InlineData("-(5 + 3) * 2", null)]
    [InlineData("10 / (2 + 3) + 7 * (2 - 1)", 9d)]
    [InlineData("2^(3^2)", 512d)]
    [InlineData("4 * (3 - 7) + 18 / (3 + 3)", -13d)]
    [InlineData("1 - 2 - 3 - 4", -8d)]
    public void VerifyNumberBoxParserWorks(string input, double? expected)
    {
        var result = NumberBoxParser.Compute(input);
        Assert.Equal(expected, result);
    }

    [AvaloniaFact]
    public void ChangingTextBoxTextUpdatesValueAndText()
    {
        var c = GetNumberBox();
        var tb = c.nb.GetTemplateChildren().FirstOrDefault(x => x is TextBox tb && tb.Name == "InputBox") as TextBox;

        tb.Text = "35";
        tb.RaiseEvent(new KeyEventArgs { Key = Key.Enter, KeyDeviceType = KeyDeviceType.Keyboard, PhysicalKey = PhysicalKey.Enter, Source = tb, RoutedEvent = InputElement.KeyUpEvent });
        c.nb.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(35, c.nb.Value);
        Assert.Equal("35", c.nb.Text);

        // Test errors - should not change Value or Text
        tb.Text = "53s";
        tb.RaiseEvent(new KeyEventArgs { Key = Key.Enter, KeyDeviceType = KeyDeviceType.Keyboard, PhysicalKey = PhysicalKey.Enter, Source = tb, RoutedEvent = InputElement.KeyUpEvent });
        c.nb.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(35, c.nb.Value);
        Assert.Equal("35", c.nb.Text);

        // Test expressions
        c.nb.AcceptsExpression = true;
        tb.Text = "35 + 65";
        tb.RaiseEvent(new KeyEventArgs { Key = Key.Enter, KeyDeviceType = KeyDeviceType.Keyboard, PhysicalKey = PhysicalKey.Enter, Source = tb, RoutedEvent = InputElement.KeyUpEvent });
        c.nb.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(100, c.nb.Value);
        Assert.Equal("100", c.nb.Text);
    }

    [AvaloniaFact]
    public void LosingFocusUpdatesValue()
    {
        var c = GetNumberBox();
        var b = new Button();
        c.w.Content = null;
        c.w.Content = new StackPanel
        {
            Children =
            {
                c.nb,
                b
            }
        };

        c.w.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var tb = c.nb.GetTemplateChildren().FirstOrDefault(x => x is TextBox tb && tb.Name == "InputBox") as TextBox;
        tb.Focus();
        tb.Text = "344";
        Assert.True(b.Focus());
        Dispatcher.UIThread.RunJobs();

        var item = c.w.FocusManager.GetFocusedElement();
        Assert.Equal(344, c.nb.Value);
    }

    private static (Window w, FANumberBox nb) GetNumberBox()
    {
        var nb = new FANumberBox();
        var wnd = new Window
        {
            Content = nb
        };

        wnd.Show();
        Dispatcher.UIThread.RunJobs();
        return (wnd, nb);
    }
}
