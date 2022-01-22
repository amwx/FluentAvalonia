using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a button with two parts that can be invoked separately. One part behaves like a toggle button and the other part invokes a flyout.
	/// </summary>
	public partial class ToggleSplitButton : SplitButton, IStyleable
	{
		Type IStyleable.StyleKey => typeof(SplitButton);
				
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
	}
}
