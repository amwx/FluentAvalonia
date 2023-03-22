namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="SelectionModel.ChildrenRequested"/> event.
/// </summary>
internal class SelectionModelChildrenRequestedEventArgs : EventArgs
{
    internal SelectionModelChildrenRequestedEventArgs(
        object source,
        IndexPath sourceIndexPath,
        bool throwOnAccess)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));
        Initialize(source, sourceIndexPath, throwOnAccess);
    }

    /// <summary>
    /// Gets or sets an observable which produces the children of the <see cref="Source"/>
    /// object.
    /// </summary>
    public object Children { get; set; }

    /// <summary>
    /// Gets the object whose children are being requested.
    /// </summary>        
    public object Source
    {
        get
        {
            if (_throwOnAccess)
            {
                throw new InvalidOperationException("Source can only be accesed in the ChildrenRequested event handler.");
            }

            return _source!;
        }
    }

    /// <summary>
    /// Gets the index of the object whose children are being requested.
    /// </summary>        
    public IndexPath SourceIndex
    {
        get
        {
            if (_throwOnAccess)
            {
                throw new InvalidOperationException("Source can only be accesed in the ChildrenRequested event handler.");
            }

            return _sourceIndexPath;
        }
    }
        
    internal void Initialize(
        object source,
        IndexPath sourceIndexPath,
        bool throwOnAccess)
    {
        if (!throwOnAccess && source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        _source = source;
        _sourceIndexPath = sourceIndexPath;
        _throwOnAccess = throwOnAccess;
        Children = null;
    }

    private object _source;
    private IndexPath _sourceIndexPath;
    private bool _throwOnAccess;
}
