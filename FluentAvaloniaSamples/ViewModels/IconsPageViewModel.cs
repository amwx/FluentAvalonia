using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class IconsPageViewModel : ViewModelBase
    {
        public IconsPageViewModel()
        {
			var symbols = Enum.GetValues<Symbol>();
			Symbols = new List<SymbolItem>(symbols.Length);
			var type = typeof(Symbol);
			for (int i = 0; i < symbols.Length; i++)
			{
				var str = symbols[i].ToString();
				if (str.Contains("Filled"))
					continue;

				var field = type.GetField(str);
				if (field.GetCustomAttribute<ObsoleteAttribute>() == null)
				{
					Symbols.Add(new SymbolItem
					{
						Symbol = str,
						SymbolFilled = Enum.TryParse<Symbol>(str + "Filled", out Symbol res) ? res.ToString() : null
					});
				}
			}
			Symbols = Symbols.OrderBy(x => x.Symbol.Substring(0, 1).ToUpper()).ToList();
        }

        public List<SymbolItem> Symbols { get; }

		public string Header => DescriptionServiceProvider.Instance.GetInfo("Icons", "Header");
		        
        public string SymbolIconUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Icons", "SymbolIcon", "UsageNotes");
		public string SymbolIconSourceXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "SymbolIconSource", "XamlSource");

		public string FontIconUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Icons", "FontIcon", "UsageNotes");
		public string FontIconSourceXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "FontIconSource", "XamlSource");

		public string PathIconXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "PathIcon", "XamlSource");
		public string PathIconSourceXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "PathIconSource", "XamlSource");

		public string BitmapIconUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Icons", "BitmapIcon", "UsageNotes");
		public string BitmapIconSourceXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "BitmapIconSource", "XamlSource");

		public string ImageIconUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Icons", "ImageIcon", "UsageNotes");
		public string ImageIconSourceXamlSource => DescriptionServiceProvider.Instance.GetInfo("Icons", "ImageIconSource", "XamlSource");
	}

	public class SymbolItem
	{
		public string Symbol { get; set; }
		public string SymbolFilled { get; set; }
	}
}
