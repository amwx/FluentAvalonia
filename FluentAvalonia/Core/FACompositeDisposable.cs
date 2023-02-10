using System.Collections.Generic;
using System;
using System.Collections;

namespace FluentAvalonia.Core;

internal class FACompositeDisposable : ICollection<IDisposable>, IEnumerable<IDisposable>, IEnumerable, IDisposable
{
    public FACompositeDisposable()
    {
        _list = new List<IDisposable>();
    }

    public FACompositeDisposable(int capacity)
    {
        _list = new List<IDisposable>(capacity);
    }

    public FACompositeDisposable(params IDisposable[] disposables)
    {
        _list = new List<IDisposable>(disposables);
    }

    public FACompositeDisposable(IEnumerable<IDisposable> disposables)
    {
        _list = new List<IDisposable>(disposables);
    }

    public int Count => _list.Count;

    bool ICollection<IDisposable>.IsReadOnly => false;

    public void Add(IDisposable item) => _list.Add(item);

    public void Clear()
    {
        Dispose();
    }

    public bool Contains(IDisposable item) => _list.Contains(item);

    public void CopyTo(IDisposable[] array, int arrayIndex) =>
        _list.CopyTo(array, arrayIndex);

    public void Dispose()
    {
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            _list[i].Dispose();
            _list.RemoveAt(i);
        }
    }

    public IEnumerator<IDisposable> GetEnumerator() => _list.GetEnumerator();

    public bool Remove(IDisposable item)
    {
        var index = _list.IndexOf(item);
        if (index == -1)
            return false;

        _list.RemoveAt(index);

        item.Dispose();
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    private readonly List<IDisposable> _list;
}
