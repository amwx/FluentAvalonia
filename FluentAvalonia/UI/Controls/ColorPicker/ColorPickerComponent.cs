using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.UI.Controls
{
    public abstract class ColorPickerComponent : Control
    {

        static ColorPickerComponent()
        {
            FocusableProperty.OverrideDefaultValue<ColorPickerComponent>(true);
        }

        public static readonly DirectProperty<ColorPickerComponent, Color2> ColorProperty =
            AvaloniaProperty.RegisterDirect<ColorPickerComponent, Color2>(nameof(Color),
                x => x.Color, (x, v) => x.Color = v);

        public static readonly DirectProperty<ColorPickerComponent, ColorComponent> ComponentProperty =
            AvaloniaProperty.RegisterDirect<ColorPickerComponent, ColorComponent>(nameof(Component),
                x => x.Component, (x, v) => x.Component = v);

        public event TypedEventHandler<ColorPickerComponent, ColorChangedEventArgs> ColorChanged;

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
        /// The component of color (R,G,B,A,Hue,Sat,Value) the component displays.
        /// In the case of the ColorSpectrum which displays two components at once,
        /// this is set to the third component (i.e. if ColorSpectrum is displaying
        /// Value and Saturation, the Component is set to Hue).
        /// </summary>
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

        //When Hue becomes ambiguous (Saturation or Value = 0), we need to save it
        //So that the color can switch to Grey without losing the original Hue
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

        protected virtual void OnColorChanged(Color2 oldColor, Color2 newColor)
        {
            ColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
        }

        protected virtual void OnComponentChanged(ColorComponent newValue)
        {
            InvalidateVisual();
        }

        protected int ComponentRange
        {
            get
            {
                if (_component == ColorComponent.Hue)
                {
                    return 359;
                }
                else if (_component == ColorComponent.Saturation || _component == ColorComponent.Value)
                {
                    return 100;
                }
                else
                {
                    return 255;
                }
            }
        }

        private int _explicitHue = -1;//Rename this
        private Color2 _color = Color2.FromHSVf(71, 0.54f, .5f);
        private ColorComponent _component;
    }

    public sealed class ColorChangedEventArgs
    {
        public Color2 OldColor { get; }
        public Color2 NewColor { get; }

        public ColorChangedEventArgs(Color2 oldC, Color2 newC)
        {
            OldColor = oldC;
            NewColor = newC;
        }
    }
}
