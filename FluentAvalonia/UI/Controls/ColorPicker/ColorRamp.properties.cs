using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines a control displaying a gradient slider for modifying a specific component of a color
	/// </summary>
	public partial class ColorRamp : ColorPickerComponent
    {
		/// <summary>
		/// Defines the <see cref="Orientation"/> property
		/// </summary>
        public static readonly DirectProperty<ColorRamp, Orientation> OrientationProperty =
            AvaloniaProperty.RegisterDirect<ColorRamp, Orientation>(nameof(Orientation),
                x => x.Orientation, (x, v) => x.Orientation = v);

		/// <summary>
		/// Defines the <see cref="BorderBrush"/> property
		/// </summary>
		public static readonly StyledProperty<IBrush> BorderBrushProperty =
			Border.BorderBrushProperty.AddOwner<ColorRamp>();

		/// <summary>
		/// Defines the <see cref="BorderThickness"/> property
		/// </summary>
		public static readonly StyledProperty<double> BorderThicknessProperty =
			AvaloniaProperty.Register<ColorRamp, double>(nameof(BorderThickness), 1d);

		/// <summary>
		/// Defines the <see cref="CornerRadius"/> property
		/// </summary>
		public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
			Border.CornerRadiusProperty.AddOwner<ColorRamp>();
			
		/// <summary>
		/// Gets or sets whether this ColorRamp should display horizontally or vertically
		/// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if(SetAndRaise(OrientationProperty, ref _orientation, value))
                {
                    InvalidateVisual();
                }
            }
        }

		/// <summary>
		/// Gets or sets the corner radius used when rendering this ColorRamp
		/// </summary>
		/// <remarks>
		/// NOTE: only the TopLeft value of the CornerRadius struct is used
		/// </remarks>
		public CornerRadius CornerRadius
		{
			get => GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		/// <summary>
		/// Gets or sets the Brush used to render the border of this ColorRamp
		/// </summary>
		public IBrush BorderBrush
		{
			get => GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}

		/// <summary>
		/// Gets or sets the width of the border of this ColorRamp
		/// </summary>
		public double BorderThickness
		{
			get => GetValue(BorderThicknessProperty);
			set => SetValue(BorderThicknessProperty, value);
		}

		private Orientation _orientation;	
    }
}
