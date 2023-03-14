using System.Collections;

namespace FluentAvalonia.UI.Controls;

internal struct SelectedItemInfo
{
    public SelectedItemInfo(SelectionNode node, IndexPath path)
    {
        Node = new WeakReference<SelectionNode>(node);
        Path = path;
    }

    public WeakReference<SelectionNode> Node;
    public IndexPath Path;
}

internal class SelectedItems<T> : IReadOnlyList<T>
{
    public SelectedItems(IList<SelectedItemInfo> infos,
        Func<IList<SelectedItemInfo>, int, T> getAtImpl)
    {
        _infos = infos;
        _getAtImpl = getAtImpl;
        foreach(var info in infos)
        {
            if (info.Node.TryGetTarget(out var selNode))
            {
                _totalCount += selNode.SelectedCount;
            }
            else
            {
                throw new InvalidOperationException("Selection changed after the SelectedIndices/Items property was read");
            }
        }
    }

    public int Count => _totalCount;

    public int Size() => Count;

    public T this[int index] => GetAt(index);

    public T GetAt(int index) => _getAtImpl(_infos, index);

    public IEnumerator<T> GetEnumerator() => new Iterator<T>(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IList<SelectedItemInfo> _infos;
    private int _totalCount;
    private Func<IList<SelectedItemInfo>, int, T> _getAtImpl;

    private class Iterator<TInner> : IEnumerator<TInner>
    {
        public Iterator(IReadOnlyList<TInner> owner)
        {
            _owner = owner;
        }

        public TInner Current
        {
            get
            {
                var items = _owner;
                if (_currentIndex < items.Count)
                {
                    return items.ElementAt(_currentIndex);
                }

                return default;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (_currentIndex < _owner.Count)
            {
                ++_currentIndex;
                return (_currentIndex < _owner.Count);
            }

            return false;
        }

        public void Reset()
        {

        }

        private IReadOnlyList<TInner> _owner;
        private int _currentIndex;
    }
}
