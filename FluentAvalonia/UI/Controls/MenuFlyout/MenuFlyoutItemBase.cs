using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutItemBase : TemplatedControl
	{
		static MenuFlyoutItemBase()
		{
			FocusableProperty.OverrideDefaultValue<MenuFlyoutItemBase>(true);
		}
	}
}
