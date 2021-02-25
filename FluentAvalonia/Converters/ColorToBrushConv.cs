using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FluentAvalonia.Converters
{
    public class ColorToBrushConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color c)
                return new SolidColorBrush(c);

            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ISolidColorBrush sc)
                return sc.Color;

            return BindingOperations.DoNothing;
        }
    }
}
