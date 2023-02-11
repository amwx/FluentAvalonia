using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an icon that uses a vector path as its content.
/// </summary>
public partial class FAPathIcon : FAIconElement
{
    /// <summary>
    /// Defines the <see cref="Data"/> property
    /// </summary>
    public static StyledProperty<Geometry> DataProperty =
        Path.DataProperty.AddOwner<FAPathIcon>();

    /// <summary>
    /// Gets or sets a Geometry that specifies the shape to be drawn. 
    /// In XAML. this can also be set using a string that describes Move and draw commands syntax.
    /// </summary>
    public Geometry Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Stretch"/> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        Shape.StretchProperty.AddOwner<FAPathIcon>();

    /// <summary>
    /// Gets or sets a <see cref="Stretch"/> enumeration value that describes how the shape fills its allocated space.
    /// </summary>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="StretchDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        Viewbox.StretchDirectionProperty.AddOwner<FAPathIcon>();

    /// <summary>
    /// Gets or sets a value controlling in what direction contents will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }
}
