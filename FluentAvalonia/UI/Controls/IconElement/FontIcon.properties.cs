using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls;

public partial class FontIcon : FAIconElement
{
    /// <summary>
    /// Defines the <see cref="FontFamily"/> property
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<FontIcon>();

    /// <summary>
    /// Defines the <see cref="FontSize"/> property
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<FontIcon>();

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<FontIcon>();

    /// <summary>
    /// Defines the <see cref="FontStyle"/> property
    /// </summary>
    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<FontIcon>();

    /// <summary>
    /// Defines the <see cref="Glyph"/> property
    /// </summary>
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<FontIcon, string>(nameof(Glyph));

    /// <summary>
    /// Gets or sets the <see cref="Avalonia.Media.FontFamily"/> to use when rendering
    /// the glyph
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size to use when rendering the glyph
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Avalonia.Media.FontWeight"/> to use 
    /// when rendering the glyph
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Avalonia.Media.FontStyle"/> to use 
    /// when rendering the glyph
    /// </summary>
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the glyph this FontIcon renders
    /// </summary>
    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
}
