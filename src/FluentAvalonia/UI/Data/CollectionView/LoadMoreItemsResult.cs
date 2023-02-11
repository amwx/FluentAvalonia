namespace FluentAvalonia.UI.Data;

/// <summary>
/// Wraps the asynchronous results of a LoadMoreItemsAsync call
/// </summary>
public struct LoadMoreItemsResult
{
    /// <summary>
    /// The number of items that were actually loaded
    /// </summary>
    public uint Count;
}
