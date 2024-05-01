namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides methods that support mapping between an item's unique identifier and index
/// </summary>
public interface IKeyIndexMapping
{
    /// <summary>
    /// Retreives the unique identifier (key) for the item at the specified index
    /// </summary>
    /// <param name="index">The index of the item to get the key for</param>
    /// <returns>The unique idenfier (key) for the item at the specified _index_</returns>
    string KeyFromIndex(int index);

    /// <summary>
    /// Retreives the index of the item that has the specified unique identifier (key)
    /// </summary>
    /// <param name="key">The unique identifier (key) of the item to find the index of</param>
    /// <returns>The index of the item with specified _key_</returns>
    int IndexFromKey(string key);
}