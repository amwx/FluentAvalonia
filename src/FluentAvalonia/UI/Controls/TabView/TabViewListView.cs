using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;

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
        if (!tvi.IsSelected)
        {
            // Bug Fix: When containers are being virtualized, they may "come back online" with
            // old state left over.
            // This is also a bug in WinUI, but it materializes differently. In Avalonia, we can see
            // this very clearly because WinUI pins and does not recycle the SelectedItem container,
            // but Avalonia does. Thus, when the previously selected container is reused, it still
            // has the visual state we apply to selected items, specifically the :noborder pseudoclass
            // Because WinUI pins the container, we never see this issue
            // HOWEVER, we can see it in in WinUI with the LeftOfSelectedTab/RightOfSelectedTab states
            // If you select an item and then scroll away such that SelIndex-1 and Selindex+1 container
            // are recycled, inspect them, you'll see the margin still applied to the Border line indicating
            // the state was never cleared. If you scroll to those new containers you'll see a tiny little
            // gap in the bottom border because of this. 
            // Fix here by just ensuring this state get's updated. I don't want to call TabView.UpdateBottom...
            // because that iterates over containers and that isn't great in this path. 
            // Also added unit test to ensure this is fixed.

            var selIndex = SelectedIndex;
            int state = -1;
            if (selIndex != -1)
            {
                if (index == selIndex)
                {
                    state = 0;
                }
                else if (index == selIndex - 1)
                {
                    state = 1;
                }
                else if (index == selIndex + 1)
                {
                    state = 2;
                }
            }

            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcNoBorder, state == 0);
            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderLeft, state == 1);
            ((IPseudoClasses)tvi.Classes).Set(SharedPseudoclasses.s_pcBorderRight, state == 2);
        }
    }


    private void UpdateBottomBorderVisualState()
    {
        PseudoClasses.Set(s_pcLeftShort, SelectedIndex == 0);
        PseudoClasses.Set(s_pcRightShort, SelectedIndex == ItemsView.Count - 1);
    }

    private TabViewItem _dragItem;
    private int _dragIndex = -1;
    private bool _isInDrag = false;
    private bool _isInReorder = false;
    private bool _processReorder;
    private Point? _initialPoint;
    private double _cxDrag = double.NaN;
    private double _cyDrag = double.NaN;

    //private object _currentItem;

    private static Popup _dragReorderPopup;
    private Point _popupOffset;
    private object _dragItemToolTip;

    private DispatcherTimer _scrollTimer;
    private int _scrollDirection;
    private Rect _noAutoScrollRect;

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
