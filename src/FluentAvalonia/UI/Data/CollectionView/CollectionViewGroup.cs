using Avalonia.Collections;
using Avalonia.Data;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FluentAvalonia.UI.Data;

internal class CollectionViewGroup : ICollectionViewGroup
{
    public CollectionViewGroup(GroupedDataCollectionView owner, object item, bool hasItemsBinding)
    {
        _owner = owner;
        Group = item;
        _hasItemsBinding = hasItemsBinding;

        Init();
    }

    public object Group { get; private set; }

    public IList<object> GroupItems { get; protected set; }

    internal virtual void UpdateGroup(object group)
    {
        Group = group;

        if (!_hasItemsBinding)
        {
            GroupItems = new CollectionWrapper(group as IEnumerable);
        }
        else
        {
            GroupItems = new CollectionWrapper(_owner.GetItemsFromGroup(group));
        }
    }

    protected virtual void Init()
    {
        if (!_hasItemsBinding)
        {
            GroupItems = new CollectionWrapper(Group as IEnumerable);
        }
        else
        {
            GroupItems = new CollectionWrapper(_owner.GetItemsFromGroup(Group));
        }

        (GroupItems as INotifyCollectionChanged).CollectionChanged += OnGroupItemsCollectionChanged;
    }

    protected virtual void OnGroupItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        _owner.GroupItemsChanged(this, args);
    }

    protected GroupedDataCollectionView _owner;
    protected bool _hasItemsBinding;
}

internal class SpecializedCollectionViewGroup : CollectionViewGroup, IComparer<object>
{
    public SpecializedCollectionViewGroup(GroupedDataCollectionView owner, object item, bool hasItemsBinding)
        : base(owner, item, hasItemsBinding)
    {

    }

    internal int HandleFilterChanged(Predicate<object> filter)
    {
        if (filter != null)
        {
            for (int i = 0; i < _view.Count; i++)
            {
                var item = _view.ElementAt(i);
                if (filter(item))
                    continue;

                RemoveFromView(i, item);
                i--;
            }

            var viewHash = new HashSet<object>(_view);
            var viewIndex = 0;
            for (int i = 0; i < _actualItems.Count(); i++)
            {
                var item = _actualItems.ElementAt(i);
                if (viewHash.Contains(item))
                {
                    viewIndex++;
                    continue;
                }

                if (HandleItemAdded(i, item, viewIndex))
                {
                    viewIndex++;
                }
            }
        }

        return _view.Count;
    }

    internal void HandleSortChanged()
    {
        _view.Sort(this);
        // Don't raise event here, this is part of a bulk action
    }

    internal int Refresh()
    {
        _view.Clear();
        var filter = _owner.Filter;
        var sortDesc = _owner.GetSortDescriptions();

        foreach (var item in _actualItems)
        {
            if (filter != null && !filter(item))
                continue;

            if (sortDesc != null && sortDesc.Count > 0)
            {
                var targetIndex = _view.BinarySearch(item, this);
                if (targetIndex < 0)
                    targetIndex = ~targetIndex;

                _view.Insert(targetIndex, item);
            }
            else
            {
                _view.Add(item);
            }
        }

        // Don't raise event here, this is part of a bulk action
        return _view.Count;
    }

    protected override void Init()
    {
        // With sorting/grouping, we want to maintain the list of actual group items so we don't
        // have to constantly evaluate the itemsbinding (if present)
        // So, we load the actual items into _actualItems, and _view will be mapped to GroupItems
        // which are the filtered/sorted items that are displayed
        if (!_hasItemsBinding)
        {
            _actualItems = new CollectionWrapper(Group as IEnumerable);
        }
        else
        {
            _actualItems = new CollectionWrapper(_owner.GetItemsFromGroup(Group));
        }

        (_actualItems as INotifyCollectionChanged).CollectionChanged += OnGroupItemsCollectionChanged;

        AttachPropertyChangedHandler(_actualItems);
        _view = new List<object>(_actualItems.Count());
        GroupItems = _view;
        SourceChanged();
    }

    internal override void UpdateGroup(object group)
    {
        DetachPropertyChangedHandler(_actualItems);

        // We're already in the middle of a collection change notification from this group
        // as this is called when a Reset action takes place. Don't notify the CollectionView
        // again or bad things may happen
        _ignoreNotify = true;
        Init();
        _ignoreNotify = false;
    }

    protected override void OnGroupItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    AttachPropertyChangedHandler(args.NewItems);
                    if (_owner.DeferCounter <= 0)
                    {
                        if (args.NewItems.Count == 1)
                        {
                            HandleItemAdded(args.NewStartingIndex, args.NewItems[0]);
                        }
                        else
                        {
                            SourceChanged();
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    DetachPropertyChangedHandler(args.OldItems);
                    if (_owner.DeferCounter <= 0)
                    {
                        if (args.OldItems.Count == 1)
                        {
                            HandleItemRemoved(args.OldStartingIndex, args.OldItems[0]);
                        }
                        else
                        {
                            SourceChanged();
                        }
                    }
                }
                break;

            default:
                SourceChanged();
                break;
        }
    }

    private void AttachPropertyChangedHandler(IEnumerable items)
    {
        if (!_owner.IsLiveShapingEnabled || items == null)
            return;

        foreach (var item in items.OfType<INotifyPropertyChanged>())
        {
            item.PropertyChanged += ItemOnPropertyChanged;
        }
    }

    private void DetachPropertyChangedHandler(IEnumerable items)
    {
        if (!_owner.IsLiveShapingEnabled || items == null)
            return;

        foreach (var item in items.OfType<INotifyPropertyChanged>())
        {
            item.PropertyChanged -= ItemOnPropertyChanged;
        }
    }

    private void ItemOnPropertyChanged(object item, PropertyChangedEventArgs args)
    {
        if (!_owner.IsLiveShapingEnabled)
            return;

        var filter = _owner.Filter;
        var filterProps = _owner.GetFilterProperties();
        var sortDesc = _owner.GetSortDescriptions();

        var filterResult = filter?.Invoke(item);

        if (filterResult.HasValue && filterProps.Contains(args.PropertyName))
        {
            var viewIndex = _view.IndexOf(item);
            if (viewIndex != -1 && !filterResult.Value)
            {
                RemoveFromView(viewIndex, item);
            }
            else if (viewIndex == -1 && filterResult.Value)
            {
                var index = _actualItems.IndexOf(item);
                HandleItemAdded(index, item);
            }
        }

        if ((filterResult ?? true) && sortDesc?.Any(sd => sd.PropertyName == args.PropertyName) == true)
        {
            var oldIndex = _view.IndexOf(item);

            // Check if item is in view:
            if (oldIndex < 0)
            {
                return;
            }

            _view.RemoveAt(oldIndex);
            var targetIndex = _view.BinarySearch(item, this);
            if (targetIndex < 0)
            {
                targetIndex = ~targetIndex;
            }

            // Only trigger expensive UI updates if the index really changed:
            if (targetIndex != oldIndex)
            {
                OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, oldIndex));

                _view.Insert(targetIndex, item);

                OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, targetIndex));
            }
            else
            {
                _view.Insert(targetIndex, item);
            }
        }
        else if (string.IsNullOrEmpty(args.PropertyName))
        {
            SourceChanged();
        }
    }

    private void SourceChanged()
    {
        _view.Clear();

        var filter = _owner.Filter;
        var sortDesc = _owner.GetSortDescriptions();

        foreach (var item in _actualItems)
        {
            if (filter != null && !filter(item))
                continue;

            if (sortDesc != null && sortDesc.Count > 0)
            {
                var targetIndex = _view.BinarySearch(item, this);
                if (targetIndex < 0)
                    targetIndex = ~targetIndex;

                _view.Insert(targetIndex, item);
            }
            else
            {
                _view.Add(item);
            }
        }

        OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnVectorChanged(NotifyCollectionChangedEventArgs args)
    {
        if (_ignoreNotify)
            return;

        // if (_owner.DeferCounter > 0)
        //     return;

        _owner.GroupItemsChanged(this, args);
    }

    private bool HandleItemAdded(int newStartingIndex, object newItem, int? viewIndex = null)
    {
        var filter = _owner.Filter;
        var sortDesc = _owner.GetSortDescriptions();
        if (filter != null && !filter(newItem))
            return false;

        var newViewIndex = _view.Count;

        if (sortDesc != null && sortDesc.Count > 0)
        {
            //_sortProperties.Clear();
            newViewIndex = _view.BinarySearch(newItem, this);
            if (newViewIndex < 0)
                newViewIndex = ~newViewIndex;
        }
        else if (filter != null)
        {
            if (_actualItems == null)
            {
                SourceChanged();
                return false;
            }

            if (newStartingIndex == 0 || _view.Count == 0)
            {
                newViewIndex = 0;
            }
            else if (newStartingIndex == _actualItems.Count() - 1)
            {
                newViewIndex = _view.Count - 1;
            }
            else if (viewIndex.HasValue)
            {
                newViewIndex = viewIndex.Value;
            }
            else
            {
                for (int i = 0, j = 0; i < _actualItems.Count(); i++)
                {
                    if (i == newStartingIndex)
                    {
                        newViewIndex = j;
                        break;
                    }

                    if (_view[j] == _actualItems.ElementAt(i))
                    {
                        j++;
                    }
                }
            }
        }

        _view.Insert(newViewIndex, newItem);
        //if (newViewIndex <= CurrentPosition)
        //{
        //    CurrentPosition++;
        //}

        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
            newItem, newViewIndex);
        OnVectorChanged(args);

        return true;
    }

    private void HandleItemRemoved(int index, object item)
    {
        var filter = _owner.Filter;
        if (filter != null && !filter(item))
        {
            return;
        }

        if (index < 0 || index >= _view.Count || !Equals(_view[index], item))
        {
            index = _view.IndexOf(item);
        }

        if (index < 0)
        {
            return;
        }

        RemoveFromView(index, item);
    }

    private void RemoveFromView(int index, object item)
    {
        _view.RemoveAt(index);

        //if (index <= CurrentPosition)
        //    CurrentPosition--;

        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
        OnVectorChanged(args);
    }

    int IComparer<object>.Compare(object x, object y)
    {
        var sortDesc = _owner.GetSortDescriptions();
        if (sortDesc != null)
        {
            for (int i = 0; i < sortDesc.Count; i++)
            {
                var desc = sortDesc[i];
                object cx, cy;

                if (desc.Property == null)
                {
                    cx = x;
                    cy = y;
                }
                else
                {
                    cx = EvaluateBinding(desc.Property, x);
                    cy = EvaluateBinding(desc.Property, y);
                }

                var cmp = desc.Comparer.Compare(cx, cy);
                if (cmp != 0)
                    return desc.Direction == SortDirection.Ascending ? cmp : -cmp;
            }
        }

        return 0;
    }

    private static object EvaluateBinding(IBinding binding, object item)
    {
        _bindingHelper ??= new GroupedDataCollectionView.BindingHelper();

        return _bindingHelper.Evaluate(binding, item);
    }

    private List<object> _view;
    private IEnumerable _actualItems;
    private static GroupedDataCollectionView.BindingHelper _bindingHelper;
    private bool _ignoreNotify;
}

internal class CollectionWrapper : IAvaloniaList<object>, IList // IList for INCC compatibility
{
    public CollectionWrapper(IEnumerable collection)
    {
        _collection = collection;

        if (collection is INotifyCollectionChanged incc)
            incc.CollectionChanged += OnBackingCollectionChanged;
    }

    public object this[int index]
    {
        get => _collection.ElementAt(index);
        set
        {
            ThrowIfNotMutable();

            if (_collection is IList list)
            {
                list[index] = value;
            }
        }
    }

    public int Count => _collection.Count();

    public bool IsReadOnly => _collection is ICollection<object> col && col.IsReadOnly;

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnBackingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    public void Add(object item)
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
        {
            l.Add(item);
        }
    }

    public void AddRange(IEnumerable<object> items)
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
        {
            foreach (var item in items)
                l.Add(item);
        }
    }

    public void Clear()
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
            l.Clear();
    }

    public bool Contains(object item) =>
        _collection.Contains(item);

    public void CopyTo(object[] array, int arrayIndex)
    {
        if (_collection is ICollection list)
        {
            list.CopyTo(array, arrayIndex);
        }
        else
        {
            // I hope this is never needed
            Enumerable.ToList<object>(_collection.Cast<object>()).CopyTo(array, arrayIndex);
        }
    }

    public IEnumerator<object> GetEnumerator()
    {
        if (_collection is IEnumerable<object> obj)
            return obj.GetEnumerator();

        return _collection.Cast<object>().GetEnumerator();
    }

    public int IndexOf(object item) =>
        _collection.IndexOf(item);

    public void Insert(int index, object item)
    {
        ThrowIfNotMutable();

        if (_collection is IList list)
        {
            list.Insert(index, item);
        }
    }

    public void InsertRange(int index, IEnumerable<object> items)
    {
        ThrowIfNotMutable();

        if (_collection is IList list)
        {
            int idx = index;
            foreach (var item in items)
                list.Insert(idx++, item);
        }
    }

    public void Move(int oldIndex, int newIndex)
    {
        throw new NotImplementedException();
    }

    public void MoveRange(int oldIndex, int count, int newIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(object item)
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
            l.Remove(item);

        return false;
    }

    public void RemoveAll(IEnumerable<object> items)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
        {
            l.RemoveAt(index);
        }
    }

    public void RemoveRange(int index, int count)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        _collection.GetEnumerator();

    private void ThrowIfNotMutable()
    {
        // We can't modify the CollectionViewGroup items if it's not able to notify,
        // even if the underlying type is something like a List
        if (!IsReadOnly || _collection is INotifyCollectionChanged)
            return;

        throw new NotSupportedException("Collection is not mutable. Collection groups must implement INotifyCollectionChanged");
    }

    object IReadOnlyList<object>.this[int index] => this[index];

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => IsReadOnly;

    int ICollection.Count => Count;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => false;

    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    int IList.Add(object value)
    {
        Add(value);
        return _collection.Count();
    }

    void IList.Clear()
    {
        Clear();
    }

    bool IList.Contains(object value) =>
        Contains(value);

    int IList.IndexOf(object value) =>
        IndexOf(value);

    void IList.Insert(int index, object value) =>
        Insert(index, value);

    void IList.Remove(object value) =>
        Remove(value);

    void IList.RemoveAt(int index) =>
        RemoveAt(index);

    void ICollection.CopyTo(Array array, int index)
    {
        CopyTo((object[])array, index);
    }

    private IEnumerable _collection;
}
