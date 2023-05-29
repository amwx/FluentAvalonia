using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a menu item that displays a sub-menu in a <see cref="FAMenuFlyout"/> control.
/// </summary>
public partial class MenuFlyoutSubItem : MenuFlyoutItemBase
{
    public MenuFlyoutSubItem()
    {
        TemplateSettings = new MenuFlyoutItemTemplateSettings();
        var al = new AvaloniaList<object>();
        al.CollectionChanged += ItemsCollectionChanged;
        Items = al;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            TemplateSettings.Icon = IconHelpers.CreateFromUnknown(change.GetNewValue<IconSource>());
        }
        else if (change.Property == ItemsSourceProperty)
        {
            if (Items.Count > 0)
            {
                throw new InvalidOperationException("Items collection must be empty before using ItemsSource.");
            }

            var newV = change.GetNewValue<IEnumerable>();

            if (_presenter != null)
            {
                _presenter.ItemsSource = newV ?? Items;
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        var args = new RoutedEventArgs(MenuItem.PointerEnteredItemEvent, this);
        RaiseEvent(args);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        var args = new RoutedEventArgs(MenuItem.PointerExitedItemEvent, this);
        RaiseEvent(args);
    }

    internal void Open(bool fromKeyboard = false)
    {
        InitPopup();
        
        _subMenu.IsOpen = true;
        _presenter.MenuOpened(fromKeyboard);
    }

    internal void Close(bool isFullClose = false)
    {
        if (_presenter == null)
            return;

        // This ensures any open submenus are closed with this
        // This seems to only be needed with OverlayPopups
        foreach (var item in _presenter.GetRealizedContainers())
        {
            if (item is MenuFlyoutSubItem mfsi)
            {
                mfsi.Close();
            }
        }

        if (_subMenu != null)
        {
            if (_subMenu.IsOpen == false)
                return;

            _subMenu.IsOpen = false;
        }

        if (isFullClose)
        {
            var anc = this.FindAncestorOfType<FAMenuFlyoutPresenter>();
            anc.CloseMenu();
        }
    }

    private void InitPopup()
    {
        if (_subMenu == null)
        {
            _presenter = new FAMenuFlyoutPresenter()
            {
                ItemsSource = ItemsSource ?? Items,
                [!ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
                InternalParent = this
            };

            _subMenu = new Popup
            {
                Child = _presenter,
                HorizontalOffset = -4,
                WindowManagerAddShadowHint = false,
                Placement = PlacementMode.AnchorAndGravity,
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
        PseudoClasses.Set(s_pcSubmenuOpen, true);
    }

    private void OnPopupClose(object sender, EventArgs e)
    {
        PseudoClasses.Set(s_pcSubmenuOpen, false);
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {  
        if (ItemsSource != null)
        {
            throw new InvalidOperationException("Cannot edit Items when ItemsSource is set");
        }
    }

    private Popup _subMenu;
    private FAMenuFlyoutPresenter _presenter;
}
