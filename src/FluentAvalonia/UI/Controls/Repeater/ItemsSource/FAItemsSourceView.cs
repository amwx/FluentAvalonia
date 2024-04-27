using Avalonia.Collections;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

// Source is combo of ItemsSourceView & InspectingDataSource

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

    public bool HasKeyIndexMapping =>
        HasKeyIndexMappingCore();

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public object GetAt(int index) =>
        GetAtCore(index);

    public string KeyFromIndex(int index) =>
        KeyFromIndexCore(index);

    public int IndexFromKey(string id) =>
        IndexFromKeyCore(id);

    public int IndexOf(object value) =>
        IndexOfCore(value);

    protected void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
    {
        _cachedSize = GetSizeCore();
        CollectionChanged?.Invoke(this, args);
    }

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

    protected virtual bool HasKeyIndexMappingCore() => 
        _uniqueIdMapping != null;

    protected string KeyFromIndexCore(int index)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.KeyFromIndex(index);

        throw new NotImplementedException();
    }

    protected virtual int IndexFromKeyCore(string id)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.IndexFromKey(id);

        throw new NotImplementedException();
    }

    protected virtual int IndexOfCore(object value)
    {
        int index = -1;
        if (_vector is IList list)
        {
            index = list.IndexOf(index);
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
