using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Platform;

namespace FluentAvaloniaTests.Helpers
{
    public class MockFontManager : IFontManagerImpl
    {
        public MockFontManager(string defaultFamilyName = "Default")
        {
            _defaultFamilyName = defaultFamilyName;
        }

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            return new[] { _defaultFamilyName };
        }

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
            CultureInfo culture, out Typeface fontKey)
        {
            fontKey = new Typeface(_defaultFamilyName);

            return false;
        }

        public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface)
        {
            return new MockGlyphTypeface();
        }

        private readonly string _defaultFamilyName;
    }

    public class MockGlyphTypeface : IGlyphTypefaceImpl
    {
        public short DesignEmHeight => 10;
        public int Ascent => 2;
        public int Descent => 10;
        public int LineGap { get; }
        public int UnderlinePosition { get; }
        public int UnderlineThickness { get; }
        public int StrikethroughPosition { get; }
        public int StrikethroughThickness { get; }
        public bool IsFixedPitch { get; }

        public ushort GetGlyph(uint codepoint)
        {
            return (ushort)codepoint;
        }

        public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints)
        {
            return new ushort[codepoints.Length];
        }

        public int GetGlyphAdvance(ushort glyph)
        {
            return 8;
        }

        public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
        {
            var advances = new int[glyphs.Length];

            for (var i = 0; i < advances.Length; i++)
            {
                advances[i] = 8;
            }

            return advances;
        }

        public void Dispose() { }
    }
}
