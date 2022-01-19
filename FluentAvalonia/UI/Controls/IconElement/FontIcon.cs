using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon that uses a glyph from the specified font.
    /// </summary>
    public partial class FontIcon : IconElement
    {       
		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
            if (change.Property == TextBlock.ForegroundProperty ||
                change.Property == TextBlock.FontSizeProperty ||
                change.Property == TextBlock.FontFamilyProperty ||
                change.Property == TextBlock.FontWeightProperty ||
                change.Property == TextBlock.FontStyleProperty ||
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

            _textLayout = new TextLayout(Glyph, new Typeface(FontFamily, FontStyle, FontWeight),
               FontSize, Foreground, TextAlignment.Left);

            InvalidateVisual();
        }

        private TextLayout _textLayout;
        bool _suspendCreate = true;
    }
}
