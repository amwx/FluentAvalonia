using System.Reflection;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.ViewModels;

public class IconElementPageViewModel : ViewModelBase
{
    public Task<List<SymbolItem>> Symbols => GetSymbols();

    private async Task<List<SymbolItem>> GetSymbols()
    {
        return await Task.Run(() =>
        {
            var symbols = Enum.GetValues<Symbol>();
            var symbolList = new List<SymbolItem>(symbols.Length);

            var type = typeof(Symbol);
            for (int i = 0; i < symbols.Length; i++)
            {
                var str = symbols[i].ToString();

                var field = type.GetField(str);
                if (field.GetCustomAttribute<ObsoleteAttribute>() == null)
                {
                    symbolList.Add(new SymbolItem(str, (int)symbols[i]));
                }
            }

            symbolList.Sort();

            return symbolList;
        });
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (RaiseAndSetIfChanged(ref _filterText, value))
            {
                RaisePropertyChanged(nameof(Filter));
            }
        }
    }

    public Predicate<object> Filter
    {
        get
        {
            return obj =>
            {
                if (string.IsNullOrEmpty(_filterText))
                    return true;

                return obj is SymbolItem si && si.Symbol.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
            };
        }
    }

    private string _filterText;
}

public class SymbolItem : IComparable<SymbolItem>
{
    public SymbolItem(string name, int value)
    {
        Symbol = name;
        Glyph = char.ConvertFromUtf32(value).ToString();

        UnicodePoint = Convert.ToString(value, 16).ToUpper();
    }

    public string Symbol { get; set; }

    public string Glyph { get; set; }

    public string UnicodePoint { get; }

    public int CompareTo(SymbolItem other)
    {
        return Symbol.CompareTo(other.Symbol);
    }
}
