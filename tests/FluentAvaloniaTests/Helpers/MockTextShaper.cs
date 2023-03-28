using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.Media.TextFormatting;
using System;

namespace FluentAvaloniaTests.Helpers;

public class MockTextShaper : ITextShaperImpl
{
    public ShapedBuffer ShapeText(ReadOnlyMemory<char> text, TextShaperOptions options)
    {
        var typeface = options.Typeface;
        var fontRenderingEmSize = options.FontRenderingEmSize;
        var bidiLevel = options.BidiLevel;
        var shapedBuffer = new ShapedBuffer(text, text.Length, typeface, fontRenderingEmSize, bidiLevel);
        var textSpan = text.Span;
        var textStartIndex = TextTestHelper.GetStartCharIndex(text);

        for (var i = 0; i < shapedBuffer.Length;)
        {
            var glyphCluster = i + textStartIndex;

            var codepoint = Codepoint.ReadAt(textSpan, i, out var count);

            var glyphIndex = typeface.GetGlyph(codepoint);

            for (var j = 0; j < count; ++j)
            {
                shapedBuffer[i + j] = new GlyphInfo(glyphIndex, glyphCluster, 10);
            }

            i += count;
        }

        return shapedBuffer;
    }
}
