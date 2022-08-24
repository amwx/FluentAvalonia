using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
				PseudoClasses.Set(":icon", change.NewValue != null);
			}
			else if (change.Property == LabelProperty)
			{
				PseudoClasses.Set(":label", change.NewValue != null);
			}
			else if (change.Property == FlyoutProperty)
			{
				if (change.OldValue is FlyoutBase oldFB)
				{
					oldFB.Closed -= OnFlyoutClosed;
					oldFB.Opened -= OnFlyoutOpened;
				}

				if (change.NewValue is FlyoutBase newFB)
				{
					newFB.Closed += OnFlyoutClosed;
					newFB.Opened += OnFlyoutOpened;

					PseudoClasses.Set(":flyout", true);
					PseudoClasses.Set(":submenuopen", newFB.IsOpen);
				}
				else
				{
					PseudoClasses.Set(":flyout", false);
					PseudoClasses.Set(":submenuopen", false);
				}				
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
