using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(s_pcSubmenuOpen)]
public partial class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuItem
{
    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        MenuFlyoutItem.TextProperty.AddOwner<MenuFlyoutSubItem>();

    /// <summary>
    /// Defines the <see cref="Icon"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        SettingsExpander.IconSourceProperty.AddOwner<MenuFlyoutSubItem>();

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
    /// Defines the <see cref="ItemContainerTheme"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<ControlTheme>();

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<MenuFlyoutItemTemplateSettings> TemplateSettingsProperty =
        MenuFlyoutItem.TemplateSettingsProperty.AddOwner<MenuFlyoutSubItem>();

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
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
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

    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ControlTheme"/> to apply for the items
    /// </summary>
    public ControlTheme ItemContainerTheme
    {
        get => GetValue(ItemContainerThemeProperty);
        set => SetValue(ItemContainerThemeProperty, value);
    }

    /// <summary>
    /// Gets the template settings for this MenuFlyoutItem
    /// </summary>
    public MenuFlyoutItemTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
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
                return _presenter.ContainerFromIndex(_presenter.SelectedIndex) as IMenuItem;
            }

            return null;
        }
        set
        {
            if (_presenter != null)
            {
                _presenter.SelectedIndex = _presenter.IndexFromContainer(value as Control);
            }
        }
    }

    IEnumerable<IMenuItem> IMenuElement.SubItems
    {
        get
        {
            return _presenter.GetRealizedContainers().Cast<IMenuItem>();
            //_presenter?.ItemContainerGenerator.Containers
            //    .Select(x => x.ContainerControl)
            //    .OfType<IMenuItem>();
        }
    }


    void IMenuItem.RaiseClick()
    { }

    bool IMenuItem.StaysOpenOnClick { get => false; set { } }

    private IEnumerable _items;

    private const string s_pcSubmenuOpen = ":submenuopen";
}
