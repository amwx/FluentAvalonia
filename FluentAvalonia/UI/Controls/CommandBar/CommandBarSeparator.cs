using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
	public class CommandBarSeparator : TemplatedControl, ICommandBarElement
	{
		/// <summary>
		/// Defines the <see cref="IsInOverflow"/> property
		/// </summary>
		public static readonly DirectProperty<CommandBarSeparator, bool> IsInOverflowProperty =
			   AvaloniaProperty.RegisterDirect<CommandBarSeparator, bool>(nameof(IsInOverflow),
				   x => x.IsInOverflow);

		/// <summary>
		/// Defines the <see cref="DynamicOverflowOrder"/> property
		/// </summary>
		public static readonly DirectProperty<CommandBarSeparator, int> DynamicOverflowOrderProperty =
			AvaloniaProperty.RegisterDirect<CommandBarSeparator, int>(nameof(DynamicOverflowOrder),
				x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

		/// <summary>
		/// Defines the <see cref="IsCompact"/> property
		/// </summary>
		public static readonly StyledProperty<bool> IsCompactProperty =
			AvaloniaProperty.Register<CommandBarSeparator, bool>(nameof(IsCompact));

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
