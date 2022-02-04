using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.ViewModels
{
    public class IconElementPageViewModel : ViewModelBase
    {
        public IList<SymbolItem> Symbols
        {
            get
            {
                if (_symbols == null)
                {
                    if (Design.IsDesignMode)
                        LoadSymbols();
                    else
                        LoadSymbolsAsync();
                }

                return _symbols;
            }
        }

        private async void LoadSymbolsAsync()
        {
            var symbols = Enum.GetValues<Symbol>();

            var result = await Task.Run(() =>
            {
                var symbols = Enum.GetValues<Symbol>();
                var symbolList = new List<SymbolItem>(symbols.Length);

                var type = typeof(Symbol);
                for (int i = 0; i < symbols.Length; i++)
                {
                    var str = symbols[i].ToString();
                    if (str.Contains("Filled"))
                        continue;

                    var field = type.GetField(str);
                    if (field.GetCustomAttribute<ObsoleteAttribute>() == null)
                    {
                        symbolList.Add(new SymbolItem
                        {
                            Symbol = str,
                            SymbolFilled = Enum.TryParse(str + "Filled", out Symbol res) ? res.ToString() : null
                        });
                    }
                }

                return symbolList.OrderBy(x => x.Symbol.Substring(0, 1).ToUpper()).ToList();

            });

            _symbols = result;
            RaisePropertyChanged(nameof(Symbols));
        }

        private void LoadSymbols()
        {
            var symbols = Enum.GetValues<Symbol>();
            var symbolList = new List<SymbolItem>(symbols.Length);

            var type = typeof(Symbol);
            for (int i = 0; i < symbols.Length; i++)
            {
                var str = symbols[i].ToString();
                if (str.Contains("Filled"))
                    continue;

                var field = type.GetField(str);
                if (field.GetCustomAttribute<ObsoleteAttribute>() == null)
                {
                    symbolList.Add(new SymbolItem
                    {
                        Symbol = str,
                        SymbolFilled = Enum.TryParse(str + "Filled", out Symbol res) ? res.ToString() : null
                    });
                }
            }
            _symbols = symbolList.OrderBy(x => x.Symbol.Substring(0, 1).ToUpper()).ToList();
            RaisePropertyChanged(nameof(Symbols));
        }

        private IList<SymbolItem> _symbols;
    }

    public class SymbolItem
    {
        public string Symbol { get; set; }
        public string SymbolFilled { get; set; }
    }
}
