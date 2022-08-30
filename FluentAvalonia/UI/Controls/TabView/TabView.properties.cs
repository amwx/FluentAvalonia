using System;
using System.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia;
using System.Windows.Input;
using FluentAvalonia.Core;
using System.Collections.Specialized;
using Avalonia.Input;
using Avalonia.Metadata;

namespace FluentAvalonia.UI.Controls;

public partial class TabView
{
    /// <summary>
    /// Defines the <see cref="TabWidthMode"/> property
    /// </summary>
    public static readonly StyledProperty<TabViewWidthMode> TabWidthModeProperty =
        AvaloniaProperty.Register<TabView, TabViewWidthMode>(nameof(TabWidthMode),
            defaultValue: TabViewWidthMode.Equal);

    /// <summary>
    /// Defines the <see cref="CloseButtonOverlayMode"/> property
    /// </summary>
    public static readonly StyledProperty<TabViewCloseButtonOverlayMode> CloseButtonOverlayModeProperty =
        AvaloniaProperty.Register<TabView, TabViewCloseButtonOverlayMode>(nameof(CloseButtonOverlayMode),
            defaultValue: TabViewCloseButtonOverlayMode.Auto);

    /// <summary>
    /// Definse the <see cref="TabStripHeader"/> property
    /// </summary>
    public static readonly StyledProperty<object> TabStripHeaderProperty =
        AvaloniaProperty.Register<TabView, object>(nameof(TabStripHeader));

    /// <summary>
    /// Define the <see cref="TabStripHeaderTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabStripHeaderTemplateProperty =
        AvaloniaProperty.Register<TabView, IDataTemplate>(nameof(TabStripHeaderTemplate));

    /// <summary>
    /// Defines the <see cref="TabStripFooter"/> property
    /// </summary>
    public static readonly StyledProperty<object> TabStripFooterProperty =
        AvaloniaProperty.Register<TabView, object>(nameof(TabStripFooter));

    /// <summary>
    /// Defines the <see cref="TabStripFooterTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabStripFooterTemplateProperty =
        AvaloniaProperty.Register<TabView, IDataTemplate>(nameof(TabStripFooterTemplate));

    /// <summary>
    /// Defines the <see cref="IsAddTabButtonVisible"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsAddTabButtonVisibleProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(IsAddTabButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="AddTabButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> AddTabButtonCommandProperty =
        AvaloniaProperty.Register<TabView, ICommand>(nameof(AddTabButtonCommand));

    /// <summary>
    /// Defines the <see cref="AddTabButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> AddTabButtonCommandParameterProperty =
        AvaloniaProperty.Register<TabView, object>(nameof(AddTabButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="TabItems"/> property
    /// </summary>
    public static readonly DirectProperty<TabView, IEnumerable> TabItemsProperty =
        AvaloniaProperty.RegisterDirect<TabView, IEnumerable>(nameof(TabItems),
            x => x.TabItems, (x, v) => x.TabItems = v);

    /// <summary>
    /// Defines the <see cref="TabItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TabItemTemplateProperty =
        AvaloniaProperty.Register<TabView, IDataTemplate>(nameof(TabItemTemplate));

    /// <summary>
    /// Defines the <see cref="CanDragTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanDragTabsProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(CanDragTabs), false);

    /// <summary>
    /// Defines the <see cref="CanReorderTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanReorderTabsProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(CanReorderTabs), true);

    /// <summary>
    /// Defines the <see cref="AllowDropTabs"/> property
    /// </summary>
    public static readonly StyledProperty<bool> AllowDropTabsProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(AllowDropTabs), true);

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property
    /// </summary>
    public static readonly DirectProperty<TabView, int> SelectedIndexProperty =
        SelectingItemsControl.SelectedIndexProperty.AddOwner<TabView>(x => x.SelectedIndex,
            (x, v) => x.SelectedIndex = v);

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property
    /// </summary>
    public static readonly DirectProperty<TabView, object> SelectedItemProperty =
        SelectingItemsControl.SelectedItemProperty.AddOwner<TabView>(x => x.SelectedItem,
            (x, v) => x.SelectedItem = v);

    /// <summary>
    /// Gets or sets how the tabs should be sized
    /// </summary>
    public TabViewWidthMode TabWidthMode
    {
        get => GetValue(TabWidthModeProperty);
        set => SetValue(TabWidthModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the behavior of the close button within tabs
    /// </summary>
    public TabViewCloseButtonOverlayMode CloseButtonOverlayMode
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
    public IEnumerable TabItems
    {
        get => _tabItems;
        set => SetAndRaise(TabItemsProperty, ref _tabItems, value);
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
    /// Raised when the user attempts to close a Tab via clicking the x-to-close button
    /// </summary>
    public event TypedEventHandler<TabView, TabViewTabCloseRequestedEventArgs> TabCloseRequested;

    /// <summary>
    /// Occurs when the user completes a drag and drop operation by dropping a tab outside 
    /// of the tab strip area
    /// </summary>
    public event TypedEventHandler<TabView, TabViewTabDroppedOutsideEventArgs> TabDroppedOutside;

    /// <summary>
    /// Occurs when the add (+) tab button has been clicked
    /// </summary>
    public event TypedEventHandler<TabView, EventArgs> AddTabButtonClick;

    /// <summary>
    /// Raised when the items collection has changed
    /// </summary>
    public event TypedEventHandler<TabView, NotifyCollectionChangedEventArgs> TabItemsChanged;

    /// <summary>
    /// Occurs when the currently selected tab changes
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Occurs when a drag operation is initiated
    /// </summary>
    public event TypedEventHandler<TabView, TabViewTabDragStartingEventArgs> TabDragStarting;

    /// <summary>
    /// Raised when the user completes the drag action
    /// </summary>
    public event TypedEventHandler<TabView, TabViewTabDragCompletedEventArgs> TabDragCompleted;

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


    private IEnumerable _tabItems;
    private int _selectedIndex = 0;
    private object _selectedItem;
}
