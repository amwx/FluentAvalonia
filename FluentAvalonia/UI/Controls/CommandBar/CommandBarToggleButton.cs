using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Input;
using Avalonia.Input;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a button control that can switch states and be displayed in a CommandBar.
	/// </summary>
	public partial class CommandBarToggleButton : ToggleButton, ICommandBarElement, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarToggleButton);

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IconProperty)
			{
				PseudoClasses.Set(":icon", change.GetNewValue<IconElement>() != null);
			}
			else if (change.Property == LabelProperty)
			{
				PseudoClasses.Set(":label", change.GetNewValue<string>() != null);
			}
			else if (change.Property == HotKeyProperty)
			{
				PseudoClasses.Set(":hotkey", change.GetNewValue<KeyGesture>() != null);
			}
			else if (change.Property == IsCompactProperty)
			{
				PseudoClasses.Set(":compact", change.GetNewValue<bool>());
			}
			else if (change.Property == CommandProperty)
			{
                var (oldValue, newValue) = change.GetOldAndNewValue<XamlUICommand>();

				if (oldValue != null)
				{
					if (Label == oldValue.Label)
					{
						Label = null;
					}
					if (Icon is IconSourceElement ise && ise.IconSource == oldValue.IconSource)
					{
						Icon = null;
					}

					if (HotKey == oldValue.HotKey)
					{
						HotKey = null;
					}

					if (ToolTip.GetTip(this).ToString() == oldValue.Description)
					{
						ToolTip.SetTip(this, null);
					}
				}

				if (newValue != null)
				{
					if (string.IsNullOrEmpty(Label))
					{
						Label = newValue.Label;
					}

					if (Icon == null)
					{
						Icon = new IconSourceElement { IconSource = newValue.IconSource };
					}

					if (HotKey == null)
					{
						HotKey = newValue.HotKey;
					}

					if (ToolTip.GetTip(this) == null)
					{
						ToolTip.SetTip(this, newValue.Description);
					}
				}
			}
		}

		protected override void OnClick()
		{
			base.OnClick();
			if (IsInOverflow)
			{
				var cb = this.FindLogicalAncestorOfType<CommandBar>();
				if (cb != null)
				{
					cb.IsOpen = false;
				}
			}
		}
	}
}
