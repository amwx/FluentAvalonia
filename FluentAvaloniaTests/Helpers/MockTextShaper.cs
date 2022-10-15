using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.Media.TextFormatting;

namespace FluentAvaloniaTests.Helpers;

public class MockTextShaper : ITextShaperImpl
{
    public ShapedBuffer ShapeText(ReadOnlySlice<char> text, TextShaperOptions options)
    {
        var typeface = options.Typeface;
        var fontRenderingEmSize = options.FontRenderingEmSize;
        var bidiLevel = options.BidiLevel;

        var shapedBuffer = new ShapedBuffer(text, text.Length, typeface, fontRenderingEmSize, bidiLevel);

        for (var i = 0; i < shapedBuffer.Length;)
        {
            var glyphCluster = i + text.Start;
            var codepoint = Codepoint.ReadAt(text, i, out var count);

            var glyphIndex = typeface.GetGlyph(codepoint);

            shapedBuffer[i] = new GlyphInfo(glyphIndex, glyphCluster, 10);

            i += count;
        }

        return shapedBuffer;
    }
}
