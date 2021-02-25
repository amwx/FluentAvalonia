using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIcon : IconElement
    {
        static SymbolIcon()
        {
            FontSizeProperty.OverrideDefaultValue<SymbolIcon>(18d);
            TextBlock.ForegroundProperty.Changed.AddClassHandler<SymbolIcon>((x, _) => x.GenerateText());
        }

        public static readonly DirectProperty<SymbolIcon, Symbol> SymbolProperty =
            AvaloniaProperty.RegisterDirect<SymbolIcon, Symbol>("Symbol",
                x => x.Symbol, (x,v) => x.Symbol = v);

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<SymbolIcon>();

        public Symbol Symbol
        {
            get => _symbol;
            set
            {
                if (SetAndRaise(SymbolProperty, ref _symbol, value))
                {
                    GenerateText();
                    InvalidateMeasure();
                }
            }
        }

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_textLayout == null)
                GenerateText();

            return _textLayout.Size;
        }

        public override void Render(DrawingContext context)
        {
            if (_textLayout == null)
                GenerateText();

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
            var code = (int)_symbol;
            var glyph = char.ConvertFromUtf32(code).ToString();

            //Hardcode straight to FluentSystemIcons-Regular.ttf, since the Symbol enum is derived from that
            Typeface tf = new Typeface(new FontFamily("avares://FluentAvalonia/Fonts#FluentSystemIcons-Regular"));

            _textLayout = new TextLayout(glyph, tf,
               FontSize, Foreground, TextAlignment.Left);
        }

        private Symbol _symbol;
        private TextLayout _textLayout;
    }
}
