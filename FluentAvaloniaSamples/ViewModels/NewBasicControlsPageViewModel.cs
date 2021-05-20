using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
	public class NewBasicControlsPageViewModel
	{
		public NewBasicControlsPageViewModel()
		{
			ComboBoxItems = new List<string> { "Item 1", "Item 2", "Item 3" };			
		}

		public List<string> ComboBoxItems { get; }

		public string Header => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "Header");

		public string ButtonUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "Button", "UsageNotes");
		
		public string HyperlinkButtonUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "HyperlinkButton", "UsageNotes");


		public string ComboBoxXamlSource => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "ComboBox", "XamlSource");
		public string ComboBoxUsageNotes => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "ComboBox", "UsageNotes");

		public string SplitButtonXamlSource => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "SplitButton", "XamlSource");
		public string ToggleSplitButtonXamlSource => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "ToggleSplitButton", "XamlSource");
		public string DropDownButtonXamlSource => DescriptionServiceProvider.Instance.GetInfo("Basic Controls", "DropDownButton", "XamlSource");
	}
}
