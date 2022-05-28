using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a specialized command bar that provides layout for CommandBarButton and related command elements.
	/// </summary>
	public partial class CommandBar : ContentControl
	{
		public CommandBar()
		{
			PrimaryCommands = new AvaloniaList<ICommandBarElement>();
			SecondaryCommands = new AvaloniaList<ICommandBarElement>();

			_primaryCommands.CollectionChanged += OnPrimaryCommandsChanged;
			_secondaryCommands.CollectionChanged += OnSecondaryCommandsChanged;

			// Don't initialize the actual item lists here, we'll do that as needed

			PseudoClasses.Add(":dynamicoverflow");
			PseudoClasses.Add(":compact");
			PseudoClasses.Add(":labelbottom");
		}
				
		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			_appliedTemplate = false;

			base.OnApplyTemplate(e);

			_primaryItemsHost = e.NameScope.Find<ItemsControl>("PrimaryItemsControl");
			_contentHost = e.NameScope.Find<ContentControl>("ContentControl");

			_overflowItemsHost = e.NameScope.Find<CommandBarOverflowPresenter>("SecondaryItemsControl");

			_moreButton = e.NameScope.Find<Button>("MoreButton");
			_moreButton.Click += OnMoreButtonClick;
			_appliedTemplate = true;

			AttachItems();
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == DefaultLabelPositionProperty)
			{
				var newVal = change.GetNewValue<CommandBarDefaultLabelPosition>();
				PseudoClasses.Set(":labelright", newVal == CommandBarDefaultLabelPosition.Right);
				PseudoClasses.Set(":labelbottom", newVal == CommandBarDefaultLabelPosition.Bottom);
				PseudoClasses.Set(":labelcollapsed", newVal == CommandBarDefaultLabelPosition.Collapsed);
			}
			else if (change.Property == ClosedDisplayModeProperty)
			{
				var newVal = change.GetNewValue<CommandBarClosedDisplayMode>();
				PseudoClasses.Set(":compact", newVal == CommandBarClosedDisplayMode.Compact);
				PseudoClasses.Set(":minimal", newVal == CommandBarClosedDisplayMode.Minimal);
				PseudoClasses.Set(":hidden", newVal == CommandBarClosedDisplayMode.Hidden);
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (_isDynamicOverflowEnabled)
			{
				if (!_moreButton.IsVisible)
					_moreButton.IsVisible = true;

				var sz = base.MeasureOverride(Size.Infinity);

				if (_primaryCommands.Count == 0)
				{
					_moreButton.IsVisible = true;
					_overflowSeparator.IsVisible = false;
					return sz;
				}

				double availWid = availableSize.Width;
				// TODO: May need special case if CommandBar is returned to a container
				// with infinite width and items are in overflow, but this seems like 
				// a very rare, specific thing, so I'm not gonna worry about that here.

				// 5px is to give us just a little more space
				var availWidForItems = availableSize.Width - 
					(_contentHost != null ? _contentHost.DesiredSize.Width : 0) - 
					_moreButton.DesiredSize.Width - 5;

				if (_minRecoverWidth < availWidForItems && _numInOverflow > 0)
				{
					double trackWid = _primaryItemsHost.DesiredSize.Width;
					while (_numInOverflow > 0)
					{
						var items = GetReturnToPrimaryItems();
						double groupWid = 0;

						for (int i = 0; i < items.Count; i++)
						{
							groupWid += _widthCache[items[i]];

							_overflowItems.Remove(items[i]);
							var originalIndex = Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(items[i]));
							_primaryItems.Insert(originalIndex, items[i]);
							_numInOverflow--;

							// Unhide
							if (items[i] is CommandBarSeparator sep)
								sep.IsVisible = true;
						}

						trackWid += groupWid;
						_minRecoverWidth += groupWid;

						if (trackWid >= availWidForItems)
							break;
					}
				}
				else if (_primaryItemsHost.DesiredSize.Width > availWidForItems)
				{
					// Move to Overflow
					double trackWid = 0;

					while (_primaryItemsHost.DesiredSize.Width - trackWid > availWidForItems)
					{
						var items = GetNextItemsToOverflow();
						if (items != null)
						{
							for (int i = 0; i < items.Count; i++)
							{
								var itemAsIControl = items[i] as IControl;
								UpdateWidthCacheForItem(items[i], itemAsIControl.DesiredSize.Width);

								trackWid += itemAsIControl.DesiredSize.Width;

								_primaryItems.Remove(items[i]);
								_overflowItems.Insert(_numInOverflow, items[i]);
								_numInOverflow++;

								// WinUI hides toplevel separartors when the go into overflow
								if (items[i] is CommandBarSeparator sep)
									sep.IsVisible = false;
							}
						}
						else
							break;
					}
					_minRecoverWidth = _primaryItemsHost.DesiredSize.Width;// + trackWid;
				}

				_overflowSeparator.IsVisible = _numInOverflow > 0 && SecondaryCommands.Count > 0;
			}

			var overflowVis = OverflowButtonVisibility;
			if (overflowVis == CommandBarOverflowButtonVisibility.Auto)
			{
				_moreButton.IsVisible = _overflowItems != null && (_isDynamicOverflowEnabled ? _overflowItems.Count > 1 : _overflowItems.Count > 0);						
			}
			else
			{
				_moreButton.IsVisible = overflowVis == CommandBarOverflowButtonVisibility.Visible;
			}
						
			return base.MeasureOverride(availableSize);
		}


		protected virtual void OnOpening() 
		{
			Opening?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnClosing() 
		{
			Closing?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnOpened() 
		{
			if (_overflowItems != null)
			{
				// TODO: Focus via keyboard
				if (_overflowItems.Count > 0)
				{
					(_overflowItems[0] as IControl).Focus();
				}
			}	

			Opened?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnClosed() 
		{
			Closed?.Invoke(this, EventArgs.Empty);
			_moreButton?.Focus();
		}

		private void OnPrimaryCommandsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_appliedTemplate)
				return;

			if (_isDynamicOverflowEnabled)
			{
				// While not the most performant or best solution, we return all Overflowed items back
				// to the primary list. This probably isn't a huge deal since you probably aren't 
				// modifying these items too often, but it avoid a lot of complexities with items in
				// both lists b/c of DynamicOverflow. The number of items should be fairly small too
				// so it really shouldn't matter.
				ReturnOverflowToPrimary();				
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:		
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						if (e.NewItems[i] is ICommandBarElement ele)
						{
							if (ele.DynamicOverflowOrder != 0)
								_hasOrderedOverflow++;
						}
					}
					_primaryItems.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
					break;

				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						if (e.OldItems[i] is ICommandBarElement ele)
						{
							if (ele.DynamicOverflowOrder != 0)
								_hasOrderedOverflow--;
						}
					}
					_primaryItems.RemoveAll(e.OldItems.Cast<ICommandBarElement>());
					break;

				case NotifyCollectionChangedAction.Reset:
					_hasOrderedOverflow = 0;
					_primaryItems.Clear();
					break;

				case NotifyCollectionChangedAction.Replace:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						if (e.OldItems[i] is ICommandBarElement ele)
						{
							if (ele.DynamicOverflowOrder != 0)
								_hasOrderedOverflow--;
						}
					}
					_primaryItems.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
					_primaryItems.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						if (e.NewItems[i] is ICommandBarElement ele)
						{
							if (ele.DynamicOverflowOrder != 0)
								_hasOrderedOverflow++;
						}
					}
					break;

				case NotifyCollectionChangedAction.Move:
					_primaryItems.Move(e.OldStartingIndex, e.NewStartingIndex);
					break;
			}

			PseudoClasses.Set(":primaryonly", _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
			PseudoClasses.Set(":secondaryonly", _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
			InvalidateMeasure();
		}

		private void OnSecondaryCommandsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_appliedTemplate)
				return;

			// TODO: Test that this works...
			int startIndex = _numInOverflow == 0 ? 0 : _numInOverflow + 1;
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						_overflowItems.Insert(e.NewStartingIndex + i + startIndex, e.NewItems[i] as ICommandBarElement);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						_overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					_overflowItems.RemoveRange(startIndex, _overflowItems.Count - startIndex);
					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						_overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
					}
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						_overflowItems.Insert(e.NewStartingIndex + i + startIndex, e.NewItems[i] as ICommandBarElement);
					}
					break;
			}

			PseudoClasses.Set(":primaryonly", _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
			PseudoClasses.Set(":secondaryonly", _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
		}

		private void AttachItems()
		{
			if (_primaryCommands.Count > 0)
			{
				_primaryItems = new AvaloniaList<ICommandBarElement>();
				_primaryItems.AddRange(_primaryCommands);

				if (_isDynamicOverflowEnabled)
				{
					for (int i = 0; i < _primaryItems.Count; i++)
					{
						if (_primaryItems[i].DynamicOverflowOrder != 0)
							_hasOrderedOverflow++;
					}
				}

				_primaryItemsHost.Items = _primaryItems;
			}

			if (_secondaryCommands.Count > 0 || IsDynamicOverflowEnabled)
			{
				_overflowSeparator = new CommandBarSeparator();

				_overflowItems = new AvaloniaList<ICommandBarElement>();
				_overflowItems.Add(_overflowSeparator);
				_overflowItems.AddRange(_secondaryCommands);

				_overflowItemsHost.Items = _overflowItems;
			}

			PseudoClasses.Set(":primaryonly", _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
			PseudoClasses.Set(":secondaryonly", _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
		}
			
		private void ReturnOverflowToPrimary()
		{
			for (int i = _numInOverflow - 1; i >= 0; i--)
			{
				var item = _overflowItems[i];
				_overflowItems.RemoveAt(i);
				_primaryItems.Insert(Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(item)), item);				
			}
			_numInOverflow = 0;
		}

		private void OnMoreButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			IsOpen = !IsOpen;
		}

		private IList<ICommandBarElement> GetNextItemsToOverflow()
		{
			if (_hasOrderedOverflow > 0)
			{
				if (_primaryItems.Count == 0)
					return null;

				// TODO: Don't loop over this multiple times...
				List<ICommandBarElement> l = new List<ICommandBarElement>(2);
				int nextOverflowOrder = int.MaxValue;
				bool hasAnotherOrder = false;
				for (int i = _primaryItems.Count - 1; i >= 0; i--)
				{
					if (_primaryItems[i].DynamicOverflowOrder == 0)
						continue;

					hasAnotherOrder = true;
					nextOverflowOrder = Math.Min(nextOverflowOrder, _primaryItems[i].DynamicOverflowOrder);
				}

				if (hasAnotherOrder)
				{
					for (int i = 0; i < _primaryItems.Count; i++)
					{
						if (_primaryItems[i].DynamicOverflowOrder == nextOverflowOrder)
						{
							l.Add(_primaryItems[i]);
						}
					}

					return l;
				}
				else
				{
					return new[] { _primaryItems[_primaryItems.Count - 1] };
				}
			}
			else
			{
				if (_primaryItems.Count == 0)
					return null;

				return new[] { _primaryItems[_primaryItems.Count - 1] };
			}
		}

		private IList<ICommandBarElement> GetReturnToPrimaryItems()
		{
			if (_overflowItems[_numInOverflow - 1].DynamicOverflowOrder == 0)
				return new[] { _overflowItems[_numInOverflow - 1] };

			int currentGroup = _overflowItems[_numInOverflow - 1].DynamicOverflowOrder;

			int count = 1;
			for (int i = _numInOverflow - 2; i >= 0; i--)
			{
				if (_overflowItems[i].DynamicOverflowOrder == currentGroup)
				{
					count++;
					continue;
				}
				break;
			}

			return _overflowItems.GetRange(_numInOverflow - count, count).ToList();
		}

		private void UpdateWidthCacheForItem(ICommandBarElement item, double wid)
		{
			if (_widthCache == null)
				_widthCache = new Dictionary<ICommandBarElement, double>();

			if (_widthCache.ContainsKey(item))
				_widthCache[item] = wid;
			else
				_widthCache.Add(item, wid);
		}

		private bool _appliedTemplate = false;

		// These are the actual lists sent to the Items Controls
		// We don't want to move items in the actual lists to not
		// interfere with what user specified
		private AvaloniaList<ICommandBarElement> _primaryItems;
		private AvaloniaList<ICommandBarElement> _overflowItems;

		private ItemsControl _primaryItemsHost;
		private CommandBarOverflowPresenter _overflowItemsHost;
		private ContentControl _contentHost;
		private Button _moreButton;

		private CommandBarSeparator _overflowSeparator;

		private int _hasOrderedOverflow = 0;
		private Dictionary<ICommandBarElement, double> _widthCache;
		private int _numInOverflow = 0;
		private double _minRecoverWidth;
	}
}
