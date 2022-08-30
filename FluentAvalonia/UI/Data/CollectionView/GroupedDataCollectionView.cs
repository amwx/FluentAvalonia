using Avalonia.Collections;
using FluentAvalonia.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Data;

// Add IList here to satisfy ItemsSourceView in Avalonia as it only checks for the non-generic IList
internal class GroupedDataCollectionView : ICollectionView, IList
{
    public GroupedDataCollectionView(IEnumerable collection, bool isGrouped, string itemsPath = null)
    {
        if (collection == null)
            throw new ArgumentNullException("Collection");

        _collection = collection;
        _isGrouped = isGrouped;
        if (isGrouped)
        {
            CreateGroups(collection, itemsPath);
        }
        else
        {
            _count = collection.Count();
        }

        if (collection is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += OnBackingCollectionChanged;
        }
    }

    // If you're following this class to write your own implementation of ICollectionView,
    // this property must be **null** if no grouping is used. This check is used internally
    // to determine whether the ICollectionView has groups.
    public IAvaloniaList<ICollectionViewGroup> CollectionGroups { get; private set; }

    public int CurrentPosition { get; private set; }

    public bool HasMoreItems { get; private set; }

    public bool IsCurrentAfterLast => CurrentPosition >= Count;

    public bool IsCurrentBeforeFirst => CurrentPosition < 0;

    public int Count => _count;

    int ICollection.Count => _count;

    public object CurrentItem
    {
        get
        {
            if (!_isGrouped)
                return CurrentPosition >= 0 && CurrentPosition < Count ? _collection.ElementAt(CurrentPosition) : null;

            return GetCurrentGrouped(CurrentPosition);
        }
    }

    public bool IsReadOnly => _collection is IReadOnlyCollection<object> || _collection is IList l && l.IsReadOnly
        || _collection is ICollection<object> col && col.IsReadOnly;

    public bool IsFixedSize => _collection is IList l && l.IsFixedSize;

    public bool IsSynchronized => throw new NotSupportedException();

    public object SyncRoot => throw new NotSupportedException();

    public object this[int index]
    {
        get => _isGrouped ? GetCurrentGrouped(index) : _collection.ElementAt(index);
        set
        {
            if (_isGrouped)
                throw new NotSupportedException("Grouped list is not mutable");

            if (_collection is IList l)
            {
                l.Add(value);
            }
            else if (_collection is IList<object> lGen)
            {
                lGen.Add(value);
            }

            throw new NotSupportedException("Collection is not a mutable list");
        }
    }

    public event EventHandler<object> CurrentChanged;
    public event CurrentChangingEventHandler CurrentChanging;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void OnBackingCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        // This fires if the collection holding the items source changes
        // In the event of grouping, this means the group collection changes
        // If a group is added, we only fire the collection changed if the group has items to add

        if (_isGrouped)
        {
            int GetItemCountToIndex(int index)
            {
                int ct = 0;
                for (int i = 0; i < CollectionGroups.Count; i++)
                {
                    if (i == index)
                        break;

                    ct += CollectionGroups[i].GroupItems?.Count ?? 0;
                }

                return ct;
            }

            // If grouping, we need to make sure to add the CollectionGroup and ensure the item count
            // is up to date. If a group is added, but has no items, no notification is sent
            int idx = 0;
            int itemCount = 0;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < args.NewStartingIndex; i++)
                    {
                        itemCount += CollectionGroups[i].GroupItems?.Count ?? 0;
                    }

                    // We've added a new group, create a new CollectionGroup & update view count
                    for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
                    {
                        var cg = new CollectionViewGroup(this, args.NewItems[idx++], _itemsPath);

                        CollectionGroups.Insert(i, cg);
                        _count += cg.GroupItems?.Count ?? 0;

                        if (cg.GroupItems == null || cg.GroupItems.Count == 0)
                            continue;

                        // If new items were added, let's raise a notification
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            cg.GroupItems as IList, itemCount));

                        itemCount += cg.GroupItems.Count;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < args.OldItems.Count + args.OldStartingIndex; i++)
                    {
                        itemCount += CollectionGroups[i].GroupItems?.Count ?? 0;
                    }

                    for (int i = args.OldStartingIndex + args.OldItems.Count - 1; i >= args.OldStartingIndex; i--)
                    {
                        var cg = CollectionGroups[i];

                        CollectionGroups.RemoveAt(i);

                        if (cg.GroupItems == null || cg.GroupItems.Count == 0)
                            continue;

                        _count -= cg.GroupItems.Count;

                        itemCount -= cg.GroupItems.Count;

                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            cg.GroupItems as IList, itemCount));
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionGroups.Clear();
                    _count = 0;
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                    // If this was a reset without clearing, we'll attempt to reset everything
                    // NOTE: This is untested...
                    CreateGroups(_collection, _itemsPath?.Path);

                    break;

                case NotifyCollectionChangedAction.Replace:
                    // Replacing a group requires special handling, because we need to fire 2 CollectionChanged
                    // notifications. One remove with all the items being removed by the old groups,
                    // and another with the new items being added. 

                    // Remove the items & fire the notification
                    List<object> remove = new List<object>();
                    int start = GetItemCountToIndex(args.OldStartingIndex);
                    for (int i = args.OldStartingIndex; i < args.OldStartingIndex + args.OldItems.Count; i++)
                    {
                        if (CollectionGroups[i].GroupItems != null)
                        {
                            itemCount += CollectionGroups[i].GroupItems.Count;
                            remove.AddRange(CollectionGroups[i].GroupItems);
                        }
                    }
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                        remove, start));

                    _count -= itemCount;

                    remove.Clear();
                    itemCount = 0;
                    // Now add the new group back in
                    for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
                    {
                        var newGroup = new CollectionViewGroup(this, args.NewItems[idx++], _itemsPath);
                        CollectionGroups[i] = newGroup;

                        if (newGroup.GroupItems != null)
                            remove.AddRange(newGroup.GroupItems);

                        itemCount += newGroup.GroupItems?.Count ?? 0;

                        if (newGroup.GroupItems == null || newGroup.GroupItems.Count == 0)
                            continue;
                    }

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                           remove, start));

                    _count += itemCount;
                    break;

                case NotifyCollectionChangedAction.Move:
                    // Moving doesn't adjust the overall item count, but we do need to reorder the CollectionGroups
                    // and propagate the move notification

                    for (int i = args.OldStartingIndex; i < args.OldStartingIndex + args.OldItems.Count; i++)
                    {
                        var group = CollectionGroups[i];
                        int oldIndex = GetItemCountToIndex(i);
                        int newIndex = GetItemCountToIndex(args.NewStartingIndex + idx);
                        CollectionGroups.Move(i, args.NewStartingIndex + idx);

                        if (group.GroupItems == null || group.GroupItems.Count == 0)
                        {
                            idx++;
                            continue;
                        }

                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                            group.GroupItems as IList,
                            newIndex, oldIndex));

                        idx++;
                    }
                    break;
            }

        }
        else
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _count += args.NewItems.Count;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _count -= args.OldItems.Count;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (args.OldItems != null)
                    {
                        _count -= args.OldItems.Count;
                    }
                    else
                    {
                        _count = _collection.Count();
                    }
                    break;

                    // Move/Replace don't modify count, no action required
            }

            CollectionChanged?.Invoke(this, args);
        }







        //int idx = 0;
        //switch (args.Action)
        //{
        //	case NotifyCollectionChangedAction.Add:
        //		if (_isGrouped) 
        //		{
        //			// A group has been added, we need to add a CollectionGroup and update the total count
        //			// If the group is not empty, we'll also propagate the CollectionChanged
        //			int tracker = 0;
        //			for (int i = 0; i < args.NewStartingIndex; i++)
        //			{
        //				tracker += CollectionGroups[i].GroupItems?.Count ?? 0;
        //			}

        //			for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
        //			{
        //				var g = new CollectionViewGroup(this, args.NewItems[idx++], _itemsPath);
        //				_count += g.GroupItems?.Count ?? 0;
        //				CollectionGroups.Insert(i, g);

        //				if (g.GroupItems != null && g.GroupItems.Count > 0)
        //				{
        //					CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
        //						g.GroupItems, tracker));

        //					tracker += g.GroupItems.Count;
        //				}
        //			}
        //		}
        //		break;

        //	case NotifyCollectionChangedAction.Remove:
        //		break;

        //	case NotifyCollectionChangedAction.Replace:
        //		break;

        //	case NotifyCollectionChangedAction.Reset:
        //		break;

        //	case NotifyCollectionChangedAction.Move:
        //		break;
        //}




        //if (_isGrouped)
        //{
        //	int idx = 0;
        //	int itemCount = 0;
        //	switch (args.Action)
        //	{
        //		case NotifyCollectionChangedAction.Add:
        //			for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
        //			{
        //				var g = new CollectionViewGroup(this, args.NewItems[idx++], _itemsPath);
        //				itemCount += g.GroupItems.Count;
        //				CollectionGroups.Insert(i, g);
        //			}

        //			_count += itemCount;
        //			break;

        //		case NotifyCollectionChangedAction.Remove:
        //			for (int i = args.OldStartingIndex + args.OldItems.Count - 1; i >= args.OldStartingIndex; i--)
        //			{
        //				_count -= CollectionGroups[i].GroupItems.Count;
        //				CollectionGroups.RemoveAt(i);
        //			}
        //			break;

        //		case NotifyCollectionChangedAction.Replace:
        //			for (int i = args.OldStartingIndex + args.OldItems.Count - 1; i >= args.OldStartingIndex; i--)
        //			{
        //				_count -= CollectionGroups[i].GroupItems.Count;
        //			}

        //			for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
        //			{
        //				var g = new CollectionViewGroup(this, args.NewItems[idx++], _itemsPath);
        //				itemCount += g.GroupItems.Count;
        //				CollectionGroups.Insert(i, g);
        //			}

        //			_count += itemCount;
        //			break;

        //		case NotifyCollectionChangedAction.Reset:
        //			break;

        //		case NotifyCollectionChangedAction.Move:
        //			break;
        //	}
        //}
        //else
        //{
        //	// If not grouping, just propagate the event
        //	CollectionChanged?.Invoke(sender, args);
        //}
    }

    internal void GroupItemsChanged(ICollectionViewGroup sender, NotifyCollectionChangedEventArgs args)
    {
        // We need to translate the index from the ICollectionViewGroup to the flattened collection
        // that the ICollectionView represents
        int TranslateGroupIndexToFlattenedIndex(int index)
        {
            var gIndex = CollectionGroups.IndexOf(sender);
            if (gIndex == -1)
                throw new ArgumentException("Invalid group index");

            if (gIndex == 0)
                return index;

            int count = 0;
            for (int i = 0; i < gIndex; i++)
            {
                count += CollectionGroups[i].GroupItems?.Count ?? 0;
            }

            return count + index;
        }

        NotifyCollectionChangedEventArgs newArgs = null;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _count += args.NewItems.Count;

                newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    args.NewItems, TranslateGroupIndexToFlattenedIndex(args.NewStartingIndex));
                break;

            case NotifyCollectionChangedAction.Remove:
                _count -= args.OldItems.Count;

                newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    args.OldItems, TranslateGroupIndexToFlattenedIndex(args.OldStartingIndex));
                break;

            case NotifyCollectionChangedAction.Reset:
                // Reset just marks the list invalid, OldItems may be null
                if (args.OldItems != null)
                {
                    // We're fortunate enough to get the old items, we can just subtract the count
                    _count -= args.OldItems.Count;
                }
                else
                {
                    // Since we don't know how many were removed or changed here, we need to loop over
                    // all the groups again and get the count
                    _count = 0;
                    for (int i = 0; i < CollectionGroups.Count; i++)
                    {
                        _count += CollectionGroups[i].GroupItems?.Count ?? 0;
                    }
                }

                newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                break;

            // Move and Replace actions don't modify the item counts, but we still need to translate the INCC args
            case NotifyCollectionChangedAction.Move:
                var oldIndex = TranslateGroupIndexToFlattenedIndex(args.OldStartingIndex);
                var newIndex = TranslateGroupIndexToFlattenedIndex(args.NewStartingIndex);

                newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                    args.NewItems, newIndex, oldIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                var index = TranslateGroupIndexToFlattenedIndex(args.NewStartingIndex);

                newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                    args.NewItems, args.OldItems, index);
                break;
        }

        CollectionChanged?.Invoke(sender, newArgs);
    }

    private void CreateGroups(IEnumerable collection, string itemsPath = null)
    {
        var groups = new List<CollectionViewGroup>();

        // ItemsPath is only respected with grouping
        if (!string.IsNullOrEmpty(itemsPath))
            _itemsPath = new PropertyPath(itemsPath);

        if (collection is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var cvg = new CollectionViewGroup(this, list[i], _itemsPath);
                groups.Add(cvg);
                _count += groups[groups.Count - 1].GroupItems?.Count ?? 0;
            }
        }
        else if (collection is IList<object> genList)
        {
            for (int i = 0; i < genList.Count; i++)
            {
                var cvg = new CollectionViewGroup(this, genList[i], _itemsPath);
                groups.Add(cvg);
                _count += groups[groups.Count - 1].GroupItems?.Count ?? 0;
            }
        }
        else
        {
            foreach (var item in collection)
            {
                var cvg = new CollectionViewGroup(this, item, _itemsPath);
                groups.Add(cvg);
                _count += groups[groups.Count - 1].GroupItems?.Count ?? 0;
            }
        }

        if (CollectionGroups == null) // First time
        {
            CollectionGroups = new AvaloniaList<ICollectionViewGroup>(groups);
        }
        else // Collection Reset (CollectionGroups should already be cleared)
        {
            CollectionGroups.AddRange(groups);
        }

        //CollectionGroups.CollectionChanged += (s, e) =>
        //{
        //	Debug.WriteLine("CollectionGroups changed");
        //};
    }

    private object GetCurrentGrouped(int index)
    {
        if (index == -1)
            return null;

        int tracker = 0;
        for (int i = 0; i < CollectionGroups.Count; i++)
        {
            if (CollectionGroups[i] is ICollectionViewGroup g)
            {
                if (index >= tracker + g.GroupItems.Count)
                {
                    tracker += g.GroupItems.Count;
                    continue;
                }

                int realIndex = index - tracker;

                return g.GroupItems[realIndex];
            }
        }

        return null;
    }

    public Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        throw new NotImplementedException();
    }

    public bool MoveCurrentTo(object item) =>
        MoveCurrentToPosition(IndexOf(item));

    public bool MoveCurrentToFirst() =>
        MoveCurrentToPosition(Count > 0 ? 0 : -1);

    public bool MoveCurrentToLast()
        => MoveCurrentToPosition(Count > 0 ? Count - 1 : -1);

    public bool MoveCurrentToNext() =>
        MoveCurrentToPosition(CurrentPosition + 1);

    public bool MoveCurrentToPrevious() =>
        MoveCurrentToPosition(CurrentPosition - 1);

    public bool MoveCurrentToPosition(int pos)
    {
        if (pos == CurrentPosition)
            return true;

        if (pos < 0 || pos >= Count)
            return false;

        var args = new CurrentChangingEventArgs();
        CurrentChanging?.Invoke(this, args);

        if (args.Cancel)
            return false;

        CurrentPosition = pos;
        CurrentChanged?.Invoke(this, null);

        return true;
    }

    public int IndexOf(object item)
    {
        if (!_isGrouped)
            return _collection.IndexOf(item);

        int index = 0;
        for (int i = 0; i < CollectionGroups.Count; i++)
        {
            var tmp = CollectionGroups[i].GroupItems.IndexOf(item);
            if (tmp != -1)
                return index + tmp;

            index += CollectionGroups[i].GroupItems.Count;
        }

        return -1;
    }

    void IList.Insert(int index, object item) => Insert(index, item);

    public void Insert(int index, object item)
    {
        if (_isGrouped)
            throw new NotImplementedException("Modifying a grouped CollectionView is not supported. Edit the source collection(s) instead."); // WinUI throws this when grouping

        if (_collection is IList<object> genList)
        {
            genList.Insert(index, item);
            _count++;
        }
        else if (_collection is IList list)
        {
            list.Insert(index, item);
            _count++;
        }
        else
            throw new NotSupportedException("Collection is not mutable");
    }

    void IList.RemoveAt(int index) => RemoveAt(index);

    public void RemoveAt(int index)
    {
        if (_isGrouped)
            throw new NotImplementedException("Modifying a grouped CollectionView is not supported. Edit the source collection(s) instead."); // WinUI throws this when grouping

        if (_collection is IList<object> genList)
        {
            genList.RemoveAt(index);
            _count--;
        }
        else if (_collection is IList list)
        {
            list.RemoveAt(index);
            _count--;
        }
        else
            throw new NotSupportedException("Collection is not mutable");
    }

    int IList.Add(object item)
    {
        Add(item);
        return _count;
    }

    public void Add(object item) =>
        Insert(_count, item);

    void IList.Clear() => Clear();

    public void Clear()
    {
        if (_isGrouped)
            throw new NotImplementedException(); // WinUI throws this when grouping

        if (_collection is IList<object> genList)
        {
            genList.Clear();
            _count = 0;
        }
        else if (_collection is IList list)
        {
            list.Clear();
            _count = 0;
        }
        else
            throw new NotSupportedException("Collection is not mutable");
    }

    bool IList.Contains(object value) => Contains(value);

    public bool Contains(object item) => IndexOf(item) != -1;

    public void CopyTo(object[] array, int arrayIndex) =>
        throw new NotImplementedException();

    void IList.Remove(object item) => Remove(item);

    public bool Remove(object item)
    {
        if (_isGrouped)
            throw new NotImplementedException("Modifying a grouped CollectionView is not supported. Edit the source collection(s) instead."); // WinUI throws this when grouping

        if (_collection is IList<object> genList)
        {
            _count--;
            return genList.Remove(item);
        }
        else if (_collection is IList list)
        {
            _count--;
            list.Remove(item);
            return true;
        }
        else
            throw new NotSupportedException("Collection is not mutable");
    }

    public IEnumerator<object> GetEnumerator()
    {
        if (!_isGrouped)
            return (_collection as IEnumerable<object>)?.GetEnumerator();

        return new GroupEnumerator(this);
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (!_isGrouped)
            return (_collection as IEnumerable).GetEnumerator();

        return new GroupEnumerator(this);
    }

    private IEnumerable _collection;
    private bool _isGrouped;
    private PropertyPath _itemsPath;
    private int _count;
}

internal class GroupEnumerator : IEnumerator, IEnumerator<object>
{
    public GroupEnumerator(GroupedDataCollectionView owner)
    {
        _owner = owner;
    }

    public object Current { get; private set; }

    public bool MoveNext()
    {
        int groupCount = _owner.CollectionGroups.Count;
        if (groupCount == 0)
            return false;

        if (_lastGroupIndex == groupCount - 1)
        {
            if (_owner.CollectionGroups[_lastGroupIndex] is ICollectionViewGroup g)
            {
                if (_curPos == g.GroupItems.Count - 1) //End of Collection
                {
                    Current = null;
                    return false;
                }
                else
                {
                    // While we're in the last group, handle this here...
                    _curPos++;
                    Current = g.GroupItems[_curPos];
                    return true;
                }
            }
        }
        else
        {
            // We're not in the last group, proceed as normal
            if (_lastGroupIndex == -1)
                _lastGroupIndex = 0;

            var g = _owner.CollectionGroups[_lastGroupIndex] as ICollectionViewGroup;
            if (g == null)
                return false;

            if (_curPos == g.GroupItems.Count - 1)
            {
                //move to the next group
                //Some groups may contain no items, so we need to search for it
                for (int i = _lastGroupIndex + 1; i < groupCount; i++)
                {
                    if (_owner.CollectionGroups[i] is ICollectionViewGroup nextG &&
                        g.GroupItems.Count > 0)
                    {
                        _lastGroupIndex = i;
                        g = nextG;
                        _curPos = -1;
                        break;
                    }
                }
            }

            _curPos++;
            Current = g.GroupItems[_curPos];
            return true;
        }

        return false;//Not sure why this is needed, the else should handle everything???
    }

    public void Reset()
    {
        _lastGroupIndex = -1;
        _curPos = -1;
        Current = null;
    }

    public void Dispose()
    {
        // N/A
    }

    private int _curPos = -1;
    private int _lastGroupIndex = -1;
    private GroupedDataCollectionView _owner;
}
