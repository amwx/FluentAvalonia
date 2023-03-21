using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a menu item that displays a sub-menu in a <see cref="FAMenuFlyout"/> control.
/// </summary>
public partial class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuItem
{
    public MenuFlyoutSubItem()
    {
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
        else if (change.Property == ItemsProperty)
        {
            var (oldV, newV) = change.GetOldAndNewValue<IEnumerable>();
            if (oldV is INotifyCollectionChanged oldINCC)
            {
                oldINCC.CollectionChanged -= ItemsCollectionChanged;
            }

            _generatedItems.Clear();

            if (newV is INotifyCollectionChanged newINCC)
            {
                newINCC.CollectionChanged += ItemsCollectionChanged;
            }

            if ((_subMenu != null && _subMenu.IsOpen) && newV != null)
            {
                GenerateItems();
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

    /// <summary>
    /// Opens the SubMenu
    /// </summary>
    public void Open()
    {
        InitPopup();
        if (_generatedItems.Count == 0)
        {
            GenerateItems();
        }

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
                ItemsSource = _generatedItems,
                [!ItemContainerThemeProperty] = this[!ItemContainerThemeProperty]
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
        PseudoClasses.Set(s_pcSubmenuOpen, true);
    }

    private void OnPopupClose(object sender, EventArgs e)
    {
        PseudoClasses.Set(s_pcSubmenuOpen, false);
    }

    private void GenerateItems()
    {
        if (_items == null)
            return;

        _generatedItems.Clear();

        var first = _items.ElementAt(0);
        // If the first item is a MenuFlyoutItemBase, assume all are (added via xaml or code)
        if (first is MenuFlyoutItemBase)
        {
            _generatedItems.AddRange(_items.Cast<MenuFlyoutItemBase>());
            return;
        }

        var itemTemplate = ItemTemplate;
        foreach (var item in _items)
        {
            var template = _presenter.FindDataTemplate(item, itemTemplate);
            _generatedItems.Add(FAMenuFlyout.CreateContainer(item, template));
        }
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _generatedItems.Clear();

        if (_subMenu != null && _subMenu.IsOpen)
        {
            GenerateItems();
        }    
    }

    bool IMenuElement.MoveSelection(NavigationDirection direction, bool wrap) => false;

    private Popup _subMenu;
    private FAMenuFlyoutPresenter _presenter;
    private readonly AvaloniaList<MenuFlyoutItemBase> _generatedItems = new AvaloniaList<MenuFlyoutItemBase>();
}
