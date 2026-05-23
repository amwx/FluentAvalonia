using Avalonia;
using Avalonia.Controls.Documents;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an icon source that uses a glyph from the SymbolThemeFontFamily resource as its content.
/// </summary>
public class FASymbolIconSource : FAIconSource
{
    /// <summary>
    /// Defines the <see cref="Symbol"/> property
    /// </summary>
    public static readonly StyledProperty<FASymbol> SymbolProperty =
        FASymbolIcon.SymbolProperty.AddOwner<FASymbolIconSource>();

    /// <summary>
    /// Defines the <see cref="FontSize"/> property
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
       TextElement.FontSizeProperty.AddOwner<FASymbolIconSource>();

    /// <summary>
    /// Gets or sets the <see cref="FluentAvalonia.UI.Controls.FASymbol"/> this icon displays
    /// </summary>
    public FASymbol Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size this icon uses when rendering
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
}
