using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    public class FontIconSource : IconSource
    {
        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<FontIconSource>();

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<FontIconSource>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<FontIconSource>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<FontIconSource>();

        public static readonly StyledProperty<string> GlyphProperty =
            FontIcon.GlyphProperty.AddOwner<FontIconSource>();

        public FontFamily FontFamily
        {
            get => GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public FontWeight FontWeight
        {
            get => GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public FontStyle FontStyle
        {
            get => GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public string Glyph
        {
			get => GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
        }
    }
}
