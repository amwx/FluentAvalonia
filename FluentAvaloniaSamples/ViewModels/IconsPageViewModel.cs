using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class IconsPageViewModel : ViewModelBase
    {
        public IconsPageViewModel()
        {
			Symbols = (from item in (Symbol[])Enum.GetValues(typeof(Symbol)) select item.ToString())
				.OrderBy(x => x.Substring(0, 1).ToUpper()).ToList();
        }

        public List<string> Symbols { get; }

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
}
