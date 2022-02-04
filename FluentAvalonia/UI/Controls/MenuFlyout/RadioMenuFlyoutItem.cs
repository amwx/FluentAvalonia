using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia;
using System;
using System.Collections.Generic;
using Avalonia.Data;
using System.Diagnostics;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
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

        public static readonly new DirectProperty<RadioMenuFlyoutItem, bool> IsCheckedProperty =
            AvaloniaProperty.RegisterDirect<RadioMenuFlyoutItem, bool>(nameof(IsChecked),
                x => x.IsChecked, (x, v) => x.IsChecked = v);

        /// <summary>
        /// Gets or sets the name that specifies which RadioMenuFlyoutItem controls are mutually exclusive.
        /// </summary>
        public string GroupName
		{
            get => _groupName;
			set
            {
                value ??= string.Empty;
                SetAndRaise(GroupNameProperty, ref _groupName, value);
            }
		}

        public new bool IsChecked
        {
            get => _isChecked;
            set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
        }

        protected internal bool IsCheckedInternal
        {
            get => base.IsChecked;
            set => base.IsChecked = value;
        }

		Type IStyleable.StyleKey => typeof(RadioMenuFlyoutItem);

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsCheckedProperty)
			{
                var newValue = change.NewValue.GetValueOrDefault<bool>();
                if (IsCheckedInternal != newValue)
                {
                    _isSafeUncheck = true;
                    IsCheckedInternal = newValue;
                    _isSafeUncheck = false;
                    UpdateCheckedItemInGroup();
                }
			}
            else if (change.Property == ToggleMenuFlyoutItem.IsCheckedProperty)
            {
                if (!IsCheckedInternal)
                {
                    if (_isSafeUncheck)
                    {
                        // The uncheck is due to another radio button being checked -- that's all right.
                        IsChecked = false;
                    }
                    else
                    {
                        // The uncheck is due to user interaction -- not allowed.
                        IsCheckedInternal = true;
                    }
                }
                else if (!IsChecked)
                {
                    IsChecked = true;
                    UpdateCheckedItemInGroup();
                }
            }
            else if (change.Property == GroupNameProperty)
            {
                // WinUI doesn't do this here, but this fixes an issue where radio items
                // would misbehave if IsChecked as assigned before group
                UpdateCheckedItemInGroup();
            }
		}

        private void UpdateCheckedItemInGroup()
        {
            if (IsChecked)
            {
                var groupName = GroupName;
                if (string.IsNullOrEmpty(groupName))
                    return;

                if (SelectionMap.TryGetValue(groupName, out var group))
                {
                    if (group.TryGetTarget(out var item))
                    {
                        item.IsChecked = false;
                    }

                    SelectionMap[groupName] = new WeakReference<RadioMenuFlyoutItem>(this);
                }
                else
                {
                    SelectionMap.Add(groupName, new WeakReference<RadioMenuFlyoutItem>(this));
                }

            }
        }

		private string _groupName = string.Empty;
        private bool _isChecked = false;
        private bool _isSafeUncheck = false;

		internal static readonly SortedDictionary<string, WeakReference<RadioMenuFlyoutItem>> SelectionMap;
	}
}
