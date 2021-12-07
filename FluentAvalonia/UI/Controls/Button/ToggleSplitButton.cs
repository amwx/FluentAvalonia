using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class ToggleSplitButton : SplitButton, IStyleable
	{
		public static readonly DirectProperty<ToggleSplitButton, bool> IsCheckedProperty =
			AvaloniaProperty.RegisterDirect<ToggleSplitButton, bool>(nameof(IsChecked),
				x => x.IsChecked, (x, v) => x.IsChecked = v);

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

		Type IStyleable.StyleKey => typeof(SplitButton);

		public event TypedEventHandler<ToggleSplitButton, ToggleSplitButtonIsCheckedChangedEventArgs> IsCheckedChanged;

		protected override void OnClickPrimary(object sender, RoutedEventArgs e)
		{
			Toggle();

			base.OnClickPrimary(sender, e);
		}

		private void Toggle()
		{
			IsChecked = !IsChecked;
		}

		private void OnIsCheckedChanged()
		{
			if (_hasLoaded)
			{
				var ea = new ToggleSplitButtonIsCheckedChangedEventArgs();
				IsCheckedChanged?.Invoke(this, ea);
			}

			UpdateVisualStates();
		}

		private bool _isChecked;
	}

	public class ToggleSplitButtonIsCheckedChangedEventArgs : EventArgs
	{

	}
}
