using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    // Tests disabled as ListView/GridView project suspended for time being

	//public class ItemsStackPanelTests
	//{
	//	public (ItemsControl itemsControl, ItemsStackPanel panel) GetItemsControl(Orientation orientation = Orientation.Vertical)
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Orientation = orientation
	//		};

	//		var sv = new ScrollViewer
	//		{
	//			Template = new FuncControlTemplate<ScrollViewer>(CreateScrollViewerTemplate),
	//			VerticalScrollBarVisibility = orientation == Orientation.Vertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
	//			HorizontalScrollBarVisibility = orientation == Orientation.Horizontal ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,				
	//		};

	//		var ic = new ItemsControl
	//		{
	//			Template = new FuncControlTemplate<ItemsControl>((x, ns) =>
	//			{
	//				sv.Content = new FAItemsPresenter() { Name = "Presenter" }.RegisterInNameScope(ns);
	//				return sv;
	//			}),
	//			ItemsPanel = new FuncTemplate<IPanel>(() => isp),
	//			Items = new List<string>
	//			{
	//				"Item1",
	//				"Item2",
	//				"Item3"
	//			}
	//		};

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();
	//		sv.ApplyTemplate();
	//		((ContentPresenter)sv.Presenter).UpdateChild();

	//		return (ic, isp);
	//	}

	//	public (ItemsControl itemsControl, ItemsStackPanel panel) GetItemsControlForLayoutTest(Orientation orientation = Orientation.Vertical)
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Orientation = orientation
	//		};

	//		var sv = new ScrollViewer
	//		{
	//			Template = new FuncControlTemplate<ScrollViewer>(CreateScrollViewerTemplate),
	//			VerticalScrollBarVisibility = orientation == Orientation.Vertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
	//			HorizontalScrollBarVisibility = orientation == Orientation.Horizontal ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,				
	//		};

	//		var ic = new ItemsControl
	//		{
	//			Template = new FuncControlTemplate<ItemsControl>((x, ns) =>
	//			{
	//				sv.Content = new FAItemsPresenter() { Name = "Presenter" }.RegisterInNameScope(ns);
	//				return sv;
	//			}),
	//			ItemsPanel = new FuncTemplate<IPanel>(() => isp),
	//		};

	//		if (orientation == Orientation.Vertical)
	//		{
	//			ic.Items = new List<Control>
	//			{
	//				new Border { Height = 20, Width = 120 },
	//				new Border { Height = 30 },
	//				new Border { Height = 50 }
	//			};
	//		}
	//		else
	//		{
	//			ic.Items = new List<Control>
	//			{
	//				new Border { Height = 20, Width = 120 },
	//				new Border { Height = 30 },
	//				new Border { Height = 50 }
	//			};
	//		}

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();
	//		sv.ApplyTemplate();
	//		((ContentPresenter)sv.Presenter).UpdateChild();

	//		return (ic, isp);
	//	}

	//	public (ItemsControl itemsControl, ItemsStackPanel panel) GetItemsControlForLayoutBoundsTest(Orientation orientation = Orientation.Vertical)
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Orientation = orientation
	//		};

	//		var sv = new ScrollViewer
	//		{
	//			Template = new FuncControlTemplate<ScrollViewer>(CreateScrollViewerTemplate),
	//			VerticalScrollBarVisibility = orientation == Orientation.Vertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
	//			HorizontalScrollBarVisibility = orientation == Orientation.Horizontal ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
	//		};

	//		var ic = new ItemsControl
	//		{
	//			Template = new FuncControlTemplate<ItemsControl>((x, ns) =>
	//			{
	//				sv.Content = new FAItemsPresenter() { Name = "Presenter" }.RegisterInNameScope(ns);
	//				return sv;
	//			}),
	//			ItemsPanel = new FuncTemplate<IPanel>(() => isp),
	//		};

	//		if (orientation == Orientation.Vertical)
	//		{
	//			ic.Items = new List<Control>
	//			{
	//				new Border { Width = 50, Height = 10, HorizontalAlignment = HorizontalAlignment.Left },
	//				new Border { Width = 150, Height = 10, HorizontalAlignment = HorizontalAlignment.Left },
	//				new Border { Width = 50, Height = 10, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 150, Height = 10, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 50, Height = 10, HorizontalAlignment = HorizontalAlignment.Right },
	//				new Border { Width = 150, Height = 10, HorizontalAlignment = HorizontalAlignment.Right },
	//				new Border { Width = 50, Height = 10, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 150, Height = 10, HorizontalAlignment = HorizontalAlignment.Center },
	//			};
	//		}
	//		else
	//		{
	//			ic.Items = new List<Control>
	//			{
	//				new Border { Width = 10, Height = 50, HorizontalAlignment = HorizontalAlignment.Left },
	//				new Border { Width = 10, Height = 150, HorizontalAlignment = HorizontalAlignment.Left },
	//				new Border { Width = 10, Height = 50, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 10, Height = 150, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 10, Height = 50, HorizontalAlignment = HorizontalAlignment.Right },
	//				new Border { Width = 10, Height = 150, HorizontalAlignment = HorizontalAlignment.Right },
	//				new Border { Width = 10, Height = 50, HorizontalAlignment = HorizontalAlignment.Center },
	//				new Border { Width = 10, Height = 150, HorizontalAlignment = HorizontalAlignment.Center },
	//			};
	//		}

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();
	//		sv.ApplyTemplate();
	//		((ContentPresenter)sv.Presenter).UpdateChild();

	//		return (ic, isp);
	//	}


	//	[Fact]
	//	public void LaysOutChildrenVertically()
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Children =
	//			{
	//				new Border { Height = 20, Width = 120 },
	//				new Border { Height = 30 },
	//				new Border { Height = 50 },
	//			}
	//		};

	//		//var root = new TestRoot() { Width = 250, Height = 350 };
	//		//var ic = GetItemsControlForLayoutTest();

	//		//root.Child = ic.itemsControl;

	//		//// Both are required in order to get ScrollViewer to fire EffectiveViewportChanged,
	//		//// which ItemsStackPanel won't work without
	//		//root.LayoutManager.ExecuteInitialLayoutPass();
	//		//root.LayoutManager.ExecuteLayoutPass();

	//		isp.Measure(Size.Infinity);
	//		isp.Arrange(new Rect(isp.DesiredSize));

	//		Assert.Equal(3, isp.Children.Count);

	//		Assert.Equal(new Size(120, 100), isp.Bounds.Size);
	//		Assert.Equal(new Rect(0, 0, 120, 20), isp.Children[0].Bounds);
	//		Assert.Equal(new Rect(0, 20, 120, 30), isp.Children[1].Bounds);
	//		Assert.Equal(new Rect(0, 50, 120, 50), isp.Children[2].Bounds);
	//	}

	//	[Fact]
	//	public void LaysOutChildrenHorizontally()
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Orientation = Orientation.Horizontal,
	//			Children =
	//			{
	//				new Border { Width = 20, Height = 120 },
	//				new Border { Width = 30 },
	//				new Border { Width = 50 },
	//			}
	//		};
	//		//var root = new TestRoot();// { Width = 250, Height = 350 };
	//		//var ic = GetItemsControlForLayoutTest(Orientation.Horizontal);

	//		//root.Child = ic.itemsControl;

	//		// Both are required in order to get ScrollViewer to fire EffectiveViewportChanged,
	//		// which ItemsStackPanel won't work without
	//		//root.LayoutManager.ExecuteInitialLayoutPass();
	//		//root.LayoutManager.ExecuteLayoutPass();

	//		isp.Measure(Size.Infinity);
	//		isp.Arrange(new Rect(isp.DesiredSize));

	//		Assert.Equal(3, isp.Children.Count);

	//		Assert.Equal(new Size(100, 120), isp.Bounds.Size);
	//		Assert.Equal(new Rect(0, 0, 20, 120), isp.Children[0].Bounds);
	//		Assert.Equal(new Rect(20, 0, 30, 120), isp.Children[1].Bounds);
	//		Assert.Equal(new Rect(50, 0, 50, 120), isp.Children[2].Bounds);
	//	}

	//	[Fact]
	//	public void ArrangesVerticalWithCorrectBounds()
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Children =
	//			{
	//				new TestControl { MeasureSize = new Size(50,10), HorizontalAlignment = HorizontalAlignment.Left },
	//				new TestControl { MeasureSize = new Size(150, 10), HorizontalAlignment = HorizontalAlignment.Left },
	//				new TestControl { MeasureSize = new Size(50, 10), HorizontalAlignment = HorizontalAlignment.Center },
	//				new TestControl { MeasureSize = new Size(150, 10), HorizontalAlignment = HorizontalAlignment.Center },
	//				new TestControl { MeasureSize = new Size(50, 10), HorizontalAlignment = HorizontalAlignment.Right },
	//				new TestControl { MeasureSize = new Size(150, 10), HorizontalAlignment = HorizontalAlignment.Right },
	//				new TestControl { MeasureSize = new Size(50, 10), HorizontalAlignment = HorizontalAlignment.Stretch },
	//				new TestControl { MeasureSize = new Size(150, 10), HorizontalAlignment = HorizontalAlignment.Stretch },
	//			}
	//		};

	//		isp.Measure(new Size(100, 150));
	//		Assert.Equal(new Size(100, 80), isp.DesiredSize);

	//		isp.Arrange(new Rect(isp.DesiredSize));

	//		var bounds = isp.Children.Select(x => x.Bounds).ToArray();

	//		Assert.Equal(
	//			new[]
	//			{
	//				new Rect(0, 0, 50, 10),
	//				new Rect(0, 10, 100, 10),
	//				new Rect(25, 20, 50, 10),
	//				new Rect(0, 30, 100, 10),
	//				new Rect(50, 40, 50, 10),
	//				new Rect(0, 50, 100, 10),
	//				new Rect(0, 60, 100, 10),
	//				new Rect(0, 70, 100, 10),

	//			}, bounds);
	//	}

	//	[Fact]
	//	public void ArrangesHorizontalWithCorrectBounds()
	//	{
	//		var isp = new ItemsStackPanel
	//		{
	//			Orientation = Orientation.Horizontal,
	//			Children =
	//			{
	//				new TestControl { MeasureSize = new Size(10,50), VerticalAlignment = VerticalAlignment.Top },
	//				new TestControl { MeasureSize = new Size(10, 150), VerticalAlignment = VerticalAlignment.Top },
	//				new TestControl { MeasureSize = new Size(10, 50), VerticalAlignment = VerticalAlignment.Center },
	//				new TestControl { MeasureSize = new Size(10, 150), VerticalAlignment = VerticalAlignment.Center },
	//				new TestControl { MeasureSize = new Size(10, 50), VerticalAlignment = VerticalAlignment.Bottom },
	//				new TestControl { MeasureSize = new Size(10, 150), VerticalAlignment = VerticalAlignment.Bottom },
	//				new TestControl { MeasureSize = new Size(10, 50), VerticalAlignment = VerticalAlignment.Stretch },
	//				new TestControl { MeasureSize = new Size(10, 150), VerticalAlignment = VerticalAlignment.Stretch },
	//			}
	//		};

	//		isp.Measure(new Size(150, 100));
	//		Assert.Equal(new Size(80, 100), isp.DesiredSize);

	//		isp.Arrange(new Rect(isp.DesiredSize));

	//		var bounds = isp.Children.Select(x => x.Bounds).ToArray();

	//		Assert.Equal(
	//			new[]
	//			{
	//				new Rect(0, 0, 10, 50),
	//				new Rect(10, 0, 10, 100),
	//				new Rect(20, 25, 10, 50),
	//				new Rect(30, 0, 10, 100),
	//				new Rect(40, 50, 10, 50),
	//				new Rect(50, 0, 10, 100),
	//				new Rect(60, 0, 10, 100),
	//				new Rect(70, 0, 10, 100),

	//			}, bounds);
	//	}


	//	private Control CreateScrollViewerTemplate(ScrollViewer control, INameScope scope)
	//	{
	//		return new Grid
	//		{
	//			ColumnDefinitions = new ColumnDefinitions
	//			{
	//				new ColumnDefinition(1, GridUnitType.Star),
	//				new ColumnDefinition(GridLength.Auto),
	//			},
	//			RowDefinitions = new RowDefinitions
	//			{
	//				new RowDefinition(1, GridUnitType.Star),
	//				new RowDefinition(GridLength.Auto),
	//			},
	//			Children =
	//			{
	//				new ScrollContentPresenter
	//				{
	//					Name = "PART_ContentPresenter",
	//					[~ContentPresenter.ContentProperty] = control[~ContentControl.ContentProperty],
	//					[~~ScrollContentPresenter.ExtentProperty] = control[~~ScrollViewer.ExtentProperty],
	//					[~~ScrollContentPresenter.OffsetProperty] = control[~~ScrollViewer.OffsetProperty],
	//					[~~ScrollContentPresenter.ViewportProperty] = control[~~ScrollViewer.ViewportProperty],
	//					[~ScrollContentPresenter.CanHorizontallyScrollProperty] = control[~ScrollViewer.CanHorizontallyScrollProperty],
	//				}.RegisterInNameScope(scope),
	//				new ScrollBar
	//				{
	//					Name = "horizontalScrollBar",
	//					Orientation = Orientation.Horizontal,
	//					[~RangeBase.MaximumProperty] = control[~ScrollViewer.HorizontalScrollBarMaximumProperty],
	//					[~~RangeBase.ValueProperty] = control[~~ScrollViewer.HorizontalScrollBarValueProperty],
	//					[~ScrollBar.ViewportSizeProperty] = control[~ScrollViewer.HorizontalScrollBarViewportSizeProperty],
	//					[~ScrollBar.VisibilityProperty] = control[~ScrollViewer.HorizontalScrollBarVisibilityProperty],
	//					[Grid.RowProperty] = 1,
	//				}.RegisterInNameScope(scope),
	//				new ScrollBar
	//				{
	//					Name = "verticalScrollBar",
	//					Orientation = Orientation.Vertical,
	//					[~RangeBase.MaximumProperty] = control[~ScrollViewer.VerticalScrollBarMaximumProperty],
	//					[~~RangeBase.ValueProperty] = control[~~ScrollViewer.VerticalScrollBarValueProperty],
	//					[~ScrollBar.ViewportSizeProperty] = control[~ScrollViewer.VerticalScrollBarViewportSizeProperty],
	//					[~ScrollBar.VisibilityProperty] = control[~ScrollViewer.VerticalScrollBarVisibilityProperty],
	//					[Grid.ColumnProperty] = 1,
	//				}.RegisterInNameScope(scope),
	//			},
	//		};
	//	}

	//	private class TestControl : Control
	//	{
	//		public Size MeasureSize { get; set; }

	//		protected override Size MeasureOverride(Size availableSize)
	//		{
	//			return MeasureSize;
	//		}
	//	}
	//}
}
