using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon source that uses a vector path as its content.
    /// </summary>
    public class PathIconSource : IconSource
    {
        /// <summary>
        /// Defines the <see cref="Data"/> property
        /// </summary>
        public static StyledProperty<Geometry> DataProperty =
            PathIcon.DataProperty.AddOwner<PathIconSource>();

        /// <summary>
        /// Gets or sets a Geometry that specifies the shape to be drawn. 
        /// In XAML. this can also be set using a string that describes Move and draw commands syntax.
        /// </summary>
        public Geometry Data
        {
            get => GetValue(DataProperty);
			set => SetValue(DataProperty, value);
        }
    }
}
