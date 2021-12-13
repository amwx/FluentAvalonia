using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class ListView : ListViewBase
	{
		protected override bool IsItemItsOwnContainerOverride(object item) =>
			item is ListViewItem;

		protected override IControl GetContainerForItemOverride() =>
			new ListViewItem() { WasGeneratedByListView = true };

		protected override void PrepareItemContainerOverride(IControl container, object item)
		{
			if (item == container)
				return;

			if (container is ContentControl lvi)
			{
				lvi.DataContext = item;
				lvi.Content = item;
				lvi.ContentTemplate = this.FindDataTemplate(item, ItemTemplate);
			}
			else
			{
				container.DataContext = item;
			}
		}

		protected internal override IControl GetGroupContainerForItem(int index, object group)
		{
			return new ListViewHeaderItem();
		}
	}
}
