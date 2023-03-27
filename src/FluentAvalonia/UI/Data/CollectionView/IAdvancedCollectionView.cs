using System.ComponentModel;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Expands <see cref="ICollectionView"/> to support filtering and sorting of items
/// </summary>
public interface IAdvancedCollectionView : ICollectionView, INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the filter applied to the items in the CollectionView
    /// </summary>
    Predicate<object> Filter { get; set; }

    /// <summary>
    /// Gets or sets the list of items used to sort the items in the CollectionView
    /// </summary>
    IList<SortDescription> SortDescriptions { get; }

    /// <summary>
    /// Performs a full refresh of the items in the CollectionView
    /// </summary>
    void Refresh();

    /// <summary>
    /// Refreshes in the items in the CollectionView for an update to the filter
    /// </summary>
    void RefreshFilter();

    /// <summary>
    /// Refreshes the items in the CollectionView for an update to the sort descriptions
    /// </summary>
    void RefreshSorting();

    /// <summary>
    /// Adds a property the CollectionView should monitor via <see cref="INotifyPropertyChanged"/>
    /// to trigger updates. CollectionView must support live shaping
    /// </summary>
    void AddFilterProperty(string propertyName);

    /// <summary>
    /// Removes a property from being observed for live shaping
    /// </summary>
    /// <param name="propertyName"></param>
    void RemoveFilterProperty(string propertyName);

    /// <summary>
    /// Clears all the properties the CollectionView monitors for live shaping
    /// </summary>
    void ClearFilterProperties();
}
