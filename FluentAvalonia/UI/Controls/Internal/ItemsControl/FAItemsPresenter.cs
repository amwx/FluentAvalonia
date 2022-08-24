using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvalonia.UI.Data;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace FluentAvalonia.UI.Controls
{
	// This class is intended for internal use only and shouldn't be used outside
	// of the ListView or GridView (in retemplating)
	public class FAItemsPresenter : Control, IItemsPresenter, ILogicalScrollable, ITemplatedControl
	{
		public FAItemsPresenter()
		{
			_headerControl = new ContentControl();
			_footerControl = new ContentControl();

			LogicalChildren.Add(_headerControl);
			LogicalChildren.Add(_footerControl);
			VisualChildren.Add(_headerControl);
			VisualChildren.Add(_footerControl);
		}

		static FAItemsPresenter()
		{
			KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<FAItemsPresenter>(KeyboardNavigationMode.Once);
		}

		public static readonly StyledProperty<object> HeaderProperty =
			HeaderedContentControl.HeaderProperty.AddOwner<FAItemsPresenter>();

		public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
			HeaderedContentControl.HeaderTemplateProperty.AddOwner<FAItemsPresenter>();

		public static readonly StyledProperty<object> FooterProperty =
			AvaloniaProperty.Register<FAItemsPresenter, object>(nameof(Footer));

		public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty =
			AvaloniaProperty.Register<FAItemsPresenter, IDataTemplate>(nameof(FooterTemplate));

		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		public IDataTemplate HeaderTemplate
		{
			get => GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		public IDataTemplate FooterTemplate
		{
			get => GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		internal IPanel Panel => _itemsPanel;

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == HeaderProperty)
			{
				_headerControl.Content = change.NewValue;

				InvalidateMeasure();
			}
			else if (change.Property == HeaderTemplateProperty)
			{
				_headerControl.ContentTemplate = change.GetNewValue<IDataTemplate>();

				InvalidateMeasure();
			}
			else if (change.Property == FooterProperty)
			{

				_footerControl.Content = change.NewValue;

				InvalidateMeasure();
			}
			else if (change.Property == FooterTemplateProperty)
			{
				_footerControl.ContentTemplate = change.GetNewValue<IDataTemplate>();

				InvalidateMeasure();
			}
			else if (change.Property == TemplatedParentProperty)
			{
				change.GetNewValue<IItemsPresenterHost>()?.RegisterItemsPresenter(this);
			}
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			base.OnDetachedFromVisualTree(e);

			_itemsPanelDisposable?.Dispose();
			_itemsManager?.Dispose();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			// From my tests in UWP, ItemsPresenter will default to placing the header and footer ContentControls
			// in the topleft and topright, respectively. If a StackPanel or ItemsStackPanel is used, it will 
			// follow the orientation of the given panel. If Orientation is horizontal, no behavior change is made,
			// if the Orientation is vertical, the header and footer ContentControls are placed in the topleft and 
			// bottomleft, respectively.

			if (_itemsPanel == null)
				SetItemsPanel(new StackPanel());

			_itemsPanel.Measure(Size.Infinity);

			_headerControl?.Measure(Size.Infinity);
			_footerControl?.Measure(Size.Infinity);

			var wid = _itemsPanel.DesiredSize.Width;
			var hgt = _itemsPanel.DesiredSize.Height;

			// TODO: update this for non oriented panels
			if (_headerControl != null)
			{
				wid += _orientation == Orientation.Horizontal ? _headerControl.DesiredSize.Width : 0;
				hgt += _orientation == Orientation.Vertical ? _headerControl.DesiredSize.Height : 0;
			}

			if (_footerControl != null)
			{
				wid += _orientation == Orientation.Horizontal ? _footerControl.DesiredSize.Width : 0;
				hgt += _orientation == Orientation.Vertical ? _footerControl.DesiredSize.Height : 0;
			}

			return new Size(wid, hgt);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			ILogicalScrollable logScroll = this;

			var offset = logScroll.IsLogicalScrollEnabled ? logScroll.Offset : Vector.Zero;
			// Regardless of orientation, the header control is always in the topleft
			if (_headerControl != null)
			{
				_headerControl.Arrange(new Rect(-offset.X, -offset.Y, _headerControl.DesiredSize.Width, _headerControl.DesiredSize.Height));
			}

			var panelSize = _itemsPanel.DesiredSize;

			if (_itemsPanel is ItemsStackPanel || _itemsPanel is StackPanel)
			{
				if (_orientation == Orientation.Horizontal)
				{
					double totalWidth = _headerControl?.DesiredSize.Width ?? 0;

					_itemsPanel.Arrange(new Rect(totalWidth - offset.X, -offset.Y, panelSize.Width, finalSize.Height));

					totalWidth += panelSize.Width;

					if (_footerControl != null)
					{
						_footerControl.Arrange(new Rect(totalWidth - offset.X, -offset.Y, _footerControl.DesiredSize.Width, _footerControl.DesiredSize.Height));
					}
				}
				else if (_orientation == Orientation.Vertical)
				{
					double totalHeight = _headerControl?.DesiredSize.Height ?? 0;

					_itemsPanel.Arrange(new Rect(-offset.X, totalHeight - offset.Y, finalSize.Width, panelSize.Height));

					totalHeight += panelSize.Height;

					if (_footerControl != null)
					{
						_footerControl.Arrange(new Rect(-offset.X, totalHeight - offset.Y, _footerControl.DesiredSize.Width, _footerControl.DesiredSize.Height));
					}
				}
			}
			else if (_itemsPanel is ItemsWrapGrid || _itemsPanel is WrapPanel)
			{
				if (_orientation == Orientation.Vertical)
				{
					double totalWidth = _headerControl?.DesiredSize.Width ?? 0;

					_itemsPanel.Arrange(new Rect(totalWidth - offset.X, -offset.Y, panelSize.Width, finalSize.Height));

					totalWidth += panelSize.Width;

					if (_footerControl != null)
					{
						_footerControl.Arrange(new Rect(totalWidth - offset.X, -offset.Y, _footerControl.DesiredSize.Width, _footerControl.DesiredSize.Height));
					}
				}
				else if (_orientation == Orientation.Horizontal)
				{
					double totalHeight = _headerControl?.DesiredSize.Height ?? 0;

					_itemsPanel.Arrange(new Rect(-offset.X, totalHeight - offset.Y, finalSize.Width, panelSize.Height));

					totalHeight += panelSize.Height;

					if (_footerControl != null)
					{
						_footerControl.Arrange(new Rect(-offset.X, totalHeight - offset.Y, _footerControl.DesiredSize.Width, _footerControl.DesiredSize.Height));
					}
				}
			}
			else
			{
				double totalWidth = _headerControl?.DesiredSize.Width ?? 0;
				double totalHeight = _headerControl?.DesiredSize.Height ?? 0;

				_itemsPanel.Arrange(new Rect(-offset.X, totalHeight - offset.Y, finalSize.Width, finalSize.Height));

				totalHeight += panelSize.Height;

				if (_footerControl != null)
				{
					_footerControl.Arrange(new Rect(-offset.X, totalHeight - offset.Y, _footerControl.DesiredSize.Width, _footerControl.DesiredSize.Height));
				}
			}

			return finalSize;
		}

		public sealed override void ApplyTemplate()
		{
			if (_itemsPanel == null)
				InitControl();
		}

		private void InitControl()
		{
			_itemsOwner = TemplatedParent as ItemsControl ?? this.FindAncestorOfType<ItemsControl>();
						
			if (_itemsOwner == null)
				throw new Exception("FAItemsPresenter not used in ItemsControl");

			_itemsPanelDisposable = _itemsOwner.GetPropertyChangedObservable(ItemsControl.ItemsPanelProperty).Subscribe(OnItemsPanelChanged);

			var panel = _itemsOwner.ItemsPanel?.Build();
			if (panel == null)
				panel = new StackPanel();

			SetItemsPanel(panel);
		}

		internal void SetItemsPanel(IPanel panel)
		{
			if (_itemsPanel != null)
			{
				LogicalChildren.Remove(_itemsPanel);
				VisualChildren.Remove(_itemsPanel);

				if (_itemsPresenterManagesItems)
				{
					_itemsPanel.Children.Clear();
					_itemsManager?.Dispose();
				}

				_itemsPanel.SetValue(TemplatedParentProperty, null);
			}

			_itemsPanel = panel;
			LogicalChildren.Insert(1, _itemsPanel);
			VisualChildren.Insert(1, _itemsPanel);
						
			_itemsPanel.SetValue(TemplatedParentProperty, _itemsOwner);

			KeyboardNavigation.SetTabNavigation(_itemsPanel as InputElement, KeyboardNavigation.GetTabNavigation(this));

			// TODO: Sub to orientation change
			if (_itemsPanel is ItemsStackPanel isp)
			{
				_orientation = isp.Orientation;
			}
			else if (_itemsPanel is ItemsWrapGrid iwg)
			{
				_orientation = iwg.Orientation;
			}
			else if (_itemsPanel is StackPanel sp)
			{
				_orientation = sp.Orientation;
			}

			// Main purpose of this control is to support the ListView/GridView which defaults to the
			// ItemsStackPanel or ItemsWrapGrid. If user changes that to any of the default panels in
			// Avalonia (or a custom one) these controls won't populate because ISP/IWG handle items
			// themselves. So this allows a fallback...though no virtualization is supported this way
			// However, if you implement your own panel that extends from ModernCollectionBasePanel, 
			// we hand it off to custom panel
			if (!(panel is ModernCollectionBasePanel))
			{
				_itemsManager = new ItemsPresenterItemsManager(_itemsOwner, panel);
				_itemsPresenterManagesItems = true;
			}
			else
			{
				_itemsPresenterManagesItems = false;
			}

			((ILogicalScrollable)this).RaiseScrollInvalidated(EventArgs.Empty);

			InvalidateMeasure();
		}

		internal Size GetHeaderSize() =>
			_headerControl.DesiredSize;

		internal Size GetFooterSize() =>
			_footerControl.DesiredSize;

		private void OnItemsPanelChanged(AvaloniaPropertyChangedEventArgs args)
		{
			SetItemsPanel(args.NewValue as IPanel ?? new StackPanel());
		}

		bool ILogicalScrollable.CanHorizontallyScroll
		{
			get => _itemsPanel is ILogicalScrollable logical && logical.IsLogicalScrollEnabled ?
				logical.CanHorizontallyScroll : false;
			set
			{
				if (_itemsPanel is ILogicalScrollable logical)
					logical.CanHorizontallyScroll = value;
			}
		}

		bool ILogicalScrollable.CanVerticallyScroll
		{
			get => _itemsPanel is ILogicalScrollable logical && logical.IsLogicalScrollEnabled ?
				logical.CanVerticallyScroll : false;
			set
			{
				if (_itemsPanel is ILogicalScrollable logical)
					logical.CanVerticallyScroll = value;
			}
		}

		public bool IsLogicalScrollEnabled => 
			_itemsPanel is ILogicalScrollable;

		Size ILogicalScrollable.ScrollSize => (_itemsPanel as ILogicalScrollable)?.ScrollSize ?? new(1, 1);

		Size ILogicalScrollable.PageScrollSize => (_itemsPanel as ILogicalScrollable)?.PageScrollSize ?? new Size(16, 16);

		Size IScrollable.Extent => (_itemsPanel as ILogicalScrollable)?.Extent ?? new Size(250,2000);

		Vector IScrollable.Offset
		{
			get => (_itemsPanel as ILogicalScrollable)?.Offset ?? default;
			set
			{
				if (_itemsPanel is ILogicalScrollable scrollable)
					scrollable.Offset = CoerceOffset(value);

				((ILogicalScrollable)this).RaiseScrollInvalidated(EventArgs.Empty);
			}
		}

		Size IScrollable.Viewport => (_itemsPanel as ILogicalScrollable)?.Viewport ?? Bounds.Size;

		event EventHandler ILogicalScrollable.ScrollInvalidated
		{
			add => _scrollEvent += value;
			remove => _scrollEvent -= value;
		}

		IEnumerable IItemsPresenter.Items
		{
			get => _itemsOwner.Items;
			set { }
		}

		IPanel IItemsPresenter.Panel => Panel;

		bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect) =>
			(_itemsPanel as ILogicalScrollable)?.BringIntoView(target, targetRect) ?? false;

		IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl from) =>
			(_itemsPanel as ILogicalScrollable)?.GetControlInDirection(direction, from) ?? null;

		void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e)
		{
			_scrollEvent?.Invoke(this, e);
			InvalidateArrange();
		}
			

		void IItemsPresenter.ItemsChanged(NotifyCollectionChangedEventArgs e)
		{ }

		void IItemsPresenter.ScrollIntoView(int index)
		{
			if (_itemsPresenterManagesItems)
			{
				if (index != -1)
				{
					_itemsOwner.ItemContainerGenerator.ContainerFromIndex(index)?.BringIntoView();
				}
			}
			else if (_itemsPanel is ItemsStackPanel isp)
			{
				if (index >= isp.FirstCacheIndex && index <= isp.LastCacheIndex)
				{
					_itemsOwner.ItemContainerGenerator.ContainerFromIndex(index)?.BringIntoView();
				}
				else
				{
					// TODO:
					// Container isn't currently realized. We need to mark this as the anchor index & trigger
					// a new layout pass...
				}
			}
		}

		private Vector CoerceOffset(Vector value)
		{
			var scroll = this as ILogicalScrollable;
			var maxX = Math.Max(scroll.Extent.Width - scroll.Viewport.Width, 0);
			var maxY = Math.Max(scroll.Extent.Height - scroll.Viewport.Height, 0);
			return new Vector(MathHelpers.Clamp(value.X, 0, maxX), MathHelpers.Clamp(value.Y, 0, maxY));
		}

		private IPanel _itemsPanel;
		private ItemsControl _itemsOwner;
		private bool _itemsPresenterManagesItems;
		private IDisposable _itemsPanelDisposable;
		private ContentControl _headerControl;
		private ContentControl _footerControl;
		private Orientation? _orientation;
		private EventHandler _scrollEvent;
		private ItemsPresenterItemsManager _itemsManager;

		private class ItemsPresenterItemsManager : IDisposable
		{
			public ItemsPresenterItemsManager(ItemsControl owner, IPanel panel)
			{
				// ItemsPresenter will not handle grouping outside of ListView/GridView. Doing so really
				// requires rewriting the entire ItemsControl. See the FAItemsControl.cs file for more...
				_itemsControl = owner;
				_itemsView = owner.Items == null ? Avalonia.Controls.ItemsSourceView.Empty : new Avalonia.Controls.ItemsSourceView(owner.Items);

				_itemsDisposable = owner.GetPropertyChangedObservable(ItemsControl.ItemsProperty).Subscribe(OnItemsPropertyChanged);
				_itemsView.CollectionChanged += OnItemsChanged;

				_panel = panel;

				GenerateItems();
			}

			private void OnItemsPropertyChanged(AvaloniaPropertyChangedEventArgs args)
			{
				if (_itemsView != null)
				{
					_itemsView.CollectionChanged -= OnItemsChanged;
					_itemsView.Dispose();
				}

				if (args.NewValue is IEnumerable ie)
				{
					_itemsView = new Avalonia.Controls.ItemsSourceView(ie);
				}
				else
				{
					_itemsView = Avalonia.Controls.ItemsSourceView.Empty;
				}

				_itemsView.CollectionChanged += OnItemsChanged;

				GenerateItems();
			}

			private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
			{
				var icg = _itemsControl.ItemContainerGenerator;

				void Add()
				{
					if (args.NewStartingIndex + args.NewItems.Count < _itemsView.Count)
					{
						icg.InsertSpace(args.NewStartingIndex, args.NewItems.Count);
					}

					int idx = 0;
					for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
					{
						var ici = icg.Materialize(i, args.NewItems[idx++]);

						if (ici.Index < _panel.Children.Count)
						{
							_panel.Children.Insert(ici.Index, ici.ContainerControl);
						}
						else
						{
							_panel.Children.Add(ici.ContainerControl);
						}
					}
				}

				void Remove()
				{
					var demat = icg.RemoveRange(args.OldStartingIndex, args.OldItems.Count);

					foreach (var item in demat)
					{
						_panel.Children.Remove(item.ContainerControl);
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
						Remove();
						Add();
						break;

					case NotifyCollectionChangedAction.Reset:
						GenerateItems();
						break;

					case NotifyCollectionChangedAction.Move:
						Remove();
						Add();
						break;
				}
			}

			private void GenerateItems()
			{
				if (_panel.Children.Count > 0)
					_panel.Children.Clear();

				var icg = _itemsControl.ItemContainerGenerator;
				icg.Clear();

				if (_itemsView.Count == 0)
					return;

				for (int i = 0; i < _itemsView.Count; i++)
				{
					var ici = icg.Materialize(i, _itemsView.GetAt(i));

					_panel.Children.Add(ici.ContainerControl);
				}
			}

			public void Dispose()
			{
				if (_itemsView != null)
				{
					_itemsView.CollectionChanged -= OnItemsChanged;
					_itemsView.Dispose();
					_itemsView = null;
				}

				_itemsControl = null;

				_itemsDisposable?.Dispose();
				_itemsDisposable = null;
			}

			private IPanel _panel;
			private IDisposable _itemsDisposable;
			private ItemsControl _itemsControl;
			private Avalonia.Controls.ItemsSourceView _itemsView;
		}
	}
}
