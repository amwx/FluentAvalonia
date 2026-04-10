using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using System.Collections;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(s_pcSubmenuOpen)]
public partial class FAMenuFlyoutSubItem : FAMenuFlyoutItemBase
{
    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        FAMenuFlyoutItem.TextProperty.AddOwner<FAMenuFlyoutSubItem>();

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FAMenuFlyoutSubItem>();

    /// <summary>
    /// Defines the <see cref="Items"/> property
    /// </summary>
    public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<FAMenuFlyoutSubItem>();

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<FAMenuFlyoutSubItem>();

    /// <summary>
    /// Defines the <see cref="ItemContainerTheme"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<ControlTheme>();

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<FAMenuFlyoutItemTemplateSettings> TemplateSettingsProperty =
        FAMenuFlyoutItem.TemplateSettingsProperty.AddOwner<FAMenuFlyoutSubItem>();

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
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    
    /// <summary>
    /// Gets the items of the MenuFlyoutSubItem
    /// </summary>
    /// <remarks>
    /// NOTE: Unlike normal ItemsControls, when ItemsSource is set, this property will
    /// not act as a view over the ItemsSource
    /// </remarks>
    [Content]
    public IList Items { get; }

    /// <summary>
    /// Gets or sets the collection used to generate the content of the sub-menu.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
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
    public FAMenuFlyoutItemTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    public bool IsPointerOverSubMenu => _subMenu?.IsPointerOverPopup ?? false;

    private const string s_pcSubmenuOpen = ":submenuopen";
}
