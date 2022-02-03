using FluentAvalonia.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class NumberBoxPageViewModel : ViewModelBase, INotifyDataErrorInfo
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

		public double Value
		{
			get => _value;
			set
			{
				if (RaiseAndSetIfChanged(ref _value, value))
				{
					//Debug.WriteLine("VALUE CHANGED");
				}
			}
		}

        public int EvenValue
        {
            get => _evenValue;
            set
            {
                if (RaiseAndSetIfChanged(ref _evenValue, value))
                {
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(EvenValue)));
                }
            }
        }

        private double _value=51.2;
        private int _evenValue;

        public IEnumerable GetErrors(string propertyName)
        {
            return propertyName == nameof(EvenValue) && HasErrors ? new List<string> { "Must be an even number." } : Enumerable.Empty<string>();
        }

        public bool HasErrors => EvenValue % 2 == 1;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}
