using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls;

internal class ViewManager
{
    public ViewManager(ItemsRepeater ir)
    {
        _owner = ir;
        _resetPool = new UniqueIdElementPool(ir);
        _phaser = new Phaser(ir);
        _pinnedPool = new List<PinnedElementInfo>();
        // ItemsRepeater is not fully constructed yet. Don't interact with it.
    }

    internal int FirstRealizedIndex => _firstRealizedElementIndexHeldByLayout;

    internal int LastRealizedIndex => _lastRealizedElementIndexHeldByLayout;

    public Control GetElement(int index, bool forceCreate, bool suppressAutoRecycle)
    {
        bool elementIsAnchor = false;
        var element = forceCreate ? null : GetElementIfAlreadyHeldByLayout(index);
        if (element == null)
        {
            // check if this is the anchor made through repeater in preparation 
            // for a bring into view.
            if (_owner.MadeAnchor is Control c)
            {
                var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(c);
                if (virtInfo.Index == index)
                {
                    element = c;
                    elementIsAnchor = true;
                }
            }
        }

        if (element == null)
            element = GetElementFromUniqueIdResetPool(index);

        if (element == null || elementIsAnchor)
        { 
            var elementFromPool = GetElementFromPinnedElements(index);

            // When elementIsAnchor is True and 'element' is already set, it still needs to be removed from
            // the pinned pool if it happens to be in there, for example because it has keyboard focus.
            Debug.Assert(elementFromPool == null || element == null || elementFromPool == element);

            if (element == null && elementFromPool != null)
            {
                element = elementFromPool;
            }
        }

        if (element == null)
            element = GetElementFromElementFactory(index);

        var vi = ItemsRepeater.TryGetVirtualizationInfo(element);
        if (suppressAutoRecycle)
        {
            vi.AutoRecycleCandidate = false;
#if DEBUG && REPEATER_TRACE
            Log.Debug("GetElement: {Index} Not AutoRecycleCandidate", vi.Index);            
#endif
        }
        else
        {
            vi.AutoRecycleCandidate = true;
            vi.KeepAlive = true;
#if DEBUG && REPEATER_TRACE
            Log.Debug("GetElement: {Index} AutoRecycleCandidate", vi.Index);
#endif
        }

        return element;
    }

    public void ClearElement(Control element, bool isClearedDueToCollectionChange)
    {
        var vi = ItemsRepeater.GetVirtualizationInfo(element);
        int index = vi.Index;
        bool cleared = ClearElementToUniqueIdResetPool(element, vi) ||
            ClearElementToAnimator(element, vi) ||
            ClearElementToPinnedPool(element, vi, isClearedDueToCollectionChange);

        if (!cleared)
        {
            ClearElementToElementFactory(element);
        }

        //// Both First and Last indices need to be valid or default.
        Debug.Assert((_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault &&
            _lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault) ||
            (_firstRealizedElementIndexHeldByLayout != FirstRealizedElementIndexDefault && _lastRealizedElementIndexHeldByLayout != LastRealizedElementIndexDefault));

        if (index == _firstRealizedElementIndexHeldByLayout && index == _lastRealizedElementIndexHeldByLayout)
        {
            // First and last were pointing to the same element and that is going away.
            InvalidateRealizedIndicesHeldByLayout();
        }
        else if (index == _firstRealizedElementIndexHeldByLayout)
        {
            // The FirstElement is going away, shrink the range by one.
            ++_firstRealizedElementIndexHeldByLayout;
        }
        else if (index == _lastRealizedElementIndexHeldByLayout)
        {
            // Last element is going away, shrink the range by one at the end.
            --_lastRealizedElementIndexHeldByLayout;
        }
        else
        {
            // Index is either outside the range we are keeping track of or inside the range.
            // In both these cases, we just keep the range we have. If this clear was due to 
            // a collection change, then in the CollectionChanged event, we will invalidate these guys.
        }
    }

    // We need to clear the datacontext to prevent crashes from happening,
    //  however we only do that if we were the ones setting it.
    // That is when one of the following is the case (numbering taken from line ~642):
    // 1.2    No ItemTemplate, data is not a UIElement
    // 2.1    ItemTemplate, data is not FrameworkElement
    // 2.2.2  Itemtemplate, data is FrameworkElement, ElementFactory returned Element different to data
    //
    // In all of those three cases, we the ItemTemplateShim is NOT null.
    // Luckily when we create the items, we store whether we were the once setting the DataContext.
    internal void ClearElementToElementFactory(Control element)
    {
        _owner.OnElementClearing(element);

        var vi = ItemsRepeater.GetVirtualizationInfo(element);
        vi.MoveOwnershipToElementFactory();

        // During creation of this object, we were the one setting the DataContext, so clear it now.
        if (vi.MustClearDataContext)
        {
            element.DataContext = null;
        }

        if (_owner.ItemTemplateShim != null)
        {
            _elementFactoryRecycleArgs.Element = element;
            _elementFactoryRecycleArgs.Parent = _owner;

            _owner.ItemTemplateShim.RecycleElement(_elementFactoryRecycleArgs);

            _elementFactoryRecycleArgs.Element = null;
            _elementFactoryRecycleArgs.Parent = null;
        }
        else
        {
            // No ItemTemplate to recycle to, remove the element from the children collection.
            var children = _owner.Children;
            var idx = children.IndexOf(element);
            children.RemoveAt(idx);
        }

        _phaser.StopPhasing(element, vi);
        if (_lastFocusedElement == element)
        {
            // Focused element is going away. Remove the tracked last focused element
            // and pick a reasonable next focus if we can find one within the layout 
            // realized elements.
            var clearedIndex = vi.Index;
            MoveFocusFromClearedIndex(clearedIndex);
        }

#if DEBUG && REPEATER_TRACE
        Log.Debug("Element Cleared");
#endif
    }

    private void MoveFocusFromClearedIndex(int clearedIndex)
    {
        if (FindFocusCandidate(clearedIndex, out var focusedChild) is Control focusCandidate)
        {
            if (_lastFocusedElement != null)
            {
                // Get FocusState
            }

            // If the last focused element has focus, use its focus state, if not use programmatic.
            // focusState = focusState == winrt::FocusState::Unfocused ? winrt::FocusState::Programmatic : focusState;
            focusCandidate.Focus();

            _lastFocusedElement = focusCandidate;
            // Add pin to hold the focused element.
            UpdatePin(focusedChild, true /*addPin*/);
        }
        else
        {
            // We could not find a candiate.
            _lastFocusedElement = null;
        }
    }

    private Control FindFocusCandidate(int clearedIndex, out Control focusedChild)
    {
        focusedChild = null;
        // Walk through all the children and find elements with index before and after the cleared index.
        // Note that during a delete the next element would now have the same index.
        int previousIndex = int.MinValue;
        int nextIndex = int.MaxValue;
        Control nextElement = null;
        Control previousElement = null;
        var children = _owner.Children;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(child);
            if (virtInfo != null && virtInfo.IsHeldByLayout)
            {
                int currentIndex = virtInfo.Index;
                if (currentIndex < clearedIndex)
                {
                    if (currentIndex > previousIndex)
                    {
                        previousIndex = currentIndex;
                        previousElement = child;
                    }
                }
                else if (currentIndex >= clearedIndex)
                {
                    // Note that we use >= above because if we deleted the focused element, 
                    // the next element would have the same index now.
                    if (currentIndex < nextIndex)
                    {
                        nextIndex = currentIndex;
                        nextElement = child;
                    }
                }
            }
        }

        // Find the next element if one exists, if not use the previous element.
        // If the container itself is not focusable, find a descendent that is.
        Control focusCandidate = null;
        if (nextElement != null)
        {
            focusedChild = nextElement;
            focusCandidate = nextElement;
            if (focusCandidate == null)
            {
                // FocusManager.FindFirstFocusableElement(nextElement);
            }
        }

        if (focusCandidate == null && previousElement != null)
        {
            focusedChild = previousElement;
            focusCandidate = previousElement;
            if (previousElement == null)
            {
                // FocusManager.FindLastFocusableElement(previousElement)
            }
        }

        return focusCandidate;
    }

    internal int GetElementIndex(VirtualizationInfo vInfo)
    {
        if (vInfo == null)
            return -1;

        return vInfo.IsRealized || vInfo.IsInUniqueIdResetPool ? vInfo.Index : -1;
    }

    internal void PrunePinnedElements()
    {
        EnsureEventSubscriptions();

        // Go through pinned elements and make sure they still have
        // a reason to be pinned.
        for (int i = 0; i < _pinnedPool.Count; i++)
        {
            var ei = _pinnedPool[i];
            var vi = ei.VirtualizationInfo;

            Debug.Assert(vi.Owner == VirtualizationInfo.ElementOwner.PinnedPool);

            if (!vi.IsPinned)
            {
                _pinnedPool.RemoveAt(i);
                i--;

                // Pinning was the only thing keeping this element alive.
                ClearElementToElementFactory(ei.PinnedElement);
            }
        }
    }

    internal void UpdatePin(Control element, bool addPin)
    {
        var parent = element.GetVisualParent();
        var child = element;

        while (parent != null)
        {
            if (parent is ItemsRepeater repeater)
            {
                var virtInfo = ItemsRepeater.GetVirtualizationInfo(child);
                if (virtInfo.IsRealized)
                {
                    if (addPin)
                    {
                        virtInfo.AddPin();
                    }
                    else if (virtInfo.IsPinned)
                    {
                        if (virtInfo.RemovePin() == 0)
                        {
                            // ElementFactory is invoked during the measure pass.
                            // We will clear the element then.
                            repeater.InvalidateMeasure();
                        }
                    }
                }
            }

            child = (Control)parent;
            parent = child.GetVisualParent();
        }
    }

    internal void OnItemsSourceChanged(object _, NotifyCollectionChangedEventArgs args)
    {
        // Note: For items that have been removed, the index will not be touched. It will hold
        // the old index before it was removed. It is not valid anymore.
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    var newIndex = args.NewStartingIndex;
                    var newCount = args.NewItems.Count;
                    EnsureFirstLastRealizedIndices();
                    if (newIndex <= _lastRealizedElementIndexHeldByLayout)
                    {
                        _lastRealizedElementIndexHeldByLayout += newCount;
                        var children = _owner.Children;
                        var ct = children.Count;
                        for (int i = 0; i < ct; i++)
                        {
                            var element = children[i];
                            var vi = ItemsRepeater.GetVirtualizationInfo(element);
                            var dataIndex = vi.Index;

                            if (vi.IsRealized && dataIndex >= newIndex)
                            {
                                UpdateElementIndex(element, vi, dataIndex + newCount);
                            }
                        }
                    }
                    else
                    {
                        // Indices held by layout are not affected
                        // We could still have items in the pinned elements that need updates. This is usually a very small vector.
                        for (int i = 0; i < _pinnedPool.Count; i++)
                        {
                            var element = _pinnedPool[i];
                            var vi = element.VirtualizationInfo;
                            var dataIndex = vi.Index;

                            if (vi.IsPinned && dataIndex >= newIndex)
                            {
                                UpdateElementIndex(element.PinnedElement, vi, dataIndex + newCount);
                            }
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                {
                    // Requirement: oldStartIndex == newStartIndex. It is not a replace if this is not true.
                    // Two cases here
                    // case 1: oldCount == newCount 
                    //         indices are not affected. nothing to do here.  
                    // case 2: oldCount != newCount
                    //         Replaced with less or more items. This is like an insert or remove
                    //         depending on the counts.
                    var oldStartIndex = args.OldStartingIndex;
                    var newStartingIndex = args.NewStartingIndex;
                    var oldCount = args.OldItems.Count;
                    var newCount = args.NewItems.Count;
                    if (oldStartIndex != newStartingIndex)
                    {
                        throw new InvalidOperationException("Replace is only allowed with OldStartingIndex equals to NewStartingIndex.");
                    }

                    if (oldCount == 0)
                    {
                        throw new InvalidOperationException("Replace notification with args.OldItemsCount value of 0 is not allowed. Use Insert action instead.");
                    }

                    if (newCount == 0)
                    {
                        throw new InvalidOperationException("Replace notification with args.NewItemCount value of 0 is not allowed. Use Remove action instead.");
                    }

                    int countChange = newCount - oldCount;
                    if (countChange != 0)
                    {
                        // countChange > 0 : countChange items were added
                        // countChange < 0 : -countChange  items were removed
                        var children = _owner.Children;
                        for (int i = 0; i < children.Count; ++i)
                        {
                            var element = children[i];
                            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                            var dataIndex = virtInfo.Index;

                            if (virtInfo.IsRealized)
                            {
                                if (dataIndex >= oldStartIndex + oldCount)
                                {
                                    UpdateElementIndex(element, virtInfo, dataIndex + countChange);
                                }
                            }
                        }

                        EnsureFirstLastRealizedIndices();
                        _lastRealizedElementIndexHeldByLayout += countChange;

                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    var oldStartIndex = args.OldStartingIndex;
                    var oldCount = args.OldItems.Count;
                    var children = _owner.Children;
                    for (int i = 0; i < children.Count; ++i)
                    {
                        var element = children[i];
                        var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                        var dataIndex = virtInfo.Index;

                        if (virtInfo.IsRealized)
                        {
                            if (virtInfo.AutoRecycleCandidate && oldStartIndex <= dataIndex && dataIndex < oldStartIndex + oldCount)
                            {
                                // If we are doing the mapping, remove the element who's data was removed.
                                _owner.ClearElementImpl(element);
                            }
                            else if (dataIndex >= (oldStartIndex + oldCount))
                            {
                                UpdateElementIndex(element, virtInfo, dataIndex - oldCount);
                            }
                        }
                    }

                    InvalidateRealizedIndicesHeldByLayout();

                }
                break;

            case NotifyCollectionChangedAction.Reset:
                {
                    // If we get multiple resets back to back before
                    // running layout, we dont have to clear all the elements again.
                    if (!_isDataSourceStableResetPending)
                    {
                        if (_owner.ItemsSourceView.HasKeyIndexMapping)
                            _isDataSourceStableResetPending = true;

                        // Walk through all the elements and make sure they are cleared, they will go into
                        // the stable id reset pool.
                        var children = _owner.Children;
                        for (int i = 0; i < children.Count; ++i)
                        {
                            var element = children[i];
                            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                            if (virtInfo.IsRealized && virtInfo.AutoRecycleCandidate)
                            {
                                _owner.ClearElementImpl(element);
                            }
                        }

                    }

                    InvalidateRealizedIndicesHeldByLayout();
                }
                break;
        }
    }

    private void EnsureFirstLastRealizedIndices()
    {
        if (_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault)
        {
            // This will ensure that the indexes are updated.
            var element = GetElementIfAlreadyHeldByLayout(0);
        }
    }

    internal void OnLayoutChanging()
    {
        if (_owner.ItemsSourceView != null && _owner.ItemsSourceView.HasKeyIndexMapping)
        {
            _isDataSourceStableResetPending = true;
        }
    }

    internal void OnOwnerArranged()
    {
        if (_isDataSourceStableResetPending)
        {
            _isDataSourceStableResetPending = false;
            foreach (var entry in _resetPool)
            {
                // TODO: Task 14204306: ItemsRepeater: Find better focus candidate when focused element is deleted in the ItemsSource.
                // Focused element is getting cleared. Need to figure out semantics on where
                // focus should go when the focused element is removed from the data collection.
                ClearElement(entry.Value, true /* isClearedDueToCollectionChange */);
            }

            _resetPool.Clear();

            // Flush the realized indices once the stable reset pool is cleared to start fresh.
            InvalidateRealizedIndicesHeldByLayout();
        }
    }

    // We optimize for the case where index is not realized to return null as quickly as we can.
    // Flow layouts manage containers on their own and will never ask for an index that is already realized.
    // If an index that is realized is requested by the layout, we unfortunately have to walk the
    // children. Not ideal, but a reasonable default to provide consistent behavior between virtualizing
    // and non-virtualizing hosts.
    private Control GetElementIfAlreadyHeldByLayout(int index)
    {
        Control element = null;

        bool cachedFirstLastIndicesInvalid = _firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault;

        Debug.Assert(!cachedFirstLastIndicesInvalid || _lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault);
        
        bool isRequestedIndexInRealizedRange = (_firstRealizedElementIndexHeldByLayout <= index && 
            index <= _lastRealizedElementIndexHeldByLayout);

        if (cachedFirstLastIndicesInvalid || isRequestedIndexInRealizedRange)
        {
            // Both First and Last indices need to be valid or default.
            Debug.Assert((_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault && 
                _lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault) ||
                (_firstRealizedElementIndexHeldByLayout != FirstRealizedElementIndexDefault && 
                _lastRealizedElementIndexHeldByLayout != LastRealizedElementIndexDefault));

            var children = _owner.Children;
            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(child);
                if (virtInfo != null && virtInfo.IsHeldByLayout)
                {
                    // Only give back elements held by layout. If someone else is holding it, they will be served by other methods.
                    int childIndex = virtInfo.Index;
                    _firstRealizedElementIndexHeldByLayout = Math.Min(_firstRealizedElementIndexHeldByLayout, childIndex);
                    _lastRealizedElementIndexHeldByLayout = Math.Max(_lastRealizedElementIndexHeldByLayout, childIndex);
                    if (virtInfo.Index == index)
                    {
                        element = child;
                        // If we have valid first/last indices, we don't have to walk the rest, but if we 
                        // do not, then we keep walking through the entire children collection to get accurate
                        // indices once.
                        if (!cachedFirstLastIndicesInvalid)
                        {
                            break;
                        }
                    }
                }
            }
        }

        return element;

    }

    private Control GetElementFromUniqueIdResetPool(int index)
    {
        Control element = null;
        // See if you can get it from the reset pool.
        if (_isDataSourceStableResetPending)
        {
            element = _resetPool.Remove(index);
            if (element != null)
            {
                // Make sure that the index is updated to the current one
                var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                virtInfo.MoveOwnershipToLayoutFromUniqueIdResetPool();
                UpdateElementIndex(element, virtInfo, index);

                // Update realized indices
                _firstRealizedElementIndexHeldByLayout = Math.Max(_firstRealizedElementIndexHeldByLayout, index);
                _lastRealizedElementIndexHeldByLayout = Math.Max(_lastRealizedElementIndexHeldByLayout, index);
            }
        }

        return element;
    }

    private Control GetElementFromPinnedElements(int index)
    {
        Control element = null;

        for (int i = 0; i < _pinnedPool.Count; i++)
        {
            var elementInfo = _pinnedPool[i];
            var virtInfo = elementInfo.VirtualizationInfo;

            if (virtInfo.Index == index)
            {
                _pinnedPool.RemoveAt(i);
                element = elementInfo.PinnedElement;
                elementInfo.VirtualizationInfo.MoveOwnershipToLayoutFromPinnedPool();

                // Update realized indices
                _firstRealizedElementIndexHeldByLayout = Math.Min(_firstRealizedElementIndexHeldByLayout, index);
                _lastRealizedElementIndexHeldByLayout = Math.Max(_lastRealizedElementIndexHeldByLayout, index);
                break;
            }

        }

        return element;
    }

    // There are several cases handled here with respect to which element gets returned and when DataContext is modified.
    //
    // 1. If there is no ItemTemplate:
    //    1.1 If data is a UIElement -> the data is returned
    //    1.2 If data is not a UIElement -> a default DataTemplate is used to fetch element and DataContext is set to data**
    //
    // 2. If there is an ItemTemplate:
    //    2.1 If data is not a FrameworkElement -> Element is fetched from ElementFactory and DataContext is set to the data**
    //    2.2 If data is a FrameworkElement:
    //        2.2.1 If Element returned by the ElementFactory is the same as the data -> Element (a.k.a. data) is returned as is
    //        2.2.2 If Element returned by the ElementFactory is not the same as the data
    //                 -> Element that is fetched from the ElementFactory is returned and
    //                    DataContext is set to the data's DataContext (if it exists), otherwise it is set to the data itself**
    //
    // **data context is set only if no x:Bind was used. ie. No data template component on the root.
    private Control GetElementFromElementFactory(int index)
    {
        // The view generator is the provider of last resort.
        var data = _owner.ItemsSourceView.GetAt(index);

        Control element = null;
        var providedElementFactory = _owner.ItemTemplateShim;

        if (providedElementFactory == null)
        {
            element = data as Control;
        }

        if (element == null)
        {
            IElementFactory GetElementFactory()
            {
                if (providedElementFactory == null)
                {
                    _owner.ItemTemplate = FuncDataTemplate.Default;
                    return _owner.ItemTemplateShim;
                }

                return providedElementFactory;
            }

            var args = _elementFactoryGetArgs;

            try
            {
                args.Data = data;
                args.Parent = _owner;
                args.Index = index;

                element = GetElementFactory().GetElement(args);
            }
            finally
            {
                args.Data = null;
                args.Parent = null;
            }
        }

        var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(element);
        if (virtInfo == null)
        {
            virtInfo = ItemsRepeater.CreateAndInitializeVirtualizationInfo(element);
#if DEBUG && REPEATER_TRACE
            Log.Debug("Element Created");
#endif
        }
        else
        {
            // View obtained from ElementFactory already has a VirtualizationInfo attached to it
            // which means that the element has been recycled and not created from scratch.
#if DEBUG && REPEATER_TRACE
            Log.Debug("Element Recycled");
#endif
        }
        // Clear flag
        virtInfo.MustClearDataContext = false;

        ContainerContentChangingEventArgs cArgs = null;
        bool shouldPhase = _owner.ShouldPhase;

        // NOTE: This code has been changed from WinUI in order to support our version of phased rendering
        if (data != element)
        {
            // Prepare the element
            // If we are phasing, run phase 0 before setting DataContext. If phase 0 is not 
            // run before setting DataContext, when setting DataContext all the phases will be
            // run in the OnDataContextChanged handler in code generated by the xaml compiler (code-gen).
            
            if (element is Control)
            {
                // Set data context only if no x:Bind was used. ie. No data template component on the root.
                // If the passed in data is a UIElement and is different from the element returned by 
                // the template factory then we need to propagate the DataContext.
                // Otherwise just set the DataContext on the element as the data.
                object dataContext = null;
                if (data is Control dataAsElement)
                {
                    if (dataAsElement.DataContext != null)
                    {
                        dataContext = dataAsElement.DataContext;
                    }
                }
                else
                {
                    dataContext = data;
                }

                element.DataContext = dataContext;
                virtInfo.MustClearDataContext = true;

                // This runs phase 0 of rendering, if we have subscribers to ContainerContentChanging
                // To initiate other phases, handlers should call RegisterCallback on the ContainerContentChangingEventArgs
                // which will add that work to the build tree scheduler and phaser to complete
                if (data != element && shouldPhase)
                {
                    virtInfo.UpdatePhasingInfo(data);
                    cArgs = new ContainerContentChangingEventArgs(index, data, element, virtInfo, 0, _phaser);
                    _owner.RaiseContainerContentChanging(cArgs);// index, data, element, virtInfo);
                }
            }
            else
            {
                Debug.Assert(false, "Element returned by factory is not a FrameworkElement!");
            }
        }

        virtInfo.MoveOwnershipToLayoutFromElementFactory(index,
            _owner.ItemsSourceView.HasKeyIndexMapping ?
            _owner.ItemsSourceView.KeyFromIndex(index) : string.Empty);

        // The view generator is the only provider that prepares the element.
        var repeater = _owner;

        // Add the element to the children collection here before raising OnElementPrepared so 
        // that handlers can walk up the tree in case they want to find their IndexPath in the 
        // nested case.
        var children = repeater.Children;
        var parent = element.GetVisualParent();
        if (parent != repeater)
        {
            children.Add(element);
        }

        repeater.TransitionManager.OnElementPrepared(element);
        repeater.OnElementPrepared(element, index, virtInfo);

        // Update realized indices
        _firstRealizedElementIndexHeldByLayout = Math.Min(_firstRealizedElementIndexHeldByLayout, index);
        _lastRealizedElementIndexHeldByLayout = Math.Max(_lastRealizedElementIndexHeldByLayout, index);

        return element;
    }

    private bool ClearElementToUniqueIdResetPool(Control element, VirtualizationInfo virtInfo)
    {
        if (_isDataSourceStableResetPending)
        {
            _resetPool.Add(element);
            virtInfo.MoveOwnershipToUniqueIdResetPoolFromLayout();
        }

        return _isDataSourceStableResetPending;
    }

    private bool ClearElementToAnimator(Control element, VirtualizationInfo virtInfo)
    {
        bool cleared = _owner.TransitionManager.ClearElement(element);
        if (cleared)
        {
            var clearedIndex = virtInfo.Index;
            virtInfo.MoveOwnershipToAnimator();
            if (_lastFocusedElement == element)
            {
                // Focused element is going away. Remove the tracked last focused element
                // and pick a reasonable next focus if we can find one within the layout 
                // realized elements.
                MoveFocusFromClearedIndex(clearedIndex);
            }
        }

        return cleared;
    }

    private bool ClearElementToPinnedPool(Control element, VirtualizationInfo virtInfo, bool isClearedDueToCollectionChange)
    {
        bool moveToPinnedPool = !isClearedDueToCollectionChange && virtInfo.IsPinned;

        if (moveToPinnedPool)
        {
#if DEBUG
            for (int i = 0; i < _pinnedPool.Count; i++)
            {
                Debug.Assert(_pinnedPool[i].PinnedElement != element);
            }
#endif

            _pinnedPool.Add(new PinnedElementInfo(element, virtInfo));
            virtInfo.MoveOwnershipToLayoutFromPinnedPool();
        }

        return moveToPinnedPool;
    }

    private void UpdateFocusedElement()
    {
        var owner = _owner;

        var xamlRoot = TopLevel.GetTopLevel(owner);
        Control child = null;
        Control focusedElement = null;

        if (xamlRoot != null)
        {
            child = xamlRoot.FocusManager.GetFocusedElement() as Control;
        }

        if (child != null)
        {
            var parent = child.GetVisualParent();
           
            // Find out if the focused element belongs to one of our direct
            // children.
            while (parent != null)
            {
                if (parent is ItemsRepeater repeater)
                {
                    if (child is Control element && repeater == owner &&
                        ItemsRepeater.GetVirtualizationInfo(element).IsRealized)
                    {
                        focusedElement = element;
                    }

                    break;
                }

                child = (Control)parent;
                parent = child.GetVisualParent();
            }
        }

        // If the focused element has changed,
        // we need to unpin the old one and pin the new one.
        if (_lastFocusedElement != focusedElement)
        {
            if (_lastFocusedElement != null)
            {
                UpdatePin(_lastFocusedElement, false /* addPin */);
            }

            if (focusedElement != null)
            {
                UpdatePin(focusedElement, true /* addPin */);
            }

            _lastFocusedElement = focusedElement;
        }
    }

    private void OnFocusChanged(object _, RoutedEventArgs __)
    {
        UpdateFocusedElement();
    }

    private void EnsureEventSubscriptions()
    {
        if (!_gotFocus)
        {
            _gotFocus = true;
            _owner.GotFocus += OnFocusChanged;
            _owner.LostFocus += OnFocusChanged;
        }
    }

    private void UpdateElementIndex(Control element, VirtualizationInfo virtInfo, int index)
    {
        var oldIndex = virtInfo.Index;
        if (oldIndex != index)
        {
            virtInfo.UpdateIndex(index);
            _owner.OnElementIndexChanged(element, oldIndex, index);
        }
    }

    private void InvalidateRealizedIndicesHeldByLayout()
    {
        _firstRealizedElementIndexHeldByLayout = FirstRealizedElementIndexDefault;
        _lastRealizedElementIndexHeldByLayout = LastRealizedElementIndexDefault;
    }


    private readonly ItemsRepeater _owner;
    private readonly List<PinnedElementInfo> _pinnedPool;
    private readonly UniqueIdElementPool _resetPool;

    private Control _lastFocusedElement;
    private bool _isDataSourceStableResetPending;

    private Phaser _phaser;

    // Cached generate/clear contexts to avoid cost of creation every time.
    private readonly ElementFactoryGetArgs _elementFactoryGetArgs = new ElementFactoryGetArgs();
    private readonly ElementFactoryRecycleArgs _elementFactoryRecycleArgs = new ElementFactoryRecycleArgs();

    // These are first/last indices requested by layout and not cleared yet.
    // These are also not truly first / last because they are a lower / upper bound on the known realized range.
    // For example, if we didn't have the optimization in ElementManager.cpp, m_lastRealizedElementIndexHeldByLayout 
    // will not be accurate. Rather, it will be an upper bound on what we think is the last realized index.
    private int _firstRealizedElementIndexHeldByLayout = FirstRealizedElementIndexDefault;
    private int _lastRealizedElementIndexHeldByLayout = LastRealizedElementIndexDefault;
    private const int FirstRealizedElementIndexDefault = int.MaxValue;
    private const int LastRealizedElementIndexDefault = int.MinValue;

    private bool _gotFocus;

    private struct PinnedElementInfo
    {
        public PinnedElementInfo(Control element, VirtualizationInfo vi = null)
        {
            PinnedElement = element;
            VirtualizationInfo = vi ?? ItemsRepeater.GetVirtualizationInfo(element);
        }

        public Control PinnedElement { get; }

        public VirtualizationInfo VirtualizationInfo { get; }
    }
}
