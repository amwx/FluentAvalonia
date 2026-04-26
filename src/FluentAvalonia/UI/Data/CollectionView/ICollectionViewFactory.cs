namespace FluentAvalonia.UI.Data;

/// <summary>
/// An interface that enables implementing collections to create a 
/// view to their data. Normally, user code does not call methods on this interface.
/// </summary>
public interface ICollectionViewFactory
{
    /// <summary>
    /// Creates a new view on the collection that implements this interface.
    /// Typically, user code does not call this method.
    /// </summary>
    /// <returns></returns>
    ICollectionView CreateView();
}
