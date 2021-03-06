﻿namespace FluentAvalonia.UI.Controls
{
	public class InfoBarClosedEventArgs
	{
		internal InfoBarClosedEventArgs(InfoBarCloseReason reason)
		{
			Reason = reason;
		}

		public InfoBarCloseReason Reason { get; }
	}

	public class InfoBarClosingEventArgs
	{
		internal InfoBarClosingEventArgs(InfoBarCloseReason reason)
		{
			Reason = reason;
		}

		public InfoBarCloseReason Reason { get; }
		public bool Cancel { get; set; }
	}
}
