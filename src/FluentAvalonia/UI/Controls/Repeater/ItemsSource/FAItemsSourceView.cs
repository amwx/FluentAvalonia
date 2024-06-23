using Avalonia.Collections;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

// Source is combo of ItemsSourceView & InspectingDataSource

/// <summary>
/// Represents a standardized view of the supported interactions between a given ItemsSource object and an ItemsRepeater control.
/// </summary>
public class FAItemsSourceView
{
    public FAItemsSourceView(IEnumerable source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        _vector = source;
        ListenToCollectionChanges();

        _uniqueIdMapping = source as IKeyIndexMapping;
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count
    {
        get
        {
            if (_cachedSize == -1)
            {
                // Call the override the very first time. After this,
                // we can just update the size when there is a data source change.
                _cachedSize = GetSizeCore();
            }

            return _cachedSize;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the items source can provide a unique key for each item.
    /// </summary>
    public bool HasKeyIndexMapping =>
        HasKeyIndexMappingCore();

    /// <summary>
    /// Occurs when the collection has changed to indicate the reason for the change and which items changed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Retrieves the item at the specified index.
    /// </summary>
    public object GetAt(int index) =>
        GetAtCore(index);

    /// <summary>
    /// /// <summary>
    /// Retrieves the index of the item that has the specified unique identifier (key).
    /// </summary>
    public string KeyFromIndex(int index) =>
        KeyFromIndexCore(index);

    /// <summary>
    /// Retrieves the index of the item that has the specified unique identifier (key).
    /// </summary>
    public int IndexFromKey(string id) =>
        IndexFromKeyCore(id);

    /// <summary>
    /// Retrieves the index of the specified item.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int IndexOf(object value) =>
        IndexOfCore(value);

    /// <summary>
    /// Called when the ItemsSource has raised a CollectionChanged event
    /// </summary>
    /// <param name="args"></param>
    protected void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
    {
        _cachedSize = GetSizeCore();
        CollectionChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Gets the count of the underlying collection
    /// </summary>
    protected virtual int GetSizeCore()
    {
        if (_vector is IList list)
        {
            return list.Count;
        }
        else
        {
            return _vector.Count();
        }
    }

    /// <summary>
    /// Gets the item at the specified index from the underlying collection
    /// </summary>
    protected virtual object GetAtCore(int index)
    {
        if (_vector is IList list)
        {
            return list[index];
        }
        else
        {
            return _vector.ElementAt(index);
        }
    }

    /// <summary>
    /// Gets whether this underlying supports Key-Index mapping
    /// </summary>
    /// <returns></returns>
    protected virtual bool HasKeyIndexMappingCore() => 
        _uniqueIdMapping != null;

    /// <summary>
    /// Gets the key from the specified index
    /// </summary>
    protected string KeyFromIndexCore(int index)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.KeyFromIndex(index);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the Index from the specified key
    /// </summary>
    protected virtual int IndexFromKeyCore(string id)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.IndexFromKey(id);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Queries the underlying collection for the item at the specified index
    /// </summary>
    protected virtual int IndexOfCore(object value)
    {
        int index = -1;
        if (_vector is IList list)
        {
            index = list.IndexOf(value);
        }
        else
        {
            index = _vector.IndexOf(value);
        }

        return index;
    }

    private void UnListenToCollectionChanges()
    {
        _eventToken?.Dispose();
        _eventToken = null;
    }

    private void ListenToCollectionChanges()
    {
        if (_vector == null)
            throw new Exception("No source attached");

        if (_vector is INotifyCollectionChanged incc)
        {
            _eventToken = incc.GetWeakCollectionChangedObservable()
                .Subscribe(new SimpleObserver<NotifyCollectionChangedEventArgs>(OnCollectionChanged));
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        OnItemsSourceChanged(args);
    }

    private int _cachedSize = -1;
    private IEnumerable _vector;
    private IKeyIndexMapping _uniqueIdMapping;
    private IDisposable _eventToken;
}
