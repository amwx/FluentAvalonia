using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an icon that uses a glyph from the SymbolThemeFontFamily resource as its content.
/// </summary>
public class SymbolIcon : FAIconElement
{
    static SymbolIcon()
    {
        FontSizeProperty.OverrideDefaultValue<SymbolIcon>(18d);
    }

    /// <summary>
    /// Defines the <see cref="Symbol"/> property
    /// </summary>
    public static readonly StyledProperty<Symbol> SymbolProperty =
        AvaloniaProperty.Register<SymbolIcon, Symbol>(nameof(Symbol));

    /// <summary>
    /// Defines the <see cref="FontSize"/> property
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SymbolIcon>();

    /// <summary>
    /// Gets or sets the <see cref="FluentAvalonia.UI.Controls.Symbol"/> this icon displays
    /// </summary>
    public Symbol Symbol
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextElement.FontSizeProperty ||
            change.Property == SymbolProperty)
        {
            GenerateText();
            InvalidateMeasure();
        }
        else if (change.Property == TextElement.ForegroundProperty)
        {
            GenerateText();            
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Force invalidation of text for inherited properties now that we've attached to the tree
        if (_textLayout != null)
            GenerateText();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_textLayout == null)
            GenerateText();

        return _textLayout?.Bounds.Size ?? Size.Empty;
    }

    public override void Render(DrawingContext context)
    {
        if (_textLayout == null)
            GenerateText();

        var dstRect = new Rect(Bounds.Size);
        using (context.PushClip(dstRect))
        {
            var pt = new Point(dstRect.Center.X - _textLayout.Bounds.Width / 2,
                dstRect.Center.Y - _textLayout.Bounds.Height / 2);
            _textLayout.Draw(context, pt);
        }
    }

    private void GenerateText()
    {
        var glyph = char.ConvertFromUtf32((int)Symbol).ToString();

        _textLayout = new TextLayout(glyph,
            new Typeface(new FontFamily("avares://FluentAvalonia/Fonts#Symbols")),
           FontSize, Foreground, TextAlignment.Left);
    }

    private TextLayout _textLayout;
}
