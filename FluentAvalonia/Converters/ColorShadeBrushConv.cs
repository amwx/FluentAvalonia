using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Media;
using System;
using System.Globalization;

namespace FluentAvalonia.Converters;

public class ColorShadeBrushConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = (Color2)value;

        float amount;
        if (!float.TryParse(parameter.ToString(), out amount))
        {
            amount = 0;
        }

        return new SolidColorBrush(color.LightenPercent(amount));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
