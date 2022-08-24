using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.Core.Attributes;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvalonia.UI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls
{
	public class ItemsStackPanel : ModernCollectionBasePanel, ILogicalScrollable
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
			if (_context != null)
			{
				_context.LastPresenterHeaderSize = (Parent as FAItemsPresenter)?.GetHeaderSize() ?? Size.Empty;

				_context.Measure(availableSize, _itemsView);

				return _context.Extent;
			}

			return Size.Empty;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (_context != null)
			{
				_context.Arrange(finalSize, _itemsView);
			}

			return finalSize;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == AreStickyGroupHeadersEnabledProperty)
			{
				InvalidateMeasure();
			}
		}

		private void InitializePanel()
		{
			_context = null;
			EffectiveViewportChanged += OnEffectiveViewportChanged;

			_itemsControl = this.FindAncestorOfType<ItemsControl>();
			if (_itemsControl == null)
			{
				// Log warning about ItemsStackPanel's use outside of ItemsControl
			}		

			if (_itemsControl != null)
			{
				_groupStyleDisposable?.Dispose();
				_itemsDisposable?.Dispose();
				_itemsDisposable = _itemsControl.GetPropertyChangedObservable(ItemsControl.ItemsProperty).Subscribe(OnItemsChanged);

				_host = this.FindAncestorOfType<FAItemsPresenter>();
				if (_host == null)
					throw new InvalidOperationException("ItemsStackPanel can only be used inside an FAItemsPresenter");

				_itemsView = _itemsControl.Items != null ? new Avalonia.Controls.ItemsSourceView(_itemsControl.Items) :
					Avalonia.Controls.ItemsSourceView.Empty;

				_itemsView.CollectionChanged += OnItemsCollectionChanged;


				if (_itemsControl.Items is ICollectionView view && view.CollectionGroups != null)
				{
					_context = new GroupedStackingNonVirtualizingContext(this, view);
				}
				else
				{
					_context = new IterableStackingNonVirtualizingContext(this);
				}

				_currentGroupStyle = FAItemsControl.GetGroupStyle(_itemsControl);

				_groupStyleDisposable = _itemsControl.GetPropertyChangedObservable(FAItemsControl.GroupStyleProperty)
					.Subscribe(OnGroupStyleChanged);
			}

			// Usually ItemsStackPanel isn't allowed outside of an ItemsControl, but for unit tests sake 
			// enforcing that has been removed. Create a non-virtualzing context if we aren't the
			// panel inside of an ItemsControl
			if (_context == null)
				_context = new IterableStackingNonVirtualizingContext(this);			
		}

		private void OnGroupStyleChanged(AvaloniaPropertyChangedEventArgs args)
		{
			_currentGroupStyle = args.NewValue as GroupStyle;
		}

		private void OnItemsChanged(AvaloniaPropertyChangedEventArgs obj)
		{
			_itemsView.CollectionChanged -= OnItemsCollectionChanged;

			_itemsView.Dispose();

			var newItems = obj.NewValue as IEnumerable;

			_itemsView = newItems != null ? new Avalonia.Controls.ItemsSourceView(newItems) :
				Avalonia.Controls.ItemsSourceView.Empty;

			if (newItems is ICollectionView view && view.CollectionGroups != null)
			{
				_context = new GroupedStackingNonVirtualizingContext(this, view);
			}
			else
			{
				_context = new IterableStackingNonVirtualizingContext(this);
			}

			_itemsView.CollectionChanged += OnItemsCollectionChanged;

			_context.AreItemsDirty = true;
		}

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_context.UpdateFromItemsChange(e, _itemsView);
		}

		private void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e)
		{
			_context.Viewport = e.EffectiveViewport;
			InvalidateMeasure();
		}
		bool ILogicalScrollable.CanHorizontallyScroll
		{
			get => Orientation == Orientation.Horizontal;
			set { }
		}

		bool ILogicalScrollable.CanVerticallyScroll
		{
			get => Orientation == Orientation.Vertical;
			set { }
		}

		bool ILogicalScrollable.IsLogicalScrollEnabled => true;

		Size ILogicalScrollable.ScrollSize => Orientation == Orientation.Horizontal ?
			new(_context.AverageItemHeight, 0) : new(0, _context.AverageItemHeight);

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

		Size IScrollable.Viewport => _context.Viewport.Size;

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
		//ScrollInvalidated?.Invoke(this, e);


		private bool _areStickyGroupHeadersEnabled = true;
		private double _cacheLength = 1;
		private Thickness _groupPadding;
		private GroupHeaderPlacement _groupHeaderPlacement;

		private IDisposable _itemsDisposable, _groupStyleDisposable;
		private Avalonia.Controls.ItemsSourceView _itemsView = Avalonia.Controls.ItemsSourceView.Empty;
		private FAItemsPresenter _host;
		private ItemsControl _itemsControl;
		private StackingVirtualizingContext _context;
		private GroupStyle _currentGroupStyle;

		private abstract class StackingVirtualizingContext : IDisposable
		{
			public StackingVirtualizingContext(ItemsStackPanel owner)
			{
				Owner = owner;
				ItemContainerGenerator = owner._itemsControl?.ItemContainerGenerator;
			}

			public ItemsStackPanel Owner { get; }

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

			public Vector Offset { get; set; }

			protected IItemContainerGenerator ItemContainerGenerator { get; }

			public double AverageItemHeight { get; protected set; } = 25;

			// Flag used to determine whether Items property has changed, needing us
			// to regenerate all containers. INCC actions are handled separately
			public bool AreItemsDirty { get; set; } = true;

			protected Size LastMeasureSize { get; set; }

			public Size LastPresenterHeaderSize { get; set; }

			public Size LastPresenterFooterSize { get; set; }

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

			protected void UpdateExtent(Size itemsExtent, bool horizontal)
			{
				if (Owner._host == null) // Unit Tests only
				{
					Extent = itemsExtent;
					return;
				}

				var header = Owner._host.GetHeaderSize();
				var footer = Owner._host.GetFooterSize();
				if (horizontal)
				{
					Extent = new Size(itemsExtent.Width + header.Width + footer.Width, 
						Math.Max(itemsExtent.Height, Math.Max(header.Height, footer.Height)));
				}
				else
				{
					Extent = new Size(Math.Max(itemsExtent.Width, Math.Max(header.Width, footer.Width)), 
						itemsExtent.Height + header.Height + footer.Height);
				}				
			}

			public virtual void Dispose() { }

			private Rect _viewport;
		}

		private class IterableStackingNonVirtualizingContext : StackingVirtualizingContext
		{
			public IterableStackingNonVirtualizingContext(ItemsStackPanel owner)
				: base(owner) { }

			public override Size Measure(Size constraint, Avalonia.Controls.ItemsSourceView itemsView)
			{
				LastMeasureSize = constraint;

				if (AreItemsDirty && itemsView.Count > 0)
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

					UpdateExtent(horizontal ? new Size(stackSize, nonStackSize) : new Size(nonStackSize, stackSize), horizontal);

					Owner.FirstVisibleIndex = Owner.FirstCacheIndex = 0;
					Owner.LastVisibleIndex = Owner.LastCacheIndex = itemsView.Count;

					AreItemsDirty = false;
					((ILogicalScrollable)Owner).RaiseScrollInvalidated(EventArgs.Empty);
				}
				else
				{
					// Still run a measure pass to ensure items that changed visibility or size are
					// properly reflected
					// When virtualizing, if the extent here is different than before, we need to 
					// restart the measure pass 
					bool horizontal = Owner.Orientation == Orientation.Horizontal;
					double stackSize = 0;
					double nonStackSize = 0;

					for (int i = 0; i < Owner.Children.Count; i++)
					{
						var cont = Owner.Children[i];

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

					var newExtent = horizontal ? new Size(stackSize, nonStackSize) : new Size(nonStackSize, stackSize);

					if (newExtent != Extent)
					{
						UpdateExtent(newExtent, horizontal);
						((ILogicalScrollable)Owner).RaiseScrollInvalidated(EventArgs.Empty);
					}
				}

				return Extent;
			}

			public override void Arrange(Size finalSize, Avalonia.Controls.ItemsSourceView itemsView)
			{
				if (Owner.Children.Count == 0)
					return;

				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				Rect rc;
				double x = 0;
				double y = 0;
				//var offset = horizontal ? Offset.X : Offset.Y;
				for (int i = 0; i < Owner.Children.Count; i++)
				{
					if (!Owner.Children[i].IsVisible) // Skip arranging invisible items
						continue;

					rc = horizontal ?
						new Rect(x, 0, Owner.Children[i].DesiredSize.Width, finalSize.Height) :
						new Rect(0, y, Math.Max(finalSize.Width, Owner.Children[i].DesiredSize.Width), Owner.Children[i].DesiredSize.Height);

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

				void Add(bool insertSpace = true)
				{
					if (insertSpace)
					{
						if (args.NewStartingIndex + args.NewItems.Count < itemsView.Count)
						{
							ItemContainerGenerator.InsertSpace(args.NewStartingIndex, args.NewItems.Count);
						}
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

		private class GroupedStackingNonVirtualizingContext : StackingVirtualizingContext
		{
			public GroupedStackingNonVirtualizingContext(ItemsStackPanel panel, ICollectionView view) :
				base(panel)
			{
				CollectionView = view;

				_groupsDisposable = view.CollectionGroups.WeakSubscribe(OnCollectionGroupsChanged);
			}

			public ICollectionView CollectionView { get; }

			public override Size Measure(Size constraint, Avalonia.Controls.ItemsSourceView itemsView)
			{
				LastMeasureSize = constraint;

				if (AreItemsDirty && itemsView.Count > 0)
				{
					if (Owner.Children.Count > 0)
						Owner.Children.Clear();

					if (_groupHeaderContainers != null && _groupHeaderContainers.Count > 0)
						_groupHeaderContainers.Clear();

					bool horizontal = Owner.Orientation == Orientation.Horizontal;
					double stackSize = 0;
					double nonStackSize = 0;

					// First materialize the groups, they're listed first
					for (int i = 0; i < CollectionView.CollectionGroups.Count; i++)
					{
						var cont = CreateGroupHeader(i, CollectionView.CollectionGroups[i].Group);
						_groupHeaderContainers.Add(cont);
						Owner.Children.Add(cont);
						cont.Measure(constraint);
						var size = cont.DesiredSize;

						if (horizontal)
						{
							stackSize += size.Width;
							nonStackSize = Math.Max(size.Height, nonStackSize);
						}
						else
						{
							stackSize += size.Height;
							nonStackSize = Math.Max(size.Width, nonStackSize);
						}
					}

					// Now materialize the items
					for (int i = 0; i < CollectionView.Count; i++)
					{
						var cont = GetOrCreateElement(i);

						Owner.Children.Add(cont);
						cont.Measure(constraint);

						var size = cont.DesiredSize;

						if (horizontal)
						{
							stackSize += size.Width;
							nonStackSize = Math.Max(size.Height, nonStackSize);
						}
						else
						{
							stackSize += size.Height;
							nonStackSize = Math.Max(size.Width, nonStackSize);
						}
					}

					UpdateExtent(horizontal ? new Size(stackSize, nonStackSize) : new Size(nonStackSize, stackSize), horizontal);

					Owner.FirstVisibleIndex = Owner.FirstCacheIndex = 0;
					Owner.LastVisibleIndex = Owner.LastCacheIndex = itemsView.Count;

					(Owner as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);

					AreItemsDirty = false;
				}
				else
				{
					// Still run a measure pass to ensure items that changed visibility or size are
					// properly reflected
					// When virtualizing, if the extent here is different than before, we need to 
					// restart the measure pass 
					bool horizontal = Owner.Orientation == Orientation.Horizontal;
					double stackSize = 0;
					double nonStackSize = 0;

					for (int i = 0; i < Owner.Children.Count; i++)
					{
						var cont = Owner.Children[i];

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

					var newExtent = horizontal ? new Size(stackSize, nonStackSize) : new Size(nonStackSize, stackSize);

					if (newExtent != Extent)
					{
						UpdateExtent(newExtent, horizontal);
						(Owner as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);
					}
				}

				return Extent;
			}

			public override void Arrange(Size finalSize, Avalonia.Controls.ItemsSourceView itemsView)
			{
				if (_groupHeaderContainers.Count == 0)
					return;

				if (Owner.AreStickyGroupHeadersEnabled)
				{
					ArrangeStickyHeaders(finalSize);
				}
				else
				{
					ArrangeNormal(finalSize);
				}
			}

			public override void Dispose()
			{
				_groupsDisposable?.Dispose();
			}

			private void ArrangeNormal(Size finalSize)
			{
				if (Owner.Children.Count == 0)
					return;

				var icg = Owner._itemsControl.ItemContainerGenerator;
				bool horizontal = Owner.Orientation == Orientation.Horizontal;
				double x = 0, y = 0;
				Rect rc;
				//double offset = horizontal ? Offset.X : Offset.Y;
				int firstVisible = -1;
				int lastVisible = -1;
				var count = CollectionView.CollectionGroups.Count;
				for (int i = 0, idx = 0; i < count; i++)
				{
					var groupCount = CollectionView.CollectionGroups[i].GroupItems.Count;

					rc = horizontal ? new Rect(x, 0, Owner.Children[i].DesiredSize.Width, finalSize.Height) :
						new Rect(0, y, finalSize.Width, Owner.Children[i].DesiredSize.Height);

					Owner.Children[i].Arrange(rc);

					if (horizontal)
						x += rc.Width;
					else
						y += rc.Height;

					for (int j = 0; j < groupCount; j++)
					{
						// See note in ArrangeStickyHeaders for this
						if (Owner.Children[i].Clip != null)
							Owner.Children[i].Clip = null;

						var item = icg.ContainerFromIndex(idx);
						rc = horizontal ? new Rect(x, 0, item.DesiredSize.Width, finalSize.Height) :
								new Rect(0, y, finalSize.Width, item.DesiredSize.Height);

						item.Arrange(rc);

						if (firstVisible == -1)
						{
							if (horizontal)
							{
								if (rc.Right >= 0)
								{
									firstVisible = idx;
								}
							}
							else
							{
								if (rc.Bottom >= 0)
								{ 
									firstVisible = idx; 
								}
							}
							
						}

						if (lastVisible == -1)
						{
							if (horizontal)
							{
								if (rc.Left < Viewport.Width)
								{
									lastVisible = idx;
								}
							}
							else
							{
								if (rc.Top < Viewport.Height)
								{
									lastVisible = idx;
								}
							}
						}

						if (horizontal)
							x += rc.Width;
						else
							y += rc.Height;

						idx++;
					}
				}

				Owner.FirstVisibleIndex = firstVisible;
				Owner.LastVisibleIndex = lastVisible;				
			}

			private void ArrangeStickyHeaders(Size finalSize, bool arrange = true)
			{
				bool horizontal = Owner.Orientation == Orientation.Horizontal;
				Rect viewRC = horizontal ? Viewport.WithX(Offset.X) : Viewport.WithY(Offset.Y);

				void CollapseItemsUnderStickyHeaders()
				{
					int groupCount = _groupHeaderContainers.Count;
					int first = Owner.FirstVisibleIndex + groupCount;
					int last = Owner.LastVisibleIndex + groupCount;
					Size size = _groupHeaderContainers[_currentStickyGroupHeader].DesiredSize;

					// Iterate until we reach an item that is fully beneath the current sticky header
					// Start at first - 1 to ensure we have nothing visible
					for (int i = Math.Max(groupCount, first-1); i < Owner.Children.Count; i++)
					{
						if (horizontal)
						{
							if (Owner.Children[i].Bounds.Left > (viewRC.Left + size.Width))
								break;

							// Item is completely hidden by group header
							if (Owner.Children[i].Bounds.Right <= (viewRC.Left + size.Width))
							{
								Owner.Children[i].Arrange(Owner.Children[i].Bounds.WithX(-Owner.Children[i].Bounds.Width));
							}
							else
							{
								// Item is partially hidden by group header, re-arrange with collapsed rect
								var dx = (viewRC.Left + size.Width) - Owner.Children[i].Bounds.Left;
								var clipRect = new Rect(dx, 0, Owner.Children[i].Bounds.Width - dx, Owner.Children[i].Bounds.Height);

								// Cliping the LVI probably isn't the **best** solution here, as if user set Clip, this
								// will automatically override that.
								// But WinUI is doing something that looks like clipping, but I can't figure out which element 
								// is being clipped. Need to investigate more
								// Can't clip the panel because that would hide the sticky header...
								Owner.Children[i].Clip = new RectangleGeometry(clipRect);
							}
						}
						else
						{
							if (Owner.Children[i].Bounds.Top > (viewRC.Top + size.Height))
								break;

							// Item is completely hidden by group header
							if (Owner.Children[i].Bounds.Bottom <= (viewRC.Top + size.Height))
							{
								Owner.Children[i].Arrange(Owner.Children[i].Bounds.WithY(-Owner.Children[i].Bounds.Height));
							}
							else
							{
								// Item is partially hidden by group header, re-arrange with collapsed rect
								var dy = (viewRC.Top + size.Height) - Owner.Children[i].Bounds.Top;								
								var clipRect = new Rect(0, dy, Owner.Children[i].Bounds.Width, Owner.Children[i].Bounds.Height - dy);

								// Cliping the LVI probably isn't the **best** solution here, as if user set Clip, this
								// will automatically override that.
								// But WinUI is doing something that looks like clipping, but I can't figure out which element 
								// is being clipped. Need to investigate more
								// Can't clip the panel because that would hide the sticky header...
								Owner.Children[i].Clip = new RectangleGeometry(clipRect);
							}
						}
					}  
				}

				// Run a normal arrange first, we'll use the absolute location of the headers to 
				// determine which one goes where
				if (arrange)
					ArrangeNormal(finalSize);


				if (_currentStickyGroupHeader == -1)
				{
					// First time load only...
					_currentStickyGroupHeader = 0;

					if (horizontal)
					{
						_lastStickyRect = new Rect(0, 0,
								_groupHeaderContainers[0].DesiredSize.Width, finalSize.Height);
					}
					else
					{
						_lastStickyRect = new Rect(0, 0, finalSize.Width,
								_groupHeaderContainers[0].DesiredSize.Height);
					}
					
					// TO DO: Do we get another arrange if we start off not scrolled at the top?

					return;
				}

				
				if (horizontal)
				{
					int firstVisible = _groupHeaderContainers.Count - 1;
					for (int i = _groupHeaderContainers.Count - 1; i >= 0; i--)
					{
						if (_groupHeaderContainers[i].Bounds.Left < viewRC.Left)
						{
							break;
						}

						firstVisible = i;
					}

					var firstCont = _groupHeaderContainers[firstVisible];

					if (firstVisible == 0)
					{
						// I believe this can only happen IF Offset.Y == 0
						_lastStickyRect = new Rect(Offset.X, 0, firstCont.DesiredSize.Width, finalSize.Height);
						firstCont.Arrange(_lastStickyRect);
						return;
					}

					if (firstCont.Bounds.Left > viewRC.Left + _lastStickyRect.Width)
					{
						// First visible container is below the sticky header line
						if (firstVisible > 0)
							firstVisible--;

						firstCont = _groupHeaderContainers[firstVisible];

						_lastStickyRect = new Rect(Offset.X, 0, firstCont.DesiredSize.Width, finalSize.Height);

						firstCont.Arrange(_lastStickyRect);
						_currentStickyGroupHeader = firstVisible;
					}
					else
					{
						var delta = (viewRC.Left + _lastStickyRect.Width) - firstCont.Bounds.Left;

						var rc = _lastStickyRect.WithX(viewRC.Left - delta);

						_groupHeaderContainers[firstVisible - 1].Arrange(rc);
					}
				}
				else
				{
					int firstVisible = _groupHeaderContainers.Count - 1;
					for (int i = _groupHeaderContainers.Count - 1; i >= 0; i--)
					{
						if (_groupHeaderContainers[i].Bounds.Top < viewRC.Top)
						{
							break;
						}

						firstVisible = i;
					}

					var firstCont = _groupHeaderContainers[firstVisible];

					if (firstVisible == 0)
					{
						// I believe this can only happen IF Offset.Y == 0
						_lastStickyRect = new Rect(0, Offset.Y, finalSize.Width, firstCont.DesiredSize.Height);
						firstCont.Arrange(_lastStickyRect);
						return;
					}

					if (firstCont.Bounds.Top > viewRC.Top + _lastStickyRect.Height)
					{
						// First visible container is below the sticky header line
						if (firstVisible > 0)
							firstVisible--;

						firstCont = _groupHeaderContainers[firstVisible];

						_lastStickyRect = new Rect(0, Offset.Y, finalSize.Width, firstCont.DesiredSize.Height);

						firstCont.Arrange(_lastStickyRect);
						_currentStickyGroupHeader = firstVisible;
					}
					else
					{
						var delta = (viewRC.Top + _lastStickyRect.Height) - firstCont.Bounds.Top;

						var rc = _lastStickyRect.WithY(viewRC.Top - delta);

						_groupHeaderContainers[firstVisible - 1].Arrange(rc);
					}
				}

				

				CollapseItemsUnderStickyHeaders();
			}

			public override void UpdateFromItemsChange(NotifyCollectionChangedEventArgs args, Avalonia.Controls.ItemsSourceView itemsView)
			{
				// Since ICollectionView is a flattened list without the headers, this logic is mostly the same as
				// in the Iterable version. The only difference is we need to offset the indexes for adding/removing
				// containers by the number of group headers, which are all at the front of the Panel Children collection
				var offset = _groupHeaderContainers.Count;
				bool horizontal = Owner.Orientation == Orientation.Horizontal;

				void Add(bool insertSpace = true)
				{
					if (insertSpace)
					{
						if (args.NewStartingIndex + args.NewItems.Count < itemsView.Count)
						{
							ItemContainerGenerator.InsertSpace(args.NewStartingIndex, args.NewItems.Count);
						}
					}

					int idx = args.NewStartingIndex;
					double addExtent = 0;
					double nonStackSize = horizontal ? Extent.Height : Extent.Width;
					for (int i = 0; i < args.NewItems.Count; i++)
					{
						var cont = GetOrCreateElement(idx);
						Owner.Children.Insert(idx + offset, cont);
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
					for (int i = 0; i < args.OldItems.Count; i++)
					{
						subExtent += horizontal ? Owner.Children[idx].Bounds.Width :
							Owner.Children[idx].Bounds.Height;

						Owner.Children.RemoveAt(idx + offset);

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
						Remove(false);
						Add(true);
						break;

					case NotifyCollectionChangedAction.Reset:
						ItemContainerGenerator.Clear();
						Owner.Children.Clear();
						break;

					case NotifyCollectionChangedAction.Move:
						Remove();
						Add();
						break;
				}

				Owner.LastVisibleIndex = Owner.LastCacheIndex = itemsView.Count;
				(Owner as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);
			}

			private void OnCollectionGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				// Only handle the group header here. Adding a group will trigger an INCC notification with the group items,
				// which we will handle above. This INCC update is fired first
								
				void Add()
				{
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						var cont = CreateGroupHeader(e.NewStartingIndex + i, (e.NewItems[i] as ICollectionViewGroup).Group);
						_groupHeaderContainers.Insert(e.NewStartingIndex + i, cont);

						Owner.Children.Insert(e.NewStartingIndex + i, cont);
					}
				}

				void Remove()
				{
					for (int i = e.OldStartingIndex + e.OldItems.Count - 1; i >= e.OldStartingIndex; i--)
					{
						_groupHeaderContainers.RemoveAt(i);
						Owner.Children.RemoveAt(i);
					}
				}

				// TO CHECK: Adding should trigger a new measure pass, so we don't need to measure and change extent here
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						Add();
						break;

					case NotifyCollectionChangedAction.Remove:
						Remove();
						break;

					case NotifyCollectionChangedAction.Reset:
						Owner.Children.RemoveRange(0, _groupHeaderContainers.Count);
						_groupHeaderContainers.Clear();
						break;

					// TODO: Recycle the containers here
					case NotifyCollectionChangedAction.Replace:
						Remove();
						Add();
						break;

					case NotifyCollectionChangedAction.Move:
						Remove();
						Add();
						break;					
				}

				// TODO: Two ScrollInvalidated events will fire if groups are modified, don't
				(Owner as ILogicalScrollable)?.RaiseScrollInvalidated(EventArgs.Empty);
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
				// GridView
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

			private IDisposable _groupsDisposable;
			private List<IControl> _groupHeaderContainers;
			private int _currentStickyGroupHeader = -1;
			private Rect _lastStickyRect;
		}
	}
}
