using System;

namespace FluentAvalonia.UI.Input
{
	public class ExecuteRequestedEventArgs : EventArgs
	{
		internal ExecuteRequestedEventArgs(object param)
		{
			Parameter = param;
		}

		public object Parameter { get; }
	}
}
