using System.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia;
using System.Windows.Input;
using FluentAvalonia.Core;
using System.Collections.Specialized;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using Avalonia.Utilities;

namespace FluentAvalonia.UI.Controls;

// Note a Border (s_tpPaneResizeHandle) and a SplitView are not listed here because
// they're only required in Left/Right display modes. If anyone wrote an analyzer 
// that generates errors for missing templat parts, I don't want to cause them
// issues. However, if you retemplate anything, left/right require these controls

// Also note the custom ScrollViewer has required template parts, see below

[PseudoClasses(FASharedPseudoclasses.s_pcNoBorder, FASharedPseudoclasses.s_pcBorderLeft, FASharedPseudoclasses.s_pcBorderRight, s_pcSingleBorder)]
[PseudoClasses(s_pcTop, s_pcLeft, s_pcBottom, s_pcRight)]
[TemplatePart(s_tpTabContentPresenter, typeof(ContentPresenter))]
[TemplatePart(s_tpRightContentPresenter, typeof(ContentPresenter))]
[TemplatePart(s_tpTabContainerGrid, typeof(Grid))]
[TemplatePart(s_tpTabListView, typeof(FATabViewListView))]
[TemplatePart(s_tpAddButton, typeof(Button))]
public partial class FATabView
{
    /// <summary>
    /// Defines the <see cref="TabWidthMode"/> property
    /// </summary>
    public static readonly StyledProperty<FATabViewWidthMode> TabWidthModeProperty =
        AvaloniaProperty.Register<FATabView, FATabViewWidthMode>(nameof(TabWidthMode),
            defaultValue: FATabViewWidthMode.Equal);

    /// <summary>
    /// Defines the <see cref="CloseButtonOverlayMode"/> property
    /// </summary>
    public static readonly StyledProperty<FATabViewCloseButtonOverlayMode> CloseButtonOverlayModeProperty =
        AvaloniaProperty.Register<FATabView, FATabViewCloseButtonOverlayMode>(nameof(CloseButtonOverlayMode),
            defaultValue: FATabViewCloseButtonOverlayMode.Auto);

    /// <summary>
    /// Definse the <see cref="TabStripHeader"/> property
    /// </summary>
    public static readonly StyledProperty<object> TabStripHeaderProperty =
        AvaloniaProperty.Register<FATabView, object>(nameof(TabStripHeader));

    /// <summary>
    /// Define the <see cref="TabStripHeaderTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabStripHeaderTemplateProperty =
        AvaloniaProperty.Register<FATabView, IDataTemplate>(nameof(TabStripHeaderTemplate));

    /// <summary>
    /// Defines the <see cref="TabStripFooter"/> property
    /// </summary>
    public static readonly StyledProperty<object> TabStripFooterProperty =
        AvaloniaProperty.Register<FATabView, object>(nameof(TabStripFooter));

    /// <summary>
    /// Defines the <see cref="TabStripFooterTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabStripFooterTemplateProperty =
        AvaloniaProperty.Register<FATabView, IDataTemplate>(nameof(TabStripFooterTemplate));

    /// <summary>
    /// Defines the <see cref="IsAddTabButtonVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsAddTabButtonVisibleProperty =
        AvaloniaProperty.Register<FATabView, bool>(nameof(IsAddTabButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="AddTabButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> AddTabButtonCommandProperty =
        AvaloniaProperty.Register<FATabView, ICommand>(nameof(AddTabButtonCommand));

    /// <summary>
    /// Defines the <see cref="AddTabButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> AddTabButtonCommandParameterProperty =
        AvaloniaProperty.Register<FATabView, object>(nameof(AddTabButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="TabItems"/> property
    /// </summary>
    public static readonly DirectProperty<FATabView, IList> TabItemsProperty =
        AvaloniaProperty.RegisterDirect<FATabView, IList>(nameof(TabItems),
            x => x.TabItems);

    /// <summary>
    /// Defines the <see cref="TabItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<IEnumerable> TabItemsSourceProperty =
        AvaloniaProperty.Register<FATabView, IEnumerable>(nameof(TabItemsSource));

    /// <summary>
    /// Defines the <see cref="TabItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabItemTemplateProperty =
        AvaloniaProperty.Register<FATabView, IDataTemplate>(nameof(TabItemTemplate));

    /// <summary>
    /// Defines the <see cref="CanDragTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanDragTabsProperty =
        AvaloniaProperty.Register<FATabView, bool>(nameof(CanDragTabs), false);

    /// <summary>
    /// Defines the <see cref="CanReorderTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanReorderTabsProperty =
        AvaloniaProperty.Register<FATabView, bool>(nameof(CanReorderTabs), true);

    /// <summary>
    /// Defines the <see cref="AllowDropTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> AllowDropTabsProperty =
        AvaloniaProperty.Register<FATabView, bool>(nameof(AllowDropTabs), true);

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property
    /// </summary>
    public static readonly DirectProperty<FATabView, int> SelectedIndexProperty =
        SelectingItemsControl.SelectedIndexProperty.AddOwner<FATabView>(x => x.SelectedIndex,
            (x, v) => x.SelectedIndex = v);

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property
    /// </summary>
    public static readonly DirectProperty<FATabView, object> SelectedItemProperty =
        SelectingItemsControl.SelectedItemProperty.AddOwner<FATabView>(x => x.SelectedItem,
            (x, v) => x.SelectedItem = v);

    /// <summary>
    /// Defines the <see cref="TabStripLocation"/> property
    /// </summary>
    public static readonly StyledProperty<FATabViewTabStripLocation> TabStripLocationProperty =
        AvaloniaProperty.Register<FATabView, FATabViewTabStripLocation>(nameof(TabStripLocation));

    /// <summary>
    /// Defines the <see cref="IsVerticalPaneOpen"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsVerticalPaneOpenProperty = 
        AvaloniaProperty.Register<FATabView, bool>(nameof(IsVerticalPaneOpen), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="VerticalOpenPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> VerticalOpenPaneLengthProperty = 
        AvaloniaProperty.Register<FATabView, double>(nameof(VerticalOpenPaneLength), defaultValue: 225d);

    /// <summary>
    /// Defines the <see cref="MinimumVerticalOpenPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinimumVerticalOpenPaneLengthProperty = 
        AvaloniaProperty.Register<FATabView, double>(nameof(MinimumVerticalOpenPaneLength), defaultValue: 40d);

    /// <summary>
    /// Defines the <see cref="MaximumVerticalOpenPaneLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> MaximumVerticalOpenPaneLengthProperty = 
        AvaloniaProperty.Register<FATabView, double>(nameof(MaximumVerticalOpenPaneLength), defaultValue: 700d);

    /// <summary>
    /// Defines the <see cref="VerticalPaneDisplayMode"/> property
    /// </summary>
    public static readonly StyledProperty<SplitViewDisplayMode> VerticalPaneDisplayModeProperty = 
        AvaloniaProperty.Register<FATabView, SplitViewDisplayMode>(nameof(VerticalPaneDisplayMode), defaultValue: SplitViewDisplayMode.Inline);

    /// <summary>
    /// Gets or sets how the tabs should be sized
    /// </summary>
    public FATabViewWidthMode TabWidthMode
    {
        get => GetValue(TabWidthModeProperty);
        set => SetValue(TabWidthModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the behavior of the close button within tabs
    /// </summary>
    public FATabViewCloseButtonOverlayMode CloseButtonOverlayMode
    {
        get => GetValue(CloseButtonOverlayModeProperty);
        set => SetValue(CloseButtonOverlayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the content that is shown to the left of the tab strip
    /// </summary>
    public object TabStripHeader
    {
        get => GetValue(TabStripHeaderProperty);
        set => SetValue(TabStripHeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the IDataTemplate used to dispaly the content of the TabStripHeader
    /// </summary>
    public IDataTemplate TabStripHeaderTemplate
    {
        get => GetValue(TabStripHeaderTemplateProperty);
        set => SetValue(TabStripHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content that is shown to the right of the tab strip
    /// </summary>
    public object TabStripFooter
    {
        get => GetValue(TabStripFooterProperty);
        set => SetValue(TabStripFooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the IDataTemplate used to display the content of the TabStripFooter
    /// </summary>
    public IDataTemplate TabStripFooterTemplate
    {
        get => GetValue(TabStripFooterTemplateProperty);
        set => SetValue(TabStripFooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the add (+) tab button is visible
    /// </summary>
    public bool IsAddTabButtonVisible
    {
        get => GetValue(IsAddTabButtonVisibleProperty);
        set => SetValue(IsAddTabButtonVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to onvoke when the add (+) tab button is tapped
    /// </summary>
    public ICommand AddTabButtonCommand
    {
        get => GetValue(AddTabButtonCommandProperty);
        set => SetValue(AddTabButtonCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="AddTabButtonCommand"/> property
    /// </summary>
    public object AddTabButtonCommandParameter
    {
        get => GetValue(AddTabButtonCommandParameterProperty);
        set => SetValue(AddTabButtonCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the TabItems this TabView displays
    /// </summary>
    [Content]
    public IList TabItems
    {
        get => _tabItems;
        private set => SetAndRaise(TabItemsProperty, ref _tabItems, value);
    }

    /// <summary>
    /// Gets or sets the TabItems source for this TabView
    /// </summary>
    public IEnumerable TabItemsSource
    {
        get => GetValue(TabItemsSourceProperty);
        set => SetValue(TabItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the IDataTemplate used to display each item
    /// </summary>
    public IDataTemplate TabItemTemplate
    {
        get => GetValue(TabItemTemplateProperty);
        set => SetValue(TabItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether tabs can be dragged as a data payload
    /// </summary>
    public bool CanDragTabs
    {
        get => GetValue(CanDragTabsProperty);
        set => SetValue(CanDragTabsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the tabs in the TabStrip can be reordered through
    /// user interaction
    /// </summary>
    public bool CanReorderTabs
    {
        get => GetValue(CanReorderTabsProperty);
        set => SetValue(CanReorderTabsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that determines whether the TabView can be a drop target for the purposes
    /// of drag-and-drop operations
    /// </summary>
    public bool AllowDropTabs
    {
        get => GetValue(AllowDropTabsProperty);
        set => SetValue(AllowDropTabsProperty, value);
    }

    /// <summary>
    /// Gets or sets the index of the selected tab
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
    }

    /// <summary>
    /// Gets or sets the selected tab item
    /// </summary>
    public object SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    /// <summary>
    /// Gets or sets the location of the tab strip for this TabView
    /// </summary>
    public FATabViewTabStripLocation TabStripLocation
    {
        get => GetValue(TabStripLocationProperty);
        set => SetValue(TabStripLocationProperty, value);
    }

    /// <summary>
    /// When the tab strip is on the left or the right, returns whether the pane is open.
    /// If the tab strip is on the top or bottom, this has no effect
    /// </summary>
    public bool IsVerticalPaneOpen
    {
        get => GetValue(IsVerticalPaneOpenProperty);
        set => SetValue(IsVerticalPaneOpenProperty, value);
    }

    /// <summary>
    /// When the tab strip is on the left or the right, returns pane's open length
    /// If the tab strip is on the top or bottom, this has no effect
    /// </summary>
    public double VerticalOpenPaneLength
    {
        get => GetValue(VerticalOpenPaneLengthProperty);
        set => SetValue(VerticalOpenPaneLengthProperty, value);
    }

    /// <summary>
    /// When the tab strip is on the left or the right, returns the minimum width
    /// the pane can be opened. If the tab strip is on the top or bottom, this has no effect
    /// </summary>
    public double MinimumVerticalOpenPaneLength
    {
        get => GetValue(MinimumVerticalOpenPaneLengthProperty);
        set => SetValue(MinimumVerticalOpenPaneLengthProperty, value);
    }

    /// <summary>
    /// When the tab strip is on the left or the right, returns the maximum width
    /// the pane can be opened. If the tab strip is on the top or bottom, this has no effect
    /// </summary>
    public double MaximumVerticalOpenPaneLength
    {
        get => GetValue(MaximumVerticalOpenPaneLengthProperty);
        set => SetValue(MaximumVerticalOpenPaneLengthProperty, value);
    }

    /// <summary>
    /// When the tab strip is on the left or the right, returns the display mode of the pane.
    /// If the tab strip is on the top or bottom, this has no effect.
    /// </summary>
    public SplitViewDisplayMode VerticalPaneDisplayMode
    {
        get => GetValue(VerticalPaneDisplayModeProperty);
        set => SetValue(VerticalPaneDisplayModeProperty, value);
    }

    // Internal for Unit Tests Only
    internal FATabViewListView ListView => _listView;

    internal ContentPresenter TabContentPresenter => _tabContentPresenter;

    /// <summary>
    /// Raised when the user attempts to close a Tab via clicking the x-to-close button
    /// </summary>
    public event TypedEventHandler<FATabView, FATabViewTabCloseRequestedEventArgs> TabCloseRequested;

    /// <summary>
    /// Occurs when the user completes a drag and drop operation by dropping a tab outside 
    /// of the tab strip area
    /// </summary>
    public event TypedEventHandler<FATabView, FATabViewTabDroppedOutsideEventArgs> TabDroppedOutside;

    /// <summary>
    /// Occurs when the add (+) tab button has been clicked
    /// </summary>
    public event TypedEventHandler<FATabView, EventArgs> AddTabButtonClick;

    /// <summary>
    /// Raised when the items collection has changed
    /// </summary>
    public event TypedEventHandler<FATabView, NotifyCollectionChangedEventArgs> TabItemsChanged;

    /// <summary>
    /// Occurs when the currently selected tab changes
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Occurs when a drag operation is initiated
    /// </summary>
    public event TypedEventHandler<FATabView, FATabViewTabDragStartingEventArgs> TabDragStarting;

    /// <summary>
    /// Raised when the user completes the drag action
    /// </summary>
    public event TypedEventHandler<FATabView, FATabViewTabDragCompletedEventArgs> TabDragCompleted;

    /// <summary>
    /// Occurs when the input system reports an underlying drag event with the TabStrip as 
    /// the potential drop target
    /// </summary>
    public event EventHandler<DragEventArgs> TabStripDragOver;

    /// <summary>
    /// Occurs when the input system reports an underlying drop event with the TabStrip as
    /// the drop target
    /// </summary>
    public event EventHandler<DragEventArgs> TabStripDrop;


    private IList _tabItems;
    private int _selectedIndex = 0;
    private object _selectedItem;

    // Internal for unit test access
    internal const string s_tpTabContentPresenter = "TabContentPresenter";
    private const string s_tpRightContentPresenter = "RightContentPresenter";
    private const string s_tpTabContainerGrid = "TabContainerGrid";
    private const string s_tpTabListView = "TabListView";
    internal const string s_tpAddButton = "AddButton";

    // Technically these are template parts on the ScrollViewer, but we ref them here
    private const string s_tpScrollDecreaseButton = "ScrollDecreaseButton";
    private const string s_tpScrollIncreaseButton = "ScrollIncreaseButton";

    private const string s_tpPaneResizeHandle = "BorderResizeHandleHost";

    // These two come from the WinUI port, so they don't follow the normal naming convention for parity upstream
    private static string c_tabViewItemMinWidthName = "TabViewItemMinWidth";
    private static string c_tabViewItemMaxWidthName = "TabViewItemMaxWidth";

    private const string s_pcSingleBorder = ":singleBorder";

    internal const string s_pcTop = ":top";
    internal const string s_pcLeft = ":left";
    internal const string s_pcRight = ":right";
    internal const string s_pcBottom = ":bottom";

    private static readonly string SR_TabViewCloseButtonTooltipWithKA = "TabViewCloseButtonTooltipWithKA";
    private static readonly string SR_TabViewAddButtonTooltip = "TabViewAddButtonTooltip";
    private static readonly string SR_TabViewScrollDecreaseButtonTooltip = "TabViewScrollDecreaseButtonTooltip";
    private static readonly string SR_TabViewScrollIncreaseButtonTooltip = "TabViewScrollIncreaseButtonTooltip";
    private static readonly string SR_TabViewAddButtonName = "TabViewAddButtonName";

    // TabViewItem subs to these in OnApplyTemplate, but we need to make sure the strong ref to TabView isn't
    // held if the TabViewItem is removed
    internal static readonly WeakEvent<FATabView, FATabViewTabDragStartingEventArgs> TabDragStartingWeakEvent = 
        WeakEvent.Register<FATabView, FATabViewTabDragStartingEventArgs>(
                (c, s) =>
                {
                    TypedEventHandler<FATabView, FATabViewTabDragStartingEventArgs> handler = (_, e) => s(c, e);
                    c.TabDragStarting += handler;
                    return () => c.TabDragStarting -= handler;
                });

    internal static readonly WeakEvent<FATabView, FATabViewTabDragCompletedEventArgs> TabDragCompletedWeakEvent =
        WeakEvent.Register<FATabView, FATabViewTabDragCompletedEventArgs>(
        (c, s) =>
        {
            TypedEventHandler<FATabView, FATabViewTabDragCompletedEventArgs> handler = (_, e) => s(c, e);
            c.TabDragCompleted += handler;
            return () => c.TabDragCompleted -= handler;
        });
}
