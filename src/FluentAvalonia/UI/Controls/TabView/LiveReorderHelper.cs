using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using FluentAvalonia.Collections;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls;

internal class LiveReorderHelper
{
    public LiveReorderHelper(TabViewListView owner)
    {
        _owner = owner;
    }

    public Panel ItemsPanelRoot => _owner.ItemsPanelRoot;

    public void ProcessLiveReorder(DragEventArgs args, int dragItemIndex)
    {
        var orientation = _owner.GetLogicalOrientation();
        if (orientation == null)
            return; // We're in a panel we don't know how to deal with, exit out

        // If we don't have a cache of the current realized container bounds,
        // create it now before we start doing anything. This happens on first
        // startup, but can also be reset through the drag operation (autoscroll,
        // drag outside, etc). The cache is cleared on ResetAllItemsForLiveReorder()
        if (ShouldCacheContainerBounds())
        {
            // had to add UpdateLayout here b/c TabView inserts that extra blank space at the end
            // of the list (see TabView), which causes the panel to resize to account for that
            // but that wasn't happening until after our caching occurred, which meant our bounds
            // were incorrect and things were not lining up anymore.
            _owner.UpdateLayout();
            CacheContainerBounds();
        }

        var dragPoint = args.GetPosition(_owner);
        int draggedIndex = dragItemIndex;
        int insertionIndex = -1;
        int dragOverIndex = GetClosestElement(dragPoint);// IndexFromContainer(currentItem); // The raw item index under the pointer
        var previousDragOverIndex = _liveReorderIndices.draggedOverIndex;
        int itemsCount = _owner.ItemCount;

        if (draggedIndex == -1)
            draggedIndex = itemsCount;

        if (previousDragOverIndex == -1)
        {
            previousDragOverIndex = draggedIndex;
        }

        // The estimated insertion index in the panel
        insertionIndex = GetClosestElement(dragPoint, true /*requestingInsertionIndex*/);

        if (draggedIndex == itemsCount && insertionIndex == itemsCount - 1)
        {
            // If we didn't start in this TabView, see if the index is actually the end or -1
            var spLastElement = _owner.ContainerFromIndex(insertionIndex);
            if (spLastElement is TabViewItem tvi)
            {
                if (IsInBottomHalf(args.GetPosition(spLastElement), new Rect(spLastElement.Bounds.Size), orientation.Value))
                {
                    insertionIndex = itemsCount;
                }
            }
        }

        //var old = dragOverIndex; // Keep this here for debug purposes, if needed
        if (insertionIndex == itemsCount)
        {
            dragOverIndex = itemsCount;
        }
        else
        {
            // This adjusts the dragover index based on the direction of the drag
            dragOverIndex = GetDragOverIndex(dragOverIndex, insertionIndex, previousDragOverIndex);
        }

        // Debug.WriteLine($"\tLive Reorder DragOverIndex: {dragOverIndex} || DragOverBeforeAdj: {old} || InsertionIndex {insertionIndex} || PrevDragOverIndex {previousDragOverIndex}");

        _liveReorderIndices = new LiveReorderIndices(draggedIndex, dragOverIndex, itemsCount);

        if (previousDragOverIndex == draggedIndex || previousDragOverIndex != dragOverIndex)
        {
            StartLiveReorderTimer();
        }

        static bool IsInBottomHalf(Point pt, Rect rc, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                return (pt.X - rc.Left) >= rc.Width * 0.5;
            }
            else
            {
                return (pt.Y - rc.Top) >= rc.Height * 0.5;
            }
        }

        static int GetDragOverIndex(int closestElementIndex, int insertionIndex, int previousDragOverIndex)
        {
            int dragOverIndex = closestElementIndex;

            if (insertionIndex == closestElementIndex)
            {
                if (previousDragOverIndex < insertionIndex)
                {
                    --dragOverIndex;
                }
            }
            else
            {
                if (previousDragOverIndex >= insertionIndex)
                {
                    ++dragOverIndex;
                }
            }

            return dragOverIndex;
        }
    }

    public void ResetAllItemsForLiveReorder()
    {
        StopLiveReorderTimer();

        foreach (var item in _movedItems.AsSpan())
        {
            if (item.destinationIndex != -1)
            {
                var cont = _owner.ContainerFromIndex(item.sourceIndex);
                if (cont is Control c)
                {
                    c.Arrange(item.sourceRect);
                }
            }
        }

        _movedItems.Clear();
        _liveReorderIndices = new LiveReorderIndices(-1, -1, -1);
        ClearContainerBoundsCache();
    }

    private int GetInsertionIndexForLiveReorder()
    {
        var draggedIndex = _liveReorderIndices.draggedItemIndex;
        var insertIndex = _liveReorderIndices.draggedOverIndex;

        if (draggedIndex < insertIndex)
        {
            insertIndex++;
        }

        // make sure we don't go out of range
        if (insertIndex > _liveReorderIndices.itemsCount)
            insertIndex = _liveReorderIndices.itemsCount;

        return insertIndex;
    }

    private int GetClosestElement(Point dragPoint, bool requestingInsertionIndex = false)
    {
        // This estimates the container index given the current pointer position
        var panel = ItemsPanelRoot;
        if (panel is VirtualizingStackPanel vsp)
        {
            var firstRealized = vsp.FirstRealizedIndex;
            var lastRealized = vsp.LastRealizedIndex;
            var orientation = vsp.Orientation;
            var movedItems = _movedItems.AsSpan();
            int closestIndex = -1;
            double closestDist = double.PositiveInfinity;
            Rect closestItemRect = default;

            // Loop over the currently realized items to find the closest
            for (int i = firstRealized; i <= lastRealized; i++)
            {
                // If the item is currently in our MovedItems list, it may not be 
                // where it usually is, so we can't test the actual Bounds or we'll
                // estimate the wrong index, but we have the original bounds saved
                Rect rc = _cachedContainerBounds[i - firstRealized];
                double dist;

                if (orientation == Orientation.Horizontal)
                {
                    double cx = double.Clamp(dragPoint.X, rc.X, rc.Right);
                    dist = double.Abs(dragPoint.X - cx);
                }
                else
                {
                    double cy = double.Clamp(dragPoint.Y, rc.Y, rc.Bottom);
                    dist = double.Abs(dragPoint.Y - cy);
                }

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                    closestItemRect = rc;
                }
            }

            if (requestingInsertionIndex)
            {
                if (orientation == Orientation.Horizontal)
                {
                    if (dragPoint.X - closestItemRect.X >= closestItemRect.Width * 0.5)
                    {
                        closestIndex++;
                    }
                }
                else if (orientation == Orientation.Vertical)
                {
                    if (dragPoint.Y - closestItemRect.Y >= closestItemRect.Height * 0.5)
                    {
                        closestIndex++;
                    }
                }
            }

            return closestIndex;
        }
        else if (panel is StackPanel sp)
        {
            //var children = sp.Children;
            //var orientation = sp.Orientation;
            //var movedItems = _movedItems.AsSpan();

            //for (int i = 0; i < children.Count; i++)
            //{
            //    // If the item is currently in our MovedItems list, it may not be 
            //    // where it usually is, so we can't test the actual Bounds or we'll
            //    // estimate the wrong index, but we have the original bounds saved
            //    if (IsInMovedItems(movedItems, i, out var rc))
            //    {
            //        if (rc.Contains(dragPoint))
            //        {
            //            return i;
            //        }
            //    }
            //    else
            //    {
            //        // The item is not in moved items, so it is safe to use the Bounds directly
            //        if (children[i].Bounds.Contains(dragPoint))
            //        {
            //            return i;
            //        }
            //    }
            //}
        }

        return -1;

        //static bool IsInMovedItems(ReadOnlySpan<MovedItem> items, int sourceIndex, out Rect srcRect)
        //{
        //    foreach (var item in items)
        //    {
        //        if (item.sourceIndex == sourceIndex)
        //        {
        //            srcRect = item.sourceRect;
        //            return true;
        //        }
        //    }

        //    srcRect = default;
        //    return false;
        //}
    }

    private void StartLiveReorderTimer()
    {
        StopLiveReorderTimer();

        EnsureLiveReorderTimer();

        _liveReorderTimer.Interval = TimeSpan.FromMilliseconds(200);
        _liveReorderTimer.Start();
    }

    private void EnsureLiveReorderTimer()
    {
        if (_liveReorderTimer == null)
        {
            _liveReorderTimer = new DispatcherTimer();
            _liveReorderTimer.Tick += LiveReorderTimerTickHandler;
        }
    }

    private void LiveReorderTimerTickHandler(object sender, EventArgs e)
    {
        StopLiveReorderTimer();

        Debug.Assert(_liveReorderIndices.draggedItemIndex != -1);

        var orientation = _owner.GetLogicalOrientation().Value;

        using var newItems = new PooledList<MovedItem>();
        using var newItemsToMove = new PooledList<MovedItem>();
        using var oldItemsToMoveBack = new PooledList<MovedItem>();

        GetNewMovedItemsForLiveReorder(newItems);

        _movedItems.Update(orientation == Orientation.Vertical, newItems, newItemsToMove, oldItemsToMoveBack);

        MoveItemsForLiveReorder(false /*areNewItems*/, oldItemsToMoveBack);

        MoveItemsForLiveReorder(true, newItemsToMove);

        foreach (var item in _movedItems.AsSpan())
        {
            Debug.WriteLine($"\t MovedItems: {item.sourceIndex} -> {item.destinationIndex} || {item.sourceRect} -> {item.destinationRect}");
        }
    }

    private void GetNewMovedItemsForLiveReorder(IList<MovedItem> newItems)
    {
        int startIndex = _liveReorderIndices.draggedItemIndex;
        int endIndex = _liveReorderIndices.draggedOverIndex;
        int increment = (startIndex < endIndex) ? 1 : -1;

        // Debug.WriteLine($"GetNewMovedItems: {startIndex} -> {endIndex}");

        newItems.Clear();
        for (int i = startIndex; i != endIndex; i += increment)
        {
            int targetIndex = i - increment;

            if (i == startIndex)
            {
                targetIndex = -1;
            }

            AddNewItemForLiveReorder(i, targetIndex, newItems, _liveReorderIndices.itemsCount, this);
        }

        AddNewItemForLiveReorder(endIndex, endIndex - increment, newItems, _liveReorderIndices.itemsCount, this);

        // Debug.WriteLine($"TotalNewItems: {newItems.Count}");

        static void AddNewItemForLiveReorder(int sourceIndex, int targetIndex, IList<MovedItem> newItems,
            int itemsCount, LiveReorderHelper host)
        {
            Rect src = default;
            Rect target = default;

            if (sourceIndex != targetIndex)
            {
                src = GetLayoutSlot(host, sourceIndex);
            }

            if (targetIndex != -1 && targetIndex != itemsCount)
            {
                target = GetLayoutSlot(host, targetIndex);
            }

            newItems.Add(new MovedItem(sourceIndex, targetIndex, src, target));
        }

        static Rect GetLayoutSlot(LiveReorderHelper host, int index)
        {
            // make sure we grab the original bounds. If virtualizing, translate
            // to index in our container cache
            var adjIndex = host.ItemsPanelRoot is VirtualizingStackPanel vsp ?
                index - vsp.FirstRealizedIndex : index;

            if (adjIndex >= host._cachedContainerBounds.Count)
                return default;

            return host._cachedContainerBounds[adjIndex];
            //return host.ContainerFromIndex(index)?.Bounds ?? default;
        }
    }

    private void MoveItemsForLiveReorder(bool areNewItems, PooledList<MovedItem> newItemsToMove)
    {
        Rect rc;
        foreach (var item in newItemsToMove.AsSpan())
        {
            var container = _owner.ContainerFromIndex(item.sourceIndex);

            if (container is Control c)
            {
                if (areNewItems)
                {
                    rc = item.destinationRect;
                }
                else
                {
                    rc = item.sourceRect;
                }

                c.Arrange(rc);
            }
        }
    }

    private void StopLiveReorderTimer()
    {
        _liveReorderTimer?.Stop();
    }

    private bool ShouldCacheContainerBounds() =>
        _cachedContainerBounds == null || _cachedContainerBounds.Count == 0;

    private void CacheContainerBounds()
    {
        // Because reorder will arrange the containers in new places
        // the Bounds on the container may not actually reflect where
        // the item actually is. In WinUI, ModernCollectionBasePanel's 
        // ContainerManager caches arrange rects and are used for estimation
        // APIs. Here we are mimicing that
        // This must be called at the start of drag/drop, when the auto scroll
        // of the scrollviewer stops, or when we first drag onto the TabView
        // if that TabView didn't start the dragdrop operation

        var panel = ItemsPanelRoot;
        if (panel is VirtualizingStackPanel vsp)
        {
            var firstRealized = vsp.FirstRealizedIndex;
            var lastRealized = vsp.LastRealizedIndex;
            _cachedContainerBounds ??= new List<Rect>((lastRealized - firstRealized) + 1);

            for (int i = firstRealized; i <= lastRealized; i++)
            {
                var cont = _owner.ContainerFromIndex(i);
                _cachedContainerBounds.Add(cont.Bounds);
            }
        }
        else if (panel is StackPanel sp)
        {
            var itemCount = _owner.ItemCount;
            _cachedContainerBounds ??= new List<Rect>(itemCount);
            // Stack Panels don't virtualize and arrange in order so this is safe
            for (int i = 0; i < itemCount; i++)
            {
                _cachedContainerBounds.Add(panel.Children[i].Bounds);
            }
        }
    }

    public void ClearContainerBoundsCache(bool clearCompletely = false)
    {
        // Clear container bounds
        _cachedContainerBounds?.Clear();

        // Don't keep this memory when not in use, so when drag drop operation
        // ceases completely, set it to null
        if (clearCompletely)
            _cachedContainerBounds = null;
    }


    private readonly TabViewListView _owner;
    private LiveReorderIndices _liveReorderIndices = new LiveReorderIndices(-1, -1, -1);
    private DispatcherTimer _liveReorderTimer;
    private readonly MovedItems _movedItems = new MovedItems();
    private List<Rect> _cachedContainerBounds;
}

internal struct LiveReorderIndices
{
    public LiveReorderIndices(int dragItemIndex, int dragOverIndex, int count)
    {
        draggedItemIndex = dragItemIndex;
        draggedOverIndex = dragOverIndex;
        itemsCount = count;
    }

    public int draggedItemIndex;
    public int draggedOverIndex;
    public int itemsCount;
}

internal struct MovedItem
{
    public MovedItem(int src, int dst, Rect srcRc, Rect dstRc)
    {
        sourceIndex = src;
        destinationIndex = dst;
        sourceRect = srcRc;
        destinationRect = dstRc;
    }

    public int sourceIndex;
    public int destinationIndex;
    public Rect sourceRect;
    public Rect destinationRect;
}

internal class MovedItems : IEnumerable<MovedItem>
{
    public void Update(in bool isOrientationVertical, IList<MovedItem> newItems,
        IList<MovedItem> newItemsToMove, IList<MovedItem> oldItemsToMoveBack)
    {
        int newItemsSize = newItems.Count;
        int movedItemsSize = _items.Count;

        // our items should always be ordered from the dragIndex to the dragOverIndex
        int newItemsStartIndex = newItems[0].sourceIndex;
        int newItemsEndIndex = newItems[^1].sourceIndex;
        int newItemsIncrement = (newItemsStartIndex < newItemsEndIndex) ? 1 : -1;

        // the index we should use to start adding new items
        int startIndexForAddMovedItems = -1;

        newItemsToMove.Clear();
        oldItemsToMoveBack.Clear();

        if (movedItemsSize > 0)
        {
            int movedItemsStartIndex = _items[0].sourceIndex;
            int movedItemsEndIndex = _items[^1].sourceIndex;
            int movedItemsIncrement = (movedItemsStartIndex < movedItemsEndIndex) ? 1 : -1;

            // in the same drag motion, the sourceIndex should always be the same
            Debug.Assert(newItemsStartIndex == movedItemsStartIndex);
            //Debug.WriteLine($"FAIL FAIL FAIL FAIL FAIL FAIL FAIL FAIL newItemsStartIndex == movedItemsStartIndex {newItemsStartIndex} - {movedItemsStartIndex}");


            // remove the indices that are now out of range of new indexes
            RemoveMovedItems((newItemsIncrement == movedItemsIncrement) ? newItemsSize : 1, movedItemsSize - 1, oldItemsToMoveBack);

            // set the starting index for the items
            // basically here, we are subtracting the two ranges of _items and newItems
            if (newItemsIncrement == movedItemsIncrement)
            {
                // add the new items which should start after the end of the current moved items
                startIndexForAddMovedItems = movedItemsSize;
            }
            else
            {
                // drag index was already added so no need to go over it again
                startIndexForAddMovedItems = 1;
            }
        }
        else
        {
            // this is the first time we enter this function
            // start from the drag index (located at array index 0)
            startIndexForAddMovedItems = 0;
        }

        // add the new moved items
        if (startIndexForAddMovedItems < newItemsSize)
        {
            AddMovedItems(isOrientationVertical, startIndexForAddMovedItems, newItems, newItemsToMove);
        }
    }

    public void RemoveMovedItems(int from, int to, IList<MovedItem> oldItemsToMoveBack)
    {
        for (int i = to; i >= from; --i)
        {
            oldItemsToMoveBack.Add(_items[i]);
            _items.RemoveAt(i);
        }
    }

    public void AddMovedItems(bool isOrientationVertical, int from,
        IList<MovedItem> newItems, IList<MovedItem> newItemsToMove)
    {
        // move the new indexes that have not been moved yet
        for (int i = from; i < newItems.Count; i++)
        {
            var newItem = newItems[i];

            // if moved items is empty, this means that the first item is the dragged item
            // we simply add it to the list
            if (_items.Count > 0)
            {
                // find the item whose sourceIndex is the destinationIndex of the new item to be moved
                var destination = _items[^1];

                bool forward = (newItem.sourceIndex > newItem.destinationIndex) ? true : false;

                // if the items are of the same size, use the destinations original location
                // else if the destination is the sourceIndex of the first moved item (meaning it's the dragged item), use its original location
                // otherwise, we use the destination's current (moved) location
                if (newItem.sourceRect.Width == destination.sourceRect.Width && newItem.sourceRect.Height == destination.sourceRect.Height)
                {
                    newItem.destinationRect = new Rect(destination.sourceRect.X,
                        destination.sourceRect.Y, newItem.destinationRect.Width,
                        newItem.destinationRect.Height);
                }
                else if (destination.sourceIndex == _items[0].sourceIndex)
                {
                    newItem.destinationRect = new Rect(destination.sourceRect.X,
                        destination.sourceRect.Y, newItem.destinationRect.Width,
                        newItem.destinationRect.Height);

                    // if they're of different sizes, account for the difference of size

                    if (!forward)
                    {
                        if (isOrientationVertical)
                        {
                            newItem.destinationRect = newItem.destinationRect.WithY(
                                newItem.destinationRect.Y - newItem.sourceRect.Height - destination.sourceRect.Height);

                            if (newItem.destinationRect.Y < 0)
                            {
                                newItem.destinationRect = new Rect(newItem.sourceRect.X,
                                    newItem.sourceRect.Y + newItem.sourceRect.Height,
                                    newItem.destinationRect.Width, newItem.destinationRect.Height);
                            }
                        }
                        else
                        {
                            newItem.destinationRect = newItem.destinationRect.WithX(
                                newItem.destinationRect.X - newItem.sourceRect.Width - destination.sourceRect.Width);

                            if (newItem.destinationRect.X < 0)
                            {
                                newItem.destinationRect = new Rect(newItem.sourceRect.X + newItem.sourceRect.Width,
                                    newItem.sourceRect.Y, newItem.destinationRect.Width, newItem.destinationRect.Height);
                            }
                        }
                    }
                }
                else
                {
                    newItem.destinationRect = new Rect(destination.destinationRect.X,
                        destination.destinationRect.Y, newItem.destinationRect.Width,
                        newItem.destinationRect.Height);

                    // we should offset the location by the size of the destination in case we're moving forward (sourceIndex > destinationIndex)
                    // otherwise, we subtract the size of the item itself
                    if (isOrientationVertical)
                    {
                        if (forward)
                        {
                            newItem.destinationRect = newItem.destinationRect.WithY(
                                newItem.destinationRect.Y + destination.sourceRect.Height);
                        }
                        else
                        {
                            newItem.destinationRect = newItem.destinationRect.WithY(
                                newItem.destinationRect.Y - newItem.sourceRect.Height);
                        }
                    }
                    else
                    {
                        if (forward)
                        {
                            newItem.destinationRect = newItem.destinationRect.WithX(
                                newItem.destinationRect.X + destination.sourceRect.Width);
                        }
                        else
                        {
                            newItem.destinationRect = newItem.destinationRect.WithX(
                                newItem.destinationRect.X - newItem.sourceRect.Width);
                        }
                    }
                }

                // set the width and height to its original size
                newItem.destinationRect = new Rect(newItem.destinationRect.X,
                    newItem.destinationRect.Y,
                    newItem.sourceRect.Width, newItem.sourceRect.Height);

                // add the item to the lists of items to be moved
                newItemsToMove.Add(newItem);
                // Just ensure we keep the lists accurate incase we need this later
                // since this is a struct in a list
                newItems[i] = newItem;
            }

            _items.Add(newItem);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public ReadOnlySpan<MovedItem> AsSpan() =>
        CollectionsMarshal.AsSpan(_items);

    public IEnumerator<MovedItem> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        _items.GetEnumerator();

    private readonly List<MovedItem> _items = new List<MovedItem>();
}
