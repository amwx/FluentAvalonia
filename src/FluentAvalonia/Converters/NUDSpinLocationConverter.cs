using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace FluentAvalonia.Converters;

/// <summary>
/// Special converter for the NumericUpDown to pass info from the NumericUpDown to the textbox
/// </summary>
public class NUDSpinLocationConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return BindingOperations.DoNothing;

        var loc = Unsafe.Unbox<Location>(value);
        return loc == Location.Right;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
