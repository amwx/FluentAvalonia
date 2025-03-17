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
using FluentAvalonia.Core;
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

    [AvaloniaFact]
    public void InlineItemsHaveCorrectPseudoclasses()
    {
        var bcb = new BreadcrumbBar();
        _window.Content = bcb;
        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
            new MockClass { MockProperty = "Node 3" },
        };

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

        var ellNode = ir.TryGetElement(0) as BreadcrumbBarItem;
        Assert.NotNull(ellNode);

        Assert.Contains(":ellipsis", ellNode.Classes);

        for (int i = 1; i < 4; i++)
        {
            var node = ir.TryGetElement(i);
            Assert.Contains(":inline", node.Classes);
        }
    }

    [AvaloniaFact]
    public void EllipsisDropDownItemsHaveCorrectPseudoclasses()
    {
        var bcb = new BreadcrumbBar();
        
        bcb.ItemsSource = new List<MockClass>()
        {
            new MockClass { MockProperty = "Node 1" },
            new MockClass { MockProperty = "Node 2" },
            new MockClass { MockProperty = "Node 3" },
        };

        _window.Content = new StackPanel
        {
            Width = 60,
            Children =
            {
                bcb
            }
        };

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

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

        Assert.True(ir2.ItemsSourceView.Count > 0);

        for (int i = 0; i < ir2.ItemsSourceView.Count; i++)
        {
            var item = ir2.TryGetElement(i) as BreadcrumbBarItem;
            Assert.NotNull(item);
            Assert.DoesNotContain(":inline", item.Classes);
            Assert.Contains(":ellipsisDropDown", item.Classes);
        }
    }

    [AvaloniaFact]
    public void ClickingInlineItemRaisesItemClickedEvent()
    {
        var bcb = new BreadcrumbBar();
        _window.Content = bcb;
        bcb.ItemsSource = new List<string>()
        {
            "Node 1",
            "Node 2",
            "Node 3",
        };

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

        var node = ir.TryGetElement(1) as BreadcrumbBarItem;
        Assert.NotNull(node);

        int index = -1;
        object item = null;
        bcb.ItemClicked += (_, e) =>
        {
            index = e.Index;
            item = e.Item;
        };

        ClickControl(node);

        Assert.Equal(0, index);
        Assert.Equal(bcb.ItemsSource.ElementAt(0), item);
    }

    [AvaloniaFact]
    public void ClickingEllipsisDropDownItemRaisesItemClickedEvent()
    {
        var bcb = new BreadcrumbBar();
        
        bcb.ItemsSource = new List<string>()
        {
            "Node 1",
            "Node 2",
            "Node 3",
        };

        _window.Content = new StackPanel
        {
            Width = 60,
            Children =
            {
                bcb 
            }
        };

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

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

        var count = ir2.ItemsSourceView.Count;

        // The last item is the first in the ItemsSource list since they're added backwards
        var node = ir2.TryGetElement(count - 1) as BreadcrumbBarItem;
        Assert.NotNull(node);

        int index = -1;
        object item = null;
        bcb.ItemClicked += (_, e) =>
        {
            index = e.Index;
            item = e.Item;
        };

        ClickControl(node);

        Assert.Equal(0, index);
        Assert.Equal(bcb.ItemsSource.ElementAt(0), item);
    }

    [AvaloniaFact]
    public void LastInlineItemIsNotInteractiveByDefault()
    {
        var bcb = new BreadcrumbBar();
        _window.Content = bcb;
        bcb.ItemsSource = new List<string>()
        {
            "Node 1",
            "Node 2",
            "Node 3",
        };

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

        var node = ir.TryGetElement(3) as BreadcrumbBarItem;
        Assert.NotNull(node);

        Assert.Contains(":lastItem", node.Classes);

        int index = -1;
        bcb.ItemClicked += (_, e) =>
        {
            index = e.Index;
        };

        ClickControl(node);

        Assert.Equal(-1, index);

        // Also ensure the chevron is hidden
        var chevron = node.GetVisualDescendants()
            .Where(x => x is TextBlock && x.Name == "PART_ChevronTextBlock")
            .FirstOrDefault();

        Assert.False(chevron.IsVisible);
    }

    [AvaloniaFact]
    public void SettingIsLastItemClickEnabledMakesLastInlineItemInteractive()
    {
        var bcb = new BreadcrumbBar();
        _window.Content = bcb;
        bcb.ItemsSource = new List<string>()
        {
            "Node 1",
            "Node 2",
            "Node 3",
        };
        bcb.IsLastItemClickEnabled = true;

        _window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var ir = GetRepeater(bcb);

        Assert.NotNull(ir);
        Assert.IsType<ItemsRepeater>(ir);

        var node = ir.TryGetElement(3) as BreadcrumbBarItem;
        Assert.NotNull(node);

        Assert.Contains(":lastItem", node.Classes);
        Assert.Contains(":allowClick", node.Classes);

        int index = -1;
        bcb.ItemClicked += (_, e) =>
        {
            index = e.Index;
        };

        ClickControl(node);

        Assert.Equal(2, index);

        // Also ensure the chevron is still hidden
        var chevron = node.GetVisualDescendants()
            .Where(x => x is TextBlock && x.Name == "PART_ChevronTextBlock")
            .FirstOrDefault();

        Assert.False(chevron.IsVisible);
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

    private static bool ClickControl(Control b)
    {
        var top = TopLevel.GetTopLevel(b);
        var trans = b.TransformToVisual(top);
        if (!trans.HasValue)
            return false;

        var rc = new Rect(b.Bounds.Size);
        var pt = rc.Center.Transform(trans.Value);
        top.MouseDown(pt, MouseButton.Left, RawInputModifiers.None);
        top.MouseUp(pt, MouseButton.Left, RawInputModifiers.None);
        return true;
    }

    private Window _window;

    class MockClass
    {
        public string MockProperty { get; set; }
    }
}
