using Avalonia.Controls.Presenters;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAvalonia.Core.Attributes;
using Avalonia.Controls.Generators;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Data;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Threading;

namespace FluentAvalonia.UI.Controls
{
	internal class ListViewBase : SelectingItemsControl, IItemsPresenterHost
	{
		public static readonly new StyledProperty<ListViewSelectionMode> SelectionModeProperty =
			AvaloniaProperty.Register<ListViewBase, ListViewSelectionMode>(nameof(SelectionMode), ListViewSelectionMode.Single);

		public static readonly StyledProperty<bool> IsItemClickEnabledProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(IsItemClickEnabled), true);

		public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
			HeaderedContentControl.HeaderTemplateProperty.AddOwner<ListViewBase>();

		public static readonly StyledProperty<object> HeaderProperty =
			HeaderedContentControl.HeaderProperty.AddOwner<ListViewBase>();

		public static readonly StyledProperty<bool> CanReorderItemsProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(CanReorderItems));

		public static readonly StyledProperty<bool> CanDragItemsProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(CanDragItemsProperty));

		public static readonly StyledProperty<bool> ShowsScrollingPlaceholdersProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(ShowsScrollingPlaceholders), true);

		public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty =
			AvaloniaProperty.Register<ListViewBase, IDataTemplate>(nameof(FooterTemplate));

		public static readonly StyledProperty<object> FooterProperty =
			AvaloniaProperty.Register<ListViewBase, object>(nameof(Footer));

		public static readonly StyledProperty<ListViewReorderMode> ReorderModeProperty =
			AvaloniaProperty.Register<ListViewBase, ListViewReorderMode>(nameof(ReorderMode));

		public static readonly StyledProperty<bool> IsMultiSelectCheckBoxEnabledProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(IsMultiSelectCheckBoxEnabled), true);

		public static readonly DirectProperty<ListViewBase, IReadOnlyList<ItemIndexRange>> SelectedRangesProperty =
			AvaloniaProperty.RegisterDirect<ListViewBase, IReadOnlyList<ItemIndexRange>>(nameof(SelectedRanges),
				x => x.SelectedRanges);

		public static readonly StyledProperty<bool> SingleSelectionFollowsFocusProperty =
			AvaloniaProperty.Register<ListViewBase, bool>(nameof(SingleSelectionFollowsFocus));

		public new ListViewSelectionMode SelectionMode
		{
			get => GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		[NotImplemented]
		public bool IsSwipeEnabled
		{
			get => false;
			set { }
		}

		public bool IsItemClickEnabled
		{
			get => GetValue(IsItemClickEnabledProperty);
			set => SetValue(IsItemClickEnabledProperty, value);
		}

		[NotImplemented]
		public IncrementalLoadingTrigger IncrementalLoadingTrigger { get; set; }

		[NotImplemented]
		public double IncrementalLoadingThreshold { get; set; }

		//public TransitionCollection HeaderTransitions {get;set;}

		public IDataTemplate HeaderTemplate
		{
			get => GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		[NotImplemented]
		public double DataFetchSize { get; set; }

		public bool CanReorderItems
		{
			get => GetValue(CanReorderItemsProperty);
			set => SetValue(CanReorderItemsProperty, value);
		}

		public bool CanDragItems
		{
			get => GetValue(CanDragItemsProperty);
			set => SetValue(CanDragItemsProperty, value);
		}

		[NotImplemented]
		public bool ShowsScrollingPlaceholders
		{
			get => GetValue(ShowsScrollingPlaceholdersProperty);
			set => SetValue(ShowsScrollingPlaceholdersProperty, value);
		}

		//public TransitionCollection FooterTransitions {get;set;}

		public IDataTemplate FooterTemplate
		{
			get => GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		public ListViewReorderMode ReorderMode
		{
			get => GetValue(ReorderModeProperty);
			set => SetValue(ReorderModeProperty, value);
		}

		public bool IsMultiSelectCheckBoxEnabled
		{
			get => GetValue(IsMultiSelectCheckBoxEnabledProperty);
			set => SetValue(IsMultiSelectCheckBoxEnabledProperty, value);
		}

		[NotImplemented] // TODO:
		public IReadOnlyList<ItemIndexRange> SelectedRanges
		{
			get => _selectedRanges?.AsReadOnly();
		}

		public bool SingleSelectionFollowsFocus
		{
			get => GetValue(SingleSelectionFollowsFocusProperty);
			set => SetValue(SingleSelectionFollowsFocusProperty, value);
		}

		//public SemanticZoom SemanticZoomOwner {get;}
		//public bool IsZoomedInView { get; }
		//public bool IsActiveView { get; }

		public IPanel ItemsPanelRoot => _presenter?.Panel;

		public event DragItemsStartingEventHandler DragItemsStarting;
		public event ItemClickEventHandler ItemClick;
		public event TypedEventHandler<ListViewBase, DragItemsCompletedEventArgs> DragItemsCompleted;

		[NotImplemented]
		public event TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChanging;

		[NotImplemented]
		public event TypedEventHandler<ListViewBase, ChoosingGroupHeaderContainerEventArgs> ChoosingGroupHeaderContainer;

		[NotImplemented]
		public event TypedEventHandler<ListViewBase, ChoosingItemContainerEventArgs> ChoosingItemContainer;

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			_presenter = e.NameScope.Get<FAItemsPresenter>("ItemsHost");
			// It seems that IItemsPresenterHost.RegisterItemsPresenter isn't getting called for some reason,
			// thus this doesn't get set, and the ICG Dematerialize event handling gets unhappy, set it here
			base.Presenter = _presenter;

			UpdateVisualStateForSelectionModeChange(SelectionMode);
			InitEvents();
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == SelectionModeProperty)
			{
				var newValue = change.GetNewValue<ListViewSelectionMode>();

				UpdateVisualStateForSelectionModeChange(newValue);
			}
		}

		protected override IItemContainerGenerator CreateItemContainerGenerator()
		{
			var lvicg = new ListViewBaseItemContainerGenerator(this);

			return lvicg;
		}

		protected override void OnContainersMaterialized(ItemContainerEventArgs e)
		{
			base.OnContainersMaterialized(e);

			if (SelectedIndex == -1)
				return;

			// UpdateSelection() isn't called on load if SelectedIndex is set, so we do that here to make sure
			// the tab once element is set correctly
			var selIndex = SelectedIndex;
			for (int i = 0; i < e.Containers.Count; i++)
			{
				if (e.Containers[i].Index == selIndex)
				{
					if (KeyboardNavigation.GetTabOnceActiveElement(ItemsPanelRoot as InputElement) != e.Containers[i].ContainerControl)
					{
						KeyboardNavigation.SetTabOnceActiveElement(ItemsPanelRoot as InputElement,
							e.Containers[i].ContainerControl);
					}

					break;
				}
			}
		}

		protected override void OnGotFocus(GotFocusEventArgs e)
		{
			base.OnGotFocus(e);
		}

		public void SelectAll()
		{
			if (ItemCount == 0)
				return;

			SelectRange(new ItemIndexRange(0, ItemCount));
		}

		[NotImplemented]
		public Task<LoadMoreItemsResult> LoadMoreItemsAsync()
		{
			throw new NotImplementedException();
		}

		[NotImplemented]
		public void ScrollIntoView(object item, ScrollIntoViewAlignment alignment)
		{
			// TODO 
			
		}

		[NotImplemented]
		public void SetDesiredContainerUpdateDuration(TimeSpan duration)
		{ 
			throw new NotImplementedException();
		}

		public void SelectRange(ItemIndexRange range)
		{
			Selection.SelectRange(range.FirstIndex, range.LastIndex);
		}

		public void DeselectRange(ItemIndexRange range)
		{
			Selection.DeselectRange(range.FirstIndex, range.LastIndex);
		}

		[NotImplemented]
		public bool IsDragSource()
		{
			return false;
		}

		internal void UpdateSelectionFromItemFocus(IControl lvi)
		{
			var index = ItemContainerGenerator.IndexFromContainer(lvi);
			Presenter.ScrollIntoView(index);

			if (!SingleSelectionFollowsFocus || SelectionMode != ListViewSelectionMode.Single)
				return;
						
			if (index != -1 && SelectedIndex != index)
			{
				SelectedIndex = index;
			}
		}

		private void UpdateVisualStateForSelectionModeChange(ListViewSelectionMode mode)
		{
			switch (mode)
			{
				case ListViewSelectionMode.None:
					SelectedItems.Clear();
					PseudoClasses.Set(":noneselect", true);
					PseudoClasses.Set(":singleselect", false);
					PseudoClasses.Set(":multiselect", false);
					PseudoClasses.Set(":extendedselect", false);
					Selection.SingleSelect = true;
					break;

				case ListViewSelectionMode.Single:
					base.SelectionMode = Avalonia.Controls.SelectionMode.Single;
					PseudoClasses.Set(":noneselect", false);
					PseudoClasses.Set(":singleselect", true);
					PseudoClasses.Set(":multiselect", false);
					PseudoClasses.Set(":extendedselect", false);
					Selection.SingleSelect = true;
					break;

				case ListViewSelectionMode.Extended:
					base.SelectionMode = Avalonia.Controls.SelectionMode.Multiple;
					PseudoClasses.Set(":noneselect", false);
					PseudoClasses.Set(":singleselect", false);
					PseudoClasses.Set(":multiselect", false);
					PseudoClasses.Set(":extendedselect", true);
					Selection.SingleSelect = false;
					break;

				case ListViewSelectionMode.Multiple:
					base.SelectionMode = Avalonia.Controls.SelectionMode.Multiple;
					PseudoClasses.Set(":noneselect", false);
					PseudoClasses.Set(":singleselect", false);
					PseudoClasses.Set(":multiselect", true);
					PseudoClasses.Set(":extendedselect", false);
					Selection.SingleSelect = false;
					break;
			}
		}
			
		private void InitEvents()
		{
			// This is called from OnApplyTemplate, so if template is reapplied for some reason, this fires again
			// But we don't need to change these events if the template changes so ignore
			if (_hasPointerSubscriptions)
				return;

			AddHandler(PointerPressedEvent, OnPointerPressedPreview, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
			AddHandler(PointerReleasedEvent, OnPointerReleasedPreview, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
			AddHandler(PointerMovedEvent, OnPointerMovedPreview, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
			AddHandler(PointerCaptureLostEvent, OnPointerCaptureLostPreview, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

			// When Doing a DragDrop operation, UI thread is blocked so we lose mouse interaction if 
			// reordering is also enabled. This becomes our PointerMoved event in that case.
			// Handle all DragOver events, even if marked as handled
			AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Tunnel, true);

			_hasPointerSubscriptions = true;
		}

		private void OnPointerPressedPreview(object sender, PointerPressedEventArgs e)
		{
			if (e.Route == RoutingStrategies.Bubble)
			{
				e.Handled = true;
				return;
			}

			var point = e.GetCurrentPoint(this);
			if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
			{
				var panel = ItemsPanelRoot;
				if (e.Source is IVisual v)
				{
					var item = v.GetSelfAndVisualAncestors().Where(x => x.VisualParent == panel).FirstOrDefault() as IControl;

					if (item != null)
					{
						_pointerDownItem = item;
						_isDragging = ShouldInitDragging();
						//e.Handled = true;
						_lastPointerPoint = point.Position;
					}
				}
			}
		}

		private void OnPointerReleasedPreview(object sender, PointerReleasedEventArgs e)
		{
			if (e.Route == RoutingStrategies.Bubble)
			{
				e.Handled = true;
				return;
			}

			if (_isDragging)
			{
				_isDragging = false;

				// TODO: Only call this if we were actually performing a drag operation
				if (_reorderContext != null)
				{
					OnDragCompleted();
					return; // Don't select the item used for dragging
				}
			}

			if (_pointerDownItem != null && 
				e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
			{
				var panel = ItemsPanelRoot;
				if (e.Source is IVisual v)
				{
					// This should fire first, per WinUI docs, user should also set SelectionMode to None when using
					// IsItemClickEnabled, and supply their own selection logic.
					if (IsItemClickEnabled)
					{
						var container = GetContainerFromEventSource(e.Source);
						ItemClick?.Invoke(this, new ItemClickEventArgs(this, Items.ElementAt(ItemContainerGenerator.IndexFromContainer(container))));
					}

					var selectionMode = SelectionMode;
					if (selectionMode == ListViewSelectionMode.None)
					{
						_pointerDownItem = null;
						return;
					}

					var item = v.GetSelfAndVisualAncestors().Where(x => x.VisualParent == panel).FirstOrDefault() as IControl;

					if (item == _pointerDownItem)
					{
						var point = e.GetPosition(_pointerDownItem);

						// Cancel selection if mouse is moved outside of the item when the pointer was down
						if (point .X < 0 || point.Y < 0 ||
							point.X > _pointerDownItem.Bounds.Width || point.Y > _pointerDownItem.Bounds.Height)
							return;

						//_pointerDownItem.Focus();

						bool selected = _pointerDownItem is ISelectable s ? s.IsSelected : false;

						var container = GetContainerFromEventSource(e.Source);
						var lvi = (e.Source as IVisual).FindAncestorOfType<ListViewItem>();

						if (selectionMode == ListViewSelectionMode.Multiple)
						{
							UpdateSelectionFromEventSource(item, !selected, false, true);
						}
						else if (selectionMode == ListViewSelectionMode.Extended)
						{
							bool shift = (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
							bool ctrl = (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control;

							UpdateSelectionFromEventSource(item, true, shift, ctrl);
						}
						else
						{
							UpdateSelectionFromEventSource(item);
						}

						//e.Handled = true;
						_pointerDownItem = null;
					}
				}
			}
		}

		private void OnPointerMovedPreview(object sender, PointerEventArgs e)
		{
			if (_isDragging)
			{
				if (_reorderContext != null)
				{
					_reorderContext.HandleDrag(e.GetPosition(this));
					return;
				}

				var scaling = VisualRoot?.RenderScaling ?? 1;
				var pt = e.GetCurrentPoint(this).Position;
				var dx = pt.X - _lastPointerPoint.X;
				var dy = pt.Y - _lastPointerPoint.Y;
				var delta = Math.Sqrt(dx * dx + dy * dy) * scaling;
				//Debug.WriteLine(delta);
				if (delta > 5 * scaling)
				{
					OnDragStarted(e);
				}
			}
		}

		private void OnPointerCaptureLostPreview(object sender, PointerCaptureLostEventArgs e)
		{
			if (_isDragging)
			{
				OnDragInterruped();
				_isDragging = false;
			}
		}

		protected virtual bool ShouldInitDragging()
		{
			if (!CanDragItems && !CanReorderItems)
				return false;

			// DragDrop.AllowDrop must be set to true
			if (!DragDrop.GetAllowDrop(this))
				return false;

			var itms = Items;
			// IList and INCC are required for drag operations. Not sure if WinUI places these same
			// restrictions, but IList provides exact indexing of items, which is needed for all this
			if (itms is not IList || itms is not INotifyCollectionChanged)
				return false;

			// Drag drop / reordering currently disabled when Grouping, may revisit this in future
			if (itms is ICollectionView view && view.CollectionGroups != null)
				return false;

			// ItemsStackPanel uses virtualization, if that's our current panel, this is disabled
			// for now until virtualization logic & interaction logic gets sorted out
			// Will keep dragging active though, but reordering is disabled
			//if (ItemsPanelRoot is ItemsStackPanel)
			//	return CanDragItems;

			return true;
		}

		protected async void OnDragStarted(PointerEventArgs pArgs)
		{
			_reorderContext = new DragReorderContext(this);

			if (CanReorderItems && !(Items is ICollectionView view && view.CollectionGroups != null))
			{
				PseudoClasses.Set(":drag", true);

				_reorderContext.IsDoingReorder = true;
				_reorderContext.Begin(_pointerDownItem);
			}

			// Reordering follows a similar nature to Dragging, but we only raise the event if drag is enabled
			if (CanDragItems)
			{
				int pointerItemIndex = ItemContainerGenerator.IndexFromContainer(_pointerDownItem);

				bool includeSelected = false;
				if (_pointerDownItem is ISelectable sel)
				{
					includeSelected = sel.IsSelected;
				}
				else if (Selection.SelectedIndexes.Contains(pointerItemIndex))
				{
					includeSelected = true;
				}

				if (includeSelected)
				{
					_dragItems = new List<object>(SelectedItems.Count);
					_dragItems.AddRange(SelectedItems.Cast<object>());
				}
				else
				{
					_dragItems = new List<object>
					{
						Items.ElementAt(pointerItemIndex)
					};
				}

				var args = new DragItemsStartingEventArgs
				{
					Data = new DataPackage(),
					Items = _dragItems
				};
				DragItemsStarting?.Invoke(this, args);

				if (args.Cancel)
				{
					_isDragging = false;
					return;
				}

				_lastDropEffect = await DragDrop.DoDragDrop(pArgs, args.Data, DragDropEffects.Move);

				OnDragCompleted();
			}
		}

		protected virtual void OnDragCompleted()
		{
            var args = new DragItemsCompletedEventArgs(_lastDropEffect, _dragItems);
			
			DragItemsCompleted?.Invoke(this, args);

			PseudoClasses.Set(":drag", false);

			_reorderContext?.End();
			_reorderContext?.Dispose();
			_reorderContext = null;
			_dragItems = null;
			_lastDropEffect = DragDropEffects.None;
			_isDragging = false;
		}

		protected virtual void OnDragInterruped()
		{
			PseudoClasses.Set(":drag", false);
			//_reorderContext?.Cancel();
			_reorderContext?.Dispose();
			_reorderContext = null;
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			if (_reorderContext != null)
			{
				_reorderContext.HandleDrag(e.GetPosition(this));
			}
		}




		// WinUI ItemsControl stuff

		protected virtual bool IsItemItsOwnContainerOverride(object item) => false; // delegate to the ListView/GridView

		protected virtual IControl GetContainerForItemOverride() => null; // delegate to the ListView/GridView

		protected virtual void PrepareItemContainerOverride(IControl container, object item)
		{
			// This logic is delegated to the ListView and GridView
		}

		protected virtual void ClearItemContainerOverride(IControl container, object item)
		{
			// Don't clear anything here if item is its own container
			if (item == container)
				return;

			if (container is ListViewItem lvi)
			{
				lvi.DataContext = null;
				lvi.Content = null;
			}
			else
			{
				container.DataContext = null;
			}
		}


		internal bool IsItemItsOwnContainerCore(object item) =>
			IsItemItsOwnContainerOverride(item);

		internal void PrepareItemContainerCore(IControl control, object item, int index, bool wasContainerInRecyclePool)
		{			
			//ContainerContentChanging?.Invoke(this, new ContainerContentChangingEventArgs(control as ContentControl, item, index, wasContainerInRecyclePool));

			PrepareItemContainerOverride(control, item);		
		}
			

		internal void ClearItemContainerCore(IControl control, object item) =>
			ClearItemContainerOverride(control, item);

		internal IControl GetContainerForItemCore(int index, object item)//, IControl recycledContainer)
		{
			//IControl container = null;

			//if (ItemsPanelRoot is ItemsStackPanel)
			//{
			//	// Per WinUI docs this event only fires if using an ItemsStackPanel or ItemsWrapGrid
			//	var args = new ChoosingItemContainerEventArgs
			//	{
			//		Item = item,
			//		ItemIndex = index
			//	};

			//	ChoosingItemContainer?.Invoke(this, args);

			//	container = args.ItemContainer;
			//}

			//if (container == null)
			//	container = recycledContainer ?? GetContainerForItemOverride();

			//if (!args.IsContainerPrepared)
			//	PrepareItemContainerCore(container, item, index, container == recycledContainer);

			return GetContainerForItemOverride(); 
				//container;
		}

		protected internal virtual IControl GetGroupContainerForItem(int index, object group) =>
			new ContentControl();

		internal void RaiseChoosingGroupHeaderContainerEvent(ChoosingGroupHeaderContainerEventArgs args)
		{
			ChoosingGroupHeaderContainer?.Invoke(this, args);
		}



		// SemanticZoom related...
		//public Task<bool> TryStartConnectedAnimationAsync() => throw new NotImplementedException();
		//public ConnectedAnimation PrepareConnectedAnimation(string key, object item, string elementName)
		//public void InitializeViewChange() { }		
		//public void CompleteViewChange() { }
		//public void MakeVisible(SemanticZoomLocation loc)
		//public void StartViewChangeFrom()
		//public void StartViewChangeTo()
		//public void CompleteViewChangeFrom()
		//public void CompleteViewChangeTo()

		private bool _hasPointerSubscriptions;
		private FAItemsPresenter _presenter;
		private IControl _pointerDownItem;
		private bool _isDragging;
		private Point _lastPointerPoint;
		private List<ItemIndexRange> _selectedRanges;

		// TODO: Rename this into DragReorderContext & include dragging only in there to
		// also wrap the two properties below
		private DragReorderContext _reorderContext;

		private List<object> _dragItems;
		private DragDropEffects _lastDropEffect;

		private class DragReorderContext : IDisposable
		{
			public DragReorderContext(ListViewBase lvb)
			{
				_owner = lvb;
				_presenter = lvb._presenter;
				_panel = lvb.ItemsPanelRoot;

				_scroller = _presenter.FindAncestorOfType<ScrollViewer>();
			}

			public bool IsDoingReorder { get; set; }

			public void Begin(IControl initContainer)
			{
				if (_scroller != null)
				{
					_scrollsVertical = _scroller.Extent.Height > _scroller.Viewport.Height;
					_scrollsHorizontal = _scroller.Extent.Width > _scroller.Viewport.Width;

					var bounds = _owner.Bounds.Size;
					_autoScrollUp = bounds.Height * 0.20;
					_autoScrollDown = bounds.Height - _autoScrollUp;

					_autoScrollLeft = bounds.Width * 0.20;
					_autoScrollRight = bounds.Width - _autoScrollLeft;
				}

				int containerIndex = _owner.ItemContainerGenerator.IndexFromContainer(initContainer);
				bool isContainerSelected = false;
				if (initContainer is ISelectable select)
				{
					isContainerSelected = select.IsSelected;
				}
				else if (_owner.Selection.SelectedIndexes.Contains(containerIndex))
				{
					isContainerSelected = true;
				}

				_dragHost = initContainer;
				_lastInsertIndex = _dragHostIndex = containerIndex;

				if (IsDoingReorder)
				{
					((IPseudoClasses)initContainer.Classes).Set(":dragging", true);

					if (isContainerSelected)
					{
						// Only if the item used to start the drag is selected do we include all selected items
						// in the drag operation
						var indices = _owner.Selection.SelectedIndexes;
						if (indices.Count != 0)
						{
							for (int i = 0; i < indices.Count; i++)
							{
								if (indices[i] != _dragHostIndex)
								{
									//((IPseudoClasses)_owner.ItemContainerGenerator.ContainerFromIndex(indices[i]).Classes).Set(":dragging", true);
									((IPseudoClasses)_owner.ItemContainerGenerator.ContainerFromIndex(indices[i]).Classes).Set(":multidrag", true);
								}
							}
						}
					}
				}
				

				

				if (_popup == null)
				{
					_popup = new Popup();
					_popup.IsLightDismissEnabled = false;
					_popup.WindowManagerAddShadowHint = false;
					_popup.PlacementMode = PlacementMode.Pointer;
					((ISetLogicalParent)_popup).SetParent(_owner);
				}

				_popup.Child = new Border
				{
					Background = new VisualBrush
					{
						Visual = _dragHost,
						Stretch = Stretch.None
					},
					Width = _dragHost.Bounds.Width,
					Height = _dragHost.Bounds.Height,
					Opacity = 0.8
				};

				_popup.IsOpen = true;
			}

			public void End()
			{
				if (IsDoingReorder)
				{
					var indices = _owner.Selection.SelectedIndexes;
					if (indices.Count != 0)
					{
						for (int i = 0; i < indices.Count; i++)
						{
							//_panel.Children.Remove();
							((IPseudoClasses)_owner.ItemContainerGenerator.ContainerFromIndex(indices[i]).Classes).Set(":dragging", false);
							((IPseudoClasses)_owner.ItemContainerGenerator.ContainerFromIndex(indices[i]).Classes).Set(":multidrag", false);
						}
					}

					((IPseudoClasses)_dragHost.Classes).Set(":dragging", false);

					if (_lastInsertIndex != -1 && _lastInsertIndex != _dragHostIndex)
					{
						var list = _owner.Items as IList; // IList is required for this

						bool isContainerSelected = false;
						if (_dragHost is ISelectable select)
						{
							isContainerSelected = select.IsSelected;
						}
						else if (_owner.Selection.SelectedIndexes.Contains(_dragHostIndex))
						{
							isContainerSelected = true;
						}

						if (isContainerSelected)
						{
							// If we're moving selected items, we preserve the order and move them one my one
							// Copy the SelectedItems since that get's modified if the source collection clears
							// This will insert the items back in the order they're in the SelectedItems collection
							// which may or may not be chronological order by item index
							// This might differ from WinUI, but it's a small change and simplest.
							object[] selItems = new object[_owner.SelectedItems.Count];
							_owner.SelectedItems.CopyTo(selItems, 0);

							foreach (var sel in selItems)
							{
								list.Remove(sel);
								list.Insert(_lastInsertIndex++, sel);
							}
						}
						else
						{
							var moveItem = list[_dragHostIndex];
							// Otherwise, if we're just moving one item, simple
							list.RemoveAt(_dragHostIndex);
							list.Insert(_lastInsertIndex, moveItem);
						}
					}
				}
				

				_dragHost = null;
				_dragHostIndex = -1;
				_lastInsertIndex = -1;

				_scrollsHorizontal = false;
				_scrollsVertical = false;

				if (_timer != null && _timer.IsEnabled)
				{
					_timer.Stop();
				}

				if (_popup != null)
				{
					_popup.IsOpen = false;
					_popup.Child = null;
				}
			}

			public void HandleDrag(Point currentPoint)
			{
				HandleAutoScroll(currentPoint);

				_popup?.Host.ConfigurePosition(null, PlacementMode.Pointer, new Point());

				if (_scroller != null)
					currentPoint += _scroller.Offset;

				int insertionIndex = -1;
				for (int i = 0; i < _panel.Children.Count; i++)
				{
					if (_panel.Children[i].Classes.Contains(":dragging"))
						continue;

					if (_panel.Children[i].Bounds.Contains(currentPoint))
					{
						insertionIndex = i;
						break;
					}
				}

				if (insertionIndex == -1)
					return;

				if (IsDoingReorder)
				{
					if (_lastInsertIndex > -1 && _dragHostIndex != _lastInsertIndex && insertionIndex == _dragHostIndex)
					{
						_panel.Children.Move(_lastInsertIndex, _dragHostIndex);
						_lastInsertIndex = _dragHostIndex;
					}

					if (insertionIndex != _lastInsertIndex && _dragHostIndex != insertionIndex)
					{
						//Debug.WriteLine($"Move to {insertionIndex}");
						_panel.Children.Move(_lastInsertIndex == -1 ? _dragHostIndex : _lastInsertIndex, insertionIndex);

						_lastInsertIndex = insertionIndex;
					}
				}				
			}

			private void HandleAutoScroll(Point pt)
			{
				if (_scroller == null || (!_scrollsHorizontal && !_scrollsVertical))
					return;

				var normalRect = new Rect(_autoScrollLeft, _autoScrollUp, _autoScrollRight - _autoScrollLeft, _autoScrollDown - _autoScrollUp);
				//Debug.WriteLine($"Normal Rect {normalRect}, Point {pt}, Contains? {normalRect.Contains(pt)}");
				if (normalRect.Contains(pt))
				{
					if (_isAutoScrollingH != 0 || _isAutoScrollingV != 0)
					{
						_timer.Stop();
						_isAutoScrollingH = 0;
						_isAutoScrollingV = 0;
					}

					return;
				}

				if (_timer == null)
				{
					_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(70), DispatcherPriority.Layout, OnAutoScrollTimerRun);
				}

				if (!_timer.IsEnabled)
					_timer.Start();

				if (_scrollsHorizontal && _scrollsVertical)
				{
					_isAutoScrollingH = pt.X < normalRect.Left ? -1 : pt.X > normalRect.Right ? 1 : 0;
					_isAutoScrollingV = pt.Y < normalRect.Top ? -1 : pt.Y > normalRect.Bottom ? 1 : 0;
				}
				else if (_scrollsHorizontal)
				{
					_isAutoScrollingH = pt.X < normalRect.Left ? -1 : pt.X > normalRect.Right ? 1 : 0;
				}
				else if (_scrollsVertical)
				{
					_isAutoScrollingV = pt.Y < normalRect.Top ? -1 : pt.Y > normalRect.Bottom ? 1 : 0;
				}

				//Debug.WriteLine($"Auto Scrolling H {_isAutoScrollingH}, V {_isAutoScrollingV}");
			}

			private void OnAutoScrollTimerRun(object sender, EventArgs e)
			{
				if (_scroller != null)
				{
					if (_isAutoScrollingV == -1)
						_scroller.LineUp();
					else if (_isAutoScrollingV == 1)
						_scroller.LineDown();

					if (_isAutoScrollingH == -1)
						_scroller.LineLeft();
					else if (_isAutoScrollingH == 1)
						_scroller.LineRight();
				}
			}

			public void Dispose()
			{
				_timer?.Stop(); // Just in case
				_timer = null;

				_popup = null;

				if (_dragHost != null)
				{
					((IPseudoClasses)_dragHost.Classes).Set(":dragging", false);
					_dragHost = null;
				}
			}

			private int _lastInsertIndex = -1;
			private IControl _dragHost;
			private int _dragHostIndex = -1;
			private DispatcherTimer _timer;
			private ListViewBase _owner;
			private FAItemsPresenter _presenter;
			private IPanel _panel;
			private ScrollViewer _scroller;
			private double _autoScrollLeft;
			private double _autoScrollUp;
			private double _autoScrollRight;
			private double _autoScrollDown;
			private bool _scrollsHorizontal;
			private bool _scrollsVertical;
			// 0 = no scroll, -1 = scroll up/left, 1 = scroll down/right
			private int _isAutoScrollingH;
			private int _isAutoScrollingV;

			private Popup _popup;
		}
	}
}
