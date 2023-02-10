using Avalonia.Collections;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Represents any grouped items within a view
/// </summary>
public interface ICollectionViewGroup
{
    /// <summary>
    /// Gets or sets the grouping contextused for grouping the data, which sets the data context
    /// for the default HeaderTemplate
    /// </summary>
    object Group { get; }

    /// <summary>
    /// Gets the collection of grouped items that this ICollectionViewGroup implementation represents
    /// </summary>
    IAvaloniaList<object> GroupItems { get; }
}
