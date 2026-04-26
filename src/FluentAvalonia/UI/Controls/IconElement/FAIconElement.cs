using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.ComponentModel;
using System.Globalization;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the base class for an icon UI element.
/// </summary>
[TypeConverter(typeof(IconElementConverter))]
public class FAIconElement : Control
{
    /// <summary>
    /// Defines the <see cref="Foreground"/> property
    /// </summary>
    public static readonly AttachedProperty<IBrush> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<FAIconElement>();

    /// <summary>
    /// Gets or sets a brush that describes the foreground color.
    /// </summary>
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ForegroundProperty)
        {
            InvalidateVisual();
        }
    }
}

/// <summary>
/// Type converter for allowing strings in Xaml to be automatically interpreted as an IconElement
/// </summary>
public class IconElementConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string) || sourceType == typeof(FASymbol) || sourceType == typeof(FAIconSource))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is FASymbol symbol)
        {
            return new FASymbolIcon { Symbol = symbol };
        }
        else if (value is FAIconSource ico)
        {
            if (ico is FAFontIconSource fis)
            {
                return FAIconHelpers.CreateFontIconFromFontIconSource(fis);
            }
            else if (ico is FASymbolIconSource sis)
            {
                return FAIconHelpers.CreateSymbolIconFromSymbolIconSource(sis);
            }
            else if (ico is FAPathIconSource pis)
            {
                return FAIconHelpers.CreatePathIconFromPathIconSource(pis);
            }
            else if (ico is FABitmapIconSource bis)
            {
                return FAIconHelpers.CreateBitmapIconFromBitmapIconSource(bis);
            }
        }
        else if (value is IImage img)
        {
            return new FAImageIcon { Source = img };
        }
        else if (value is string val)
        {
            //First we try if the text is a valid Symbol
            if (Enum.TryParse<FASymbol>(val, out FASymbol sym))
            {
                return new FASymbolIcon() { Symbol = sym };
            }

            //Try a PathIcon
            if (FAPathIcon.IsDataValid(val, out Geometry g))
            {
                return new FAPathIcon() { Data = g };
            }

            try
            {
                if (Uri.TryCreate(val, UriKind.RelativeOrAbsolute, out Uri result))
                {
                    return new FABitmapIcon() { UriSource = result };
                }
            }
            catch { }

            // If we've reached this point, we'll make a FontIcon
            // Glyph can be anything (sort of), so we don't need to Try/Catch
            return new FAFontIcon() { Glyph = val };

        }
        return base.ConvertFrom(context, culture, value);
    }
}
