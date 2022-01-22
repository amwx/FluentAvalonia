using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace FluentAvalonia.UI.Controls
{
	public partial class ColorSpectrum
	{
        /// <summary>
        /// Defines the <see cref="Shape"/> property
        /// </summary>
        public static readonly DirectProperty<ColorSpectrum, ColorSpectrumShape> ShapeProperty =
            AvaloniaProperty.RegisterDirect<ColorSpectrum, ColorSpectrumShape>(nameof(Shape),
                x => x.Shape, (x, v) => x.Shape = v);

        /// <summary>
        /// Defines the <see cref="BorderBrush"/> property
        /// </summary>
		public static readonly StyledProperty<IBrush> BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner<ColorSpectrum>();

        /// <summary>
        /// Defines the <see cref="BorderThickness"/> property
        /// </summary>
		public static readonly StyledProperty<double> BorderThicknessProperty =
            ColorRamp.BorderThicknessProperty.AddOwner<ColorSpectrum>();

        /// <summary>
        /// Gets or sets the Brush used to render the border of this ColorSpectrum control
        /// </summary>
		public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the border of this ColorSpectrum control
        /// </summary>
		public double BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this ColorSpectrum should display as the
        /// ColorSpectrum, ColorWheel, or HSV ColorTriangle
        /// </summary>
		public ColorSpectrumShape Shape
        {
            get => _shape;
            set
            {
                if (SetAndRaise(ShapeProperty, ref _shape, value))
                    OnShapeChanged(value);
            }
        }

        private ColorSpectrumShape _shape = ColorSpectrumShape.Spectrum;
    }
}
