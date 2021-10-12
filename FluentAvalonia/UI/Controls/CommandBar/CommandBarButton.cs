using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using FluentAvalonia.UI.Input;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class CommandBarButton : Button, ICommandBarElement, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarButton);

		public static readonly DirectProperty<CommandBarButton, bool> IsInOverflowProperty =
			AvaloniaProperty.RegisterDirect<CommandBarButton, bool>(nameof(IsInOverflow),
				x => x.IsInOverflow);

		public static readonly StyledProperty<IconElement> IconProperty =
			AvaloniaProperty.Register<CommandBarButton, IconElement>(nameof(Icon));

		public static readonly StyledProperty<string> LabelProperty =
			AvaloniaProperty.Register<CommandBarButton, string>(nameof(Label));

		public static readonly DirectProperty<CommandBarButton, int> DynamicOverflowOrderProperty =
			AvaloniaProperty.RegisterDirect<CommandBarButton, int>(nameof(DynamicOverflowOrder),
				x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

		public static readonly StyledProperty<bool> IsCompactProperty =
			AvaloniaProperty.Register<CommandBarButton, bool>(nameof(IsCompact));

		public bool IsCompact
		{
			get => GetValue(IsCompactProperty);
			set => SetValue(IsCompactProperty, value);
		}

		public bool IsInOverflow
		{
			get => _isInOverflow;
			internal set
			{
				if (SetAndRaise(IsInOverflowProperty, ref _isInOverflow, value))
				{
					PseudoClasses.Set(":overflow", value);
				}
			}
		}

		public IconElement Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public string Label
		{
			get => GetValue(LabelProperty);
			set => SetValue(LabelProperty, value);
		}

		public int DynamicOverflowOrder
		{
			get => _dynamicOverflowOrder;
			set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IconProperty)
			{
				PseudoClasses.Set(":icon", change.NewValue.GetValueOrDefault() != null);
			}
			else if (change.Property == LabelProperty)
			{
				PseudoClasses.Set(":label", change.NewValue.GetValueOrDefault() != null);
			}
			else if (change.Property == FlyoutProperty)
			{
				if (change.OldValue.GetValueOrDefault() is FlyoutBase oldFB)
				{
					oldFB.Closed -= OnFlyoutClosed;
					oldFB.Opened -= OnFlyoutOpened;
				}

				if (change.NewValue.GetValueOrDefault() is FlyoutBase newFB)
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
				PseudoClasses.Set(":hotkey", change.NewValue.GetValueOrDefault() != null);
			}
			else if (change.Property == IsCompactProperty)
			{
				PseudoClasses.Set(":compact", change.NewValue.GetValueOrDefault<bool>());
			}
			else if (change.Property == CommandProperty)
			{
				if (change.OldValue.GetValueOrDefault() is XamlUICommand xamlComOld)
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

				if (change.NewValue.GetValueOrDefault() is XamlUICommand xamlCom)
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

		private bool _isInOverflow;
		private int _dynamicOverflowOrder;
	}
}
