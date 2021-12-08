using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.Core.Attributes;
using System;
using System.Collections.Generic;
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

			EffectiveViewportChanged += OnEffectiveViewportChanged;

			_itemsControl = this.FindAncestorOfType<ItemsControl>();
			if (_itemsControl == null)
				throw new InvalidOperationException("ItemsStackPanel can only be used inside an ItemsControl");

			_host = this.FindAncestorOfType<FAItemsPresenter>();
			if (_host == null)
				throw new InvalidOperationException("ItemsStackPanel can only be used inside an FAItemsPresenter");

			_itemsView = _itemsControl.Items != null ? new Avalonia.Controls.ItemsSourceView(_itemsControl.Items) :
				Avalonia.Controls.ItemsSourceView.Empty;


		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);

			EffectiveViewportChanged -= OnEffectiveViewportChanged;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			return base.ArrangeOverride(finalSize);
		}

		private void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e)
		{
			//var lvi = this.FindAncestorOfType<ListViewItem>();

			//if (lvi != null)
			//	Debug.WriteLine($"{lvi.Content} | {e.EffectiveViewport}");

			Debug.WriteLine($"{e.EffectiveViewport}");
		}

		private bool _areStickyGroupHeadersEnabled = true;
		private double _cacheLength = 1;
		private Thickness _groupPadding;
		private GroupHeaderPlacement _groupHeaderPlacement;

		private Avalonia.Controls.ItemsSourceView _itemsView;
		private FAItemsPresenter _host;
		private ItemsControl _itemsControl;
		private VirtualizingContext _context;

		private abstract class VirtualizingContext
		{

		}
	}
}
