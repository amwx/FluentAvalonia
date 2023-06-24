using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FAControlsGallery.Converters;
public class ResourceKeyToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Application.Current.TryGetResource(value, null, out var icon))
        {
            return icon;
        }

        return null;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
