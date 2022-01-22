using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs args);

	public class ItemClickEventArgs : RoutedEventArgs
	{
		internal ItemClickEventArgs(ListViewBase lvb, object item)
		{
			Source = lvb;
			ClickedItem = item;
		}

		public object ClickedItem { get; }
	}
}
