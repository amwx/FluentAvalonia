// Sorting & Filtering adapted from the WindowsCommunityToolkit
// AdvancedCollectionView - MIT license

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Data;

public sealed class IterableCollectionView : ICollectionView, IAdvancedCollectionView, IList, IComparer<object>
{
    public IterableCollectionView(IEnumerable collection)
        : this(collection, false, null, null, null) { }

    public IterableCollectionView(IEnumerable collection, bool isLiveShaping)
        : this(collection, isLiveShaping, null, null, null) { }

    public IterableCollectionView(IEnumerable collection, Predicate<object> filter)
        : this(collection, false, filter, null, null) { }

    public IterableCollectionView(IEnumerable collection, Predicate<object> filter,
        IList<string> filterProperties)
        : this(collection, true, filter, filterProperties, null) { }

    public IterableCollectionView(IEnumerable collection, IList<SortDescription> sortDescriptions)
        : this(collection, false, null, null, sortDescriptions) { }

    public IterableCollectionView(IEnumerable collection, bool isLiveShaping,
        Predicate<object> filter, IList<string> filterProperties,
        IList<SortDescription> sortDescriptions)
    {
        collection = collection ?? throw new ArgumentNullException(nameof(collection));

        _source = collection;
        _sourceView = ItemsSourceView.GetOrCreate(collection);
        _sourceView.CollectionChanged += SourceCollectionChanged;

        IsLiveShapingEnabled = isLiveShaping;

        // _view is only initialized if absolutely necessary
        // - Either filter or sort is specified here
        // - isLiveShaping is true, user may not have set filter/sort yet
        if (filter != null || sortDescriptions != null || isLiveShaping)
        {
            _hasFilterOrSort = true;
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

            _view = new List<object>(_sourceView.Count);
            AttachPropertyChangedHandler(_source);

            HandleSourceChanged();
            OnPropertyChanged();
        }
    }

    public bool IsLiveShapingEnabled { get; }

    public Predicate<object> Filter
    {
        get => _filter;
        set
        {
            // Don't run an equality check, always update - otherwise we end up in a situation
            // where you have to clear the filter and reapply it for something like a binding
            // the filter to text input - and that's not ideal
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

    public object this[int index]
    {
        get => _view != null ? _view[index] : _sourceView[index];
        set
        {
            if (!(_source is IList))
                ThrowForNonMutableSource();

            ((IList)_source)[index] = value;
        }
    }

    public int Count => _view != null ? _view.Count : _sourceView.Count;

    public int CurrentPosition { get; private set; }

    public bool HasMoreItems { get; private set; }

    public bool IsCurrentAfterLast => CurrentPosition >= Count;

    public bool IsCurrentBeforeFirst => CurrentPosition < 0;

    public object CurrentItem
    {
        get
        {
            var max = _view?.Count ?? _sourceView.Count;
            var pos = CurrentPosition;
            if (pos < 0 || pos >= max)
                return null;

            if (_view != null)
                return _view[pos];
            else
                return _sourceView[pos];
        }
    }

    public bool IsReadOnly => _source is IList l ? l.IsReadOnly : false;

    internal IEnumerable Source => _source;

    public event EventHandler<object> CurrentChanged;
    public event CurrentChangingEventHandler CurrentChanging;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;
    public void Add(object item) => Insert(Count, item);

    public void Clear()
    {
        if (IsReadOnly || !(_source is IList))
            ThrowForNonMutableSource();

        (_source as IList).Clear();
    }

    public bool Contains(object item) =>
        _view?.Contains(item) ?? _source.Contains(item);

    public void CopyTo(object[] array, int arrayIndex)
    {
        if (_view != null)
        {
            _view.CopyTo(array, arrayIndex);
        }
        else
        {
            var en = _source.GetEnumerator();
            while (en.MoveNext())
            {
                array[arrayIndex++] = en.Current;
            }
        }
    }

    public IEnumerator<object> GetEnumerator()
    {
        static IEnumerator<object> Enumerate(IEnumerable items)
        {
            if (items == null)
                yield break;

            foreach (var item in items)
                yield return item;
        }

        if (_view != null)
        {
            return _view.GetEnumerator();
        }
        else
        {
            return Enumerate(_source);
        }
    }

    public int IndexOf(object item)
    {
        if (_view != null)
        {
            return _view.IndexOf(item);
        }
        else
        {
            return _source.IndexOf(item);
        }
    }

    public void Insert(int index, object item)
    {
        if (!(_source is IList) || IsReadOnly)
            ThrowForNonMutableSource();

        ((IList)_source).Insert(index, item);
    }

    public bool MoveCurrentTo(object item) =>
        item == CurrentItem || MoveCurrentToPosition(IndexOf(item));

    public bool MoveCurrentToFirst() =>
        MoveCurrentToPosition(Count > 0 ? 0 : -1);

    public bool MoveCurrentToLast() =>
        MoveCurrentToPosition(Count > 0 ? Count - 1 : -1);

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

    public bool Remove(object item)
    {
        if (IsReadOnly || !(_source is IList))
            ThrowForNonMutableSource();

        ((IList)_source).Remove(item);
        return true;
    }

    public void RemoveAt(int index)
    {
        if (IsReadOnly || !(_source is IList))
            ThrowForNonMutableSource();

        ((IList)_source).RemoveAt(index);
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        HandleSourceChanged();
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

    public IDisposable DeferRefresh()
    {
        _deferCounter++;
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
        _deferCounter--;

        if (_deferCounter == 0)
        {
            MoveCurrentTo(lastCurrentItem);
            Refresh();
        }
    }

    private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (_hasFilterOrSort)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AttachPropertyChangedHandler(args.NewItems);
                    if (_deferCounter <= 0)
                    {
                        if (args.NewItems.Count == 1)
                        {
                            HandleItemAdded(args.NewStartingIndex, args.NewItems[0]);
                        }
                        else
                        {
                            HandleSourceChanged();
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    DetachPropertyChangedHandler(args.OldItems);
                    if (_deferCounter <= 0)
                    {
                        if (args.OldItems.Count == 1)
                        {
                            HandleItemRemoved(args.OldStartingIndex, args.OldItems[0]);
                        }
                        else
                        {
                            HandleSourceChanged();
                        }
                    }
                    break;

                default:
                    HandleSourceChanged();
                    break;
            }
        }
        else
        {
            // Just a simple CollectionView, just propagate the args
            OnVectorChanged(args);
        }
    }

    private void OnSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (_deferCounter > 0)
            return;

        HandleSortChanged();
    }

    private void ItemOnPropertyChanged(object item, PropertyChangedEventArgs args)
    {
        if (!IsLiveShapingEnabled)
            return;

        var filterResult = _filter?.Invoke(item);

        if (filterResult.HasValue && _filterProperties.Contains(args.PropertyName))
        {
            var viewIndex = _view.IndexOf(item);
            if (viewIndex != -1 && !filterResult.Value)
            {
                RemoveFromView(viewIndex, item);
            }
            else if (viewIndex == -1 && filterResult.Value)
            {
                var index = _source.IndexOf(item);
                HandleItemAdded(index, item);
            }
        }

        if ((filterResult ?? true) && _sortDescriptions?.Any(sd => sd.PropertyName == args.PropertyName) == true)
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
            HandleSourceChanged();
        }
    }

    private void AttachPropertyChangedHandler(IEnumerable items)
    {
        if (!IsLiveShapingEnabled || items == null)
        {
            return;
        }

        foreach (var item in items.OfType<INotifyPropertyChanged>())
        {
            item.PropertyChanged += ItemOnPropertyChanged;
        }
    }

    private void DetachPropertyChangedHandler(IEnumerable items)
    {
        if (!IsLiveShapingEnabled || items == null)
        {
            return;
        }

        foreach (var item in items.OfType<INotifyPropertyChanged>())
        {
            item.PropertyChanged -= ItemOnPropertyChanged;
        }
    }

    private void HandleSourceChanged()
    {
        var currentItem = CurrentItem;
        _view.Clear();

        foreach (var item in _source)
        {
            if (_filter != null && !_filter(item))
                continue;

            if (_sortDescriptions != null && _sortDescriptions.Count > 0)
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
        MoveCurrentTo(currentItem);
    }

    private void HandleSortChanged()
    {
        _view.Sort(this);
        OnVectorChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void HandleFilterChanged()
    {
        // If user doesn't specify Filter in the constructor _view is still null
        // here, so let's initialize it now
        _view ??= new List<object>();

        if (_filter != null)
        {
            for (int i = 0; i < _view.Count; i++)
            {
                var item = _view.ElementAt(i);
                if (_filter(item))
                    continue;

                RemoveFromView(i, item);
                i--;
            }

            var viewHash = new HashSet<object>(_view);
            var viewIndex = 0;
            for (int i = 0; i < _sourceView.Count; i++)
            {
                var item = _sourceView[i];
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
        else
        {
            Refresh();
        }
    }

    private bool HandleItemAdded(int newStartingIndex, object newItem, int? viewIndex = null)
    {
        if (_filter != null && !_filter(newItem))
            return false;

        var newViewIndex = _view.Count;

        if (_sortDescriptions != null && _sortDescriptions.Count > 0)
        {
            //_sortProperties.Clear();
            newViewIndex = _view.BinarySearch(newItem, this);
            if (newViewIndex < 0)
                newViewIndex = ~newViewIndex;
        }
        else if (_filter != null)
        {
            if (_source == null)
            {
                HandleSourceChanged();
                return false;
            }

            if (newStartingIndex == 0 || _view.Count == 0)
            {
                newViewIndex = 0;
            }
            else if (newStartingIndex == _sourceView.Count - 1)
            {
                newViewIndex = _view.Count - 1;
            }
            else if (viewIndex.HasValue)
            {
                newViewIndex = viewIndex.Value;
            }
            else
            {
                for (int i = 0, j = 0; i < _sourceView.Count; i++)
                {
                    if (i == newStartingIndex)
                    {
                        newViewIndex = j;
                        break;
                    }

                    if (_view[j] == _sourceView[i])
                    {
                        j++;
                    }
                }
            }
        }

        _view.Insert(newViewIndex, newItem);
        if (newViewIndex <= CurrentPosition)
        {
            CurrentPosition++;
        }

        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
            newItem, newViewIndex);
        OnVectorChanged(args);

        return true;
    }

    private void HandleItemRemoved(int index, object item)
    {
        if (_filter != null && !_filter(item))
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
        if (index <= CurrentPosition)
            CurrentPosition--;

        var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
        OnVectorChanged(args);
    }

    private void OnVectorChanged(NotifyCollectionChangedEventArgs args)
    {
        if (_deferCounter > 0)
            return;

        CollectionChanged?.Invoke(this, args);
        OnPropertyChanged(nameof(Count));
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private object EvaluateBinding(IBinding binding, object item)
    {
        _bindingHelper ??= new GroupedDataCollectionView.BindingHelper();

        return _bindingHelper.Evaluate(binding, item);
    }


    int IComparer<object>.Compare(object x, object y)
    {
        if (_sortDescriptions != null)
        {
            for (int i = 0; i < _sortDescriptions.Count; i++)
            {
                var desc = _sortDescriptions[i];
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


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IAvaloniaList<ICollectionViewGroup> ICollectionView.CollectionGroups => null;

    bool IList.IsFixedSize => _source is IList l && l.IsFixedSize;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => null;

    int ICollection.Count => Count;

    Task<LoadMoreItemsResult> ICollectionView.LoadMoreItemsAsync(uint count) =>
        throw new NotImplementedException();

    void IList.Insert(int index, object item) => Insert(index, item);

    int IList.Add(object item)
    {
        Add(item);
        return Count - 1;
    }

    void IList.Clear() => Clear();

    bool IList.Contains(object value) => Contains(value);

    void IList.Remove(object item) => Remove(item);

    void ICollection.CopyTo(Array array, int index) =>
        CopyTo((object[])array, index);

    private static void ThrowForNonMutableSource()
    {
        throw new NotSupportedException("Underlying source of type {_collection.GetType()} is not mutable. Source collection" +
            "must implement non-generic IList for CollectionView mutation");
    }

    private IList<SortDescription> _sortDescriptions;
    private Predicate<object> _filter;
    private IEnumerable _source;
    private ItemsSourceView _sourceView;
    private List<object> _view;
    private HashSet<string> _filterProperties;
    private int _deferCounter;
    private static GroupedDataCollectionView.BindingHelper _bindingHelper;
    private bool _hasFilterOrSort;
}
