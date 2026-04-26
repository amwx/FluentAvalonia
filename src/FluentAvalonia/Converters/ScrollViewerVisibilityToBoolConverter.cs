using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace FluentAvalonia.Converters;

/// <summary>
/// Special converter to convert ScrollBarVisbility enum values to bool
/// </summary>
public class ScrollViewerVisibilityToBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((ScrollBarVisibility)value) == ScrollBarVisibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
