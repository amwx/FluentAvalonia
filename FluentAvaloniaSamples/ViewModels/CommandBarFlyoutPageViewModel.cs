using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class CommandBarFlyoutPageViewModel : ViewModelBase
	{
		public string Header => DescriptionServiceProvider.Instance.GetInfo("CommandBarFlyout", "Header");
		public string XamlSource => DescriptionServiceProvider.Instance.GetInfo("CommandBarFlyout", "XamlSource");
		public string CSharpSource => DescriptionServiceProvider.Instance.GetInfo("CommandBarFlyout", "CSharpSource");

		public string TextCBFUsageNotes => DescriptionServiceProvider.Instance.GetInfo("TextCommandBarFlyout", "UsageNotes");

		public string LastAction
		{
			get => _lastAction;
			set => RaiseAndSetIfChanged(ref _lastAction, value);
		}

		public void Share()
		{
			LastAction = "Share";
		}

		public void Save()
		{
			LastAction = "Save";
		}

		public void Delete()
		{
			LastAction = "Delete";
		}

		public void Resize()
		{
			LastAction = "Resize";
		}

		public void Move()
		{
			LastAction = "Move";
		}

		private string _lastAction = "";
	}
}
