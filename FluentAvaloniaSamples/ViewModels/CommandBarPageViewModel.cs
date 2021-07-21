using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class CommandBarPageViewModel : ViewModelBase
	{
		public string Header => DescriptionServiceProvider.Instance.GetInfo("CommandBar", "Header");

		public string DefaultXamlSource => DescriptionServiceProvider.Instance.GetInfo("CommandBar", "DefaultXamlSource");
		public string RightXamlSource => DescriptionServiceProvider.Instance.GetInfo("CommandBar", "RightXamlSource");
		public string ToggleXamlSource => DescriptionServiceProvider.Instance.GetInfo("CommandBar", "ToggleXamlSource");
	}
}
