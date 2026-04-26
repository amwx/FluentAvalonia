// Sorting & Filtering adapted from the WindowsCommunityToolkit
// AdvancedCollectionView - MIT license

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Collections.Pooled;
using Avalonia.Data;
using Avalonia.Logging;

namespace FluentAvalonia.UI.Data;

public sealed class GroupedDataCollectionView : ICollectionView, IAdvancedCollectionView, IList
{
    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding = null)
        : this(collection, itemsBinding, false, null, null, null) { }

    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding,
        bool isLiveShaping)
        : this(collection, itemsBinding, isLiveShaping, null, null, null) { }

    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding,
        Predicate<object> filter)
        : this(collection, itemsBinding, false, filter, null, null) { }

    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding,
        Predicate<object> filter, IList<string> filterProperties)
        : this(collection, itemsBinding, true, filter, filterProperties, null) { }

    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding,
       IList<SortDescription> sortDescriptions)
        : this(collection, itemsBinding, false, null, null, sortDescriptions) { }

    public GroupedDataCollectionView(IEnumerable collection, IBinding itemsBinding,
        bool isLiveShaping,
        Predicate<object> filter, IList<string> filterProperties,
        IList<SortDescription> sortDescriptions)
    {
        collection = collection ?? throw new ArgumentNullException(nameof(collection));

        _source = collection;
        _itemsBinding = itemsBinding;

        _hasSortOrFilter = isLiveShaping || filter != null || sortDescriptions != null;

        if (_hasSortOrFilter)
        {
            IsLiveShapingEnabled = isLiveShaping;
            _filter = filter;

            if (isLiveShaping)
            {
                _filterProperties = filterProperties != null ? new HashSet<string>(filterProperties) :
                    new HashSet<string>();
            }

            if (sortDescriptions != null)
            {
                var l = new AvaloniaList<SortDescription>(sortDescriptions);
                l.CollectionChanged += OnSortDescriptionsChanged;
                _sortDescriptions = l;
            }
        }

        // If sorting or filtering is enabled, this will handle the remainder of work
        CreateGroups();

        if (collection is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += OnBackingCollectionChanged;
        }
    }

    public IAvaloniaList<ICollectionViewGroup> CollectionGroups { get; private set; }

    public int CurrentPosition { get; private set; }

    public bool IsCurrentAfterLast => CurrentPosition >= Count;

    public bool IsCurrentBeforeFirst => CurrentPosition < 0;

    public int Count => _count;

    public bool IsReadOnly => _source is IList l && l.IsReadOnly;

    public object CurrentItem => GetItemAtIndex(CurrentPosition);

    public object this[int index]
    {
        get => GetItemAtIndex(index);
        set => ThrowICollectionViewNotMutableWhenGrouping();
    }

    public bool IsLiveShapingEnabled { get; }

    public Predicate<object> Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            HandleFilterChanged();
        }
    }

    public IList<SortDescription> SortDescriptions
    {
        get
        {
            if (_sortDescriptions == null)
            {
                var l = new AvaloniaList<SortDescription>();
                l.CollectionChanged += OnSortDescriptionsChanged;
                _sortDescriptions = l;
            }

            return _sortDescriptions;
        }
    }

    internal int DeferCounter { get; private set; }

    internal IEnumerable Source => _source;

    internal IBinding ItemsBinding => _itemsBinding;

    public event EventHandler<object> CurrentChanged;
    public event CurrentChangingEventHandler CurrentChanging;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

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

    public bool Contains(object item) => IndexOf(item) != -1;

    public void CopyTo(object[] array, int arrayIndex)
    {
        try
        {
            var en = GetEnumerator();
            while (en.MoveNext())
            {
                array[arrayIndex++] = en.Current;
            }
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, "CollectionView")?
                .Log("CollectionView", "Unable to copy source collection to array", ex);
        }
    }

    public IEnumerator<object> GetEnumerator() => new GroupEnumerator(this);

    public IDisposable DeferRefresh()
    {
        DeferCounter++;
        return new RefreshDeferer(ReleaseDefer, CurrentItem);
    }

    internal void UpdateViewFromCollectionViewSource(Predicate<object> filter, IList<string> filterProperties,
        IList<SortDescription> sortDescriptions)
    {
        using var defer = DeferRefresh();
                
        if (filterProperties != null)
        {
            _filterProperties.Clear();
            foreach (var prop in filterProperties)
            {
                AddFilterProperty(prop);
            }
        }

        Filter = filter;

        _sortDescriptions?.Clear();
        if (sortDescriptions != null)
        {
            foreach (var item in sortDescriptions)
            {
                SortDescriptions.Add(item);
            }
        }
    }

    private void ReleaseDefer(object lastCurrentItem)
    {
        DeferCounter--;

        if (DeferCounter == 0)
        {
            MoveCurrentTo(lastCurrentItem);
            Refresh();
        }
    }

    private object GetItemAtIndex(int index)
    {
        if (index == -1)
            return null;

        int idx = 0;
        for (int i = 0; i < CollectionGroups.Count; i++)
        {
            var g = CollectionGroups[i];
            int max = idx + g.GroupItems.Count;

            if (index < max)
            {
                return g.GroupItems[index - idx];
            }

            idx = max;
        }

        return null;
    }

    private void CreateGroups()
    {
        bool useSpecialized = _hasSortOrFilter;
        var groups = new List<CollectionViewGroup>();

        _ignoreGroupChanges = true;
        var en = _source.GetEnumerator();
        while (en.MoveNext())
        {
            var cvg = useSpecialized ? new SpecializedCollectionViewGroup(this, en.Current, _itemsBinding != null) :
                new CollectionViewGroup(this, en.Current, _itemsBinding != null);
            groups.Add(cvg);
            _count += cvg.GroupItems.Count;
        }
        _ignoreGroupChanges = false;

        if (CollectionGroups == null)
        {
            // First time
            CollectionGroups = new AvaloniaList<ICollectionViewGroup>(groups);
        }
        else
        {
            // Source INCC reset event
            CollectionGroups.Clear();
            CollectionGroups.AddRange(groups);
        }
    }

    private void OnBackingCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        bool isSpecialized = _hasSortOrFilter;
        var groups = CollectionGroups;
        int dItems = 0;
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    var insertIndexInView = GetItemCountToIndex(groups, args.NewStartingIndex);
                    var list = new List<CollectionViewGroup>(args.NewItems.Count);

                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        var g = isSpecialized ?
                            new SpecializedCollectionViewGroup(this, args.NewItems[i], _itemsBinding != null) :
                            new CollectionViewGroup(this, args.NewItems[i], _itemsBinding != null);
                        dItems += g.GroupItems.Count;
                        list.Add(g);
                    }

                    groups.InsertRange(args.NewStartingIndex, list);

                    if (dItems == 0)
                        return;

                    _count += dItems;

                    IList<object> inccList = PopulateINCCList(args.NewStartingIndex, args.NewItems.Count, dItems);

                    OnVectorChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add, (IList)inccList, insertIndexInView));
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    var insertIndexInView = GetItemCountToIndex(CollectionGroups, args.OldStartingIndex);
                    dItems = GetItemCount(args.OldStartingIndex, args.OldItems.Count);

                    IList<object> inccList = PopulateINCCList(args.OldStartingIndex, args.OldItems.Count, dItems);

                    groups.RemoveRange(args.OldStartingIndex, args.OldItems.Count);

                    if (dItems > 0)
                    {
                        _count -= dItems;

                        OnVectorChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove, (IList)inccList, insertIndexInView));
                    }

                    if (inccList is IDisposable d)
                        d.Dispose();
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                {
                    var insertIndexInView = GetItemCountToIndex(groups, args.NewStartingIndex);
                    dItems = GetItemCount(args.OldStartingIndex, args.OldItems.Count);

                    IList<object> inccListOld = PopulateINCCList(args.OldStartingIndex, args.NewItems.Count, dItems);

                    _count -= dItems;

                    var list = new List<CollectionViewGroup>(args.NewItems.Count);
                    dItems = 0;
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        var g = isSpecialized ?
                            new SpecializedCollectionViewGroup(this, args.NewItems[i], _itemsBinding != null) :
                            new CollectionViewGroup(this, args.NewItems[i], _itemsBinding != null);
                        dItems += g.GroupItems.Count;
                        list.Add(g);
                    }

                    _count += dItems;
                    CollectionGroups.InsertRange(args.NewStartingIndex, list);
                    IList<object> inccListNew = null;

                    if (dItems > 0)
                    {
                        inccListNew = PopulateINCCList(args.NewStartingIndex, args.NewItems.Count, dItems);

                        OnVectorChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            (IList)inccListNew, (IList)inccListOld, insertIndexInView));
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                {
                    _count = 0;
                    CollectionGroups.Clear();
                    CreateGroups();

                    OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                break;

            case NotifyCollectionChangedAction.Move:
                {
                    var removeIndexInView = GetItemCountToIndex(CollectionGroups, args.OldStartingIndex);
                    dItems = GetItemCount(args.OldStartingIndex, args.OldItems.Count);
                    var inccList = PopulateINCCList(args.OldStartingIndex, args.OldItems.Count, dItems);

                    if (args.OldItems.Count == 1)
                    {
                        CollectionGroups.Move(args.OldStartingIndex, args.NewStartingIndex);
                    }
                    else
                    {
                        // MoveRange is really flaky and may not give the desired result
                        // it will fall apart with 1 item moves
                        //   new[] {0,1,2} --> MoveRange(0,1,2) -> {1,0,2}, but should be {1,2,0}
                        CollectionGroups.MoveRange(args.OldStartingIndex, args.OldItems.Count, args.NewStartingIndex);
                    }

                    var insertIndexInView = GetItemCountToIndex(CollectionGroups, args.NewStartingIndex);

                    OnVectorChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move, (IList)inccList,
                            insertIndexInView, removeIndexInView));
                }
                break;
        }

        IList<object> PopulateINCCList(int groupStart, int groupCount, int itemCount)
        {
            var l = new List<object>(itemCount);
            for (int i = groupStart; i < groupStart + groupCount; i++)
            {
                var g = CollectionGroups[i];
                if (g.GroupItems.Count == 0)
                    continue;

                l.AddRange(g.GroupItems);
            }

            return l;
        }

        int GetItemCount(int start, int count)
        {
            int ct = 0;
            for (int i = start; i < start + count; i++)
            {
                ct += CollectionGroups[i].GroupItems.Count;
            }

            return ct;
        }

        static int GetItemCountToIndex(IList<ICollectionViewGroup> groups, int index)
        {
            int ct = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                if (i == index)
                    break;

                ct += groups[i].GroupItems?.Count ?? 0;
            }

            return ct;
        }
    }

    internal IEnumerable GetItemsFromGroup(object group)
    {
        _helper ??= new BindingHelper();

        var items = _helper.Evaluate(_itemsBinding, group) as IEnumerable;

        if (items == null)
            throw new ArgumentException($"Unable to resolve items from group of type {group.GetType()}");

        return items;
    }

    internal void GroupItemsChanged(ICollectionViewGroup sender, NotifyCollectionChangedEventArgs args)
    {
        // With sorting/filtering, ignore events here when bulk operations are happening
        if (_ignoreGroupChanges)
            return;

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
                if (sender is CollectionViewGroup g)
                {
                    g.UpdateGroup(sender.Group);
                }

                // Because this is a reset, we have to recalculate the view count as we probably
                // don't have the info in the EventArgs, and the actual list is already cleared
                var ct = 0;
                for (int i = 0; i < CollectionGroups.Count; i++)
                {
                    ct += CollectionGroups[i].GroupItems.Count;
                }
                _count = ct;

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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }


    // SORTING & FILTERING 

    internal IList<SortDescription> GetSortDescriptions()
    {
        // The getter for the property will materialize the items
        // Therefore, we need a way to get the sort descriptions for SpecializedCollectionViewGroup
        // without materializing if there aren't any sort descriptions
        return _sortDescriptions;
    }

    internal HashSet<string> GetFilterProperties() => _filterProperties;

    public void Refresh()
    {
        var currentItem = CurrentItem;

        var groups = CollectionGroups;
        int count = 0;
        foreach (var g in groups)
        {
            count += (g as SpecializedCollectionViewGroup).Refresh();
        }

        _count = count;
        OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        MoveCurrentTo(currentItem);
    }

    public void RefreshFilter()
    {
        HandleFilterChanged();
    }

    public void RefreshSorting()
    {
        HandleSortChanged();
    }

    public void AddFilterProperty(string propertyName)
    {
        if (!IsLiveShapingEnabled)
            return;

        _filterProperties.Add(propertyName);
    }

    public void RemoveFilterProperty(string propertyName)
    {
        if (!IsLiveShapingEnabled)
            return;

        _filterProperties.Remove(propertyName);
    }

    public void ClearFilterProperties()
    {
        if (!IsLiveShapingEnabled)
            return;

        _filterProperties.Clear();
    }

    private void OnSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (DeferCounter > 0)
            return;

        HandleSortChanged();
    }

    private void HandleSortChanged()
    {
        try
        {
            _ignoreGroupChanges = true;

            var groups = CollectionGroups;
            foreach (var group in groups)
            {
                (group as SpecializedCollectionViewGroup).HandleSortChanged();
            }

            OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        finally
        {
            _ignoreGroupChanges = false;
        }
    }

    private void HandleFilterChanged()
    {
        try
        {
            _ignoreGroupChanges = true;

            if (_filter != null)
            {
                int count = 0;
                var groups = CollectionGroups;
                foreach (var group in groups)
                {
                    count += (group as SpecializedCollectionViewGroup).HandleFilterChanged(_filter);
                }

                // Now raise a collection wise reset
                _count = count;
                OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                Refresh();
            }
        }
        finally
        {
            _ignoreGroupChanges = false;
        }
    }

    private void OnVectorChanged(NotifyCollectionChangedEventArgs args)
    {
        CollectionChanged?.Invoke(this, args);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }



    // Editing the collection view is a no-op when grouping, these all throw

    public void Add(object item) => ThrowICollectionViewNotMutableWhenGrouping();

    public void Insert(int index, object item) => ThrowICollectionViewNotMutableWhenGrouping();

    public bool Remove(object item)
    {
        ThrowICollectionViewNotMutableWhenGrouping();
        return false;
    }

    public void RemoveAt(int index) => ThrowICollectionViewNotMutableWhenGrouping();

    public void Clear() => ThrowICollectionViewNotMutableWhenGrouping();

    bool IList.IsFixedSize => _source is IList l && l.IsFixedSize;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => null;

    int ICollection.Count => _count;

    Task<LoadMoreItemsResult> ICollectionView.LoadMoreItemsAsync(uint count) =>
        throw new NotImplementedException();

    bool ICollectionView.HasMoreItems => false;

    void IList.Insert(int index, object item) => ThrowICollectionViewNotMutableWhenGrouping();

    void IList.RemoveAt(int index) => ThrowICollectionViewNotMutableWhenGrouping();

    int IList.Add(object item)
    {
        ThrowICollectionViewNotMutableWhenGrouping();
        return -1;
    }

    void IList.Clear() => ThrowICollectionViewNotMutableWhenGrouping();

    bool IList.Contains(object value) => Contains(value);

    void IList.Remove(object value) => Remove(value);

    void ICollection.CopyTo(Array array, int index)
    {
        CopyTo((object[])array, index);
    }

    IEnumerator IEnumerable.GetEnumerator() => new GroupEnumerator(this);

    private static void ThrowICollectionViewNotMutableWhenGrouping()
    {
        throw new InvalidOperationException("CollectionView is not mutable when grouping. Edit the source collection or group lists instead");
    }

    private IEnumerable _source;
    private IBinding _itemsBinding;
    private int _count;
    private static BindingHelper _helper;
    private bool _hasSortOrFilter;
    private Predicate<object> _filter;
    private HashSet<string> _filterProperties;
    private IList<SortDescription> _sortDescriptions;
    private bool _ignoreGroupChanges;

    internal class BindingHelper : StyledElement
    {
        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<BindingHelper, object>("Value");

        public object Evaluate(IBinding binding, object dataContext)
        {
            dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            if (binding is null)
            {
                _lastBinding = null;
                return dataContext;
            }

            if (!dataContext.Equals(DataContext))
                DataContext = dataContext;

            if (_lastBinding != binding)
            {
                _lastBinding = binding;
                Bind(ValueProperty, binding);
            }

            return GetValue(ValueProperty);
        }

        private IBinding _lastBinding;
    }


    private struct GroupEnumerator : IEnumerator, IEnumerator<object>
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

            if (_lastGroupIndex == -1)
                _lastGroupIndex = 0;

            var g = _owner.CollectionGroups[_lastGroupIndex];

            _curPos++;

            // We've reached the end
            if (_curPos == g.GroupItems.Count && _lastGroupIndex == _owner.CollectionGroups.Count - 1)
            {
                Current = null;
                return false;
            }
            else if (_curPos == g.GroupItems.Count)
            {
                // We've reached the end of the current group, move to the next
                _curPos = 0;
                _lastGroupIndex++;
                g = _owner.CollectionGroups[_lastGroupIndex];
            }

            Current = g.GroupItems[_curPos];
            return true;
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
}
