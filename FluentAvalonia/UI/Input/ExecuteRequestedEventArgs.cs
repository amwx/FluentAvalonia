using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Input
{
	public class ExecuteRequestedEventArgs
	{
		internal ExecuteRequestedEventArgs(object param)
		{
			Parameter = param;
		}

		public object Parameter { get; }
	}
}
