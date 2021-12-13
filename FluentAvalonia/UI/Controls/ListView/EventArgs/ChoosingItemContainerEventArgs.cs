using Avalonia.Controls;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Provides event data for the ListViewBase.ChoosingItemContainer event
	/// </summary>
	public class ChoosingItemContainerEventArgs : EventArgs
	{
		internal ChoosingItemContainerEventArgs() { }

		/// <summary>
		/// Gets or sets the UI container that will be used to display the current data item
		/// </summary>
		public ContentControl ItemContainer { get; set; }

		/// <summary>
		/// Gets or sets whether the container is ready for use
		/// </summary>
		public bool IsContainerPrepared { get; set; }

		/// <summary>
		/// Gets the data item associated with this ItemContainer
		/// </summary>
		public object Item { get; internal init; }

		/// <summary>
		/// Gets the index in the ItemsSource of the data item for which a container is being selected
		/// </summary>
		public int ItemIndex { get; internal init; }
	}
}
