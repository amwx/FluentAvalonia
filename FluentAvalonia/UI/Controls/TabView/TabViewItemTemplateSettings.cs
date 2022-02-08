using Avalonia;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    public class TabViewItemTemplateSettings : AvaloniaObject
    {
        /// <summary>
        /// Defines the <see cref="IconElement"/> property
        /// </summary>
        public static readonly StyledProperty<IconElement> IconElementProperty =
            AvaloniaProperty.Register<TabViewItemTemplateSettings, IconElement>(nameof(IconElement));
        
        /// <summary>
        /// Defines the <see cref="TabGeometry"/> property
        /// </summary>
        public static readonly StyledProperty<Geometry> TabGeometryProperty =
            AvaloniaProperty.Register<TabViewItemTemplateSettings, Geometry>(nameof(TabGeometry));

        /// <summary>
        /// Gets the IconElement that relates to the IconSource of the current TabViewItem
        /// </summary>
        public IconElement IconElement
        {
            get => GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }

        /// <summary>
        /// Gets the geometry of the current TabViewItem
        /// </summary>
        public Geometry TabGeometry
        {
            get => GetValue(TabGeometryProperty);
            internal set => SetValue(TabGeometryProperty, value);
        }
    }
}
