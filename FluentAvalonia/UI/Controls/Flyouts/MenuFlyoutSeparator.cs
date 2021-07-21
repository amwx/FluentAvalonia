using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutSeparator : MenuFlyoutItemBase
	{
		static MenuFlyoutSeparator()
		{
			FocusableProperty.OverrideDefaultValue<MenuFlyoutSeparator>(false);
		}
	}
}
