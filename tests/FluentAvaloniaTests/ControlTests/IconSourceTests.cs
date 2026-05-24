using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class IconSourceTests
{
    [AvaloniaFact]
    public void IconSourceWithUnsetForegroundUsesInherited()
    {
        var w = GetWindow();
        w.Content = new FAIconSourceElement
        {
            IconSource = (FASymbolIconSource)(w.Resources["SymbolIcon"])
        };
        w.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        w.SetValue(TextElement.ForegroundProperty, Brushes.Red);
        Dispatcher.UIThread.RunJobs();

        var sym = w.FindDescendantOfType<FASymbolIcon>();

        Assert.NotNull(sym);

        Assert.Equal(Brushes.Red, sym.GetValue(TextElement.ForegroundProperty));
    }

    [AvaloniaFact]
    public void IconSourceUsesSetForeground()
    {
        var w = GetWindow();

        var ico = (FASymbolIconSource)(w.Resources["SymbolIcon"]);
        ico.Foreground = Brushes.DarkSlateBlue;
        w.Content = new FAIconSourceElement
        {
            IconSource = ico
        };
        w.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        w.SetValue(TextElement.ForegroundProperty, Brushes.Red);
        Dispatcher.UIThread.RunJobs();

        var sym = w.FindDescendantOfType<FASymbolIcon>();

        Assert.NotNull(sym);

        Assert.Equal(Brushes.DarkSlateBlue, sym.GetValue(TextElement.ForegroundProperty));
    }

    [AvaloniaFact]
    public void IconUpdatesForegroundIfChangedOnIconSource()
    {
        var w = GetWindow();
        var ico = (FASymbolIconSource)(w.Resources["SymbolIcon"]);
        ico.Foreground = Brushes.DarkSlateBlue;
        w.Content = new FAIconSourceElement
        {
            IconSource = ico
        };
        w.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        w.SetValue(TextElement.ForegroundProperty, Brushes.Red);
        Dispatcher.UIThread.RunJobs();

        var sym = w.FindDescendantOfType<FASymbolIcon>();

        Assert.NotNull(sym);

        Assert.Equal(Brushes.DarkSlateBlue, sym.GetValue(TextElement.ForegroundProperty));

        ico.Foreground = Brushes.Yellow;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Brushes.Yellow, sym.GetValue(TextElement.ForegroundProperty));
    }

    private static Window GetWindow()
    {
        var w = new Window();
        w.Resources.Add("SymbolIcon", new FASymbolIconSource { Symbol = FASymbol.Save });
        w.Show();
        Dispatcher.UIThread.RunJobs();
        return w;
    }
}
