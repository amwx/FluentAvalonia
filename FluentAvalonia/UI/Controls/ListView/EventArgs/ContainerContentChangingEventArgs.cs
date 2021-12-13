using Avalonia.Controls;
using FluentAvalonia.Core.Attributes;
using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Provides event data for the ContainerContentChanging event
	/// </summary>
	public class ContainerContentChangingEventArgs : EventArgs
	{
		internal ContainerContentChangingEventArgs(ContentControl container, object item, int index, bool inRecycleQueue)
		{
			InRecycleQueue = inRecycleQueue;
			ItemContainer = container;
			Item = item;
			ItemIndex = index;
		}

		/// <summary>
		/// Gets or sets a value that marks the routed event as handled. A **true** value
		/// for handled events prevents most handlers along the event route from handling
		/// the same event again
		/// </summary>
		/// <remarks>This property along with x:Phase is not implemented (x:Phase is not in Avalonia)</remarks>
		[NotImplemented]
		public bool Handled { get; set; }

		/// <summary>
		/// Gets a value that indicates whether this container is in the recycle queue of the
		/// ListViewBase and is not being used to visualize a data item
		/// </summary>
		public bool InRecycleQueue { get; }

		/// <summary>
		/// Gets the data item associated with this container
		/// </summary>
		public object Item { get; }

		/// <summary>
		/// Gets the UI container used to display the current data item
		/// </summary>
		public ContentControl ItemContainer { get; }

		/// <summary>
		/// Gets the index in the ItemsSource of the data item associated with this container
		/// </summary>
		public int ItemIndex { get; }

		/// <summary>
		/// This property is not implemented (x:Phase is not in Avalonia)
		/// </summary>
		[NotImplemented]
		public uint Phase { get; }
	}
}
