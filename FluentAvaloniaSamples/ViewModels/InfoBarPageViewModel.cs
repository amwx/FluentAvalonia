using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class InfoBarPageViewModel : ViewModelBase
	{
		public InfoBarPageViewModel()
		{
			SeverityList = new List<InfoBarSeverity>
			{
				InfoBarSeverity.Informational,
				InfoBarSeverity.Success,
				InfoBarSeverity.Warning,
				InfoBarSeverity.Error
			};

			LongMessage = "A long essential app message for your users to be informed of, acknowledge, or take action on." +
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin dapibus dolor vitae justo rutrum, ut lobortis" +
				" nibh mattis. Aenean id elit commodo, semper felis nec.";
		}

		public string Header => DescriptionServiceProvider.Instance.GetInfo("InfoBar", "Header");

		public InfoBarPage Owner { get; set; }

		public List<InfoBarSeverity> SeverityList { get; }

		private bool _bar2IsOpen = true;
		public bool Bar2IsOpen
		{
			get => _bar2IsOpen;
			set
			{
				RaiseAndSetIfChanged(ref _bar2IsOpen, value);
				RaisePropertyChanged("Bar2XamlSource");
			}
		}

		public string LongMessage { get; }

		public string ShortMessage { get; } = "Short Message!";

		public string ActiveMessage
		{
			get => _msgType == 0 ? LongMessage : ShortMessage;
		}

		private int _msgType = 0;
		public int MessageType
		{
			get => _msgType;
			set
			{
				RaiseAndSetIfChanged(ref _msgType, value);
				RaisePropertyChanged("ActiveMessage");
				RaisePropertyChanged("Bar2XamlSource");
			}
		}

		private int _buttonType = 0;
		public int ButtonType
		{
			get => _buttonType;
			set
			{
				RaiseAndSetIfChanged(ref _buttonType, value);
				Owner.SetButton(value);
				RaisePropertyChanged("Bar2XamlSource");
			}
		}

		public string Bar2XamlSource
		{
			get
			{
				string open = Bar2IsOpen ? "True" : "False";
				string msg = MessageType == 0 ? "A long essential app message..." : "Short Message!";
				string src = "<ui:InfoBar " +
					$"IsOpen=\"{open}\" " +
					"Title=\"Title\" " +
					$"Message=\"{msg}\">\n";

				if (_buttonType == 0)
				{
					src += "</ui:InfoBar>";
				}
				else if (_buttonType == 1)
				{
					src += "    <ui:InfoBar.ActionButton>\n" +
						"        <Button Content=\"Action\" Click=\"InfoBarButton_Click\" />\n" +
						"    </ui:InfoBar.ActionButton>\n</ui:InfoBar>";
				}
				else if (_buttonType == 2)
				{
					src += "    <ui:InfoBar.ActionButton>\n" +
						"        <ui:HyperlinkButton Content=\"Informational Link\" NavigateUri=\"https://www.example.com\" />\n" +
						"    </ui:InfoBar.ActionButton>\n</ui:InfoBar>";
				}

				return src;
			}
		}
	}
}
