using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluentAvalonia.UI.Controls;

public partial class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuItem
{
    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, string>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<MenuFlyoutItem, FAIconElement>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="Items"/> property
    /// </summary>
    public static readonly DirectProperty<MenuFlyoutSubItem, IEnumerable> ItemsProperty =
        ItemsControl.ItemsProperty.AddOwner<MenuFlyoutSubItem>(x => x.Items,
            (x, v) => x.Items = v);

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<MenuFlyoutSubItem>();

    /// <summary>
    /// Gets or sets the text content of a MenuFlyoutSubItem.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the graphic content of the menu flyout subitem.
    /// </summary>
    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection used to generate the content of the sub-menu.
    /// </summary>
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

    bool IMenuItem.IsTopLevel => false;

    IMenuElement IMenuItem.Parent => Parent as IMenuElement;

    IMenuItem IMenuElement.SelectedItem
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

    bool IMenuItem.StaysOpenOnClick { get => false; set { } }

    private IEnumerable _items;
}
