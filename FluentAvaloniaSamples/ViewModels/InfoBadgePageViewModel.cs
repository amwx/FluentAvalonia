using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class InfoBadgePageViewModel : ViewModelBase
	{
		public string Header => "Badging in a non-intrusive and intuitive way to display notifications or bring focus to an area within an app - whether that be for notifications, indicating new content, or showing an alert";

		public double InfoBadgeOpacity => _opacity;

		public bool ShowInfoBadge
		{
			get => _visible;
			set
			{
				RaiseAndSetIfChanged(ref _visible, value);

				_opacity = value ? 1 : 0;
				RaisePropertyChanged("InfoBadgeOpacity");
			}
		}

		private bool _visible = true;
		private double _opacity = 1;
	}
}
