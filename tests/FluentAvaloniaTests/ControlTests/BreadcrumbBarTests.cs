using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class BreadcrumbBarTests : IDisposable
{
    public BreadcrumbBarTests()
    {
        _window = new Window();
        _window.Show();
    }

    [AvaloniaFact]
    public void VerifyBreadcrumbDefaultAPIValues()
    {
        var bcb = new BreadcrumbBar();

        Assert.Null(bcb.ItemsSource);
        Assert.Null(bcb.ItemTemplate);
    }

    [AvaloniaFact]
    public void VerifyDefaultBreadcrumb()
    {
        var bcb = new BreadcrumbBar();
        _window.Content = bcb;
        _window.UpdateLayout();

        var ir = bcb.GetTemplateChildren().Where(x => x.Name == "PART_ItemsRepeater").FirstOrDefault();

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

        var node = (ir as ItemsRepeater).TryGetElement(1);
        Assert.Null(node);
    }

    [AvaloniaFact]
    public void VerifyCustomItemTemplate()
    {
        var bcb = new BreadcrumbBar();
        var bcb2 = new BreadcrumbBar();

        bcb.ItemsSource = new List<string> { "Node 1", "Node 2" };
        // Set a custom ItemTemplate to be wrapped in a BreadcrumbBarItem.
        var itemTemplate = new FuncDataTemplate<string>((x, ns) =>
        {
            return new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding()
            };
        });
        bcb.ItemTemplate = itemTemplate;

        bcb2.ItemsSource = new List<string> { "Node 1", "Node 2" };
        var itemTemplate2 = new FuncDataTemplate<string>((x, ns) =>
        {
            return new BreadcrumbBarItem
            {
                Foreground = Brushes.Blue,
                Content = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding()
                }
            };
        });
        bcb2.ItemTemplate = itemTemplate2;

        var sp = new StackPanel
        {
            Children =
            {
                bcb,
                bcb2
            }
        };

        _window.Content = sp;
        _window.UpdateLayout();
        var ir = GetRepeater(bcb);
        var ir2 = GetRepeater(bcb2);
        Assert.NotNull(ir);
        Assert.NotNull(ir2);

        var node1 = ir.TryGetElement(1) as BreadcrumbBarItem;
        var node2 = ir2.TryGetElement(1) as BreadcrumbBarItem;

        Assert.NotNull(node1);
        Assert.NotNull(node2);

        bool testCondition = !(node1.Foreground is ISolidColorBrush solid && solid.Color == Colors.Blue);
        Assert.True(testCondition);

        testCondition = node2.Foreground is ISolidColorBrush solid2 && solid2.Color == Colors.Blue;
        Assert.True(testCondition);
    }

    [AvaloniaFact]
    public void VerifyNumericItemsSource()
    {
        var bcb = new BreadcrumbBar();

        bcb.ItemsSource = new List<int> { 1, 2, 3, 4 };
        // Set a custom ItemTemplate to be wrapped in a BreadcrumbBarItem.
        var itemTemplate = new FuncDataTemplate<object>((x, ns) =>
        {
            return new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding()
            };
        });
        bcb.ItemTemplate = itemTemplate;

        _window.Content = bcb;
        _window.UpdateLayout();
        
        var ir = GetRepeater(bcb);
        Assert.NotNull(ir);

        var node1 = ir.TryGetElement(1) as BreadcrumbBarItem;

        Assert.NotNull(node1);
    }

    [AvaloniaFact]
    public void VerifyObjectItemsSource()
    {
        var bcb = new BreadcrumbBar();

        bcb.ItemsSource = new List<MockClass>() 
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
        };
        var itemTemplate = new FuncDataTemplate<MockClass>((x, ns) =>
        {
            return new BreadcrumbBarItem
            {
                Foreground = Brushes.Blue,
                Content = x,
                ContentTemplate = new FuncDataTemplate<string>((_, __) =>
                {
                    return new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding()
                    };
                })
            };
        });
        bcb.ItemTemplate = itemTemplate;

        _window.Content = bcb;
        _window.UpdateLayout();

        var ir = GetRepeater(bcb);
        Assert.NotNull(ir);

        var node = ir.TryGetElement(1) as BreadcrumbBarItem;
        Assert.NotNull(node);
    }

    [AvaloniaFact]
    public void VerifyDropDownItemTemplate()
    {
        var bcb = new BreadcrumbBar();

        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
        };
        var itemTemplate = new FuncDataTemplate<MockClass>((x, ns) =>
        {
            return new BreadcrumbBarItem
            {
                DataContext = x,
                Content = new Binding(),
                ContentTemplate = new FuncDataTemplate<MockClass>((_, __) =>
                {
                    return new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding("MockProperty"),
                        Foreground = Brushes.Blue
                    };
                })
            };
        });
        bcb.ItemTemplate = itemTemplate;

        var stackPanel = new StackPanel
        {
            Width = 60,
            Children =
            {
                bcb
            }
        };

        _window.Content = stackPanel;
        _window.UpdateLayout();

        // We have to run this controls get loaded, otherwise Loaded event never fires
        // and the BCBItems never fully initialize
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);
        Assert.NotNull(ir);

        var n1 = ir.TryGetElement(0) as BreadcrumbBarItem;
        Assert.NotNull(n1);

        var eb = n1.GetTemplateChildren().Where(x => x.Name == "PART_ItemButton")
            .FirstOrDefault() as Button;
        Assert.NotNull(eb);

        var peer = new ButtonAutomationPeer(eb);
        var inv = peer as IInvokeProvider;
        inv.Invoke();
        Dispatcher.UIThread.RunJobs();

        var rootGrid = n1.GetTemplateChildren()
            .Where(x => x is Grid g && g.Resources.Count > 0)
            .FirstOrDefault();
        Assert.NotNull(rootGrid);
        var flyout = rootGrid.Resources["PART_EllipsisFlyout"] as Flyout;
        Assert.NotNull(flyout);

        var ir2 = flyout.Content as ItemsRepeater;
        Assert.NotNull(ir2);

        // The WinUI version of this test makes no sense, so this is custom from here
        var ellNode1 = ir2.TryGetElement(0) as BreadcrumbBarItem;
        var presenter = ellNode1.GetTemplateChildren()
            .Where(x => x is ContentPresenter cp && cp.Name == "PART_EllipsisDropDownItemContentPresenter")
            .FirstOrDefault();
        Assert.NotNull(presenter);
        var textBlock = presenter.GetVisualDescendants()
            .Where(x => x is TextBlock).FirstOrDefault() as TextBlock;
        Assert.NotNull(textBlock);
        bool testCondition = textBlock.Foreground is ISolidColorBrush b && b.Color == Colors.Blue;
        Assert.True(testCondition);
    }

    [AvaloniaFact]
    public void VerifyDropDownItemTemplateWithNoControl()
    {
        var bcb = new BreadcrumbBar();

        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
        };
        var itemTemplate = new FuncDataTemplate<MockClass>((x, ns) =>
        {
            return new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(),
                Foreground = Brushes.Blue
            };
        });
        bcb.ItemTemplate = itemTemplate;

        var stackPanel = new StackPanel
        {
            Width = 60,
            Children =
            {
                bcb
            }
        };

        _window.Content = stackPanel;
        _window.UpdateLayout();

        // We have to run this controls get loaded, otherwise Loaded event never fires
        // and the BCBItems never fully initialize
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);
        Assert.NotNull(ir);

        var n1 = ir.TryGetElement(0) as BreadcrumbBarItem;
        Assert.NotNull(n1);

        var eb = n1.GetTemplateChildren().Where(x => x.Name == "PART_ItemButton")
            .FirstOrDefault() as Button;
        Assert.NotNull(eb);

        var peer = new ButtonAutomationPeer(eb);
        var inv = peer as IInvokeProvider;
        inv.Invoke();
        Dispatcher.UIThread.RunJobs();

        var rootGrid = n1.GetTemplateChildren()
            .Where(x => x is Grid g && g.Resources.Count > 0)
            .FirstOrDefault();
        Assert.NotNull(rootGrid);
        var flyout = rootGrid.Resources["PART_EllipsisFlyout"] as Flyout;
        Assert.NotNull(flyout);

        var ir2 = flyout.Content as ItemsRepeater;
        Assert.NotNull(ir2);

        // The WinUI version of this test makes no sense, so this is custom from here
        var ellNode1 = ir2.TryGetElement(0) as BreadcrumbBarItem;
        var presenter = ellNode1.GetTemplateChildren()
            .Where(x => x is ContentPresenter cp && cp.Name == "PART_EllipsisDropDownItemContentPresenter")
            .FirstOrDefault();
        Assert.NotNull(presenter);
        var textBlock = presenter.GetVisualDescendants()
            .Where(x => x is TextBlock).FirstOrDefault() as TextBlock;
        Assert.NotNull(textBlock);
        bool testCondition = textBlock.Foreground is ISolidColorBrush b && b.Color == Colors.Blue;
        Assert.True(testCondition);
    }

    [AvaloniaFact]
    public void VerifyCollectionChangeGetsRespected()
    {
        var bcb = new BreadcrumbBar();

        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
        };
        var itemTemplate = new FuncDataTemplate<MockClass>((x, ns) =>
        {
            return new BreadcrumbBarItem
            {
                DataContext = x,
                Content = new Binding(),
                ContentTemplate = new FuncDataTemplate<MockClass>((_, __) =>
                {
                    return new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding("MockProperty"),
                        Foreground = Brushes.Blue
                    };
                })
            };
        });
        bcb.ItemTemplate = itemTemplate;

        _window.Content = bcb;

        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);
        var node2 = ir.TryGetElement(1) as BreadcrumbBarItem;
        Assert.NotNull(node2);

        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
            new MockClass { MockProperty = "Node 3" },
            new MockClass { MockProperty = "Node 4" },
        };

        Dispatcher.UIThread.RunJobs();
        var node3 = ir.TryGetElement(3) as BreadcrumbBarItem;
        Assert.NotNull(node3);
    }

    public void Dispose()
    {
        _window.Close();
    }

    private static ItemsRepeater GetRepeater(BreadcrumbBar b)
    {
        return b.GetTemplateChildren().Where(x => x.Name == "PART_ItemsRepeater")
            .FirstOrDefault() as ItemsRepeater;
    }

    private static bool ClickButton(Window w, Button b)
    {
        var top = TopLevel.GetTopLevel(b);
        var trans = b.TransformToVisual(top);
        if (!trans.HasValue)
            return false;

        var rc = new Rect(b.Bounds.Size);
        var pt = rc.Center.Transform(trans.Value);
        w.MouseDown(pt, MouseButton.Left, RawInputModifiers.None);
        w.MouseUp(pt, MouseButton.Left, RawInputModifiers.None);
        return true;
    }

    private Window _window;

    class MockClass
    {
        public string MockProperty { get; set; }
    }
}
