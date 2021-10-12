using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls
{
	public class CommandBarOverflowPresenter : ItemsControl, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarOverflowPresenter);

		protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
		{
			base.ItemsChanged(e);

			_hasIcons = 0;
			_hasToggle = 0;

			if (e.NewValue is IList l)
			{
				RegisterItems(l);
			}

			UpdateVisualState();
		}

		protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			base.ItemsCollectionChanged(sender, e);

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					RegisterItems(e.NewItems);
					break;

				case NotifyCollectionChangedAction.Remove:
					UnregisterItems(e.OldItems);
					break;

				case NotifyCollectionChangedAction.Reset:					
					// Reset may not be called with item list
					if (e.OldItems != null)
					{
						UnregisterItems(e.OldItems);
					}
					else
					{
						_hasIcons = 0;
						_hasToggle = 0;
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					UnregisterItems(e.OldItems);
					RegisterItems(e.NewItems);
					break;
			}

			UpdateVisualState();
		}

		private void RegisterItems(IList l)
		{
			for (int i = 0; i < l.Count; i++)
			{
				if (l[i] is CommandBarButton cbb)
				{
					if (cbb.Icon != null)
						_hasIcons++;

					cbb.IsInOverflow = true;
				}
				else if (l[i] is CommandBarToggleButton cbtb)
				{
					_hasToggle++;

					if (cbtb.Icon != null)
						_hasIcons++;

					cbtb.IsInOverflow = true;
				}
				else if (l[i] is CommandBarElementContainer cont)
				{
					cont.IsInOverflow = true;
				}
				else if (l[i] is CommandBarSeparator sep)
				{
					sep.IsInOverflow = true;
				}
			}
		}

		private void UnregisterItems(IList l)
		{
			for (int i = 0; i < l.Count; i++)
			{
				if (l[i] is CommandBarButton cbb)
				{
					if (cbb.Icon != null)
						_hasIcons--;

					cbb.IsInOverflow = false;
				}
				else if (l[i] is CommandBarToggleButton cbtb)
				{
					_hasToggle--;

					if (cbtb.Icon != null)
						_hasIcons--;

					cbtb.IsInOverflow = false;
				}
				else if (l[i] is CommandBarElementContainer cont)
				{
					cont.IsInOverflow = false;
				}
				else if (l[i] is CommandBarSeparator sep)
				{
					sep.IsInOverflow = false;
				}
			}
		}

		private void UpdateVisualState()
		{
			PseudoClasses.Set(":icons", _hasIcons > 0);
			PseudoClasses.Set(":toggle", _hasToggle > 0);
		}

		private int _hasIcons;
		private int _hasToggle;
	}
}
