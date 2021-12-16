using System;

namespace FluentAvalonia.UI.Input
{
	public class CanExecuteRequestedEventArgs : EventArgs
	{
		internal CanExecuteRequestedEventArgs(object param)
		{
			Parameter = param;
		}

		public bool CanExecute { get; set; } = true;
		public object Parameter { get; }
	}
}
