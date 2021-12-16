using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Provides event data for the ListViewBase.ChoosingGroupHeaderContainer event
	/// </summary>
	public class ChoosingGroupHeaderContainerEventArgs : EventArgs
	{
		internal ChoosingGroupHeaderContainerEventArgs() { }

		/// <summary>
		/// Gets or sets the UI container that will be used to display the current data group
		/// </summary>
		public ListViewBaseHeaderItem GroupHeaderContainer { get; set; }

		/// <summary>
		/// Gets the data group associated with this GroupHeaderContainer
		/// </summary>
		public object Group { get; internal init; }

		/// <summary>
		/// Gets the index in the ItemsSource of the data group for which a container is being selected
		/// </summary>
		public int GroupIndex { get; internal init; }
	}
}
