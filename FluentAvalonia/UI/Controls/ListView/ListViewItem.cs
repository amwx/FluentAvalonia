using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls
{
	public class ListViewItem : ListBoxItem
	{
		public ListViewItem()
		{

		}

		static ListViewItem()
		{
			FocusableProperty.OverrideDefaultValue<ListViewItem>(true);
		}		

		protected override void OnGotFocus(GotFocusEventArgs e)
		{
			base.OnGotFocus(e);

			var lv = this.FindAncestorOfType<ListViewBase>();
			if (lv != null)
			{
				lv.UpdateSelectionFromItemFocus(this);
			}
		}
	}
}
