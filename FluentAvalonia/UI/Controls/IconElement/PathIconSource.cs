using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    public class PathIconSource : IconSource
    {
        public PathIconSource()
        {

        }

        public static DirectProperty<PathIconSource, Geometry> DataProperty =
            AvaloniaProperty.RegisterDirect<PathIconSource, Geometry>("Data",
                x => x.Data, (x,v) => x.Data = v);

        public Geometry Data
        {
            get => _data;
            set 
            { 
                SetAndRaise(DataProperty, ref _data, value);
            }
        }
               

        private Geometry _data;
    }
}
