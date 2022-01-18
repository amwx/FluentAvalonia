using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
	public class GridViewHeaderItem : ListViewBaseHeaderItem, IStyleable
	{
		// TODO: let this have its own style
		Type IStyleable.StyleKey => typeof(ListViewHeaderItem);
	}
}
