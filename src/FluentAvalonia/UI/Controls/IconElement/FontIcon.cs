using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an icon that uses a glyph from the specified font.
/// </summary>
public partial class FontIcon : FAIconElement
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextElement.FontSizeProperty ||
            change.Property == TextElement.FontFamilyProperty ||
            change.Property == TextElement.FontWeightProperty ||
            change.Property == TextElement.FontStyleProperty ||
            change.Property == GlyphProperty)
        {
            _textLayout = null;
            InvalidateMeasure();
        }
        else if (change.Property == TextElement.ForegroundProperty)
        {
            _textLayout = null;
            // FAIconElement calls InvalidateVisual
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_textLayout == null)
        {
            GenerateText();
        }

        return new Size(_textLayout.Width, _textLayout.Height);
    }

    public override void Render(DrawingContext context)
    {
        if (_textLayout == null)
            GenerateText();

        var dstRect = new Rect(Bounds.Size);
        using (context.PushClip(dstRect))
        {
            var pt = new Point(dstRect.Center.X - _textLayout.Width * 0.5,
                               dstRect.Center.Y - _textLayout.Height * 0.5);
            _textLayout.Draw(context, pt);
        }
    }

    private void GenerateText()
    {
        _textLayout = new TextLayout(Glyph, new Typeface(FontFamily, FontStyle, FontWeight),
           FontSize, Foreground, TextAlignment.Left);
    }

    private TextLayout _textLayout;
}
