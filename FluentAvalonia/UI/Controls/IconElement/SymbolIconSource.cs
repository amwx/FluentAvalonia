using Avalonia;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIconSource : IconSource
    {
        public SymbolIconSource()
        {

        }

        public static readonly DirectProperty<SymbolIconSource, Symbol> SymbolProperty =
            AvaloniaProperty.RegisterDirect<SymbolIconSource, Symbol>("Symbol",
                x => x.Symbol, (x,v) => x.Symbol = v);
              
        public Symbol Symbol
        {
            get => _symbol;
            set 
            {
                SetAndRaise(SymbolProperty, ref _symbol, value);
            }
        }

        private Symbol _symbol;
    }
}
