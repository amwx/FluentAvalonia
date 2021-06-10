using Avalonia;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIconSource : IconSource
    {
        public static readonly StyledProperty<Symbol> SymbolProperty =
            AvaloniaProperty.Register<SymbolIconSource, Symbol>("Symbol");
              
        public Symbol Symbol
        {
            get => GetValue(SymbolProperty);
			set => SetValue(SymbolProperty, value);
        }
    }
}
