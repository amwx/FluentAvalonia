using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents an item in a <see cref="MenuFlyout"/> that a user can change 
	/// between two states, checked or unchecked.
	/// </summary>
	public class ToggleMenuFlyoutItem : MenuFlyoutItem, IStyleable
	{
		/// <summary>
		/// Defines the <see cref="IsChecked"/> Property
		/// </summary>
		public static readonly StyledProperty<bool> IsCheckedProperty =
			AvaloniaProperty.Register<ToggleMenuFlyoutItem, bool>(nameof(IsChecked), 
                defaultBindingMode: BindingMode.TwoWay);

		/// <summary>
		/// Gets or sets whether the ToggleMenuFlyoutItem is checked.
		/// </summary>
		public bool IsChecked
		{
			get => GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		Type IStyleable.StyleKey => typeof(ToggleMenuFlyoutItem);

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsCheckedProperty)
			{
				PseudoClasses.Set(":checked", change.GetNewValue<bool>());
			}
		}

		protected override void OnClick()
		{
			base.OnClick();
			IsChecked = !IsChecked;
		}
	}
}
