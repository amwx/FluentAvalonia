using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using FluentAvalonia.UI.Input;
using System;

namespace FluentAvalonia.UI.Controls
{
	public partial class CommandBarButton : Button, ICommandBarElement, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarButton);

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
			else if (change.Property == FlyoutProperty)
			{
                var (oldValue, newValue) = change.GetOldAndNewValue<FlyoutBase>();
				if (oldValue != null)
				{
                    oldValue.Closed -= OnFlyoutClosed;
                    oldValue.Opened -= OnFlyoutOpened;
				}

				if (newValue != null)
				{
                    newValue.Closed += OnFlyoutClosed;
                    newValue.Opened += OnFlyoutOpened;

					PseudoClasses.Set(":flyout", true);
					PseudoClasses.Set(":submenuopen", newValue.IsOpen);
				}
				else
				{
					PseudoClasses.Set(":flyout", false);
					PseudoClasses.Set(":submenuopen", false);
				}				
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

		private void OnFlyoutOpened(object sender, EventArgs e)
		{
			PseudoClasses.Set(":submenuopen", true);
		}

		private void OnFlyoutClosed(object sender, EventArgs e)
		{
			PseudoClasses.Set(":submenuopen", false);
		}
	}
}
