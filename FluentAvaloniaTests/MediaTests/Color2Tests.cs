using FluentAvalonia.UI.Media;
using Xunit;

namespace FluentAvaloniaTests.MediaTests
{
    public class Color2Tests
    {
        // Tests are done with SlateBlue color
        //   RGB: 106, 90, 205        
        //   HSV: 248, 56, 80
        //   HSL: 248, 53, 58
        //  CMYK: 48, 56, 0, 20
        //   HEX: #6A5ACD
        //  Float rounding is done to 3 decimal places
        //  RGBf: 0.417, 0.353, 0.804
        //  HSVf: 248, 0.56, 0.80
        //  HSLf: 248, 0.53, 0.58
        // CMYKf: 0.48, 0.56, 0, 0.2

        [Fact]
        public void CreateFromRGBAWorks()
        {
            // This tests the RGBA constructor
            var c = new Color2(106, 90, 205, 175);

            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(175, c.A);

            // This tests the static FromRGB method
            c = Color2.FromRGB(106, 90, 205);
            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void CreateFromHSVWorks()
        {
            var c = Color2.FromHSV(248, 56, 80, 150);

            Assert.Equal(248, c.Hue);
            Assert.Equal(56, c.Saturation);
            Assert.Equal(80, c.Value);

            // Use InRange here as floating precision may mean we're off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(150, c.A);
        }

        [Fact]
        public void CreateFromHSLWorks()
        {
            var c = Color2.FromHSL(248, 53, 58, 150);

            Assert.Equal(248, c.Hue);
            Assert.Equal(53, c.HSLSaturation);
            Assert.Equal(58, c.Lightness);

            // Use InRange here as floating precision may mean were off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);            
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(150, c.A);
        }

        [Fact]
        public void CreateFromCMYKWorks()
        {
            var c = Color2.FromCMYK(48, 56, 0, 20, 150);

            Assert.Equal(48, c.CMYKCyan);
            Assert.Equal(56, c.CMYKMagenta);
            Assert.Equal(0, c.CMYKYellow);
            Assert.Equal(20, c.CMYKBlack);
            Assert.Equal(150, c.A);

            // Use InRange here as floating precision may mean we're off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(150, c.A);
        }

        [Fact]
        public void CreateFromRGBfWorks()
        {
            var c = Color2.FromRGBf(0.417f, 0.353f, 0.804f, 0.75f);

            Assert.Equal(0.417f, c.Rf);
            Assert.Equal(0.353f, c.Gf);
            Assert.Equal(0.804f, c.Bf);
            Assert.Equal(0.75, c.Af);            
        }

        [Fact]
        public void CreateFromHSVfWorks()
        {
            var c = Color2.FromHSVf(248, 0.56f, 0.8f, 0.4f);

            Assert.Equal(248, c.Huef);
            Assert.Equal(0.56f, c.Saturationf);
            Assert.Equal(0.8f, c.Valuef);
            Assert.Equal(0.4f, c.Af);

            // Use InRange here as floating precision may mean we're off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(102, c.A);
        }

        [Fact]
        public void CreateFromHSLfWorks()
        {
            var c = Color2.FromHSLf(248, 0.53f, 0.58f, 0.4f);

            Assert.Equal(248, c.Huef);
            Assert.Equal(0.53f, c.HSLSaturationf);
            Assert.Equal(0.58f, c.Lightnessf);
            Assert.Equal(0.4f, c.Af);

            // Use InRange here as floating precision may mean we're off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(102, c.A);
        }

        [Fact]
        public void CreateFromCMYKfWorks()
        {
            var c = Color2.FromCMYKf(0.48f, 0.56f, 0f, 0.20f, 0.4f);

            Assert.Equal(0.48f, c.CMYKCyanf);
            Assert.Equal(0.56f, c.CMYKMagentaf);
            Assert.Equal(0f, c.CMYKYellowf);
            Assert.Equal(0.20f, c.CMYKBlackf);
            Assert.Equal(0.4f, c.Af);

            // Use InRange here as floating precision may mean we're off by 1. It's still the same color though
            Assert.InRange(c.R, 105, 107);
            Assert.InRange(c.G, 89, 91);
            Assert.InRange(c.B, 204, 206);
            Assert.Equal(102, c.A);
        }

        [Fact]
        public void FromUIntWorks()
        {
            var c = Color2.FromUInt(0xFF6A5ACD);

            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void ToHexWorks()
        {
            // This is a tricky one to test from HSV/HSL/CMYK as floating point
            // precision could make us *slightly* off, so only testing from RGB
            var str = Color2.FromRGB(106, 90, 205).ToHexString(false).ToUpper();
            Assert.Equal("#6A5ACD", str);

            str = Color2.FromRGB(106, 90, 205).ToHexString(true).ToUpper();
            Assert.Equal("#FF6A5ACD", str);
        }

        [Fact]
        public void ToHTMLWorks()
        {
            // This is a tricky one to test from HSV/HSL/CMYK as floating point
            // precision could make us *slightly* off, so only testing from RGB
            var str = Color2.FromRGB(106, 90, 205).ToHTML(false);
            Assert.Equal("rgb( 106, 90, 205 )", str);

            str = Color2.FromRGB(106, 90, 205).ToHTML(true);
            Assert.Equal("rgba( 106, 90, 205, 255 )", str);
        }

        [Fact]
        public void ParseWorks()
        {
            var c = Color2.Parse("#6A5ACD");
            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(255, c.A);

            c = Color2.Parse("#FF6A5ACD");
            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(255, c.A);

            // Test shorthand
            c = Color2.Parse("#ABA");
            Assert.Equal(170, c.R);
            Assert.Equal(187, c.G);
            Assert.Equal(170, c.B);
            Assert.Equal(255, c.A);

            // Erroneous hex string should result in Empty color
            c = Color2.Parse("6A5ACD");
            Assert.Equal(Color2.Empty, c);
        }

        [Fact]
        public void FromAvaloniaColorWorks()
        {
            var c = new Color2(Avalonia.Media.Colors.SlateBlue);

            Assert.Equal(106, c.R);
            Assert.Equal(90, c.G);
            Assert.Equal(205, c.B);
            Assert.Equal(255, c.A);
        }

        [Fact]
        public void ToAvaloniaColorWorks()
        {
            var c = new Color2(106, 90, 205);

            var avCol = (Avalonia.Media.Color)c;
            Assert.Equal(Avalonia.Media.Colors.SlateBlue, avCol);
        }
    }
}
