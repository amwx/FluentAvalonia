using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAvalonia.Core.Attributes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Generators;
using System.Reactive.Disposables;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Data;
using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Input;
using System.Collections;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls
{
	public class ItemsWrapGrid : ModernCollectionBasePanel, ILogicalScrollable
	{
		static ItemsWrapGrid()
		{
			OrientationProperty.OverrideDefaultValue<ItemsWrapGrid>(Orientation.Horizontal);
		}

		public static readonly DirectProperty<ItemsWrapGrid, double> CacheLengthProperty =
			ItemsStackPanel.CacheLengthProperty.AddOwner<ItemsWrapGrid>(x => x.CacheLength, 
				(x, v) => x.CacheLength = v);

		public static readonly DirectProperty<ItemsWrapGrid, GroupHeaderPlacement> GroupHeaderPlacementProperty =
			ItemsStackPanel.GroupHeaderPlacementProperty.AddOwner<ItemsWrapGrid>(x => x.GroupHeaderPlacement, 
				(x, v) => x.GroupHeaderPlacement = v);

		public static readonly DirectProperty<ItemsWrapGrid, Thickness> GroupPaddingProperty =
			ItemsStackPanel.GroupPaddingProperty.AddOwner<ItemsWrapGrid>(x => x.GroupPadding, 
				(x, v) => x.GroupPadding = v);

		public static readonly StyledProperty<Orientation> OrientationProperty =
			StackPanel.OrientationProperty.AddOwner<ItemsWrapGrid>();

		public static readonly DirectProperty<ItemsWrapGrid, bool> AreStickyGroupHeadersEnabledProperty =
			ItemsStackPanel.AreStickyGroupHeadersEnabledProperty.AddOwner<ItemsWrapGrid>(x => x.AreStickyGroupHeadersEnabled, 
				(x, v) => x.AreStickyGroupHeadersEnabled = v);

		public static readonly StyledProperty<int> MaximumsRowsOrColumnsProperty =
			UniformGridLayout.MaximumRowsOrColumnsProperty.AddOwner<ItemsWrapGrid>();

		public static readonly StyledProperty<double> ItemWidthProperty =
			AvaloniaProperty.Register<ItemsWrapGrid, double>(nameof(ItemWidth), double.NaN);

		public static readonly StyledProperty<double> ItemHeightProperty =
			AvaloniaProperty.Register<ItemsWrapGrid, double>(nameof(ItemHeight), double.NaN);

		public Orientation Orientation
		{
			get => GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		[NotImplemented] // Future TODO
		public Thickness GroupPadding
		{
			get => _groupPadding;
			set => SetAndRaise(GroupPaddingProperty, ref _groupPadding, value); 
		}

		[NotImplemented] // Future TODO
		public GroupHeaderPlacement GroupHeaderPlacement
		{
			get => _groupHeaderPlacement;
			set => SetAndRaise(GroupHeaderPlacementProperty, ref _groupHeaderPlacement, value);
		}

		// Will be implemented with Virtualization
		public double CacheLength
		{
			get => _cacheLength;
			set => SetAndRaise(CacheLengthProperty, ref _cacheLength, value);
		}

		[NotImplemented]
		public PaneScrollingDirection ScrollingDirection { get; private set; }

		public bool AreStickyGroupHeadersEnabled
		{
			get => _areStickyGroupHeadersEnabled;
			set => SetAndRaise(AreStickyGroupHeadersEnabledProperty, ref _areStickyGroupHeadersEnabled, value);
		}

		public int MaximumRowsOrColumns
		{
			get => GetValue(MaximumsRowsOrColumnsProperty);
			set => SetValue(MaximumsRowsOrColumnsProperty, value);
		}

		public double ItemWidth
		{
			get => GetValue(ItemWidthProperty);
			set => SetValue(ItemWidthProperty, value);
		}

		public double ItemHeight
		{
			get => GetValue(ItemHeightProperty);
			set => SetValue (ItemHeightProperty, value);
		}

		public override void ApplyTemplate()
		{
			base.ApplyTemplate();

			if (_context == null)
				InitializePanel();

			if (_effectiveViewportChangedToken == null)
			{
				EffectiveViewportChanged += OnEffectiveViewportChanged;
				_effectiveViewportChangedToken = Disposable.Create(() => EffectiveViewportChanged -= OnEffectiveViewportChanged);
			}
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnAttachedToVisualTree(e);
			InitializePanel();
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);

			_effectiveViewportChangedToken?.Dispose();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (_context != null)
			{
				_context.LastPresenterHeaderSize = (Parent as FAItemsPresenter)?.GetHeaderSize() ?? Size.Empty;

				_context.Measure(availableSize);

				return _context.PanelExtent;
			}

			return Size.Empty;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (_context != null)
			{
				_context.Arrange(finalSize);
			}

			return finalSize;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == ItemWidthProperty)
			{
				if (_context != null)
				{
					_context.ItemWidth = change.GetNewValue<double>();
					InvalidateMeasure();
				}
			}
			else if (change.Property == ItemHeightProperty)
			{
				if (_context != null)
				{
					_context.ItemHeight = change.GetNewValue<double>();
					InvalidateMeasure();
				}
			}
			else if (change.Property == MaximumsRowsOrColumnsProperty)
			{
				if (_context != null)
				{
					_context.MaxRowsOrColumns = change.GetNewValue<int>();
					InvalidateMeasure();
				}
			}
		}

		private void InitializePanel()
		{
			_context = null;

			_itemsControl = this.FindAncestorOfType<ItemsControl>();
			if (_itemsControl == null)
			{
				// Log warning about ItemsWrapGrid's use outside of ItemsControl
			}

			if (_itemsControl != null)
			{
				_groupStyleDisposable?.Dispose();
				_itemsDisposable?.Dispose();

				_itemsDisposable = _itemsControl.GetPropertyChangedObservable(ItemsControl.ItemsProperty).Subscribe(OnItemsChanged);

				_host = this.FindAncestorOfType<FAItemsPresenter>();
				if (_host == null)
					throw new InvalidOperationException("ItemsWrapGrid can only be used inside an FAItemsPresenter");

				if (_itemsControl.Items is ICollectionView view && view.CollectionGroups != null)
				{
					_context = new GroupedWrappingVirtualizingContext(this, _itemsControl.Items != null ?
							new Avalonia.Controls.ItemsSourceView(_itemsControl.Items) : Avalonia.Controls.ItemsSourceView.Empty, view);
				}
				else
				{
					_context = new IterableWrappingVirtualizingContext(this, _itemsControl.Items != null ? 
						new Avalonia.Controls.ItemsSourceView(_itemsControl.Items) :
						Avalonia.Controls.ItemsSourceView.Empty);

					_context.ItemWidth = ItemWidth;
					_context.ItemHeight = ItemHeight;
					_context.MaxRowsOrColumns = MaximumRowsOrColumns;
				}

				_currentGroupStyle = FAItemsControl.GetGroupStyle(_itemsControl);

				_groupStyleDisposable = _itemsControl.GetPropertyChangedObservable(FAItemsControl.GroupStyleProperty)
					.Subscribe(OnGroupStyleChanged);
			}

			// Usually ItemsWrapGrid isn't allowed outside of an ItemsControl, but for unit tests sake 
			// enforcing that has been removed. Create a non-virtualzing context if we aren't the
			// panel inside of an ItemsControl
			if (_context == null)
			{ 
				_context = new IterableWrappingVirtualizingContext(this, Avalonia.Controls.ItemsSourceView.Empty);
				_context.ItemWidth = ItemWidth;
				_context.ItemHeight = ItemHeight;
				_context.MaxRowsOrColumns = MaximumRowsOrColumns;
			}
		}

		private void OnGroupStyleChanged(AvaloniaPropertyChangedEventArgs args)
		{
			_currentGroupStyle = args.NewValue as GroupStyle;
		}

		private void OnItemsChanged(AvaloniaPropertyChangedEventArgs args)
		{
			_context?.Dispose();

			var newItems = _itemsControl.Items as IEnumerable;

			if (newItems is ICollectionView view && view.CollectionGroups != null)
			{
				_context = new GroupedWrappingVirtualizingContext(this, newItems != null ?
					new Avalonia.Controls.ItemsSourceView(newItems) : Avalonia.Controls.ItemsSourceView.Empty, view);
			}
			else
			{
				_context = new IterableWrappingVirtualizingContext(this, newItems != null ?
					new Avalonia.Controls.ItemsSourceView(newItems) : Avalonia.Controls.ItemsSourceView.Empty);
			}
		}

		private void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs args)
		{
			if (_context != null)
			{ 
				_context.Viewport = args.EffectiveViewport;
				InvalidateMeasure();
			}
		}

		bool ILogicalScrollable.CanHorizontallyScroll
		{
			get => Orientation == Orientation.Vertical;
			set { }
		}

		bool ILogicalScrollable.CanVerticallyScroll
		{
			get => Orientation == Orientation.Horizontal;
			set { }
		}

		bool ILogicalScrollable.IsLogicalScrollEnabled => true;

		Size ILogicalScrollable.ScrollSize => Orientation == Orientation.Vertical ?
			new(_context.ItemWidth, 0) : new(0, _context.ItemHeight);

		Size ILogicalScrollable.PageScrollSize => _context.Viewport.Size;

		Size IScrollable.Extent => _context.Extent;

		Vector IScrollable.Offset
		{
			get => _context.Offset;
			set
			{
				if (value != _context.Offset)
				{
					_context.Offset = value;
					InvalidateMeasure();
				}
			}
		}

		Size IScrollable.Viewport => _context?.Viewport.Size ?? Size.Empty;

		event EventHandler ILogicalScrollable.ScrollInvalidated
		{
			add { }
			remove { }
		}

		bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect) => false;

		IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl from) =>
			null;

		void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e) =>
			(_host as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);


		private bool _areStickyGroupHeadersEnabled;
		private double _cacheLength = 1;
		private GroupHeaderPlacement _groupHeaderPlacement;
		private Thickness _groupPadding;
		private IDisposable _effectiveViewportChangedToken;
		private IDisposable _groupStyleDisposable;
		private IDisposable _itemsDisposable;

		private GroupStyle _currentGroupStyle;

		private ItemsControl _itemsControl;
		private FAItemsPresenter _host;

		private WrappingVirtualizingContext _context;

		private abstract class WrappingVirtualizingContext : IDisposable
		{
			public WrappingVirtualizingContext(ItemsWrapGrid owner, Avalonia.Controls.ItemsSourceView itemsView)
			{
				Owner = owner;
				ItemContainerGenerator = owner._itemsControl?.ItemContainerGenerator;
				ItemsView = itemsView;

				_itemsViewDisposable = itemsView.GetWeakCollectionChangedObservable().Subscribe(OnItemsChanged);
			}

			public ItemsWrapGrid Owner { get; }

			protected IItemContainerGenerator ItemContainerGenerator { get; }

			protected Avalonia.Controls.ItemsSourceView ItemsView { get; }

			public Rect Viewport
			{
				get => _viewport;
				set
				{
					if (_viewport != value)
					{
						_viewport = value;
						(Owner as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);
					}
				}
			}

			public Size Extent { get; protected set; }

			public Size PanelExtent { get; protected set; } // Size of panel only, Extent tracks panel + header/footer for scrolling

			public Vector Offset { get; set; }

			public double ItemWidth { get; set; } = double.NaN;

			public double ItemHeight { get; set; } = double.NaN;

			public int MaxRowsOrColumns { get; set; }

			protected Size LastMeasureSize { get; set; }

			public Size LastPresenterHeaderSize { get; set; }

			public Size LastPresenterFooterSize { get; set; }

			public bool AreItemsDirty { get; set; } = true;

			public abstract Size Measure(Size constraint);

			public abstract void Arrange(Size finalSize);

			protected abstract void OnItemsChanged(NotifyCollectionChangedEventArgs args);

			protected IControl GetOrCreateElement(int index)
			{
				var cont = ItemContainerGenerator.ContainerFromIndex(index);

				if (cont == null)
				{
					cont = ItemContainerGenerator.Materialize(index, ItemsView.GetAt(index)).ContainerControl;
				}

				return cont;
			}

			protected void UpdateExtent(Size itemsExtent, bool horizontal)
			{
				var header = Owner._host.GetHeaderSize();
				var footer = Owner._host.GetFooterSize();
				if (horizontal)
				{
					Extent = new Size(Math.Max(itemsExtent.Width, Math.Max(header.Width, footer.Width)),
						itemsExtent.Height + header.Height + footer.Height);
				}
				else
				{
					Extent = new Size(itemsExtent.Width + header.Width + footer.Width,
						Math.Max(itemsExtent.Height, Math.Max(header.Height, footer.Height)));
				}
				PanelExtent = itemsExtent;

				((ILogicalScrollable)Owner)?.RaiseScrollInvalidated(EventArgs.Empty);
			}

			protected void SetItemSizeFromFirstItem()
			{
				if (ItemsView.Count == 0 || (!double.IsNaN(ItemWidth) && !double.IsNaN(ItemHeight)))
					return;

				var cont = ItemContainerGenerator.Materialize(0, ItemsView.GetAt(0)).ContainerControl;
				Owner.Children.Add(cont);
				cont.Measure(Size.Infinity);

				if (double.IsNaN(ItemWidth))
				{
					ItemWidth = cont.DesiredSize.Width;
				}

				if (double.IsNaN(ItemHeight))
				{
					ItemHeight = cont.DesiredSize.Height;
				}

				// Definitly not the best but avoid complications later with calling materialize on an item that's
				// already been materialized. With ListViewBaseICG, this will just move this to the recycle queue

				Owner.Children.Remove(cont);
				ItemContainerGenerator.Dematerialize(0, 1);
			}

			public virtual void Dispose()
			{
				_itemsViewDisposable?.Dispose();
			}

			private IDisposable _itemsViewDisposable;
			private Rect _viewport;
		}

		private class IterableWrappingVirtualizingContext : WrappingVirtualizingContext
		{
			public IterableWrappingVirtualizingContext(ItemsWrapGrid owner, Avalonia.Controls.ItemsSourceView itemsView)
				: base(owner, itemsView)
			{
			}

			public override Size Measure(Size constraint)
			{
				if (Viewport == Rect.Empty)
					return Size.Empty; // Don't do anything until we get a valid viewport

				LastMeasureSize = constraint;
				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				if (AreItemsDirty && ItemsView.Count > 0)
				{
					if (Owner.Children.Count == 0)
						Owner.Children.Clear();

					
					// In WinUI, if the ItemWidth or ItemHeight isn't set, then it defaults
					// to the size of the first item
					SetItemSizeFromFirstItem();

					int invisibleCount = 0;
					for(int i = 0; i < ItemsView.Count; i++)
					{
						var cont = GetOrCreateElement(i);
						Owner.Children.Add(cont);

						if (!cont.IsVisible)
							invisibleCount++;

						cont.Measure(new Size(ItemWidth, ItemHeight));
					}

					if (horizontal)
					{
						if (Owner.MaximumRowsOrColumns == 0)
						{
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Width / ItemWidth);
						}
						else
						{
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
						}

						var rows = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							rows++;

						UpdateExtent(new Size(ItemWidth * MaxRowsOrColumns, rows * ItemHeight), true);
					}
					else
					{
						if (Owner.MaximumRowsOrColumns == 0)
						{
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
						}
						else
						{
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
						}

						var cols = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							cols++;

						UpdateExtent(new Size(ItemWidth * cols, ItemHeight * MaxRowsOrColumns), true);
					}

					Owner.FirstVisibleIndex = Owner.FirstCacheIndex = 0;
					Owner.LastVisibleIndex = Owner.LastCacheIndex = ItemsView.Count;
					AreItemsDirty = false;
				}
				else
				{
					int invisibleCount = 0;
					for (int i = 0; i < Owner.Children.Count; i++)
					{
						if (!Owner.Children[i].IsVisible)
							invisibleCount++;

						Owner.Children[i].Measure(constraint);
					}

					Size newExtent;
					if (horizontal)
					{
						if (Owner.MaximumRowsOrColumns == 0)
						{
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Width / ItemWidth);
						}
						else
						{
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
						}

						var rows = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							rows++;

						newExtent = new Size(ItemWidth * MaxRowsOrColumns, rows * ItemHeight);
					}
					else
					{
						if (Owner.MaximumRowsOrColumns == 0)
						{
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
						}
						else
						{
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
						}

						var cols = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							cols++;

						newExtent = new Size(ItemWidth * cols, ItemHeight * MaxRowsOrColumns);
					}

					if (newExtent != Extent)
					{
						UpdateExtent(newExtent, false);
					}
				}

				return Extent;
			}

			public override void Arrange(Size finalSize)
			{
				if (Owner.Children.Count == 0)
					return;

				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				Rect rc;
				double x = 0;
				double y = 0;
				int rowOrCol = 0;
				for (int i = 0; i < Owner.Children.Count; i++)
				{
					if (!Owner.Children[i].IsVisible)
						continue;

					rc = new Rect(x, y, ItemWidth, ItemHeight);
					Owner.Children[i].Arrange(rc);
					rowOrCol++;

					if (horizontal)
					{
						x += ItemWidth;
						if (rowOrCol >= MaxRowsOrColumns)
						{
							x = 0;
							y += ItemHeight;
							rowOrCol = 0;
						}
					}
					else
					{
						y += ItemHeight;
						if (rowOrCol >= MaxRowsOrColumns)
						{
							y = 0;
							x += ItemWidth;
							rowOrCol = 0;
						}
					}
				}
			}

			protected override void OnItemsChanged(NotifyCollectionChangedEventArgs args)
			{
				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				void Add(bool insertSpace = true)
				{
					if (args.NewStartingIndex == 0)
						SetItemSizeFromFirstItem();

					if (insertSpace)
					{
						if (args.NewStartingIndex + args.NewItems.Count < ItemsView.Count)
						{
							ItemContainerGenerator.InsertSpace(args.NewStartingIndex, args.NewItems.Count);
						}
					}

					int idx = args.NewStartingIndex;
					int invisibleCount = 0;
					for (int i = 0; i < args.NewItems.Count; i++)
					{
						var cont = GetOrCreateElement(idx);
						Owner.Children.Insert(idx, cont);
						cont.Measure(LastMeasureSize);

						if (!cont.IsVisible)
							invisibleCount++;

						idx++;
					}

					if (horizontal)
					{
						if (Owner.MaximumRowsOrColumns == 0)
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Width / ItemWidth);
						else
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;

						var rows = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							rows++;

						UpdateExtent(new Size(ItemWidth * MaxRowsOrColumns, rows * ItemHeight), true);
					}
					else
					{
						if (Owner.MaximumRowsOrColumns == 0)
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
						else
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;

						var cols = Math.DivRem(ItemsView.Count - invisibleCount, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							cols++;

						UpdateExtent(new Size(ItemWidth * cols, ItemHeight * MaxRowsOrColumns), true);
					}
				}

				void Remove(bool useDematerialize = false)
				{
					if (useDematerialize)
						ItemContainerGenerator.Dematerialize(args.OldStartingIndex, args.OldItems.Count);
					else
						ItemContainerGenerator.RemoveRange(args.OldStartingIndex, args.OldItems.Count);

					int idx = args.OldStartingIndex + args.OldItems.Count - 1;
					for (int i = 0; i <args.OldItems.Count; i++)
					{
						Owner.Children.RemoveAt(idx);

						idx--;
					}

					// TODO: This won't handle invisible items, need to store that information...

					if (args.OldStartingIndex == 0)
						SetItemSizeFromFirstItem();

					if (horizontal)
					{
						if (Owner.MaximumRowsOrColumns == 0)
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Width / ItemWidth);
						else
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;

						var rows = Math.DivRem(ItemsView.Count, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							rows++;

						UpdateExtent(new Size(ItemWidth * MaxRowsOrColumns, rows * ItemHeight), true);
					}
					else
					{
						if (Owner.MaximumRowsOrColumns == 0)
							MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
						else
							MaxRowsOrColumns = Owner.MaximumRowsOrColumns;

						var cols = Math.DivRem(ItemsView.Count, MaxRowsOrColumns, out int rem);
						if (rem != 0)
							cols++;

						UpdateExtent(new Size(ItemWidth * cols, ItemHeight * MaxRowsOrColumns), true);
					}
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

				Owner.LastVisibleIndex = Owner.LastCacheIndex = ItemsView.Count;
			}
		}

		private class GroupedWrappingVirtualizingContext : WrappingVirtualizingContext
		{
			public GroupedWrappingVirtualizingContext(ItemsWrapGrid owner, Avalonia.Controls.ItemsSourceView itemsView, ICollectionView view)
				:base (owner, itemsView)
			{
				CollectionView = view;
			}

			public ICollectionView CollectionView { get; }

			private double GroupHeaderHeight { get; set; }// Used only in vertical mode

			public override Size Measure(Size constraint)
			{
				if (Viewport == Rect.Empty)
					return Size.Empty; // Don't do anything until we get a valid viewport

				LastMeasureSize = constraint;
				bool horizontal = Owner.Orientation == Orientation.Horizontal;
				if (_groupHeaderContainers == null)
					_groupHeaderContainers = new List<IControl>();

				if (AreItemsDirty && ItemsView.Count > 0)
				{
					if (Owner.Children.Count == 0)
						Owner.Children.Clear();
										
					_groupHeaderContainers.Clear();

					// In WinUI, if the ItemWidth or ItemHeight isn't set, then it defaults
					// to the size of the first item
					SetItemSizeFromFirstItem();

					if (horizontal)
					{
						MeasureHorizontal(constraint, true);
					}
					else
					{
						MeasureVertical(constraint, true);
					}

					Owner.FirstVisibleIndex = Owner.FirstCacheIndex = 0;
					Owner.LastVisibleIndex = Owner.LastCacheIndex = ItemsView.Count;
					AreItemsDirty = false;
				}
				else
				{
					if (horizontal)
					{
						MeasureHorizontal(constraint, false);
					}
					else
					{
						MeasureVertical(constraint, false);
					}
				}

				return Extent;
			}

			private void MeasureHorizontal(Size constraint, bool addingContainers)
			{
				double headerHeight = 0;
				double headerWidth = 0;
				double itemsHeight = 0;
				int idx = 0;

				if (MaxRowsOrColumns == 0)
				{
					if (Owner.MaximumRowsOrColumns == 0)
						MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
					else
						MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
				}

				for (int i = 0; i < CollectionView.CollectionGroups.Count; i++)
				{
					IControl cont;
					if (addingContainers)
					{
						cont = CreateGroupHeader(i, CollectionView.CollectionGroups[i].Group);
						_groupHeaderContainers.Add(cont);
						Owner.Children.Insert(i, cont);
					}
					else
					{
						cont = _groupHeaderContainers[i];
					}
					
					cont.Measure(constraint);

					headerHeight += cont.DesiredSize.Height;
					headerWidth = Math.Max(headerHeight, cont.DesiredSize.Width);


					var count = CollectionView.CollectionGroups[i].GroupItems.Count;
					int invisibleCount = 0;
					for (int j = 0; j < count; j++)
					{
						IControl element;
						if (addingContainers)
						{
							element = GetOrCreateElement(idx);
							Owner.Children.Add(element);
							element.Measure(constraint);
						}
						else
						{
							element = Owner.Children[_groupHeaderContainers.Count + idx];
						}						

						if (!element.IsVisible)
							invisibleCount++;

						idx++;
					}

					var groupRows = Math.DivRem(count - invisibleCount, MaxRowsOrColumns, out int rem);
					if (rem != 0)
						groupRows++;

					itemsHeight += groupRows * ItemHeight;					
				}

				UpdateExtent(new Size(Math.Max(headerWidth, ItemWidth * MaxRowsOrColumns), itemsHeight + headerHeight), true);
			}

			private void MeasureVertical(Size constraint, bool addingContainers)
			{
				double headerHeight = 0;
				double headerWidth = 0;
				double itemsWidth = 0;

				if (MaxRowsOrColumns == 0)
				{
					if (Owner.MaximumRowsOrColumns == 0)
						MaxRowsOrColumns = (int)Math.Floor(Viewport.Height / ItemHeight);
					else
						MaxRowsOrColumns = Owner.MaximumRowsOrColumns;
				}

				int idx = 0;
				int gCount = CollectionView.CollectionGroups.Count;
				for (int i = 0; i < gCount; i++)
				{
					IControl cont;
					if (addingContainers)
					{
						cont = CreateGroupHeader(i, CollectionView.CollectionGroups[i].Group);
						_groupHeaderContainers.Add(cont);
						Owner.Children.Insert(i, cont);
					}
					else
					{
						cont = _groupHeaderContainers[i];
					}

					cont.Measure(constraint);

					// Group headers are arrange horizontally, so we need max height and their width

					headerHeight = Math.Max(headerHeight, cont.DesiredSize.Height);
					headerWidth += cont.DesiredSize.Width;

					var count = CollectionView.CollectionGroups[i].GroupItems.Count;
					int invisibleCount = 0;
					for (int j = 0; j < count; j++)
					{
						IControl element;
						if (addingContainers)
						{
							element = GetOrCreateElement(idx);
							Owner.Children.Add(element);
							element.Measure(constraint);
						}
						else
						{
							element = Owner.Children[_groupHeaderContainers.Count + idx];
						}

						if (!element.IsVisible)
							invisibleCount++;

						idx++;
					}

					var groupCols = Math.DivRem(count - invisibleCount, MaxRowsOrColumns, out int rem);
					if (rem != 0)
						groupCols++;

					var groupWidth = groupCols * ItemWidth;
					bool isHeaderLonger = _groupHeaderContainers[i].DesiredSize.Width > groupWidth;

					itemsWidth += (isHeaderLonger ? _groupHeaderContainers[i].DesiredSize.Width : groupWidth);
				}

				UpdateExtent(new Size(itemsWidth, MaxRowsOrColumns * ItemHeight), false);
				// Save the tallest group header as that's gives our placement for starting when arranging
				GroupHeaderHeight = headerHeight;
			}

			public override void Arrange(Size finalSize)
			{
				if (Owner.Children.Count == 0)
					return;

				// Am going to keep Sticky headers disabled for now, will add later when I do virtualization

				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				if (horizontal)
				{
					ArrangeHorizontalNormal(finalSize);
				}
				else
				{
					ArrangeVerticalNormal(finalSize);
				}
			}

			private void ArrangeHorizontalNormal(Size finalSize)
			{
				Rect rc;
				double x = 0;
				double y = 0;
				int col = 0;
				int groupCount = CollectionView.CollectionGroups.Count;
				int idx = groupCount;
				for (int i = 0; i < groupCount; i++)
				{
					rc = new Rect(0, y, finalSize.Width, _groupHeaderContainers[i].DesiredSize.Height);
					_groupHeaderContainers[i].Arrange(rc);
					y += rc.Height;

					int count = CollectionView.CollectionGroups[i].GroupItems.Count;
					for (int j = 0; j < count; j++)
					{
						rc = new Rect(x, y, ItemWidth, ItemHeight);
						Owner.Children[idx++].Arrange(rc);
						x += ItemWidth;
						col++;

						if (col >= MaxRowsOrColumns && j != count - 1)
						{
							x = 0;
							y += ItemHeight;
							col = 0;
						}
					}

					y += ItemHeight;
					x = 0;
					col = 0;
				}
			}
			
			private void ArrangeVerticalNormal(Size finalSize)
			{
				Rect rc;
				double x = 0;
				double y = 0;
				int row = 0;
				int groupCount = CollectionView.CollectionGroups.Count;
				int idx = groupCount;

				for (int i = 0; i < groupCount; i++)
				{
					rc = new Rect(x, y, _groupHeaderContainers[i].DesiredSize.Width, _groupHeaderContainers[i].DesiredSize.Height);
					_groupHeaderContainers[i].Arrange(rc);

					y = GroupHeaderHeight;

					int count = CollectionView.CollectionGroups[i].GroupItems.Count;
					for (int j = 0; j < count; j++)
					{
						rc = new Rect(x, y, ItemWidth, ItemHeight);
						Owner.Children[idx++].Arrange(rc);
						y += ItemHeight;
						row++;

						if (row >= MaxRowsOrColumns && j != count - 1)
						{
							x += ItemWidth;
							y = GroupHeaderHeight;
							row = 0;
						}
					}

					y = 0;
					x += ItemWidth;
					row = 0;

					// If our group is shorter than our group header, move to where the next group should start
					if (_groupHeaderContainers[i].Bounds.Right > x)
						x = _groupHeaderContainers[i].Bounds.Right;
				}

			}

			protected override void OnItemsChanged(NotifyCollectionChangedEventArgs args)
			{
				// For now, just completely clear & remeasure
				_groupHeaderContainers.Clear();
				ItemContainerGenerator.Clear();
				Owner.Children.Clear();
				AreItemsDirty = true;
				Owner.InvalidateMeasure();
			}

			private IControl CreateGroupHeader(int index, object group)
			{
				if (_groupHeaderContainers == null)
					_groupHeaderContainers = new List<IControl>();
				// Keep GroupHeader order in sync with container list

				IControl container = null;
				if (Owner._itemsControl is ListViewBase lvb)
				{
					// We'll fire this event even if we're not virtualizing, since technically this is 
					// still an ItemsStackPanel
					var args = new ChoosingGroupHeaderContainerEventArgs
					{
						GroupIndex = index,
						Group = group
					};

					lvb.RaiseChoosingGroupHeaderContainerEvent(args);

					container = args.GroupHeaderContainer;

					if (container == null)
						container = lvb.GetGroupContainerForItem(index, group);
				}
				else
				{
					container = new ContentControl();
				}

				if (container is ContentControl cc)
				{
					cc.Content = group;
					cc.ContentTemplate = Owner.FindDataTemplate(group,
						Owner._currentGroupStyle?.HeaderTemplate ??
						Owner._currentGroupStyle?.HeaderTemplateSelector?.SelectTemplate(group, container)) ??
						FuncDataTemplate.Default;

					cc.DataContext = group;
				}
				else
				{
					container.DataContext = group;
				}
				return container;
			}

			private List<IControl> _groupHeaderContainers;
		}
	}
}
