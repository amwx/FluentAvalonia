using FluentAvalonia.UI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	internal delegate void DragItemsStartingEventHandler(object sender, DragItemsStartingEventArgs args);

	internal class DragItemsStartingEventArgs : EventArgs
	{
		public bool Cancel { get; set; }
		public DataPackage Data { get; internal init; }
		public IList<object> Items { get; internal init; }
	}
}
