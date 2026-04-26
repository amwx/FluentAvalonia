using Avalonia.Controls;
using Avalonia;
using FluentAvalonia.UI.Controls.Primitives;
using System.Collections;
using FluentAvalonia.Core;
using Avalonia.Controls.Metadata;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(SharedPseudoclasses.s_pcLeftNav, SharedPseudoclasses.s_pcTopNav, SharedPseudoclasses.s_pcTopOverflow)]
[PseudoClasses(SharedPseudoclasses.s_pcIconLeft, SharedPseudoclasses.s_pcIconOnly, SharedPseudoclasses.s_pcContentOnly)]
[PseudoClasses(s_pcSelected)]
[PseudoClasses(s_pcIconCollapsed)]
[PseudoClasses(SharedPseudoclasses.s_pcChevronClosed, SharedPseudoclasses.s_pcChevronOpen, SharedPseudoclasses.s_pcChevronHidden)]
[PseudoClasses(s_pcInfoBadge)]
[TemplatePart(s_tpFlyoutContentGrid, typeof(Panel))]
[TemplatePart(s_tpNVIPresenter, typeof(NavigationViewItemPresenter))]
[TemplatePart(s_tpNVIRootGrid, typeof(Grid))]
[TemplatePart(s_tpNVIMenuItemsHost, typeof(ItemsRepeater))]
public partial class NavigationViewItem
{
    /// <summary>
    /// Defines the <see cref="CompactPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> CompactPaneLengthProperty =
       AvaloniaProperty.Register<NavigationViewItem, double>(nameof(CompactPaneLength), 48.0);

    /// <summary>
    /// Defines the <see cref="HasUnrealizedChildren"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, bool> HasUnrealizedChildrenProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(HasUnrealizedChildren),
            x => x.HasUnrealizedChildren, (x, v) => x.HasUnrealizedChildren = v);

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        SettingsExpander.IconSourceProperty.AddOwner<NavigationViewItem>();

    /// <summary>
    /// Defines the <see cref="IsChildSelected"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, bool> IsChildSelectedProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(IsChildSelectedProperty),
            x => x.IsChildSelected, (x, v) => x.IsChildSelected = v);

    /// <summary>
    /// Defines the <see cref="IsExpanded"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(IsExpanded),
            x => x.IsExpanded, (x, v) => x.IsExpanded = v);

    /// <summary>
    /// Defines the <see cref="MenuItems"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, IList<object>> MenuItemsProperty =
        NavigationView.MenuItemsProperty.AddOwner<NavigationViewItem>(x => x.MenuItems);

    /// <summary>
    /// Defines the <see cref="MenuItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<IEnumerable> MenuItemsSourceProperty =
        NavigationView.MenuItemsSourceProperty.AddOwner<NavigationViewItem>();

    /// <summary>
    /// Defines the <see cref="SelectsOnInvoked"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationViewItem, bool> SelectsOnInvokedProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItem, bool>(nameof(SelectsOnInvoked),
            x => x.SelectsOnInvoked, (x, v) => x.SelectsOnInvoked = v);

    /// <summary>
    /// Defines the <see cref="InfoBadge"/> property
    /// </summary>
    public static readonly StyledProperty<InfoBadge> InfoBadgeProperty =
        AvaloniaProperty.Register<NavigationViewItem, InfoBadge>(nameof(InfoBadge));

    /// <summary>
    /// Gets the CompactPaneLength of the NavigationView that hosts this item.
    /// </summary>
    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        private set => SetValue(CompactPaneLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the current item has child items that haven't been shown.
    /// </summary>
    public bool HasUnrealizedChildren
    {
        get => _hasUnrealizedChildren;
        set
        {
            if (SetAndRaise(HasUnrealizedChildrenProperty, ref _hasUnrealizedChildren, value))
            {
                OnHasUnrealizedChildrenPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon to show next to the menu item text.
    /// </summary>
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that indicates whether or not descendant item is selected.
    /// </summary>
    public bool IsChildSelected
    {
        get => _isChildSelected;
        set => SetAndRaise(IsChildSelectedProperty, ref _isChildSelected, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether a tree node is expanded. Ignored if there are no menu items.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetAndRaise(IsExpandedProperty, ref _isExpanded, value))
            {
                OnIsExpandedPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the collection of menu items displayed as children of the NavigationViewItem.
    /// </summary>
    public IList<object> MenuItems
    {
        get => _menuItems;
        private set => SetAndRaise(MenuItemsProperty, ref _menuItems, value);
    }

    /// <summary>
    /// Gets or sets an object source used to generate the content of the NavigationViewItem submenu.
    /// </summary>
    public IEnumerable MenuItemsSource
    {
        get => GetValue(MenuItemsSourceProperty);
        set => SetValue(MenuItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether invoking a navigation menu item also selects it.
    /// </summary>
    public bool SelectsOnInvoked
    {
        get => _selectsOnInvoked;
        set => SetAndRaise(SelectsOnInvokedProperty, ref _selectsOnInvoked, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="InfoBadge"/> to display in the NavigationViewItem
    /// </summary>
    public InfoBadge InfoBadge
    {
        get => GetValue(InfoBadgeProperty);
        set => SetValue(InfoBadgeProperty, value);
    }

    //HELPER PROPERTIES

    internal Control SelectionIndicator => _presenter?.SelectionIndicator;

    internal NavigationViewItemPresenter NVIPresenter => _presenter;

    private bool HasChildren =>
        (MenuItems != null && MenuItems.Count() > 0) ||
        (MenuItemsSource != null && _repeater != null &&
        _repeater.ItemsSourceView != null &&
        _repeater.ItemsSourceView.Count > 0) ||
        HasUnrealizedChildren;

    private bool ShouldShowIcon => IconSource != null;

    private bool ShouldEnableToolTip => IsOnLeftNav && _isClosedCompact;

    private bool ShouldShowContent => Content != null;

    private bool IsOnLeftNav => Position == NavigationViewRepeaterPosition.LeftNav ||
        Position == NavigationViewRepeaterPosition.LeftFooter;

    private bool IsOnTopPrimary => Position == NavigationViewRepeaterPosition.TopPrimary;

    internal bool ShouldRepeaterShowInFlyout => (_isClosedCompact && IsTopLevelItem) || IsOnTopPrimary;

    internal bool IsRepeaterVisible => _repeater?.IsVisible ?? false;

    internal ItemsRepeater GetRepeater => _repeater;

    private bool _hasUnrealizedChildren;
    private bool _isChildSelected;
    private bool _isExpanded;
    private IList<object> _menuItems;
    private bool _selectsOnInvoked = true;

    private const string s_tpNVIPresenter = "NVIPresenter";
    private const string s_tpNVIRootGrid = "NVIRootGrid";
    private const string s_tpNVIMenuItemsHost = "NVIMenuItemsHost";
    private const string s_tpFlyoutContentGrid = "FlyoutContentGrid";
        
    private const string s_pcSelected = ":selected";
    private const string s_pcIconCollapsed = ":iconcollapsed";
    private const string s_pcInfoBadge = ":infobadge";
}
