using System;

namespace FluentAvalonia.UI.Input
{
	/// <summary>
	/// Provides event data for the ExecuteRequested event.
	/// </summary>
	public class ExecuteRequestedEventArgs : EventArgs
	{
		internal ExecuteRequestedEventArgs(object param)
		{
			Parameter = param;
		}

		/// <summary>
		/// Gets the command parameter passed into the Execute method that raised this event.
		/// </summary>
		public object Parameter { get; }
	}
}
