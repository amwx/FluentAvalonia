using System.Collections;

namespace FluentAvalonia.UI.Controls;

internal class BreadcrumbIterable : IEnumerable
{
    public BreadcrumbIterable(IEnumerable src)
    {
        ItemsSource = src;
    }

    public IEnumerable ItemsSource { get; set; }

    public IEnumerator GetEnumerator() => new BreadcrumbIterator(ItemsSource);

    public class BreadcrumbIterator : IEnumerator
    {
        public BreadcrumbIterator(IEnumerable itemsSource)
        {
            // WinUI sets this, but I think IIterator calls Current before MoveNext
            // whereas IEnumerator will call MoveNext then Current, so we will leave
            // the value as -1, set in the field decl
            //_currentIndex = 0;
            if (itemsSource != null)
            {
                _itemsSource = new FAItemsSourceView(itemsSource);
                // Add 1 to account for the leading null/ellipsis element
                _size = _itemsSource.Count + 1;
            }
            else
            {
                _size = 1;
            }
        }

        public object Current
        {
            get
            {
                if (_currentIndex == 0)
                    return null;
                else if (HasCurrent())
                {
                    return _itemsSource.GetAt(_currentIndex - 1);
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public bool MoveNext()
        {
            if (HasCurrent())
            {
                _currentIndex++;
                return HasCurrent();
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void Reset()
        {

        }

        private bool HasCurrent() => _currentIndex < _size;

        private readonly FAItemsSourceView _itemsSource;
        private int _currentIndex = -1;
        private readonly int _size;
    }
}
