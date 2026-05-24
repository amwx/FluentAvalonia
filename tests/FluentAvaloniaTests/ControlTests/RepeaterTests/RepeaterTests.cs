using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class RepeaterTests
{
    [AvaloniaFact]
    public void ValidateElementToIndexMapping()
    {
        var ctx = new RepeaterTestContext();

        var ef = new FARecyclingElementFactory();
        ef.RecyclePool = new FARecyclePool();
        ef.Templates["Item"] = new FuncDataTemplate<string>((x, ns) => new TextBlock { Text = x, Height = 50 });

        ctx.ItemsRepeater.ItemsSource = Enumerable.Range(0, 10).Select(x => $"Item {x}");
        ctx.ItemsRepeater.ItemTemplate = ef;

        RunJobs();

        for (int i = 0; i < 10; i++)
        {
            var element = ctx.ItemsRepeater.TryGetElement(i);
            Assert.NotNull(element);
            Assert.Equal($"Item {i}", ((TextBlock)element).Text);
            Assert.Equal(i, ctx.ItemsRepeater.GetElementIndex(element));
        }

        // Verify TryGetElement returns null for out of range elements
        Assert.Null(ctx.ItemsRepeater.TryGetElement(20));
    }

    [AvaloniaFact]
    public void CanSetItemsSource()
    {
        var ctx = new RepeaterTestContext();
        var ir = ctx.ItemsRepeater;

        try
        {
            ir.ItemsSource = Enumerable.Range(0, 5).Select(x => $"Item {x}");
            RunJobs();

            ir.ItemsSource = null;
            RunJobs();
        }
        catch (Exception ex)
        {
            Assert.Fail($"CanSetItemsSource failed {ex.Message}");
        }
    }

    [AvaloniaFact]
    public void EnsureGetOrCreateElementThrowsForNullDataSource()
    {
        var ctx = new RepeaterTestContext();
        var ir = ctx.ItemsRepeater;

        // Per GetOrCreateImpl, we throw a basic Exception for null ItemsSource
        Assert.Throws<Exception>(() => ir.GetOrCreateElement(0));
    }

    [AvaloniaFact]
    public void VerifyCurrentAnchor()
    {
        var ctx = new RepeaterTestContext();
        var ir = ctx.ItemsRepeater;
        var data = new ObservableCollection<string>(
            Enumerable.Range(0, 500).Select(x => $"Item {x}"));
       
        ir.ItemsSource = data;
        ir.ItemTemplate = new FuncDataTemplate<string>((x, ns) => new TextBlock { Text = x, Height = 50 });
        RunJobs();

        bool scrollChanged = false;
        ctx.ScrollViewer.ScrollChanged += (s, e) =>
        {
            scrollChanged = true;
        };

        for (int i = 1; i < 10; i++)
        {
            ctx.ScrollViewer.Offset = new Avalonia.Vector(0, i * 200);
            RunJobs();
            Assert.True(scrollChanged);
            scrollChanged = false;

            var anchor = ctx.ScrollViewer.CurrentAnchor;
            var anchorIndex = ctx.ItemsRepeater.GetElementIndex(anchor);
            Assert.Equal(i * 4, anchorIndex);
        }
    }

    [AvaloniaFact]
    public void VerifyFocusedElementIsRecycledOnCollectionReset()
    {
        var ctx = new RepeaterTestContext();
        var ir = ctx.ItemsRepeater;
        ir.ItemsSource = new ObservableCollection<string>(
            Enumerable.Range(0, 10).Select(x => $"Item {x + 1}"));
        ir.ItemTemplate = new FuncDataTemplate<string>((x, ns) => new Button { Content = x });

        RunJobs();

        var element = ir.TryGetElement(4);
        Assert.True(element.Focus(NavigationMethod.Tab));

        RunJobs();

        ir.ItemsSource = null;
        RunJobs();

        for (int i = 0; i < 10; i++)
        {
            Assert.Null(ir.TryGetElement(i));
        }

        Assert.NotEqual(element, ctx.Window.FocusManager.GetFocusedElement());
    }

    private static void RunJobs() => Dispatcher.UIThread.RunJobs();
}
