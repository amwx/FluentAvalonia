using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIconSource : IconSource
    {
        public static readonly StyledProperty<Symbol> SymbolProperty =
            SymbolIcon.SymbolProperty.AddOwner<SymbolIconSource>();

        public static readonly StyledProperty<double> FontSizeProperty =
           TextBlock.FontSizeProperty.AddOwner<SymbolIconSource>();

        public static readonly StyledProperty<bool> UseFilledProperty =
            SymbolIcon.UseFilledProperty.AddOwner<SymbolIconSource>();
              
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

        public bool UseFilled
        {
            get => GetValue(UseFilledProperty);
            set => SetValue(UseFilledProperty, value);
        }
    }
}
