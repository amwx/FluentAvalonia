using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Metadata;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Provides a data source that adds grouping and current-item support to collection classes.
/// </summary>
public class CollectionViewSource : AvaloniaObject, ISupportInitialize
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
    public static readonly DirectProperty<CollectionViewSource, IBinding> ItemsBindingProperty =
     AvaloniaProperty.RegisterDirect<CollectionViewSource, IBinding>(nameof(ItemsBinding),
         x => x.ItemsBinding, (x, v) => x.ItemsBinding = v);

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

    public static readonly DirectProperty<CollectionViewSource, Predicate<object>> FilterProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, Predicate<object>>(nameof(Filter),
            x => x.Filter, (x, v) => x.Filter = v);

    public static readonly DirectProperty<CollectionViewSource, IList<string>> LiveFilterPropertiesProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, IList<string>>(nameof(LiveFilterProperties),
            x => x.LiveFilterProperties);

    public static readonly DirectProperty<CollectionViewSource, IList<SortDescription>> SortDescriptionsProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, IList<SortDescription>>(nameof(SortDescriptions),
             x => x.SortDescriptions);

    public static readonly DirectProperty<CollectionViewSource, bool> IsLiveShapingEnabledProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, bool>(nameof(IsLiveShapingEnabled),
            x => x.IsLiveShapingEnabled, (x, v) => x.IsLiveShapingEnabled = v);

    /// <summary>
    /// Gets or sets a value that indicates whether source data is grouped.
    /// </summary>
    public bool IsSourceGrouped
    {
        get => _isSourceGrouped;
        set => SetAndRaise(IsSourceGroupedProperty, ref _isSourceGrouped, value);
    }

    /// <summary>
    /// Gets or sets the property path to follow from the top level item to find groups within the CollectionViewSource.
    /// </summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(Source))]
    public IBinding ItemsBinding
    {
        get => _itemsBinding;
        set => SetAndRaise(ItemsBindingProperty, ref _itemsBinding, value);
    }

    /// <summary>
    /// Gets or sets the collection object from which to create this view.
    /// </summary>
    public IEnumerable Source
    {
        get => _source;
        set => SetAndRaise(SourceProperty, ref _source, value);
    }

    /// <summary>
    /// Gets the view object that is currently associated with this instance of CollectionViewSource.
    /// </summary>
    public ICollectionView View
    {
        get => _view;
        private set => SetAndRaise(ViewProperty, ref _view, value);
    }

    public Predicate<object> Filter
    {
        get => _filter;
        set
        {
            SetAndRaise(FilterProperty, ref _filter, value);
            UpdateView();
            // TODO: Refresh has problems...
            //if ()
            //{

            //}
            //else if (_view is IAdvancedCollectionView acv)
            //{
            //    acv.RefreshFilter();
            //}
        }
    }

    /// <summary>
    /// Gets a list (comma separated) of properties that should be used for live filtering
    /// of the CollectionView
    /// </summary>
    /// <remarks>
    /// In order to use this property, <see cref="IsLiveShapingEnabled"/> must be set to true
    /// or an error will be thrown when creating the ICollectionView
    /// </remarks>
    public AvaloniaList<string> LiveFilterProperties
    {
        get
        {
            if (_liveFilterProperties == null)
            {
                _liveFilterProperties = new AvaloniaList<string>();
                _liveFilterProperties.CollectionChanged += SortOrFilterListChanged;
            }

            return _liveFilterProperties;
        }
    }

    /// <summary>
    /// Gets a list of <see cref="SortDescription"/> that is used for sorting the ICollectionView
    /// </summary>
    public AvaloniaList<SortDescription> SortDescriptions
    {
        get
        {
            if (_sortDescriptions == null)
            {
                _sortDescriptions = new AvaloniaList<SortDescription>();
                _sortDescriptions.CollectionChanged += SortOrFilterListChanged;
            }

            return _sortDescriptions;
        }
    }

    /// <summary>
    /// Gets or sets whether the ICollectionView should respond to changes of the properties 
    /// specified in <see cref="LiveFilterProperties"/> or <see cref="SortDescription"/>
    /// </summary>
    public bool IsLiveShapingEnabled
    {
        get => _isLiveShapingEnabled;
        set => SetAndRaise(IsLiveShapingEnabledProperty, ref _isLiveShapingEnabled, value);
    }

    public void BeginInit()
    {
        _isInitializing = true;
    }

    public void EndInit()
    {
        _isInitializing = false;
        UpdateView();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceProperty ||
            change.Property == ItemsBindingProperty ||
            change.Property == IsSourceGroupedProperty ||
            change.Property == IsLiveShapingEnabledProperty)
        {
            UpdateView();
        }
    }

    private void SortOrFilterListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        if (_isInitializing)
            return;

        if (Source is ICollectionViewFactory factory)
        {
            View = factory.CreateView();
            return;
        }

        if (Source is IEnumerable ie)
        {
            if (_isSourceGrouped)
            {
                // We have to completely recreate the view if the source changes,
                // live shaping changes, or the itemsbinding changes
                // The other properties can be updated without a full recreation
                if (_view is GroupedDataCollectionView gdcv &&
                    (gdcv.Source == _source && gdcv.IsLiveShapingEnabled == _isLiveShapingEnabled &&
                    gdcv.ItemsBinding == _itemsBinding))
                {
                    gdcv.UpdateViewFromCollectionViewSource(_filter, _liveFilterProperties, _sortDescriptions);
                    return;
                }
                else
                {
                    View = new GroupedDataCollectionView(ie, _itemsBinding, _isLiveShapingEnabled,
                        _filter, _liveFilterProperties, _sortDescriptions);
                }
            }
            else
            {
                // Same as above
                if (_view is IterableCollectionView icv &&
                    (icv.Source == _source && icv.IsLiveShapingEnabled == _isLiveShapingEnabled))
                {
                    icv.UpdateViewFromCollectionViewSource(_filter, _liveFilterProperties, _sortDescriptions);
                }
                else
                {
                    View = new IterableCollectionView(ie, _isLiveShapingEnabled,
                        _filter, _liveFilterProperties, _sortDescriptions);
                }
            }
        }
        else
        {
            View = null;
        }
    }



    private bool _isInitializing;
    private bool _isSourceGrouped;
    private IBinding _itemsBinding;
    private IEnumerable _source;
    private ICollectionView _view;
    private AvaloniaList<string> _liveFilterProperties;
    private AvaloniaList<SortDescription> _sortDescriptions;
    private bool _isLiveShapingEnabled;
    private Predicate<object> _filter;
}
