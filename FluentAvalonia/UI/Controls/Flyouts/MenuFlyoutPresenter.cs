using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutPresenter : MenuBase, IStyleable
	{
		public MenuFlyoutPresenter()
			: base(new MenuFlyoutInteractionHandler(true))
		{

		}

		public MenuFlyoutPresenter(IMenuInteractionHandler handler)
			: base(handler) { }

		Type IStyleable.StyleKey => typeof(Avalonia.Controls.MenuFlyoutPresenter);

		public override void Close()
		{
			// DefaultMenuInteractionHandler calls this
			var host = this.FindLogicalAncestorOfType<Popup>();
			if (host != null)
			{
				for (int i = 0; i < LogicalChildren.Count; i++)
				{
					if (LogicalChildren[i] is IMenuItem item)
					{
						item.IsSubMenuOpen = false;
					}
				}

				SelectedIndex = -1;
				host.IsOpen = false;
			}
		}

		public override void Open()
		{
			throw new NotSupportedException("Use MenuFlyout.ShowAt(Control) instead");
		}

		protected override IItemContainerGenerator CreateItemContainerGenerator()
		{
			return new MenuFlyoutPresenterItemContainerGenerator(this);
		}

		internal bool InternalMoveSelection(NavigationDirection dir, bool wrap) => MoveSelection(dir, wrap);

		protected override void OnContainersMaterialized(ItemContainerEventArgs e)
		{
			base.OnContainersMaterialized(e);
			for (int i = 0; i <e.Containers.Count; i++)
			{
				if (e.Containers[i].ContainerControl is ToggleMenuFlyoutItem tmfi)
				{
					if (tmfi.Icon != null)
					{
						_iconCount++;
					}

					_toggleCount++;
				}
				else if (e.Containers[i].ContainerControl is MenuFlyoutItem mfi)
				{
					if (mfi.Icon != null)
					{
						_iconCount++;
					}
				}
			}

			UpdateVisualState();
		}

		protected override void OnContainersDematerialized(ItemContainerEventArgs e)
		{
			base.OnContainersDematerialized(e);
			for (int i = 0; i < e.Containers.Count; i++)
			{
				if (e.Containers[i].ContainerControl is ToggleMenuFlyoutItem tmfi)
				{
					if (tmfi.Icon != null)
					{
						_iconCount--;
					}

					_toggleCount--;
				}
				else if (e.Containers[i].ContainerControl is MenuFlyoutItem mfi)
				{
					if (mfi.Icon != null)
					{
						_iconCount--;
					}

				}
			}

			UpdateVisualState();
		}

		private void UpdateVisualState()
		{
			PseudoClasses.Set(":icons", _iconCount > 0);
			PseudoClasses.Set(":toggle", _toggleCount > 0);
		}


		private int _iconCount = 0;
		private int _toggleCount = 0;
	}	
}
