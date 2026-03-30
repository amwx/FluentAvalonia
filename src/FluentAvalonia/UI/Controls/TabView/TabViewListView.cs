using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Data;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents the ListView used in the TabStrip of a <see cref="TabView"/>
/// </summary>
/// <remarks>
/// This control should not be used outside of a TabView
/// </remarks>
[PseudoClasses(s_pcReorder)]
[TemplatePart(s_tpScrollViewer, typeof(ScrollViewer))]
public sealed class TabViewListView : ListBox
{
    public TabViewListView()
    {
        ItemsView.CollectionChanged += OnItemsChanged;


        Tapped += (s, e) =>
        {
            if (e.Source is Visual v && v.FindAncestorOfType<TabViewItem>(true) is TabViewItem tvi)
            {
                var index = IndexFromContainer(tvi);
                UpdateSelection(index, true);
                e.Handled = true;
            }
        };
    }


    /// <summary>
    /// Defines the <see cref="CanReorderItems"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanReorderItemsProperty =
        AvaloniaProperty.Register<TabViewListView, bool>(nameof(CanReorderItems));

    /// <summary>
    /// Defines the <see cref="CanDragItems"/> property
    /// </summary>
    public static readonly StyledProperty<bool> CanDragItemsProperty =
        AvaloniaProperty.Register<TabViewListView, bool>(nameof(CanDragItems));

    /// <summary>
    /// Gets or sets whether this ListView can reorder items
    /// </summary>
    public bool CanReorderItems
    {
        get => GetValue(CanReorderItemsProperty);
        set => SetValue(CanReorderItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether dragging items is supported on this ListView
    /// </summary>
    public bool CanDragItems
    {
        get => GetValue(CanDragItemsProperty);
        set => SetValue(CanDragItemsProperty, value);
    }

    internal ScrollViewer Scroller { get; private set; }

    /// <summary>
    /// Occurs when a drag operation that involves one of the items in the view is initiated.
    /// </summary>
    public event DragItemsStartingEventHandler DragItemsStarting;

    /// <summary>
    /// Occurs when a drag operation that involves one of the items in the view is ended.
    /// </summary>
    public event TypedEventHandler<TabViewListView, DragItemsCompletedEventArgs> DragItemsCompleted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Scroller = e.NameScope.Find<ScrollViewer>(s_tpScrollViewer);

        // HACK: DragDrop events work differently in Avalonia than WinUI. In WinUI, they aren't true
        // routed events, so the OriginalSource parameter returns TabView or TabViewItem, etc., not
        // template items. In Avalonia, we get template items, but only like 1 or 2. This means that
        // we can't actually detect if we've left the TabStrip during a drag operation. As of me writing
        // this, the only things we detect from the local Drag handlers is some template items in the
        // TabViewItems - ABSOLUTELY NOTHING FROM TABVIEWLISTVIEW, arghhhhh...That's annoying
        // So this seems to work - if we grab a drag enter handler on the TabView itself and do a bounds
        // check on this, we know if the pointer left the tab strip or not. 
        _parent = this.FindAncestorOfType<TabView>();
        _parent.AddHandler(DragDrop.DragLeaveEvent, OnParentDragEnter);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedIndexProperty)
        {
            UpdateBottomBorderVisualState();
        }
    }

    private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var tv = this.FindAncestorOfType<TabView>();
        tv?.OnItemsChanged(e);
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        bool isItem = item is TabViewItem;
        recycleKey = isItem ? null : nameof(TabViewItem);
        return !isItem;
    }

    protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
    {
        var cont = this.FindDataTemplate(item, ItemTemplate)?.Build(item);
        
        if (cont is TabViewItem tvi)
        {
            tvi.IsContainerFromTemplate = true;
            return tvi;
        }

        return new TabViewItem();
    }

    protected override void ContainerForItemPreparedOverride(Control container, object item, int index)
    {
        // NOTE: BE CAREFUL HERE! Avalonia has two separate preparation events
        // PrepareContainerForItemOverride - does not have the container connected yet
        // This one does - I've raised an issue b/c this is dumb
        var tvi = container as TabViewItem;

        // WinUI: Due to virtualization, a TabViewItem might be recycled to display a different tab data item.
        //        In that case, there is no need to set the TabWidthMode of the TabViewItem or its parent TabView
        //        as they are already set correctly here.
        //
        //        We know we are currently looking at a TabViewItem being recycled if its parent TabView has
        //        already been set.
        var tabLocation = TabViewTabStripLocation.Top; // Default to top
        if (tvi.ParentTabView == null)
        {
            var parentTV = container.FindAncestorOfType<TabView>();
            if (parentTV != null)
            {
                tvi.OnTabViewWidthModeChanged(parentTV.TabWidthMode);
                tvi.ParentTabView = parentTV;
                tabLocation = parentTV.TabStripLocation;
            }
        }
        else
        {
            tabLocation = tvi.ParentTabView.TabStripLocation;
        }

        tvi.HandleTabStripLocationChanged(tabLocation);

        // Special b/c we use the header and not Content. Somehow this *just works* in
        // WinUI b/c they don't have to do this.
        if (container == item || tvi.IsContainerFromTemplate)
        {
            base.ContainerForItemPreparedOverride(container, item, index);
            return;
        }

        tvi.Header = item;

        var itemTemplate = this.FindDataTemplate(item, ItemTemplate);

        if (itemTemplate != null)
            tvi.HeaderTemplate = itemTemplate;

        base.ContainerForItemPreparedOverride(container, item, index);
    }


    private void UpdateBottomBorderVisualState()
    {
        PseudoClasses.Set(s_pcLeftShort, SelectedIndex == 0);
        PseudoClasses.Set(s_pcRightShort, SelectedIndex == ItemsView.Count - 1);
    }

    private void UpdateBottomBorderVisualState()
    {
        PseudoClasses.Set(s_pcLeftShort, SelectedIndex == 0);
        PseudoClasses.Set(s_pcRightShort, SelectedIndex == ItemsView.Count - 1);
    }

    private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var tv = this.FindAncestorOfType<TabView>();
        tv?.OnItemsChanged(e);
    }

    private void UpdateDragInfo()
    {
        FAUISettings.GetSystemDragSize(VisualRoot.RenderScaling, out _cxDrag, out _cyDrag);
    }

    private TabViewItem _dragItem;
    private int _dragIndex = -1;
    private bool _isDragItemFocused;
    private bool _isDragItemSelected;
    private bool _isInDrag = false;
    private bool _isInReorder = false;
    private IDisposable _dragItemOpacitySub;
    private bool _processReorder;
    private Point? _initialPoint;
    private double _cxDrag = double.NaN;
    private double _cyDrag = double.NaN;
    private Control _parent;
    private bool _isDragWithinTabStrip;
    // True if there is a drag drop operation started by this listview
    private bool _isDraggingOverSelf;

    private LiveReorderHelper _liveReorderHelper;
    //private LiveReorderIndices _liveReorderIndices = new LiveReorderIndices(-1,-1,-1);
    private DispatcherTimer _liveReorderTimer;
    //private readonly MovedItems _movedItems = new MovedItems();
    //private List<Rect> _cachedContainerBounds;
    private Point? _lastDragOverPoint;

    // For 12.0/v3 - Avalonia has decided to make the decision that the lowest common denominator
    // in the platform backends decides the entire public API. As part of this, DoDragDrop now
    // requires the initial pressed args, so we have to store them away so we can start DragDrop.
    // I tried to object, and failed (https://github.com/AvaloniaUI/Avalonia/pull/20988)
    // And you guessed it, freakin' Wayland
    private PointerPressedEventArgs _initArgs;
    
    private DispatcherTimer _scrollTimer;
    private Vector _currentAutoPanVelocity;

    private const string s_tpScrollViewer = "ScrollViewer";

    private const string s_pcReorder = ":reorder";
    private const string s_pcLeftShort = ":leftShort";
    private const string s_pcRightShort = ":rightShort";

    private enum AutoScrollAction
    {
        NoAction,
        ScrollStarted,
        ScrollEnded,
    }
}
