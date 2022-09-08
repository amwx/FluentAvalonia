using Avalonia;
using Avalonia.Media;
using System;
using System.ComponentModel;
using System.Globalization;

namespace FluentAvalonia.UI.Controls;

[TypeConverter(typeof(IconSourceConverter))]
public abstract class IconSource : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Foreground"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<IconSource, IBrush>(nameof(Foreground));

    /// <summary>
    /// Gets or sets a brush that describes the foreground color.
    /// </summary>
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
}

/// <summary>
/// Type converter for allowing strings in Xaml to be automatically interpreted as an IconElement
/// </summary>
public class IconSourceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string) || sourceType == typeof(Symbol))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is Symbol symbol)
        {
            return new SymbolIconSource { Symbol = symbol };
        }
        else if (value is IImage img)
        {
            return new ImageIconSource { Source = img };
        }
        else if (value is string val)
        {
            //First we try if the text is a valid Symbol
            if (Enum.TryParse<Symbol>(val, out Symbol sym))
            {
                return new SymbolIconSource() { Symbol = sym };
            }

            //Try a PathIcon
            if (FAPathIcon.IsDataValid(val, out Geometry g))
            {
                return new PathIconSource() { Data = g };
            }

            try
            {
                if (Uri.TryCreate(val, UriKind.RelativeOrAbsolute, out Uri result))
                {
                    return new BitmapIconSource() { UriSource = result };
                }
            }
            catch { }

            //If we've reached this point, we'll make a FontIcon
            //Glyph can be anything (sort of), so we don't need to Try/Catch
            return new FontIconSource() { Glyph = val };

        }
        return base.ConvertFrom(context, culture, value);
    }
}
