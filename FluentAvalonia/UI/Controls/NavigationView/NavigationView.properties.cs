using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace FluentAvalonia.UI.Controls;

public partial class NavigationView : HeaderedContentControl
{
    /// <summary>
    /// Defines the <see cref="AlwaysShowHeader"/> property
    /// </summary>
    public static readonly StyledProperty<bool> AlwaysShowHeaderProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(AlwaysShowHeader), true);

    /// <summary>
    /// Defines the <see cref="AutoCompleteBox"/> property
    /// </summary>
    public static readonly StyledProperty<AutoCompleteBox> AutoCompleteBoxProperty =
        AvaloniaProperty.Register<NavigationView, AutoCompleteBox>(nameof(AutoCompleteBox));

    /// <summary>
    /// Defines the <see cref="CompactModeThresholdWidth"/> property
    /// </summary>
    public static readonly StyledProperty<double> CompactModeThresholdWidthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(CompactModeThresholdWidth),
            641.0, coerce: CoercePropertyValueToGreaterThanZero);

    /// <summary>
    /// Defines the <see cref="CompactPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(CompactPaneLength),
            48.0, coerce: CoercePropertyValueToGreaterThanZero);

    /// <summary>
    /// Defines the <see cref="ContentOverlay"/> property
    /// </summary>
    public static readonly StyledProperty<IControl> ContentOverlayProperty =
        AvaloniaProperty.Register<NavigationView, IControl>(nameof(ContentOverlay));

    /// <summary>
    /// Defines the <see cref="DisplayMode"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, NavigationViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, NavigationViewDisplayMode>(nameof(DisplayMode),
            x => x.DisplayMode);

    /// <summary>
    /// Defines the <see cref="ExpandedModeThresholdWidth"/> property
    /// </summary>
    public static readonly StyledProperty<double> ExpandedModeThresholdWidthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(ExpandedModeThresholdWidth), 1008.0,
            coerce: CoercePropertyValueToGreaterThanZero);

    /// <summary>
    /// Defines the <see cref="FooterMenuItemsProperty"/>
    /// </summary>
    public static readonly DirectProperty<NavigationView, IEnumerable> FooterMenuItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable>(nameof(FooterMenuItems),
            x => x.FooterMenuItems, (x, v) => x.FooterMenuItems = v);

    //In WinUI, this is enum NavigationViewBackButtonVisible
    //Visible
    //Collapsed
    //Auto - depends on form factor, for now, not concern
    //So fall back to bool
    /// <summary>
    /// Defines the <see cref="IsBackButtonVisible"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(IsBackButtonVisible),
            x => x.IsBackButtonVisible, (x, v) => x.IsBackButtonVisible = v);

    /// <summary>
    /// Defines the <see cref="IsBackEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsBackEnabledProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsBackEnabled), false);

    /// <summary>
    /// Defines the <see cref="IsPaneOpen"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, bool> IsPaneOpenProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(IsPaneOpen),
            x => x.IsPaneOpen, (x, v) => x.IsPaneOpen = v);

    /// <summary>
    /// Defines the <see cref="IsPaneToggleButtonVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsPaneToggleButtonVisibleProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsPaneToggleButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="IsPaneVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsPaneVisibleProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsPaneVisible), true);

    /// <summary>
    /// Defines the <see cref="IsSettingsVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsSettingsVisibleProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsSettingsVisible), true);

    //SKIP for now, IsTitleBarAutoPaddingEnabled...

    /// <summary>
    /// Defines the <see cref="MenuItems"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, IEnumerable> MenuItemsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable>(nameof(MenuItems),
            o => o.MenuItems, (o, v) => o.MenuItems = v);

    /// <summary>
    /// Defines the <see cref="MenuItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> MenuItemTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate>(nameof(MenuItemTemplate));

    /// <summary>
    /// Defines the <see cref="MenuItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<DataTemplateSelector> MenuItemTemplateSelectorProperty =
    AvaloniaProperty.Register<NavigationView, DataTemplateSelector>(nameof(MenuItemTemplateSelector));

    /// <summary>
    /// Defines the <see cref="OpenPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(OpenPaneLength),
            320.0, coerce: CoercePropertyValueToGreaterThanZero);

    //OverflowLabelModeProperty removed, as it was deprecated

    /// <summary>
    /// Defines the <see cref="PaneCustomContent"/> property
    /// </summary>
    public static readonly StyledProperty<IControl> PaneCustomContentProperty =
        AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneCustomContent));

    /// <summary>
    /// Defines the <see cref="PaneDisplayMode"/> property
    /// </summary>
    public static readonly StyledProperty<NavigationViewPaneDisplayMode> PaneDisplayModeProperty =
        AvaloniaProperty.Register<NavigationView, NavigationViewPaneDisplayMode>(nameof(PaneDisplayMode),
            NavigationViewPaneDisplayMode.Auto);

    /// <summary>
    /// Defines the <see cref="PaneFooter"/> property
    /// </summary>
    public static readonly StyledProperty<IControl> PaneFooterProperty =
        AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneFooter));

    /// <summary>
    /// Defines the <see cref="PaneHeader"/> property
    /// </summary>
    public static readonly StyledProperty<IControl> PaneHeaderProperty =
        AvaloniaProperty.Register<NavigationView, IControl>(nameof(PaneHeader));

    /// <summary>
    /// Defines the <see cref="PaneTitle"/> property
    /// </summary>
    public static readonly StyledProperty<string> PaneTitleProperty =
        AvaloniaProperty.Register<NavigationView, string>(nameof(PaneTitle));

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, object> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, object>(nameof(SelectedItem),
            x => x.SelectedItem, (x, v) => x.SelectedItem = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    //WinUI uses an enum here, but only has Disabled/Enabled, so just use bool
    /// <summary>
    /// Defines the <see cref="SelectionFollowsFocus"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, bool> SelectionFollowsFocusProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(SelectionFollowsFocus),
            x => x.SelectionFollowsFocus, (x, v) => x.SelectionFollowsFocus = v);

    /// <summary>
    /// Defines the <see cref="SettingsItem"/> property
    /// </summary>
    public static readonly DirectProperty<NavigationView, NavigationViewItem> SettingsItemProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, NavigationViewItem>(nameof(SettingsItem),
            x => x.SettingsItem, (x, v) => x.SettingsItem = v);

    //Ignore Shoulder Navigation (xbox)

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<NavigationViewTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<NavigationView, NavigationViewTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Gets or sets a value that indicates whether the header is always visible.
    /// </summary>
    public bool AlwaysShowHeader
    {
        get => GetValue(AlwaysShowHeaderProperty);
        set => SetValue(AlwaysShowHeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="Avalonia.Controls.AutoCompleteBox"/> to be displayed in the NavigationView.
    /// </summary>
    public AutoCompleteBox AutoCompleteBox
    {
        get => GetValue(AutoCompleteBoxProperty);
        set => SetValue(AutoCompleteBoxProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum window width at which the NavigationView enters Compact display mode.
    /// </summary>
    public double CompactModeThresholdWidth
    {
        get => GetValue(CompactModeThresholdWidthProperty);
        set => SetValue(CompactModeThresholdWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the NavigationView pane in its compact display mode.
    /// </summary>
    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets a UI element that is shown at the top of the control, below the pane 
    /// if PaneDisplayMode is Top.
    /// </summary>
    public IControl ContentOverlay
    {
        get => GetValue(ContentOverlayProperty);
        set => SetValue(ContentOverlayProperty, value);
    }

    /// <summary>
    /// Gets a value that specifies how the pane and content areas of a NavigationView are being shown.
    /// </summary>
    public NavigationViewDisplayMode DisplayMode
    {
        get => _displayMode;
        private set => SetAndRaise(DisplayModeProperty, ref _displayMode, value);
    }

    /// <summary>
    /// Gets or sets the minimum window width at which the NavigationView enters Expanded display mode.
    /// </summary>
    public double ExpandedModeThresholdWidth
    {
        get => GetValue(ExpandedModeThresholdWidthProperty);
        set => SetValue(ExpandedModeThresholdWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the list of objects to be used as navigation items in the footer menu.
    /// </summary>
    public IEnumerable FooterMenuItems
    {
        get => _footerMenuItems;
        set
        {
            var old = _footerMenuItems;
            if (SetAndRaise(FooterMenuItemsProperty, ref _footerMenuItems, value))
            {
                if (old is INotifyCollectionChanged oldINCC)
                {
                    oldINCC.CollectionChanged -= OnFooterItemsSourceCollectionChanged;
                }
                if (value is INotifyCollectionChanged newINCC)
                {
                    newINCC.CollectionChanged += OnFooterItemsSourceCollectionChanged;
                }

                UpdateFooterRepeaterItemsSource(true, true);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the back button is visible or not.
    /// </summary>
    public bool IsBackButtonVisible
    {
        get => _isBackVisible;
        set
        {
            if (SetAndRaise(IsBackButtonVisibleProperty, ref _isBackVisible, value))
            {
                UpdateBackAndCloseButtonsVisibility();
                UpdateAdaptiveLayout(Bounds.Width);
                if (IsTopNavigationView)
                {
                    InvalidateTopNavPrimaryLayout();
                }

                // Enabling back button shifts grid instead of resizing, so let's update the layout.
                if (_backButton != null)
                {
                    //Don't have update layout, so
                    _backButton.InvalidateMeasure();
                }
                UpdatePaneLayout();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the back button is enabled or disabled.
    /// </summary>
    public bool IsBackEnabled
    {
        get => GetValue(IsBackEnabledProperty);
        set => SetValue(IsBackEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that specifies whether the NavigationView pane is expanded to its full width.
    /// </summary>
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set
        {
            if (SetAndRaise(IsPaneOpenProperty, ref _isPaneOpen, value))
            {
                OnIsPaneOpenChanged();
                UpdateVisualStateForDisplayModeGroup(_displayMode);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the menu toggle button is shown.
    /// </summary>
    public bool IsPaneToggleButtonVisible
    {
        get => GetValue(IsPaneToggleButtonVisibleProperty);
        set => SetValue(IsPaneToggleButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that determines whether the pane is shown.
    /// </summary>
    public bool IsPaneVisible
    {
        get => GetValue(IsPaneVisibleProperty);
        set => SetValue(IsPaneVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the settings button is shown.
    /// </summary>
    public bool IsSettingsVisible
    {
        get => GetValue(IsSettingsVisibleProperty);
        set => SetValue(IsSettingsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the DataTemplate used to display each menu item.
    /// </summary>
    public IDataTemplate MenuItemTemplate
    {
        get => GetValue(MenuItemTemplateProperty);
        set => SetValue(MenuItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a reference to a custom DataTemplateSelector logic class. The DataTemplateSelector 
    /// referenced by this property returns a template to apply to items.
    /// </summary>
    /// <remarks>
    /// This property should generally not be used but was added to support different containers for different
    /// data types. Should a more "Avalonia-like" solution arise, this property will be removed
    /// </remarks>
    public DataTemplateSelector MenuItemTemplateSelector
    {
        get => GetValue(MenuItemTemplateSelectorProperty);
        set => SetValue(MenuItemTemplateSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of menu items displayed in the NavigationView.
    /// </summary>
    public IEnumerable MenuItems
    {
        get => _menuItems;
        set
        {
            var old = _menuItems;
            if (SetAndRaise(MenuItemsProperty, ref _menuItems, value))
            {
                if (_menuItems is INotifyCollectionChanged oldINCC)
                {
                    oldINCC.CollectionChanged -= OnMenuItemsSourceCollectionChanged;
                }
                if (value is INotifyCollectionChanged newINCC)
                {
                    newINCC.CollectionChanged += OnMenuItemsSourceCollectionChanged;
                }
                UpdateRepeaterItemsSource(true);
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the NavigationView pane when it's fully expanded.
    /// </summary>
    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    //OverflowLabelMode removed, deprecated in WinUI

    /// <summary>
    /// Gets or sets a UI element that is shown in the NavigationView pane.
    /// </summary>
    public IControl PaneCustomContent
    {
        get => GetValue(PaneCustomContentProperty);
        set => SetValue(PaneCustomContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how and where the NavigationView pane is shown.
    /// </summary>
    public NavigationViewPaneDisplayMode PaneDisplayMode
    {
        get => GetValue(PaneDisplayModeProperty);
        set => SetValue(PaneDisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the content for the pane footer.
    /// </summary>
    public IControl PaneFooter
    {
        get => GetValue(PaneFooterProperty);
        set => SetValue(PaneFooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the content for the pane header.
    /// </summary>
    public IControl PaneHeader
    {
        get => GetValue(PaneHeaderProperty);
        set => SetValue(PaneHeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the label adjacent to the menu icon when the NavigationView pane is open.
    /// </summary>
    public string PaneTitle
    {
        get => GetValue(PaneTitleProperty);
        set => SetValue(PaneTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
        }
    }

    //WinUI uses an enum here, but only has Disabled/Enabled, so just use bool
    /// <summary>
    /// Gets or sets a value that indicates whether item selection changes when keyboard focus changes.
    /// </summary>
    /// <remarks>
    /// Do not set this property to true if you have hierarchical navigation as things get weird. This
    /// behavior also occurs in WinUI
    /// </remarks>
    public bool SelectionFollowsFocus
    {
        get => _selectionFollowsFocus;
        set => SetAndRaise(SelectionFollowsFocusProperty, ref _selectionFollowsFocus, value);
    }

    /// <summary>
    /// Gets the navigation item that represents the entry point to app settings.
    /// </summary>
    public NavigationViewItem SettingsItem
    {
        get => _settingsItem;
        internal set => SetAndRaise(SettingsItemProperty, ref _settingsItem, value);
    }

    /// <summary>
    /// Gets an object that provides calculated values that can be referenced as TemplateBinding sources 
    /// when defining templates for a NavigationView control.
    /// </summary>
    public NavigationViewTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        protected set => SetValue(TemplateSettingsProperty, value);
    }

    /// <summary>
    /// Coerces a double to ensure its valid for use, i.e. >= 0 and not NaN or infinity
    /// Used for: CompactModeThresholdWidthProperty, CompactPaneLengthProperty, 
    /// ExpandedModeThresholdWidthProperty, and OpenPaneLengthProperty
    /// </summary>
    /// <returns></returns>
    private static double CoercePropertyValueToGreaterThanZero(IAvaloniaObject arg1, double arg2)
    {
        if (double.IsNaN(arg2) || double.IsInfinity(arg2))
            return 0;
        return Math.Max(arg2, 0.0);
    }

    /// <summary>
    /// Occurs when the NavigationView pane is closing.
    /// </summary>
    public event TypedEventHandler<NavigationView, NavigationViewPaneClosingEventArgs> PaneClosing;

    /// <summary>
    /// Occurs when the NavigationView pane is closed.
    /// </summary>
    public event TypedEventHandler<NavigationView, EventArgs> PaneClosed;

    /// <summary>
    /// Occurs when the NavigationView pane is opening.
    /// </summary>
    public event TypedEventHandler<NavigationView, EventArgs> PaneOpening;

    /// <summary>
    /// Occurs when the NavigationView pane is opened.
    /// </summary>
    public event TypedEventHandler<NavigationView, EventArgs> PaneOpened;

    /// <summary>
    /// Occurs when the back button receives an interaction such as a click or tap.
    /// </summary>
    public event EventHandler<NavigationViewBackRequestedEventArgs> BackRequested;

    /// <summary>
    /// Occurs when the currently selected item changes.
    /// </summary>
    public event EventHandler<NavigationViewSelectionChangedEventArgs> SelectionChanged;

    /// <summary>
    /// Occurs when an item in the menu receives an interaction such a a click or tap.
    /// </summary>
    public event EventHandler<NavigationViewItemInvokedEventArgs> ItemInvoked;

    /// <summary>
    /// Occurs when the DisplayMode property changes.
    /// </summary>
    public event EventHandler<NavigationViewDisplayModeChangedEventArgs> DisplayModeChanged;

    /// <summary>
    /// Occurs when a node in the tree starts to expand.
    /// </summary>
    public event EventHandler<NavigationViewItemExpandingEventArgs> ItemExpanding;

    /// <summary>
    /// Occurs when a node in the tree is collapsed.
    /// </summary>
    public event EventHandler<NavigationViewItemCollapsedEventArgs> ItemCollapsed;

    /// <summary>
    /// Property that stores disposables to each NavigationViewItem when their created in the ItemsRepeater,
    /// so they can be disposed when the item is removed
    /// </summary>
    internal static readonly AttachedProperty<CompositeDisposable> NavigationViewItemRevokersProperty =
        AvaloniaProperty.RegisterAttached<NavigationView, NavigationViewItem, CompositeDisposable>("NavigationViewItemRevokers");

    private bool _isPaneOpen = true;
    private object _selectedItem;
    private bool _isBackVisible;
    private bool _selectionFollowsFocus;
    private IEnumerable _menuItems;
    private IEnumerable _footerMenuItems;
    private NavigationViewDisplayMode _displayMode = NavigationViewDisplayMode.Minimal;
    private NavigationViewItem _settingsItem;
}
