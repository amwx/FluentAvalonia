using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class CommandBarElementContainer : ContentControl, ICommandBarElement, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CommandBarElementContainer);

		public static readonly DirectProperty<CommandBarElementContainer, bool> IsInOverflowProperty =
			AvaloniaProperty.RegisterDirect<CommandBarElementContainer, bool>(nameof(IsInOverflow),
				   x => x.IsInOverflow);

		public static readonly DirectProperty<CommandBarElementContainer, int> DynamicOverflowOrderProperty =
			AvaloniaProperty.RegisterDirect<CommandBarElementContainer, int>(nameof(DynamicOverflowOrder),
				x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

		public static readonly StyledProperty<bool> IsCompactProperty =
			AvaloniaProperty.Register<CommandBarElementContainer, bool>(nameof(IsCompact));
		
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

		public int DynamicOverflowOrder
		{
			get => _dynamicOverflowOrder;
			set => SetAndRaise(DynamicOverflowOrderProperty, ref _dynamicOverflowOrder, value);
		}

		private bool _isInOverflow;
		private int _dynamicOverflowOrder;
	}
}
