using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon that uses a glyph from the SymbolThemeFontFamily resource as its content.
    /// </summary>
    public class SymbolIcon : IconElement
    {
        static SymbolIcon()
        {
            FontSizeProperty.OverrideDefaultValue<SymbolIcon>(18d);
        }

        /// <summary>
        /// Defines the <see cref="Symbol"/> property
        /// </summary>
        public static readonly StyledProperty<Symbol> SymbolProperty =
            AvaloniaProperty.Register<SymbolIcon, Symbol>(nameof(Symbol));

        /// <summary>
        /// Defines the <see cref="FontSize"/> property
        /// </summary>
        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<SymbolIcon>();

        /// <summary>
        /// Gets or sets the <see cref="FluentAvalonia.UI.Controls.Symbol"/> this icon displays
        /// </summary>
        public Symbol Symbol
        {
			get => GetValue(SymbolProperty);
			set => SetValue(SymbolProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size this icon uses when rendering
        /// </summary>
        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == TextBlock.FontSizeProperty ||
				change.Property == SymbolProperty)
			{
				GenerateText();
				InvalidateMeasure();
			}
			else if (change.Property == TextBlock.ForegroundProperty)
			{
				GenerateText();
			}
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
            var glyph = char.ConvertFromUtf32((int)Symbol).ToString();

            _textLayout = new TextLayout(glyph, 
				new Typeface(new FontFamily("avares://FluentAvalonia/Fonts#Symbols")),
               FontSize, Foreground, TextAlignment.Left);
        }

        private TextLayout _textLayout;
    }
}
