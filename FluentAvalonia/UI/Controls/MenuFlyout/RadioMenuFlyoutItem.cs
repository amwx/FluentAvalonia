using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
	// WinUI does something weird here, where they derive publically from MenuFlyoutItem
	// but secretly behind the scenes derive from ToggleMenuFlyoutItem
	// Why? Who knows, just derive straight from ToggleMenuFlyoutItem
	// Either way, I can't make sense of what they actually do, so this is just a simple
	// radiobutton approach

	/// <summary>
	/// Represents a menu item that is mutually exclusive with other radio menu items in its group.
	/// </summary>
	public class RadioMenuFlyoutItem : ToggleMenuFlyoutItem, IStyleable
	{
		static RadioMenuFlyoutItem()
		{
			if (SelectionMap == null)
			{
				SelectionMap = new SortedDictionary<string, WeakReference<RadioMenuFlyoutItem>>();
			}
		}

		/// <summary>
		/// Defines the <see cref="GroupName"/> property
		/// </summary>
		public static readonly DirectProperty<RadioMenuFlyoutItem, string> GroupNameProperty =
			RadioButton.GroupNameProperty.AddOwner<RadioMenuFlyoutItem>(x => x.GroupName,
				(x, v) => x.GroupName = v);

		/// <summary>
		/// Gets or sets the name that specifies which RadioMenuFlyoutItem controls are mutually exclusive.
		/// </summary>
		public string GroupName
		{
			get => _groupName;
			set => SetAndRaise(GroupNameProperty, ref _groupName, value);
		}

		Type IStyleable.StyleKey => typeof(RadioMenuFlyoutItem);

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsCheckedProperty)
			{
				OnIsCheckedChanged(change.NewValue.GetValueOrDefault<bool>());
			}
		}

		private void OnIsCheckedChanged(bool check)
		{
			if (check)
			{
				if (SelectionMap.TryGetValue(_groupName, out var oldChecked))
				{
					if (oldChecked.TryGetTarget(out var target))
					{
						target.IsChecked = false;
					}

					SelectionMap[_groupName] = new WeakReference<RadioMenuFlyoutItem>(this);
				}
				else
				{
					SelectionMap.Add(_groupName, new WeakReference<RadioMenuFlyoutItem>(this));
				}
			}
		}

		private string _groupName = string.Empty;

		internal static readonly SortedDictionary<string, WeakReference<RadioMenuFlyoutItem>> SelectionMap;
	}
}
