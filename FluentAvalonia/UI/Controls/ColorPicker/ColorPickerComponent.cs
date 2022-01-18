using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines the base class for all component controls of a <see cref="ColorPicker"/>
	/// </summary>
	public abstract partial class ColorPickerComponent : Control
    {
        static ColorPickerComponent()
        {
            FocusableProperty.OverrideDefaultValue<ColorPickerComponent>(true);
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
    }
}
