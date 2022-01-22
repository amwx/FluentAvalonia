using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    // Tests disabled as ListView/GridView project suspended for time being

	//public class FAItemsPresenterTests
	//{
	//	public ItemsControl GetItemsControl()
	//	{
	//		return new ItemsControl
	//		{
	//			Template = new FuncControlTemplate<ItemsControl>((x, ns) =>
	//			{
	//				return new FAItemsPresenter() { Name = "Presenter" }.RegisterInNameScope(ns);
	//			})
	//		};
	//	}
			

	//	[Fact]
	//	public void ShouldAddContainers()
	//	{
	//		var ic = GetItemsControl();
	//		ic.Items = new[] { "Item1", "Item2" };

	//		ic.ApplyTemplate();

	//		Assert.NotNull(ic.Presenter);

	//		ic.Presenter.ApplyTemplate();

	//		Assert.NotNull(ic.Presenter.Panel);
	//		Assert.Equal(2, ic.Presenter.Panel.Children.Count);

	//		Assert.IsType<ContentPresenter>(ic.Presenter.Panel.Children[0]);
	//		Assert.IsType<ContentPresenter>(ic.Presenter.Panel.Children[1]);
	//		Assert.Equal("Item1", ((ContentPresenter)ic.Presenter.Panel.Children[0]).Content);
	//		Assert.Equal("Item2", ((ContentPresenter)ic.Presenter.Panel.Children[1]).Content);
	//	}

	//	[Fact]
	//	public void ShouldCreateContainersOnlyOnce()
	//	{
	//		var ic = GetItemsControl();
	//		ic.Items = new[] { "Item1", "Item2" };

	//		int count = 0;
	//		ic.ItemContainerGenerator.Materialized += (s, e) => count++;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		Assert.Equal(2, ic.Presenter.Panel.Children.Count);
	//		Assert.Equal(2, count);
	//	}

	//	[Fact]
	//	public void ShouldRemoveContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		Assert.Equal(2, ic.Presenter.Panel.Children.Count);

	//		col.RemoveAt(0);
	//		Assert.Single(ic.Presenter.Panel.Children);
	//		Assert.Equal("Item2", (ic.Presenter.Panel.Children[0] as ContentPresenter).Content);
	//	}

	//	[Fact]
	//	public void ClearingItemsShouldClearContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		Assert.Equal(2, ic.Presenter.Panel.Children.Count);

	//		col.Clear();

	//		Assert.Empty(ic.Presenter.Panel.Children);
	//	}

	//	[Fact]
	//	public void ReplacingItemsShouldUpdateContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2",
	//			"Item3"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		col[1] = "Replaced";

	//		Assert.Equal("Replaced", (ic.Presenter.Panel.Children[1] as ContentPresenter).Content);
	//	}

	//	[Fact]
	//	public void MovingItemsShouldUpdateContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2",
	//			"Item3"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		col.Move(0, 2);

	//		Assert.Equal("Item2", (ic.Presenter.Panel.Children[0] as ContentPresenter).Content);
	//		Assert.Equal("Item3", (ic.Presenter.Panel.Children[1] as ContentPresenter).Content);
	//		Assert.Equal("Item1", (ic.Presenter.Panel.Children[2] as ContentPresenter).Content);
	//	}

	//	[Fact]
	//	public void InsertingItemsShouldUpdateContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2",
	//			"Item3"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		col.Insert(1, "Insert");

	//		Assert.Equal("Item1", (ic.Presenter.Panel.Children[0] as ContentPresenter).Content);
	//		Assert.Equal("Insert", (ic.Presenter.Panel.Children[1] as ContentPresenter).Content);
	//		Assert.Equal("Item2", (ic.Presenter.Panel.Children[2] as ContentPresenter).Content);
	//	}

	//	[Fact]
	//	public void SettingItemsToNullShouldRemoveContainers()
	//	{
	//		var col = new ObservableCollection<string>
	//		{
	//			"Item1",
	//			"Item2",
	//			"Item3"
	//		};

	//		var ic = GetItemsControl();
	//		ic.Items = col;

	//		ic.ApplyTemplate();
	//		ic.Presenter.ApplyTemplate();

	//		Assert.Equal(3, ic.Presenter.Panel.Children.Count);

	//		ic.Items = null;

	//		Assert.Empty(ic.Presenter.Panel.Children);
	//	}
	//}
}
