using Avalonia;
using System.Collections;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Provides a data source that adds grouping and current-item support to collection classes.
/// </summary>
public class CollectionViewSource : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="IsSourceGrouped"/> property
    /// </summary>
    public static readonly DirectProperty<CollectionViewSource, bool> IsSourceGroupedProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, bool>(nameof(IsSourceGrouped),
            x => x.IsSourceGrouped, (x, v) => x.IsSourceGrouped = v);

    /// <summary>
    /// Defines the <see cref="ItemsPath"/> property
    /// </summary>
    public static readonly DirectProperty<CollectionViewSource, string> ItemsPathProperty =
     AvaloniaProperty.RegisterDirect<CollectionViewSource, string>(nameof(ItemsPath),
         x => x.ItemsPath, (x, v) => x.ItemsPath = v);

    /// <summary>
    /// Defines the <see cref="Source"/> property
    /// </summary>
    public static readonly DirectProperty<CollectionViewSource, IEnumerable> SourceProperty =
     AvaloniaProperty.RegisterDirect<CollectionViewSource, IEnumerable>(nameof(Source),
         x => x.Source, (x, v) => x.Source = v);

    /// <summary>
    /// Defines the <see cref="View"/> property
    /// </summary>
    public static readonly DirectProperty<CollectionViewSource, ICollectionView> ViewProperty =
     AvaloniaProperty.RegisterDirect<CollectionViewSource, ICollectionView>(nameof(View),
         x => x.View);

    /// <summary>
    /// Gets or sets a value that indicates whether source data is grouped.
    /// </summary>
    public bool IsSourceGrouped
    {
        get => _isSourceGrouped;
        set
        {
            if (SetAndRaise(IsSourceGroupedProperty, ref _isSourceGrouped, value))
            {
                UpdateView();
            }
        }
    }

    /// <summary>
    /// Gets or sets the property path to follow from the top level item to find groups within the CollectionViewSource.
    /// </summary>
    public string ItemsPath
    {
        get => _itemsPath;
        set
        {
            if (SetAndRaise(ItemsPathProperty, ref _itemsPath, value))
            {
                UpdateView();
            }
        }
    }

    /// <summary>
    /// Gets or sets the collection object from which to create this view.
    /// </summary>
    public IEnumerable Source
    {
        get => _source;
        set
        {
            if (SetAndRaise(SourceProperty, ref _source, value))
            {
                UpdateView();
            }
        }
    }

    /// <summary>
    /// Gets the view object that is currently associated with this instance of CollectionViewSource.
    /// </summary>
    public ICollectionView View
    {
        get
        {
            if (_view == null)
            {
                UpdateView();
            }

            return _view;
        }
        private set => SetAndRaise(ViewProperty, ref _view, value);
    }

    private void UpdateView()
    {
        if (_source == null)
            return;

        if (Source is IEnumerable ie)
        {
            View = new GroupedDataCollectionView(ie, _isSourceGrouped, _itemsPath);
        }
        else if (Source is ICollectionViewFactory factory)
        {
            View = factory.CreateView();
        }
        else
        {
            View = null;
        }
    }

    private bool _isSourceGrouped;
    private string _itemsPath;
    private IEnumerable _source;
    private ICollectionView _view;
}
