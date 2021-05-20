using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class NumberBoxPageViewModel : ViewModelBase
    {
        public NumberBoxPageViewModel()
        {
            
        }

        public NumberBoxSpinButtonPlacementMode SpinPlacementMode
        {
            get => spinPlacement;
            set => RaiseAndSetIfChanged(ref spinPlacement, value);
        }

        private NumberBoxSpinButtonPlacementMode spinPlacement = NumberBoxSpinButtonPlacementMode.Inline;

		public string Header => DescriptionServiceProvider.Instance.GetInfo("NumberBox", "Header");

		public string FormattedXaml => DescriptionServiceProvider.Instance.GetInfo("NumberBox", "NumberBoxFormat", "XamlSource");
		public string FormattedCSharp => DescriptionServiceProvider.Instance.GetInfo("NumberBox", "NumberBoxFormat", "CSharpSource");
		public string UsageNotes => DescriptionServiceProvider.Instance.GetInfo("NumberBox", "NumberBoxFormat", "UsageNotes");
	}
}
