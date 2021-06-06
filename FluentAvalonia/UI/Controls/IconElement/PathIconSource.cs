using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    public class PathIconSource : IconSource
    {
        public static StyledProperty<Geometry> DataProperty =
            AvaloniaProperty.Register<PathIconSource, Geometry>("Data");

        public Geometry Data
        {
            get => GetValue(DataProperty);
			set => SetValue(DataProperty, value);
        }
    }
}
