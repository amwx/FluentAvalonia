using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines the base class for all component controls of a <see cref="ColorPicker"/>
	/// </summary>
	public  abstract partial class ColorPickerComponent : Control
    {
        /// <summary>
        /// Defines the <see cref="Color"/> property
        /// </summary>
        public static readonly DirectProperty<ColorPickerComponent, Color2> ColorProperty =
            AvaloniaProperty.RegisterDirect<ColorPickerComponent, Color2>(nameof(Color),
                x => x.Color, (x, v) => x.Color = v);

        /// <summary>
        /// Defines the <see cref="Component"/> property
        /// </summary>
        public static readonly DirectProperty<ColorPickerComponent, ColorComponent> ComponentProperty =
            AvaloniaProperty.RegisterDirect<ColorPickerComponent, ColorComponent>(nameof(Component),
                x => x.Component, (x, v) => x.Component = v);

        /// <summary>
        /// Event raised when the <see cref="Color"/> property changes
        /// </summary>
        public event TypedEventHandler<ColorPickerComponent, ColorChangedEventArgs> ColorChanged;

        /// <summary>
        /// Gets or sets the color this component is displaying
        /// </summary>
        public Color2 Color
        {
            get => _color;
            set
            {
                var old = _color;
                if (SetAndRaise(ColorProperty, ref _color, value))
                {
                    if (_color.Hue >= 0)
                        _explicitHue = value.Hue;
                    OnColorChanged(old, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color component (or channel) this control should display
        /// </summary>
        /// <remarks>
        /// The component of color (R,G,B,A,Hue,Sat,Value) the component displays.
        /// In the case of the ColorSpectrum which displays two components at once,
        /// this is set to the third component (i.e., if ColorSpectrum is displaying
        /// Value and Saturation, the Component is set to Hue).
        /// </remarks>
        public ColorComponent Component
        {
            get => _component;
            set
            {
                if (SetAndRaise(ComponentProperty, ref _component, value))
                {
                    OnComponentChanged(value);
                }
            }
        }

        /// <summary>
        /// Stores the last Hue value when hue becomes ambiguous (Saturation/Value = 0)
        /// </summary>
        /// <remarks>
        /// Storing this allows changing the current color to grey, without losing the
        /// original hue
        /// </remarks>
        public int Hue
        {
            get
            {
                if (_color.Hue >= 0)
                    return _color.Hue;
                else
                    return _explicitHue;//Rename this
            }
        }

        private int _explicitHue = -1;//Rename this
        private Color2 _color = Color2.FromHSVf(71, 0.54f, .5f);
        private ColorComponent _component;
    }
}
