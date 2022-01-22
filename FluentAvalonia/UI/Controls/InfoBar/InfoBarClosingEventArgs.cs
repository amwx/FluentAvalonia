namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Provides data for the InfoBar.Closing event.
	/// </summary>
	public class InfoBarClosingEventArgs
	{
		internal InfoBarClosingEventArgs(InfoBarCloseReason reason)
		{
			Reason = reason;
		}

		/// <summary>
		/// Gets a constant that specifies whether the cause of the Closing event was 
		/// due to user interaction (Close button click) or programmatic closure.
		/// </summary>
		public InfoBarCloseReason Reason { get; }

		/// <summary>
		/// Gets or sets a value that indicates whether the Closing event should be 
		/// canceled in the InfoBar.
		/// </summary>
		public bool Cancel { get; set; }
	}
}
