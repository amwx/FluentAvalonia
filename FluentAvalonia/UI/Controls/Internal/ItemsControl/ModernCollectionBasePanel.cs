using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls.Primitives
{
	/// <summary>
	/// Provides the base class for the ItemsStackPanel and the ItemsWrapGrid. Naming is taken from
	/// inspecting UWP, likely an internal class within DirectUI. This allows you to write custom
	/// virtualzing panels that can be used in an ItemsControl (must use FAItemsPresenter)
	/// </summary>
	public abstract class ModernCollectionBasePanel : Panel
	{
		public int FirstCacheIndex { get; protected set; }

		public int FirstVisibleIndex { get; protected set; }

		public int LastCacheIndex { get; protected set; }

		public int LastVisibleIndex { get; protected set; }
	}
}
