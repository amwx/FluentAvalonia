using Avalonia.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuItem
	{
		public MenuFlyoutSubItem()
		{
			_items = new AvaloniaList<object>();
		}

		public static readonly StyledProperty<string> TextProperty =
			AvaloniaProperty.Register<MenuFlyoutItem, string>(nameof(Text));

		public static readonly StyledProperty<IconElement> IconProperty =
			AvaloniaProperty.Register<MenuFlyoutItem, IconElement>(nameof(Icon));

		public static readonly DirectProperty<MenuFlyoutSubItem, IEnumerable> ItemsProperty =
			ItemsControl.ItemsProperty.AddOwner<MenuFlyoutSubItem>(x => x.Items,
				(x, v) => x.Items = v);

		public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
			ItemsControl.ItemTemplateProperty.AddOwner<MenuFlyoutSubItem>();

		public string Text
		{
			get => GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public IconElement Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		[Content]
		public IEnumerable Items
		{
			get => _items;
			set => SetAndRaise(ItemsProperty, ref _items, value);
		}

		public bool HasSubMenu => true;
		public bool IsPointerOverSubMenu => _subMenu?.IsPointerOverPopup ?? false;
		public bool IsSubMenuOpen
		{
			get => _subMenu?.IsOpen ?? false;
			set
			{
				if (value)
					Open();
				else
					Close();
			}
		}
		public bool IsTopLevel => false;
		IMenuElement IMenuItem.Parent => Parent as IMenuElement;
		public IMenuItem SelectedItem
		{
			get
			{
				if (_presenter != null && _presenter.SelectedIndex != -1)
				{
					return _presenter.ItemContainerGenerator.ContainerFromIndex(_presenter.SelectedIndex) as IMenuItem;
				}

				return null;
			}
			set
			{
				if (_presenter != null)
				{
					_presenter.SelectedIndex = _presenter.ItemContainerGenerator.IndexFromContainer(value);
				}
			}
		}
		IEnumerable<IMenuItem> IMenuElement.SubItems
		{
			get
			{
				return _presenter?.ItemContainerGenerator.Containers
					.Select(x => x.ContainerControl)
					.OfType<IMenuItem>();
			}
		}


		void IMenuItem.RaiseClick()
		{ }

		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);

			var point = e.GetCurrentPoint(null);
			RaiseEvent(new PointerEventArgs(MenuItem.PointerEnterItemEvent, this, e.Pointer, this.VisualRoot, point.Position,
				e.Timestamp, point.Properties, e.KeyModifiers));
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			base.OnPointerLeave(e);

			var point = e.GetCurrentPoint(null);
			RaiseEvent(new PointerEventArgs(MenuItem.PointerLeaveItemEvent, this, e.Pointer, this.VisualRoot, point.Position,
				e.Timestamp, point.Properties, e.KeyModifiers));
		}

		public void Open()
		{
			InitPopup();
			_subMenu.IsOpen = true;
		}

		public void Close()
		{
			_subMenu.IsOpen = false;
			_presenter.SelectedIndex = -1;
		}

		public bool MoveSelection(NavigationDirection direction, bool wrap)
		{
			if (_presenter.SelectedIndex == -1)
			{
				int index = 0;
				var ct = _presenter.ItemCount;
				for (int i = 0; i < ct; i++)
				{
					if (_presenter.ItemContainerGenerator.ContainerFromIndex(i).Focusable)
					{
						index = i;
						break;
					}
				}

				FocusManager.Instance.Focus(_presenter.ItemContainerGenerator.ContainerFromIndex(index), NavigationMethod.Directional);
				_presenter.SelectedIndex = index;

				return true;
			}

			var sel = _presenter?.InternalMoveSelection(direction, wrap) ?? false;
			if (sel)
			{
				FocusManager.Instance.Focus(_presenter.ItemContainerGenerator.ContainerFromIndex(_presenter.SelectedIndex), NavigationMethod.Directional);
			}

			return sel;
		}

		private void InitPopup()
		{
			if (_subMenu == null)
			{
				_presenter = new MenuFlyoutPresenter()
				{
					[!ItemsControl.ItemsProperty] = this[!ItemsProperty],
					[!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty]
				};

				_subMenu = new Popup
				{
					Child = _presenter,
					HorizontalOffset = -4,
					WindowManagerAddShadowHint = false,
					PlacementMode = PlacementMode.AnchorAndGravity,
					PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.TopRight,
					PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomRight,
					PlacementTarget = this
				};

				LogicalChildren.Add(_subMenu);

				_subMenu.Opened += OnPopupOpen;
				_subMenu.Closed += OnPopupClose;
			}
		}

		private void OnPopupOpen(object sender, EventArgs e)
		{
			PseudoClasses.Set(":submenuopen", true);
		}

		private void OnPopupClose(object sender, EventArgs e)
		{
			PseudoClasses.Set(":submenuopen", false);
		}


		private Popup _subMenu;
		private MenuFlyoutPresenter _presenter;
		private IEnumerable _items;
	}
}
