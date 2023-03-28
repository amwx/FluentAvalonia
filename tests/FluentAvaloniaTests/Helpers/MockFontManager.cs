using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform;

namespace FluentAvaloniaTests.Helpers;

public class MockFontManager : IFontManagerImpl
{
    private readonly string _defaultFamilyName;
    public MockFontManager(string defaultFamilyName = "Default")
    {
        _defaultFamilyName = defaultFamilyName;
    }

    public string GetDefaultFontFamilyName() =>
        _defaultFamilyName;

    public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false) =>
        new[] { _defaultFamilyName };

    public bool TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight, FontStretch stretch, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
    {
        glyphTypeface = new MockGlyphTypeface(familyName);
        return true;
    }

    public bool TryCreateGlyphTypeface(Stream stream, [NotNullWhen(true)] out IGlyphTypeface glyphTypeface)
    {
        throw new NotImplementedException();
    }

    public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, FontFamily fontFamily, CultureInfo culture, out Typeface typeface)
    {
        typeface = default;
        return false;
    }
}

public class MockGlyphTypeface : IGlyphTypeface
{
    readonly string _familyName;
    public MockGlyphTypeface(string familyName)
    {
        _familyName = familyName;
    }
   
    public string FamilyName => _familyName;

    public FontWeight Weight => FontWeight.Normal;

    public FontStyle Style => FontStyle.Normal;

    public FontStretch Stretch => FontStretch.Normal;

    public int GlyphCount => throw new NotImplementedException();

    public FontMetrics Metrics => throw new NotImplementedException();

    public FontSimulations FontSimulations => throw new NotImplementedException();

    public ushort GetGlyph(uint codepoint) =>
        (ushort)codepoint;

    public int GetGlyphAdvance(ushort glyph) => 8;

    public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
    {
        var advances = new int[glyphs.Length];

        for (var i = 0; i < advances.Length; i++)
        {
            advances[i] = 8;
        }

        return advances;
    }

    public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints) => 
        new ushort[codepoints.Length];


    public bool TryGetGlyph(uint codepoint, out ushort glyph)
    {
        throw new NotImplementedException();
    }

    public bool TryGetGlyphMetrics(ushort glyph, out GlyphMetrics metrics)
    {
        throw new NotImplementedException();
    }

    public bool TryGetTable(uint tag, out byte[] table)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {

    }
}
