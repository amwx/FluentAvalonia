using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIcon : IconElement
    {
        static SymbolIcon()
        {
            FontSizeProperty.OverrideDefaultValue<SymbolIcon>(18d);
        }

        public static readonly StyledProperty<Symbol> SymbolProperty =
            AvaloniaProperty.Register<SymbolIcon, Symbol>(nameof(Symbol));

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<SymbolIcon>();

		[Obsolete("This property no longer does anything. Filled Icons are now in the Symbol Enum")]
		public static readonly StyledProperty<bool> UseFilledProperty =
			AvaloniaProperty.Register<SymbolIcon, bool>(nameof(UseFilled));

        public Symbol Symbol
        {
			get => GetValue(SymbolProperty);
			set => SetValue(SymbolProperty, value);
        }

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

		[Obsolete("This property no longer does anything. Filled Icons are now in the Symbol Enum")]
		public bool UseFilled
		{
			get => GetValue(UseFilledProperty);
			set => SetValue(UseFilledProperty, value);
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
