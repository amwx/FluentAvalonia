using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class ToggleMenuFlyoutItem : MenuFlyoutItem, IStyleable
	{
		public static readonly StyledProperty<bool> IsCheckedProperty =
			AvaloniaProperty.Register<ToggleMenuFlyoutItem, bool>(nameof(IsChecked), defaultBindingMode: BindingMode.TwoWay);

		public bool IsChecked
		{
			get => GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		Type IStyleable.StyleKey => typeof(ToggleMenuFlyoutItem);

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsCheckedProperty)
			{
				PseudoClasses.Set(":checked", change.NewValue.GetValueOrDefault<bool>());
			}
		}

		protected override void OnClick()
		{
			base.OnClick();
			IsChecked = !IsChecked;
		}
	}
}
