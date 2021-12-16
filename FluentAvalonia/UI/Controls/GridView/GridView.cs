using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
	public class GridView : ListViewBase
	{
		protected override bool IsItemItsOwnContainerOverride(object item) =>
			   item is ListViewItem;

		protected override IControl GetContainerForItemOverride() =>
			new GridViewItem() { WasGeneratedByListView = true };

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
