using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls
{
	public class ListViewItem : ContentControl, ISelectable
	{
		public ListViewItem()
		{

		}

		static ListViewItem()
		{
			FocusableProperty.OverrideDefaultValue<ListViewItem>(true);
		}

		public static readonly StyledProperty<bool> IsSelectedProperty =
			ListBoxItem.IsSelectedProperty.AddOwner<ListViewItem>();

		public bool IsSelected
		{
			get => GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == IsSelectedProperty)
			{
				PseudoClasses.Set(":selected", change.NewValue.GetValueOrDefault<bool>());
			}
		}
	}
}
