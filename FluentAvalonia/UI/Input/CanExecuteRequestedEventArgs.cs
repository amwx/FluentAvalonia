using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Input
{
	public class CanExecuteRequestedEventArgs
	{
		internal CanExecuteRequestedEventArgs(object param)
		{
			Parameter = param;
		}

		public bool CanExecute { get; set; } = true;
		public object Parameter { get; }
	}
}
