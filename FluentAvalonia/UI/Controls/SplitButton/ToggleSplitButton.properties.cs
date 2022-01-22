using Avalonia;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls
{
	public partial class ToggleSplitButton
	{
		/// <summary>
		/// Defines the <see cref="IsChecked"/> property
		/// </summary>
		public static readonly DirectProperty<ToggleSplitButton, bool> IsCheckedProperty =
			AvaloniaProperty.RegisterDirect<ToggleSplitButton, bool>(nameof(IsChecked),
				x => x.IsChecked, (x, v) => x.IsChecked = v);

		/// <summary>
		/// Gets or sets whether the ToggleSplitButton is checked.
		/// </summary>
		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				SetAndRaise(IsCheckedProperty, ref _isChecked, value);
				OnIsCheckedChanged();
			}
		}

		internal override bool InternalIsChecked => IsChecked;

		public event TypedEventHandler<ToggleSplitButton, ToggleSplitButtonIsCheckedChangedEventArgs> IsCheckedChanged;

		private bool _isChecked;
	}
}
