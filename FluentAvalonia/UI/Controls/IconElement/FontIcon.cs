using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class FontIcon : IconElement
    {
        static FontIcon()
        {
            AffectsRender<SymbolIcon>(TextBlock.ForegroundProperty);
            ForegroundProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontFamilyProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontSizeProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontWeightProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
            FontStyleProperty.Changed.AddClassHandler<FontIcon>((x, v) => x.GenerateText());
        }

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<FontIcon>();

        public static readonly DirectProperty<FontIcon, string> GlyphProperty =
            AvaloniaProperty.RegisterDirect<FontIcon, string>("Glyph",
                x => x.Glyph, (x, v) => x.Glyph = v);

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
            get => _glyph;
            set
            {
                SetAndRaise(GlyphProperty, ref _glyph, value);
                GenerateText();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_suspendCreate || _textLayout == null)
            {
                _suspendCreate = false;
                GenerateText();
            }

            return _textLayout.Size;
        }

        public override void Render(DrawingContext context)
        {
            if (_textLayout == null)
                return;

            var dstRect = new Rect(Bounds.Size);
            using (context.PushClip(dstRect))
            using (context.PushPreTransform(Matrix.CreateTranslation(dstRect.Center.X - _textLayout.Size.Width / 2,
                dstRect.Center.Y - _textLayout.Size.Height / 2)))
            {
                _textLayout.Draw(context);
            }
        }

        private void GenerateText()
        {
            if (_suspendCreate)
                return;

            _textLayout = new TextLayout(_glyph, new Typeface(FontFamily, FontStyle, FontWeight),
               FontSize, Foreground, TextAlignment.Left);

            InvalidateVisual();
        }

        private string _glyph;
        private TextLayout _textLayout;
        bool _suspendCreate = true;
    }
}
