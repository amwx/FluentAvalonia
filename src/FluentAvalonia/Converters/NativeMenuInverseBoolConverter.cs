using System.Globalization;
using Avalonia.Data.Converters;

namespace FluentAvalonia.Converters;

/// <summary>
/// Converter that invert a boolean value. 
/// </summary>
public class NativeMenuInverseBoolConverter : IValueConverter
{
    /// <summary>
    /// It return value if value is null.
    /// </summary>
    public bool Default { get; set; }

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : Default;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : !Default;
    }
}
