using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Controls
{
	public partial class CommandBarButton : Button, ICommandBarElement, IStyleable
	{
		/// <summary>
		/// Defines the <see cref="IsInOverflow"/> property
		/// </summary>
		public static readonly DirectProperty<CommandBarButton, bool> IsInOverflowProperty =
			AvaloniaProperty.RegisterDirect<CommandBarButton, bool>(nameof(IsInOverflow),
				x => x.IsInOverflow);

		/// <summary>
		/// Defines the <see cref="Icon"/> property
		/// </summary>
		public static readonly StyledProperty<IconElement> IconProperty =
			AvaloniaProperty.Register<CommandBarButton, IconElement>(nameof(Icon));

		/// <summary>
		/// Defines the <see cref="Label"/> property
		/// </summary>
		public static readonly StyledProperty<string> LabelProperty =
			AvaloniaProperty.Register<CommandBarButton, string>(nameof(Label));

		/// <summary>
		/// Defines the <see cref="DynamicOverflowOrder"/> property
		/// </summary>
		public static readonly DirectProperty<CommandBarButton, int> DynamicOverflowOrderProperty =
			AvaloniaProperty.RegisterDirect<CommandBarButton, int>(nameof(DynamicOverflowOrder),
				x => x.DynamicOverflowOrder, (x, v) => x.DynamicOverflowOrder = v);

		/// <summary>
		/// Defines the <see cref="IsCompact"/> property
		/// </summary>
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

		/// <summary>
		/// Gets or sets the graphic content of the app bar toggle button.
		/// </summary>
		public IconElement Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		/// <summary>
		/// Gets or sets the text description displayed on the app bar toggle button.
		/// </summary>
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

		private bool _isInOverflow;
		private int _dynamicOverflowOrder;
	}
}
