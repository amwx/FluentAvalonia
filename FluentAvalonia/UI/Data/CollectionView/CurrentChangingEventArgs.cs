using System;
using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Data
{
	/// <summary>
	/// Represents a method that can handle the CurrentChanging event of an ICollectionView implementation
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event data</param>
	public delegate void CurrentChangingEventHandler([In] object sender, [In] CurrentChangingEventArgs e);

	/// <summary>
	/// Provides data for the CurrentChanging event
	/// </summary>
	public class CurrentChangingEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the CurrentChangingEventArgs class
		/// </summary>
		public CurrentChangingEventArgs() { }

		/// <summary>
		/// Initializes a new instance of the CurrentChangingEventArgs class
		/// </summary>
		/// <param name="canCancel">True to disable the ability to cancel a CurrentItem change; False 
		/// to enable cancellation</param>
		public CurrentChangingEventArgs([In] bool canCancel)
		{
			IsCancelable = canCancel;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the CurrentItem change should be cancelled
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets a value that indicates whether the CurrentItem change can be canceled
		/// </summary>
		public bool IsCancelable {get;}
	}
}
