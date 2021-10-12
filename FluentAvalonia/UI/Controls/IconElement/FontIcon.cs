using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace FluentAvalonia.UI.Controls
{
    public class FontIcon : IconElement
    {
        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<FontIcon>();

        public static readonly StyledProperty<string> GlyphProperty =
            AvaloniaProperty.Register<FontIcon, string>(nameof(Glyph));

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

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == TextBlock.ForegroundProperty ||
				change.Property == TextBlock.FontSizeProperty ||
				change.Property == TextBlock.FontFamilyProperty ||
				change.Property == TextBlock.FontWeightProperty ||
				change.Property == TextBlock.FontStyleProperty ||
				change.Property == GlyphProperty)
			{
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

            _textLayout = new TextLayout(Glyph, new Typeface(FontFamily, FontStyle, FontWeight),
               FontSize, Foreground, TextAlignment.Left);

            InvalidateVisual();
        }

        private TextLayout _textLayout;
        bool _suspendCreate = true;
    }
}
