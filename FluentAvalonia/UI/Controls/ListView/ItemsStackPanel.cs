using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.Core.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class ItemsStackPanel : Panel
	{
		public static readonly DirectProperty<ItemsStackPanel, double> CacheLengthProperty =
			AvaloniaProperty.RegisterDirect<ItemsStackPanel, double>(nameof(CacheLength),
				x => x.CacheLength, (x, v) => x.CacheLength = v);

		public static readonly DirectProperty<ItemsStackPanel, GroupHeaderPlacement> GroupHeaderPlacementProperty =
			AvaloniaProperty.RegisterDirect<ItemsStackPanel, GroupHeaderPlacement>(nameof(GroupHeaderPlacement),
				x => x.GroupHeaderPlacement, (x, v) => x.GroupHeaderPlacement = v);

		public static readonly DirectProperty<ItemsStackPanel, Thickness> GroupPaddingProperty =
			AvaloniaProperty.RegisterDirect<ItemsStackPanel, Thickness>(nameof(GroupPadding),
				x => x.GroupPadding, (x, v) => x.GroupPadding = v);

		public static readonly StyledProperty<Orientation> OrientationProperty =
			StackPanel.OrientationProperty.AddOwner<ItemsStackPanel>();

		public static readonly DirectProperty<ItemsStackPanel, bool> AreStickyGroupHeadersEnabledProperty =
			AvaloniaProperty.RegisterDirect<ItemsStackPanel, bool>(nameof(AreStickyGroupHeadersEnabled),
				x => x.AreStickyGroupHeadersEnabled, (x, v) => x.AreStickyGroupHeadersEnabled = v);

		public Orientation Orientation
		{
			get => GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		[NotImplemented]
		public ItemsUpdatingScrollMode ItemsUpdatingScrollMode { get; set; }

		public Thickness GroupPadding
		{
			get => _groupPadding;
			set => SetAndRaise(GroupPaddingProperty, ref _groupPadding, value);
		}

		public GroupHeaderPlacement GroupHeaderPlacement
		{
			get => _groupHeaderPlacement;
			set => SetAndRaise(GroupHeaderPlacementProperty, ref _groupHeaderPlacement, value);
		}

		public double CacheLength
		{
			get => _cacheLength;
			set => SetAndRaise(CacheLengthProperty, ref _cacheLength, value);
		}

		public int FirstCacheIndex { get; private set; }

		public int FirstVisibleIndex { get; private set; }

		public int LastCacheIndex { get; private set; }

		public int LastVisibleIndex { get; private set; }

		[NotImplemented]
		public PaneScrollingDirection ScrollingDirection { get; private set; }

		public bool AreStickyGroupHeadersEnabled
		{
			get => _areStickyGroupHeadersEnabled;
			set => SetAndRaise(AreStickyGroupHeadersEnabledProperty, ref _areStickyGroupHeadersEnabled, value);
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);

			InitializePanel();
		}

		public override void ApplyTemplate()
		{
			base.ApplyTemplate();

			// Only for UnitTests
			if (_context == null)
				InitializePanel();
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);

			EffectiveViewportChanged -= OnEffectiveViewportChanged;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (_context != null && _itemsView != null)
			{
				_context.Measure(availableSize, _itemsView);

				return _context.Extent;
			}

			return Size.Empty;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (_context != null && _itemsView != null)
			{
				_context.Arrange(finalSize, _itemsView);
			}

			return finalSize;
		}

		private void InitializePanel()
		{
			EffectiveViewportChanged += OnEffectiveViewportChanged;

			_itemsControl = this.FindAncestorOfType<ItemsControl>();
			if (_itemsControl == null)
				throw new InvalidOperationException("ItemsStackPanel can only be used inside an ItemsControl");

			_itemsDisposable?.Dispose();
			_itemsDisposable = _itemsControl.GetPropertyChangedObservable(ItemsControl.ItemsProperty).Subscribe(OnItemsChanged);

			_host = this.FindAncestorOfType<FAItemsPresenter>();
			if (_host == null)
				throw new InvalidOperationException("ItemsStackPanel can only be used inside an FAItemsPresenter");

			_itemsView = _itemsControl.Items != null ? new Avalonia.Controls.ItemsSourceView(_itemsControl.Items) :
				Avalonia.Controls.ItemsSourceView.Empty;

			_itemsView.CollectionChanged += OnItemsCollectionChanged;

			_context = new IterableStackingNonVirtualizingContext(this);
		}

		private void OnItemsChanged(AvaloniaPropertyChangedEventArgs obj)
		{
			_itemsView.CollectionChanged -= OnItemsCollectionChanged;

			_itemsView.Dispose();

			_itemsView = obj.NewValue is IEnumerable ie ? new Avalonia.Controls.ItemsSourceView(ie) :
				Avalonia.Controls.ItemsSourceView.Empty;

			_itemsView.CollectionChanged += OnItemsCollectionChanged;

			_context.AreItemsDirty = true;
		}

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_context.UpdateFromItemsChange(e, _itemsView);
		}

		private void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e)
		{
			//var lvi = this.FindAncestorOfType<ListViewItem>();

			//if (lvi != null)
			//	Debug.WriteLine($"{lvi.Content} | {e.EffectiveViewport}");

			//Debug.WriteLine($"{e.EffectiveViewport}");
			_context.Viewport = e.EffectiveViewport;
			InvalidateMeasure();
		}

		private bool _areStickyGroupHeadersEnabled = true;
		private double _cacheLength = 1;
		private Thickness _groupPadding;
		private GroupHeaderPlacement _groupHeaderPlacement;

		private IDisposable _itemsDisposable;
		private Avalonia.Controls.ItemsSourceView _itemsView;
		private FAItemsPresenter _host;
		private ItemsControl _itemsControl;
		private StackingVirtualizingContext _context;

		private abstract class StackingVirtualizingContext
		{
			public StackingVirtualizingContext(ItemsStackPanel owner)
			{
				Owner = owner;
				ItemContainerGenerator = owner._itemsControl.ItemContainerGenerator;
			}

			public ItemsStackPanel Owner { get; }

			public Rect Viewport { get; set; }

			public Size Extent { get; protected set; }

			protected IItemContainerGenerator ItemContainerGenerator { get; }

			public double AverageItemHeight { get; protected set; } = 25;

			// Flag used to determine whether Items property has changed, needing us
			// to regenerate all containers. INCC actions are handled separately
			public bool AreItemsDirty { get; set; } = true;

			protected Size LastMeasureSize { get; set; }

			public abstract Size Measure(Size constraint, Avalonia.Controls.ItemsSourceView itemsView);

			public abstract void Arrange(Size finalSize, Avalonia.Controls.ItemsSourceView itemsView);

			public virtual void UpdateFromItemsChange(NotifyCollectionChangedEventArgs args, Avalonia.Controls.ItemsSourceView itemsView)
			{ }

			protected IControl GetOrCreateElement(int index)
			{
				var cont = ItemContainerGenerator.ContainerFromIndex(index);

				if (cont == null)
				{
					cont = ItemContainerGenerator.Materialize(index, Owner._itemsView.GetAt(index)).ContainerControl;
				}

				return cont;
			}

		}

		private class IterableStackingNonVirtualizingContext : StackingVirtualizingContext
		{
			public IterableStackingNonVirtualizingContext(ItemsStackPanel owner)
				: base(owner) { }

			public override Size Measure(Size constraint, Avalonia.Controls.ItemsSourceView itemsView)
			{
				LastMeasureSize = constraint;

				if (Viewport == Rect.Empty)
					return default;

				if (itemsView.Count == 0)
				{
					Extent = Size.Empty;
				}
				else if (AreItemsDirty)
				{
					if (Owner.Children.Count > 0)
						Owner.Children.Clear();

					bool horizontal = Owner.Orientation == Orientation.Horizontal;
					double stackSize = 0;
					double nonStackSize = 0;
					
					for (int i = 0; i < itemsView.Count; i++)
					{
						var cont = GetOrCreateElement(i);

						// Need to add the control to the tree to measure it
						Owner.Children.Add(cont);

						cont.Measure(constraint);
						var size = cont.DesiredSize;

						if (horizontal)
						{
							stackSize += size.Width;
							nonStackSize = Math.Max(nonStackSize, size.Height);
						}
						else
						{
							stackSize += size.Height;
							nonStackSize = Math.Max(nonStackSize, size.Width);
						}
					}

					Extent = horizontal ? new Size(stackSize, nonStackSize) : new Size(nonStackSize, stackSize);

					Owner.FirstVisibleIndex = Owner.FirstCacheIndex = 0;
					Owner.LastVisibleIndex = Owner.LastCacheIndex = itemsView.Count;

					AreItemsDirty = false;
				}

				return Extent;
			}

			public override void Arrange(Size finalSize, Avalonia.Controls.ItemsSourceView itemsView)
			{
				if (Viewport == Rect.Empty || itemsView.Count == 0)
					return;

				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				Rect rc;
				double x = 0;
				double y = 0;
				for (int i = 0; i < itemsView.Count; i++)
				{
					rc = horizontal ?
						new Rect(x, 0, Owner.Children[i].DesiredSize.Width, finalSize.Height) :
						new Rect(0, y, finalSize.Width, Owner.Children[i].DesiredSize.Height);

					Owner.Children[i].Arrange(rc);

					if (horizontal)
						x += rc.Width;
					else
						y += rc.Height;
				}
			}

			public override void UpdateFromItemsChange(NotifyCollectionChangedEventArgs args, Avalonia.Controls.ItemsSourceView itemsView)
			{
				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				void Add()
				{
					if (args.NewStartingIndex + args.NewItems.Count < itemsView.Count)
					{
						ItemContainerGenerator.InsertSpace(args.NewStartingIndex, args.NewItems.Count);
					}

					int idx = args.NewStartingIndex;
					double addExtent = 0;
					double nonStackSize = horizontal ? Extent.Height : Extent.Width;
					for (int i = 0; i < args.NewItems.Count; i++)
					{
						var cont = GetOrCreateElement(idx);
						Owner.Children.Insert(idx, cont);
						cont.Measure(LastMeasureSize);
						var size = cont.DesiredSize;

						addExtent += horizontal ? size.Width : size.Height;
						nonStackSize = Math.Max(nonStackSize, horizontal ? size.Height : size.Width);

						idx++;
					}

					Extent = horizontal ? new Size(Extent.Width + addExtent, nonStackSize) :
						new Size(nonStackSize, Extent.Height + addExtent);
				}

				void Remove(bool useDematerialize = false)
				{
					if (useDematerialize)
						ItemContainerGenerator.Dematerialize(args.OldStartingIndex, args.OldItems.Count);
					else
						ItemContainerGenerator.RemoveRange(args.OldStartingIndex, args.OldItems.Count);

					int idx = args.OldStartingIndex + args.OldItems.Count - 1;
					double subExtent = 0;
					// To avoid iterating over many items, we'll assume the non-stack size is unchanged
					// Since this panel only stacks, the non-stack size is automatically the Bounds 
					// direction in the non-stacking direction so this won't matter
					for (int i = 0; i < args.OldItems.Count; i ++)
					{
						subExtent += horizontal ? Owner.Children[idx].Bounds.Width :
							Owner.Children[idx].Bounds.Height;

						Owner.Children.RemoveAt(idx);

						idx--;
					}

					Extent = horizontal ? new Size(Extent.Width - subExtent, Extent.Height) :
						new Size(Extent.Width, Extent.Height - subExtent);
				}


				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						Add();
						break;

					case NotifyCollectionChangedAction.Remove:
						Remove();
						break;

					case NotifyCollectionChangedAction.Replace:
						Remove(true);
						Add();
						break;

					case NotifyCollectionChangedAction.Reset:
						ItemContainerGenerator.Clear();
						Owner.Children.Clear();
						AreItemsDirty = true;
						break;

					case NotifyCollectionChangedAction.Move:
						Remove();
						Add();
						break;
				}

				Owner.LastVisibleIndex = Owner.LastCacheIndex = itemsView.Count;				
			}
		}
	}
}
