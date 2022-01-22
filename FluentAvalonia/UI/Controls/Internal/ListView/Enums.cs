using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public enum ListViewSelectionMode
	{
		None,
		Single,
		Multiple,
		Extended
	}

	public enum IncrementalLoadingTrigger
	{
		None,
		Edge
	}

	public enum ListViewReorderMode
	{
		Disabled,
		Enabled
	}

	public enum ScrollIntoViewAlignment
	{
		Default,
		Leading
	}

	public enum ItemsUpdatingScrollMode
	{
		KeepItemsInView,
		KeepScrollOffset,
		KeepLastItemInView
	}

	public enum GroupHeaderPlacement
	{
		Top,
		Left
	}

	public enum PaneScrollingDirection
	{
		None,
		Foreword,
		Backward
	}
}
