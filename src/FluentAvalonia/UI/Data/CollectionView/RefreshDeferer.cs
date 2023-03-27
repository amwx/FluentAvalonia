// Sorting & Filtering adapted from the WindowsCommunityToolkit
// AdvancedCollectionView - MIT license

namespace FluentAvalonia.UI.Data;

internal class RefreshDeferer : IDisposable
{
    public RefreshDeferer(Action<object> release, object currentItem)
    {
        _currentItem = currentItem;
        _releaseAction = release;
    }

    public void Dispose()
    {
        _releaseAction(_currentItem);
    }

    private readonly Action<object> _releaseAction;
    private readonly ICollectionView _acvs;
    private readonly object _currentItem;
}
