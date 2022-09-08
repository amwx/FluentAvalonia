using Avalonia.Collections;
using FluentAvalonia.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentAvalonia.UI.Data;

internal class CollectionViewGroup : ICollectionViewGroup
{
    public CollectionViewGroup(GroupedDataCollectionView owner, object item, PropertyPath resolver)
    {
        _owner = owner;

        Group = item;

        if (resolver == null)
        {
            GroupItems = new CollectionWrapper(item as IEnumerable);
        }
        else
        {
            var items = resolver.ResolvePath(item);
            if (!(items is IEnumerable))
                throw new InvalidOperationException($"Property Path {resolver.Path} does not resolve an IEnumerable");

            GroupItems = new CollectionWrapper(items as IEnumerable);
        }

        GroupItems.CollectionChanged += OnGroupItemsCollectionChanged;
    }

    public object Group { get; }
    public IAvaloniaList<object> GroupItems { get; }

    private void OnGroupItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        _owner.GroupItemsChanged(this, args);
    }

    private GroupedDataCollectionView _owner;
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
            else if (_collection is IList<object> genList)
            {
                genList[index] = value;
            }
        }
    }

    object IReadOnlyList<object>.this[int index] => this[index];

    public int Count => _collection.Count();

    public bool IsReadOnly => _collection is ICollection<object> col && col.IsReadOnly;

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

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnBackingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(sender, e);
    }

    public void Add(object item)
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
        {
            l.Add(item);
        }
        else if (_collection is IList<object> genL)
        {
            genL.Add(item);
        }
        else if (_collection is ICollection<object> col)
        {
            col.Add(item);
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
        else if (_collection is IList<object> genL)
        {
            foreach (var item in genL)
                genL.Add(item);
        }
        else if (_collection is ICollection<object> col)
        {
            foreach (var item in items)
                col.Add(item);
        }
    }

    public void Clear()
    {
        ThrowIfNotMutable();

        if (_collection is IList l)
            l.Clear();
        else if (_collection is ICollection<object> col) // Also covers IList<T>
            col.Clear();
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
            list.Insert(index, item);
        else if (_collection is IList<object> genList)
            genList.Insert(index, item);
        else if (_collection is ICollection<object> col)
            ThrowForMutableActionNotSupported();
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
        else if (_collection is IList<object> genList)
        {
            int idx = index;
            foreach (var item in items)
                genList.Insert(idx++, item);
        }
        else if (_collection is ICollection<object> col)
            ThrowForMutableActionNotSupported();
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
        if (_collection is IList l)
            l.Remove(item);
        else if (_collection is ICollection<object> col) // Also covers IList<T>
            col.Remove(item);

        ThrowIfNotMutable();
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
            l.RemoveAt(index);
        else if (_collection is IList<object> genList)
            genList.RemoveAt(index);
        else if (_collection is ICollection<object> col)
            ThrowForMutableActionNotSupported();
    }

    public void RemoveRange(int index, int count)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        _collection.GetEnumerator();

    private void ThrowIfNotMutable()
    {
        if (IsReadOnly || _collection is IList || _collection is IList<object> || _collection is ICollection<object>)
            return;

        throw new NotSupportedException("Collection is not mutable");
    }

    private void ThrowForMutableActionNotSupported([CallerMemberName] string caller = null)
    {
        throw new NotSupportedException($"Collection of type {_collection.GetType()} does not support the {caller} action");
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
        throw new NotImplementedException();
    }

    private IEnumerable _collection;
}
