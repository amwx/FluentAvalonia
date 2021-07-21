using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class XamlUICommandPageViewModel : ViewModelBase
	{
		public string XamlSource => DescriptionServiceProvider.Instance.GetInfo("XamlUICommand", "XamlSource");
		public string CSharpSource => DescriptionServiceProvider.Instance.GetInfo("XamlUICommand", "CSharpSource");
		public string UsageNotes => DescriptionServiceProvider.Instance.GetInfo("XamlUICommand", "UsageNotes");
		public string Header => DescriptionServiceProvider.Instance.GetInfo("XamlUICommand", "Header");
	}
}
