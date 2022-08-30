using Avalonia;
using Avalonia.Controls.Documents;
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
        if (change.Property == TextElement.ForegroundProperty ||
            change.Property == TextElement.FontSizeProperty ||
            change.Property == TextElement.FontFamilyProperty ||
            change.Property == TextElement.FontWeightProperty ||
            change.Property == TextElement.FontStyleProperty ||
            change.Property == GlyphProperty)
        {
            GenerateText();
        }

        base.OnPropertyChanged(change);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_suspendCreate || _textLayout == null)
        {
            _suspendCreate = false;
            GenerateText();
        }

        return _textLayout.Bounds.Size;
    }

    public override void Render(DrawingContext context)
    {
        if (_textLayout == null)
            return;

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
        if (_suspendCreate)
            return;

        _textLayout = new TextLayout(Glyph, new Typeface(FontFamily, FontStyle, FontWeight),
           FontSize, Foreground, TextAlignment.Left);

        InvalidateVisual();
    }

    private TextLayout _textLayout;
    bool _suspendCreate = true;
}
