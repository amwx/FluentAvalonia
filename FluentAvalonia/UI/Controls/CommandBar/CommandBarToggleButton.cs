using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Input;

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
				PseudoClasses.Set(":icon", change.NewValue != null);
			}
			else if (change.Property == LabelProperty)
			{
				PseudoClasses.Set(":label", change.NewValue != null);
			}
			else if (change.Property == HotKeyProperty)
			{
				PseudoClasses.Set(":hotkey", change.NewValue != null);
			}
			else if (change.Property == IsCompactProperty)
			{
				PseudoClasses.Set(":compact", change.GetNewValue<bool>());
			}
			else if (change.Property == CommandProperty)
			{
				if (change.OldValue is XamlUICommand xamlComOld)
				{
					if (Label == xamlComOld.Label)
					{
						Label = null;
					}
					if (Icon is IconSourceElement ise && ise.IconSource == xamlComOld.IconSource)
					{
						Icon = null;
					}

					if (HotKey == xamlComOld.HotKey)
					{
						HotKey = null;
					}

					if (ToolTip.GetTip(this).ToString() == xamlComOld.Description)
					{
						ToolTip.SetTip(this, null);
					}
				}

				if (change.NewValue is XamlUICommand xamlCom)
				{
					if (string.IsNullOrEmpty(Label))
					{
						Label = xamlCom.Label;
					}

					if (Icon == null)
					{
						Icon = new IconSourceElement { IconSource = xamlCom.IconSource };
					}

					if (HotKey == null)
					{
						HotKey = xamlCom.HotKey;
					}

					if (ToolTip.GetTip(this) == null)
					{
						ToolTip.SetTip(this, xamlCom.Description);
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
