using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FluentAvaloniaSamples.Converters;

public class InverseBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color? col = value is Color ? (Color)value : (value is ISolidColorBrush sb ? sb.Color : null);

        if (!col.HasValue)
        {
            if (Color.TryParse(value.ToString(), out var c))
                col = c;
        }

        if (col.HasValue)
        {
            var rg = col.Value.R <= 10 ? col.Value.R / 3294.0 : Math.Pow(col.Value.R / 269.0 + 0.0513, 2.4);
            var gg = col.Value.G <= 10 ? col.Value.G / 3294.0 : Math.Pow(col.Value.G / 269.0 + 0.0513, 2.4);
            var bg = col.Value.B <= 10 ? col.Value.B / 3294.0 : Math.Pow(col.Value.B / 269.0 + 0.0513, 2.4);


            var val = 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;

            return val > 0.5 ? Brushes.Black : Brushes.White;
        }
        else
        {
            return BindingOperations.DoNothing;// AvaloniaProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
