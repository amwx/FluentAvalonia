using System;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Provides data for the InfoBar.Closed event.
	/// </summary>
	public class InfoBarClosedEventArgs : EventArgs
	{
		internal InfoBarClosedEventArgs(InfoBarCloseReason reason)
		{
			Reason = reason;
		}

		/// <summary>
		/// Gets a constant that specifies whether the cause of the Closed event 
		/// was due to user interaction (Close button click) or programmatic closure.
		/// </summary>
		public InfoBarCloseReason Reason { get; }
	}
}
