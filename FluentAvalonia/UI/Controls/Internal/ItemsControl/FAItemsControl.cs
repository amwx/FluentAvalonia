using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class FAItemsControl
	{
		// Right now, only 1 group style is allowed per items control (WinUI has a list that corresponds
		// to the group's level in the event of hierarchy). Lists and Styled/Attached properties don't mix well
		// To fix this, I'd have to completely re-write the ItemsControl to give it the properties WinUI has 
		// but that is a LARGE task - not only would the ItemsControl need rewriting, but also SelectingItemsControl
		// since that's probably even more useful. Then there also becomes confusion between the differing version
		// here and in core Avalonia
		// I started doing it for v1.1.8 for the ListView, but decided it wasn't worth it - may revisit in the future, however.
		public static readonly AttachedProperty<GroupStyle> GroupStyleProperty =
			AvaloniaProperty.RegisterAttached<FAItemsControl, ItemsControl, GroupStyle>("GroupStyle");

		public static GroupStyle GetGroupStyle(ItemsControl ic) =>
			ic.GetValue(GroupStyleProperty);

		public static void SetGroupStyle(ItemsControl ic, GroupStyle value) =>
			ic.SetValue(GroupStyleProperty, value);
	}
}
