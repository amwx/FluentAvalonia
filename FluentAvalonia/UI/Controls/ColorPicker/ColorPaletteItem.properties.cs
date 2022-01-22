using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace FluentAvalonia.UI.Controls
{
	public partial class ColorPaletteItem
	{
		/// <summary>
		/// Defines the <see cref="Color"/> property
		/// </summary>
		public static readonly DirectProperty<ColorPaletteItem, Color> ColorProperty =
			AvaloniaProperty.RegisterDirect<ColorPaletteItem, Color>(nameof(Color),
				x => x.Color, (x, v) => x.Color = v);

		/// <summary>
		/// Defines the <see cref="BorderBrush"/> property
		/// </summary>
		public static readonly StyledProperty<IBrush> BorderBrushProperty =
			Border.BorderBrushProperty.AddOwner<ColorPaletteItem>();

		/// <summary>
		/// Defines the <see cref="BorderBrushPointerOver"/> property
		/// </summary>
		public static readonly StyledProperty<IBrush> BorderBrushPointerOverProperty =
		 AvaloniaProperty.Register<ColorPaletteItem, IBrush>(nameof(BorderBrushPointerOver));

		/// <summary>
		/// Defines the <see cref="BorderBrushPressed"/> property
		/// </summary>
		public static readonly StyledProperty<IBrush> BorderBrushPressedProperty =
		 AvaloniaProperty.Register<ColorPaletteItem, IBrush>(nameof(BorderBrushPressed));

		/// <summary>
		/// Defines the <see cref="BorderThickness"/> property
		/// </summary>
		public static readonly StyledProperty<Thickness> BorderThicknessProperty =
			Border.BorderThicknessProperty.AddOwner<ColorPaletteItem>();

		/// <summary>
		/// Defines the <see cref="BorderThicknessPointerOver"/> property
		/// </summary>
		public static readonly StyledProperty<Thickness> BorderThicknessPointerOverProperty =
		 AvaloniaProperty.Register<ColorPaletteItem, Thickness>(nameof(BorderThicknessPointerOver));

		/// <summary>
		/// Defines the <see cref="BorderThicknessPressed"/> property
		/// </summary>
		public static readonly StyledProperty<Thickness> BorderThicknessPressedProperty =
		 AvaloniaProperty.Register<ColorPaletteItem, Thickness>(nameof(BorderThicknessPressed));

		/// <summary>
		/// Defines the <see cref="CornerRadius"/> property
		/// </summary>
		public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
			Border.CornerRadiusProperty.AddOwner<ColorPaletteItem>();

		/// <summary>
		/// Gets or sets the color this item should represent
		/// </summary>
		public Color Color
		{
			get => _color;
			set
			{
				if (SetAndRaise(ColorProperty, ref _color, value))
				{
					if (_colorBrush == null)
						_colorBrush = new SolidColorBrush();

					_colorBrush.Color = _color;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Border Brush this item should use when rendering
		/// </summary>
		public IBrush BorderBrush
		{
			get => GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}

		/// <summary>
		/// Gets or sets the Border Brush this item should use when rendering and the
		/// pointer is over the item
		/// </summary>
		public IBrush BorderBrushPointerOver
		{
			get => GetValue(BorderBrushPointerOverProperty);
			set => SetValue(BorderBrushPointerOverProperty, value);
		}

		/// <summary>
		/// Gets or sets the Border Brush this item should use when rendering and the
		/// pointer is pressed on the item
		/// </summary>
		public IBrush BorderBrushPressed
		{
			get => GetValue(BorderBrushPressedProperty);
			set => SetValue(BorderBrushPressedProperty, value);
		}

		/// <summary>
		/// Gets or sets the Border Thickness this item should use when rendering
		/// </summary>
		public Thickness BorderThickness
		{
			get => GetValue(BorderThicknessProperty);
			set => SetValue(BorderThicknessProperty, value);
		}

		/// <summary>
		/// Gets or sets the Border Thickness this item should use when rendering and the
		/// pointer is over th item
		/// </summary>
		public Thickness BorderThicknessPointerOver
		{
			get => GetValue(BorderThicknessPointerOverProperty);
			set => SetValue(BorderThicknessPointerOverProperty, value);
		}

		/// <summary>
		/// Gets or sets the Border Thickness this item should use when rendering and the
		/// pointer is pressed on the item
		/// </summary>
		public Thickness BorderThicknessPressed
		{
			get => GetValue(BorderThicknessPressedProperty);
			set => SetValue(BorderThicknessPressedProperty, value);
		}

		/// <summary>
		/// Gets or sets the Corner Radius this item should use when rendering
		/// </summary>
		public CornerRadius CornerRadius
		{
			get => GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		private Color _color;
	}
}
