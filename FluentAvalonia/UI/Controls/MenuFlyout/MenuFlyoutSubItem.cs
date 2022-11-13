using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a menu item that displays a sub-menu in a <see cref="FAMenuFlyout"/> control.
/// </summary>
public partial class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuItem
{
    public MenuFlyoutSubItem()
    {
        _items = new AvaloniaList<object>();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
        //      this is to just change the event name and source and re-raise it. This isn't ideal
        e.RoutedEvent = MenuItem.PointerEnteredItemEvent;
        e.Source = this;
        RaiseEvent(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
        //      this is to just change the event name and source and re-raise it. This isn't ideal
        e.RoutedEvent = MenuItem.PointerExitedItemEvent;
        e.Source = this;
        RaiseEvent(e);
    }

    /// <summary>
    /// Opens the SubMenu
    /// </summary>
    public void Open()
    {
        InitPopup();
        _subMenu.IsOpen = true;
        _presenter.RaiseMenuOpened();
    }

    /// <summary>
    /// Closes the SubMenu
    /// </summary>
    public void Close()
    {
        if (_subMenu != null)
            _subMenu.IsOpen = false;

        if (_presenter != null)
        {
            _presenter.SelectedIndex = -1;
            _presenter.RaiseMenuClosed();
        }
    }

    private void InitPopup()
    {
        if (_subMenu == null)
        {
            _presenter = new FAMenuFlyoutPresenter()
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

    bool IMenuElement.MoveSelection(NavigationDirection direction, bool wrap) => false;

    private Popup _subMenu;
    private FAMenuFlyoutPresenter _presenter;
}
