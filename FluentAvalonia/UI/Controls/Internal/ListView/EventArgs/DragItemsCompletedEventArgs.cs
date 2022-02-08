using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class DragItemsCompletedEventArgs
	{
        internal DragItemsCompletedEventArgs(DragDropEffects result, IList<object> items)
        {
            DropResult = result;
            Items = new List<object>(items);
        }

		public DragDropEffects DropResult { get; internal init; }

		public IReadOnlyList<object> Items { get; internal init; }
	}
}
