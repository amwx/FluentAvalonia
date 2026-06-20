using FAControlsGallery.Utilities;

namespace FAControlsGallery.Services;

internal sealed class RecentFavoriteService
{
    static RecentFavoriteService()
    {
        Instance = new RecentFavoriteService();
    }

    public static RecentFavoriteService Instance { get; }

    public async Task<IList<string>> GetRecentItems()
    {
        if (_recentItems == null)
        {
            var result = await StorageService.Instance.OpenFile(RecentPagesFile);

            if (result == null)
                _recentItems = new List<string>();
            else
                _recentItems = new List<string>(result.Trim().Split(','));
        }

        return _recentItems;
    }

    public async Task<IList<string>> GetFavoriteItems()
    {
        if (_favorites == null)
        {
            var result = await StorageService.Instance.OpenFile(FavoritesFile);

            if (result == null)
                _favorites = new List<string>();
            else
                _favorites = new List<string>(result.Trim().Split(','));
        }

        return _favorites;
    }

    public void AddRecentItem(string item)
    {
        _recentItems ??= new List<string>();

        var idx = _recentItems.IndexOf(item);
        if (idx > 0)
        {
            // If we've already visited this page, but went to it again, move
            // it to the front of the list, if it is not already
            _recentItems.RemoveAt(idx);
            _recentItems.Insert(0, item);
        }
        else if (idx == -1)
        {
            _recentItems.Insert(0, item);
        }
        else if (idx == 0)
        {
            // Item is already at index 0, don't do anything, including saving
            return;
        }

        SaveRecents();
    }

    public void ClearRecentItems()
    {
        _recentItems?.Clear();
        SaveRecents();
    }

    private void SaveRecents()
    {
        string textToWrite = string.Empty;
        if (_recentItems != null && _recentItems.Count > 0)
        {
            textToWrite = string.Join(',', _recentItems);
        }

        StorageService.Instance.WriteToFile(RecentPagesFile,
            textToWrite).FireAndForget();
    }

    public void SetFavorite(string item, bool favorite)
    {
        _favorites ??= new List<string>();

        if (favorite)
        {
            if (_favorites.Contains(item))
                return;

            _favorites.Insert(0, item);
        }
        else
        {
            _favorites.Remove(item);
        }

        SaveFavorites();
    }

    public void ClearFavorites()
    {
        _favorites?.Clear();
        SaveFavorites();
    }

    private void SaveFavorites()
    {
        string textToWrite = string.Empty;
        if (_favorites != null && _favorites.Count > 0)
        {
            textToWrite = string.Join(',', _favorites);
        }

        StorageService.Instance.WriteToFile(FavoritesFile,
            textToWrite).FireAndForget();
    }


    private List<string> _recentItems;
    private List<string> _favorites;
    private const string RecentPagesFile = "RecentPages.txt";
    private const string FavoritesFile = "Favorites.txt";
}
