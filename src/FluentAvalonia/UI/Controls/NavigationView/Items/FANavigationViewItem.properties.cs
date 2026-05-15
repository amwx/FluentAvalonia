using Avalonia.Controls;
using Avalonia;
using FluentAvalonia.UI.Controls.Primitives;
using System.Collections;
using FluentAvalonia.Core;
using Avalonia.Controls.Metadata;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(FASharedPseudoclasses.s_pcLeftNav, FASharedPseudoclasses.s_pcTopNav, FASharedPseudoclasses.s_pcTopOverflow)]
[PseudoClasses(FASharedPseudoclasses.s_pcIconLeft, FASharedPseudoclasses.s_pcIconOnly, FASharedPseudoclasses.s_pcContentOnly)]
[PseudoClasses(s_pcSelected)]
[PseudoClasses(s_pcIconCollapsed)]
[PseudoClasses(FASharedPseudoclasses.s_pcChevronClosed, FASharedPseudoclasses.s_pcChevronOpen, FASharedPseudoclasses.s_pcChevronHidden)]
[PseudoClasses(s_pcInfoBadge)]
[TemplatePart(s_tpFlyoutContentGrid, typeof(Panel))]
[TemplatePart(s_tpNVIPresenter, typeof(FANavigationViewItemPresenter))]
[TemplatePart(s_tpNVIRootGrid, typeof(Grid))]
[TemplatePart(s_tpNVIMenuItemsHost, typeof(FAItemsRepeater))]
public partial class FANavigationViewItem
{
    /// <summary>
    /// Defines the <see cref="CompactPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> CompactPaneLengthProperty =
       AvaloniaProperty.Register<FANavigationViewItem, double>(nameof(CompactPaneLength), 48.0);

    /// <summary>
    /// Defines the <see cref="HasUnrealizedChildren"/> property
    /// </summary>
    public static readonly DirectProperty<FANavigationViewItem, bool> HasUnrealizedChildrenProperty =
        AvaloniaProperty.RegisterDirect<FANavigationViewItem, bool>(nameof(HasUnrealizedChildren),
            x => x.HasUnrealizedChildren, (x, v) => x.HasUnrealizedChildren = v);

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FANavigationViewItem>();

    /// <summary>
    /// Defines the <see cref="IsChildSelected"/> property
    /// </summary>
    public static readonly DirectProperty<FANavigationViewItem, bool> IsChildSelectedProperty =
        AvaloniaProperty.RegisterDirect<FANavigationViewItem, bool>(nameof(IsChildSelectedProperty),
            x => x.IsChildSelected, (x, v) => x.IsChildSelected = v);

    /// <summary>
    /// Defines the <see cref="IsExpanded"/> property
    /// </summary>
    public static readonly DirectProperty<FANavigationViewItem, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<FANavigationViewItem, bool>(nameof(IsExpanded),
            x => x.IsExpanded, (x, v) => x.IsExpanded = v);

    /// <summary>
    /// Defines the <see cref="MenuItems"/> property
    /// </summary>
    public static readonly DirectProperty<FANavigationViewItem, IList<object>> MenuItemsProperty =
        FANavigationView.MenuItemsProperty.AddOwner<FANavigationViewItem>(x => x.MenuItems);

    /// <summary>
    /// Defines the <see cref="MenuItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<IEnumerable> MenuItemsSourceProperty =
        FANavigationView.MenuItemsSourceProperty.AddOwner<FANavigationViewItem>();

    /// <summary>
    /// Defines the <see cref="SelectsOnInvoked"/> property
    /// </summary>
    public static readonly DirectProperty<FANavigationViewItem, bool> SelectsOnInvokedProperty =
        AvaloniaProperty.RegisterDirect<FANavigationViewItem, bool>(nameof(SelectsOnInvoked),
            x => x.SelectsOnInvoked, (x, v) => x.SelectsOnInvoked = v);

    /// <summary>
    /// Defines the <see cref="InfoBadge"/> property
    /// </summary>
    public static readonly StyledProperty<FAInfoBadge> InfoBadgeProperty =
        AvaloniaProperty.Register<FANavigationViewItem, FAInfoBadge>(nameof(InfoBadge));

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
        get;
        set
        {
            if (SetAndRaise(HasUnrealizedChildrenProperty, ref field, value))
            {
                OnHasUnrealizedChildrenPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon to show next to the menu item text.
    /// </summary>
    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that indicates whether or not descendant item is selected.
    /// </summary>
    public bool IsChildSelected
    {
        get;
        set => SetAndRaise(IsChildSelectedProperty, ref field, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether a tree node is expanded. Ignored if there are no menu items.
    /// </summary>
    public bool IsExpanded
    {
        get;
        set
        {
            if (SetAndRaise(IsExpandedProperty, ref field, value))
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
        get;
        set => SetAndRaise(SelectsOnInvokedProperty, ref field, value);
    } = true;

    /// <summary>
    /// Gets or sets the <see cref="InfoBadge"/> to display in the NavigationViewItem
    /// </summary>
    public FAInfoBadge InfoBadge
    {
        get => GetValue(InfoBadgeProperty);
        set => SetValue(InfoBadgeProperty, value);
    }

    //HELPER PROPERTIES

    internal Control SelectionIndicator => _presenter?.SelectionIndicator;

    internal FANavigationViewItemPresenter NVIPresenter => _presenter;

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

    private bool IsOnTopPrimary
    {
        get
        {
            bool isPaneDisplayModeTop = true;
            if (GetNavigationView is FANavigationView nv)
            {
                // There is a delay between the NavigationViewPaneDisplayMode update and the 
                // position property of NavigationViewItem being updated. This function gets called
                // in that delay period, so we need to check the PaneDisplayMode as further verification
                // of whether we are in Top mode or switching away from it.
                isPaneDisplayModeTop = nv.PaneDisplayMode == FANavigationViewPaneDisplayMode.Top;
            }

            return Position == NavigationViewRepeaterPosition.TopPrimary && isPaneDisplayModeTop;
        }
    }

    internal bool ShouldRepeaterShowInFlyout => (_isClosedCompact && IsTopLevelItem) || IsOnTopPrimary;

    internal bool IsRepeaterVisible => _repeater?.IsVisible ?? false;

    internal FAItemsRepeater GetRepeater => _repeater;

    private IList<object> _menuItems;

    private const string s_tpNVIPresenter = "NVIPresenter";
    private const string s_tpNVIRootGrid = "NVIRootGrid";
    private const string s_tpNVIMenuItemsHost = "NVIMenuItemsHost";
    private const string s_tpFlyoutContentGrid = "FlyoutContentGrid";
        
    private const string s_pcSelected = ":selected";
    private const string s_pcIconCollapsed = ":iconcollapsed";
    private const string s_pcInfoBadge = ":infobadge";
}
