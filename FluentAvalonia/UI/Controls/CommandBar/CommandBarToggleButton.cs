using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Input;

namespace FluentAvalonia.UI.Controls
{
	public class CommandBarToggleButton : ToggleButton, ICommandBarElement, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarToggleButton);

		public static readonly DirectProperty<CommandBarToggleButton, bool> IsInOverflowProperty =
			   AvaloniaProperty.RegisterDirect<CommandBarToggleButton, bool>(nameof(IsInOverflow),
				   x => x.IsInOverflow);

		public static readonly StyledProperty<IconElement> IconProperty =
			AvaloniaProperty.Register<CommandBarToggleButton, IconElement>(nameof(Icon));

		public static readonly StyledProperty<string> LabelProperty =
			AvaloniaProperty.Register<CommandBarToggleButton, string>(nameof(Label));

		public static readonly DirectProperty<CommandBarToggleButton, int> DynamicOverflowOrderProperty =
			AvaloniaProperty.RegisterDirect<CommandBarToggleButton, int>(nameof(DynamicOverflowOrder),
				x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

		public static readonly StyledProperty<bool> IsCompactProperty =
			AvaloniaProperty.Register<CommandBarToggleButton, bool>(nameof(IsCompact));

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

		private bool _isInOverflow;
		private int _dynamicOverflowOrder;
	}
}
