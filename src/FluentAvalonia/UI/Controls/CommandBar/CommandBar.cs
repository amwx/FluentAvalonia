using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a specialized command bar that provides layout for CommandBarButton and related command elements.
/// </summary>
public partial class CommandBar : ContentControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandBar"/> class.
    /// </summary>
    public CommandBar()
    {
        PrimaryCommands = new AvaloniaList<ICommandBarElement>();
        SecondaryCommands = new AvaloniaList<ICommandBarElement>();

        _primaryCommands.CollectionChanged += OnPrimaryCommandsChanged;
        _secondaryCommands.CollectionChanged += OnSecondaryCommandsChanged;

        // Don't initialize the actual item lists here, we'll do that as needed

        PseudoClasses.Add(s_pcDynamicOverflow);
        PseudoClasses.Add(SharedPseudoclasses.s_pcCompact);
        PseudoClasses.Add(s_pcLabelBottom);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;

        if (_moreButton != null)
        {
            _moreButton.Click -= OnMoreButtonClick;
        }

        base.OnApplyTemplate(e);

        _primaryItemsHost = e.NameScope.Find<ItemsControl>(s_tpPrimaryItemsControl);
        _contentHost = e.NameScope.Find<ContentControl>(s_tpContentControl);

        _overflowItemsHost = e.NameScope.Find<CommandBarOverflowPresenter>(s_tpSecondaryItemsControl);

        _moreButton = e.NameScope.Find<Button>(s_tpMoreButton);
        if (_moreButton != null)
        {
            _moreButton.Click += OnMoreButtonClick;
        }
        
        _appliedTemplate = true;

        AttachItems();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
        {
            OnIsOpenedChanged(change.GetNewValue<bool>());
        }
        else if (change.Property == DefaultLabelPositionProperty)
        {
            var newVal = change.GetNewValue<CommandBarDefaultLabelPosition>();
            PseudoClasses.Set(s_pcLabelRight, newVal == CommandBarDefaultLabelPosition.Right);
            PseudoClasses.Set(s_pcLabelBottom, newVal == CommandBarDefaultLabelPosition.Bottom);
            PseudoClasses.Set(s_pcLabelCollapsed, newVal == CommandBarDefaultLabelPosition.Collapsed);
        }
        else if (change.Property == ClosedDisplayModeProperty)
        {
            var newVal = change.GetNewValue<CommandBarClosedDisplayMode>();
            PseudoClasses.Set(SharedPseudoclasses.s_pcCompact, newVal == CommandBarClosedDisplayMode.Compact);
            PseudoClasses.Set(s_pcMinimal, newVal == CommandBarClosedDisplayMode.Minimal);
            PseudoClasses.Set(s_pcHidden, newVal == CommandBarClosedDisplayMode.Hidden);
        }
        else if (change.Property == ItemsAlignmentProperty)
        {
            var val = change.GetNewValue<CommandBarItemsAlignment>();
            PseudoClasses.Set(":itemsRight", val == CommandBarItemsAlignment.Right);
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        bool isDynamic = IsDynamicOverflowEnabled;
        if (isDynamic)
        {
            if (!_moreButton.IsVisible)
                _moreButton.IsVisible = true;

            var sz = base.MeasureOverride(Size.Infinity);

            if (_primaryCommands.Count == 0)
            {
                _moreButton.IsVisible = true;
                _overflowSeparator.IsVisible = false;
                return sz;
            }

            double availWid = availableSize.Width;
            // TODO: May need special case if CommandBar is returned to a container
            // with infinite width and items are in overflow, but this seems like 
            // a very rare, specific thing, so I'm not gonna worry about that here.

            // 5px is to give us just a little more space
            var availWidForItems = availableSize.Width -
                (_contentHost != null ? _contentHost.DesiredSize.Width : 0) -
                _moreButton.DesiredSize.Width - 5;

            if (_minRecoverWidth < availWidForItems && _numInOverflow > 0)
            {
                double trackWid = _primaryItemsHost.DesiredSize.Width;
                while (_numInOverflow > 0)
                {
                    var items = GetReturnToPrimaryItems();
                    double groupWid = 0;

                    for (int i = 0; i < items.Count; i++)
                    {
                        groupWid += _widthCache[items[i]];

                        _overflowItems.Remove(items[i]);
                        var originalIndex = Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(items[i]));
                        _primaryItems.Insert(originalIndex, items[i]);
                        _numInOverflow--;

                        // Unhide
                        if (items[i] is CommandBarSeparator sep)
                            sep.IsVisible = true;
                    }

                    trackWid += groupWid;
                    _minRecoverWidth += groupWid;

                    if (trackWid >= availWidForItems)
                        break;
                }
            }
            else if (_primaryItemsHost.DesiredSize.Width > availWidForItems)
            {
                // Move to Overflow
                double trackWid = 0;

                while (_primaryItemsHost.DesiredSize.Width - trackWid > availWidForItems)
                {
                    var items = GetNextItemsToOverflow();
                    if (items != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            var itemAsIControl = items[i] as Control;
                            UpdateWidthCacheForItem(items[i], itemAsIControl.DesiredSize.Width);

                            trackWid += itemAsIControl.DesiredSize.Width;

                            _primaryItems.Remove(items[i]);
                            _overflowItems.Insert(_numInOverflow, items[i]);
                            _numInOverflow++;

                            // WinUI hides toplevel separartors when the go into overflow
                            if (items[i] is CommandBarSeparator sep)
                                sep.IsVisible = false;
                        }
                    }
                    else
                        break;
                }
                _minRecoverWidth = _primaryItemsHost.DesiredSize.Width;// + trackWid;
            }

            if (_overflowSeparator != null)
            { 
                _overflowSeparator.IsVisible = _numInOverflow > 0 && SecondaryCommands.Count > 0;

                var idx = _numInOverflow;
                var curIdx = _overflowItems.IndexOf(_overflowSeparator);
                _overflowItems.Move(curIdx, idx);
            }
        }

        var overflowVis = OverflowButtonVisibility;
        if (overflowVis == CommandBarOverflowButtonVisibility.Auto)
        {
            _moreButton.IsVisible = _overflowItems != null && (isDynamic ? _overflowItems.Count > 1 : _overflowItems.Count > 0);
        }
        else
        {
            _moreButton.IsVisible = overflowVis == CommandBarOverflowButtonVisibility.Visible;
        }

        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Invoked when the <see cref="CommandBar"/> starts to change from hidden to visible, or starts to be first displayed.
    /// </summary>
    protected virtual void OnOpening()
    {
        Opening?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked when the <see cref="CommandBar"/> starts to change from visible to hidden.
    /// </summary>
    protected virtual void OnClosing()
    {
        Closing?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked when the <see cref="CommandBar"/> changes from hidden to visible, or is first displayed.
    /// </summary>
    protected virtual void OnOpened()
    {
        if (_overflowItems != null)
        {
            // TODO: Focus via keyboard
            if (_overflowItems.Count > 0)
            {
                (_overflowItems[0] as Control).Focus();
            }
        }

        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked when the <see cref="CommandBar"/> changes from visible to hidden.
    /// </summary>
    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
        _moreButton?.Focus();
    }

    private void OnIsOpenedChanged(bool newValue)
    {
        if (newValue)
        {
            OnOpening();

            PseudoClasses.Set(":open", true);
            SetElementVisualStateForOpen(true);

            OnOpened();
        }
        else
        {
            OnClosing();

            PseudoClasses.Set(":open", false);
            SetElementVisualStateForOpen(false);

            OnClosed();
        }
    }

    private void OnPrimaryCommandsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_appliedTemplate)
            return;

        if (_primaryItems == null)
        { 
            AttachItems();
            goto SetState;
        }

        if (IsDynamicOverflowEnabled)
        {
            // While not the most performant or best solution, we return all Overflowed items back
            // to the primary list. This probably isn't a huge deal since you probably aren't 
            // modifying these items too often, but it avoid a lot of complexities with items in
            // both lists b/c of DynamicOverflow. The number of items should be fairly small too
            // so it really shouldn't matter.
            ReturnOverflowToPrimary();
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    if (e.NewItems[i] is ICommandBarElement ele)
                    {
                        if (ele.DynamicOverflowOrder != 0)
                            _hasOrderedOverflow++;
                    }
                }
                _primaryItems.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
                break;

            case NotifyCollectionChangedAction.Remove:
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    if (e.OldItems[i] is ICommandBarElement ele)
                    {
                        if (ele.DynamicOverflowOrder != 0)
                            _hasOrderedOverflow--;
                    }
                }
                _primaryItems.RemoveAll(e.OldItems.Cast<ICommandBarElement>());
                break;

            case NotifyCollectionChangedAction.Reset:
                _hasOrderedOverflow = 0;
                _primaryItems.Clear();
                break;

            case NotifyCollectionChangedAction.Replace:
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    if (e.OldItems[i] is ICommandBarElement ele)
                    {
                        if (ele.DynamicOverflowOrder != 0)
                            _hasOrderedOverflow--;
                    }
                }
                _primaryItems.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                _primaryItems.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    if (e.NewItems[i] is ICommandBarElement ele)
                    {
                        if (ele.DynamicOverflowOrder != 0)
                            _hasOrderedOverflow++;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Move:
                _primaryItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;
        }

SetState:
        PseudoClasses.Set(s_pcPrimaryOnly, _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
        PseudoClasses.Set(s_pcSecondaryOnly, _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
        InvalidateMeasure();
    }

    private void OnSecondaryCommandsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_appliedTemplate)
            return;

        if (_overflowItems == null)
        {
            AttachItems();
            goto SetState;
        }

        // TODO: Test that this works...
        int startIndex = _numInOverflow == 0 ? 0 : _numInOverflow + 1;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    _overflowItems.Insert(e.NewStartingIndex + i + startIndex, e.NewItems[i] as ICommandBarElement);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    _overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                _overflowItems.RemoveRange(startIndex, _overflowItems.Count - startIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    _overflowItems.RemoveAt(e.OldStartingIndex + i + startIndex);
                }
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    _overflowItems.Insert(e.NewStartingIndex + i + startIndex, e.NewItems[i] as ICommandBarElement);
                }
                break;
        }

SetState:
        PseudoClasses.Set(s_pcPrimaryOnly, _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
        PseudoClasses.Set(s_pcSecondaryOnly, _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);

        // Rerun measure to ensure the MoreButton has the correct visibility
        InvalidateMeasure();
    }

    private void AttachItems()
    {
        if (_primaryCommands.Count > 0)
        {
            _primaryItems = new AvaloniaList<ICommandBarElement>();
            // v2: We take this sub here to add/remove psuedoclasses as necessary based on the 
            // different states. In v1, we let the Styling system do this for us but with 
            // ControlThemes this isn't possible anymore so we have to do it ourselves
            _primaryItems.CollectionChanged += PrimaryItemsCollectionChanged;
            _primaryItems.AddRange(_primaryCommands);

            if (IsDynamicOverflowEnabled)
            {
                for (int i = 0; i < _primaryItems.Count; i++)
                {
                    if (_primaryItems[i].DynamicOverflowOrder != 0)
                        _hasOrderedOverflow++;
                }
            }

            _primaryItemsHost.ItemsSource = _primaryItems;
        }

        if (_secondaryCommands.Count > 0 || IsDynamicOverflowEnabled)
        {
            _overflowSeparator = new CommandBarSeparator
            {
                IsVisible = false
            };

            _overflowItems = new AvaloniaList<ICommandBarElement>();
            _overflowItems.Add(_overflowSeparator);
            _overflowItems.AddRange(_secondaryCommands);

            _overflowItemsHost.ItemsSource = _overflowItems;
        }

        PseudoClasses.Set(s_pcPrimaryOnly, _primaryCommands.Count > 0 && _secondaryCommands.Count == 0);
        PseudoClasses.Set(s_pcSecondaryOnly, _primaryCommands.Count == 0 && _secondaryCommands.Count > 0);
    }

    private void PrimaryItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var pos = DefaultLabelPosition;

        // Add & remove are the only options for the _primaryItems list (we control it)
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    var items = e.NewItems;
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] is Control c && c.Classes is IPseudoClasses pc)
                        {
                            pc.Set(s_pcLabelCollapsed, pos == CommandBarDefaultLabelPosition.Collapsed);
                            pc.Set(s_pcLabelRight, pos == CommandBarDefaultLabelPosition.Right);
                            pc.Set(s_pcLabelBottom, pos == CommandBarDefaultLabelPosition.Bottom);
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                {
                    var items = e.OldItems;
                    if (items != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i] is Control c && c.Classes is IPseudoClasses pc)
                            {
                                pc.Set(s_pcLabelCollapsed, false);
                                pc.Set(s_pcLabelRight, false);
                                pc.Set(s_pcLabelBottom, false);
                            }
                        }
                    }
                }
                break;
        }
    }

    private void ReturnOverflowToPrimary()
    {
        for (int i = _numInOverflow - 1; i >= 0; i--)
        {
            var item = _overflowItems[i];
            _overflowItems.RemoveAt(i);
            _primaryItems.Insert(Math.Min(_primaryItems.Count, _primaryCommands.IndexOf(item)), item);
        }
        _numInOverflow = 0;
    }

    private void OnMoreButtonClick(object sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
    }

    private IList<ICommandBarElement> GetNextItemsToOverflow()
    {
        if (_hasOrderedOverflow > 0)
        {
            if (_primaryItems.Count == 0)
                return null;

            // TODO: Don't loop over this multiple times...
            List<ICommandBarElement> l = new List<ICommandBarElement>(2);
            int nextOverflowOrder = int.MaxValue;
            bool hasAnotherOrder = false;
            for (int i = _primaryItems.Count - 1; i >= 0; i--)
            {
                if (_primaryItems[i].DynamicOverflowOrder == 0)
                    continue;

                hasAnotherOrder = true;
                nextOverflowOrder = Math.Min(nextOverflowOrder, _primaryItems[i].DynamicOverflowOrder);
            }

            if (hasAnotherOrder)
            {
                for (int i = 0; i < _primaryItems.Count; i++)
                {
                    if (_primaryItems[i].DynamicOverflowOrder == nextOverflowOrder)
                    {
                        l.Add(_primaryItems[i]);
                    }
                }

                return l;
            }
            else
            {
                return new[] { _primaryItems[_primaryItems.Count - 1] };
            }
        }
        else
        {
            if (_primaryItems.Count == 0)
                return null;

            return new[] { _primaryItems[_primaryItems.Count - 1] };
        }
    }

    private IList<ICommandBarElement> GetReturnToPrimaryItems()
    {
        if (_overflowItems[_numInOverflow - 1].DynamicOverflowOrder == 0)
            return new[] { _overflowItems[_numInOverflow - 1] };

        int currentGroup = _overflowItems[_numInOverflow - 1].DynamicOverflowOrder;

        int count = 1;
        for (int i = _numInOverflow - 2; i >= 0; i--)
        {
            if (_overflowItems[i].DynamicOverflowOrder == currentGroup)
            {
                count++;
                continue;
            }
            break;
        }

        return _overflowItems.GetRange(_numInOverflow - count, count).ToList();
    }

    private void UpdateWidthCacheForItem(ICommandBarElement item, double wid)
    {
        if (_widthCache == null)
            _widthCache = new Dictionary<ICommandBarElement, double>();

        if (_widthCache.ContainsKey(item))
            _widthCache[item] = wid;
        else
            _widthCache.Add(item, wid);
    }

    private void SetElementVisualStateForOpen(bool open)
    {
        if (_primaryItems == null)
            return;

        for (int i = 0, ct = _primaryItems.Count; i < ct; i++)
        {
            if (_primaryItems[i] is Control c && c.Classes is IPseudoClasses pc)
            {
                pc.Set(SharedPseudoclasses.s_pcOpen, open);
            }
        }
    }

    private bool _appliedTemplate = false;

    // These are the actual lists sent to the Items Controls
    // We don't want to move items in the actual lists to not
    // interfere with what user specified
    private AvaloniaList<ICommandBarElement> _primaryItems;
    private AvaloniaList<ICommandBarElement> _overflowItems;

    private ItemsControl _primaryItemsHost;
    private CommandBarOverflowPresenter _overflowItemsHost;
    private ContentControl _contentHost;
    private Button _moreButton;

    private CommandBarSeparator _overflowSeparator;

    private int _hasOrderedOverflow = 0;
    private Dictionary<ICommandBarElement, double> _widthCache;
    private int _numInOverflow = 0;
    private double _minRecoverWidth;
}
