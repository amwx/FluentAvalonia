using System.Collections;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Provides data on how a CollectionView should sort its items
/// </summary>
public class FASortDescription
{
    /// <summary>
    /// Creates a default sort description, that sorts in Ascending order using the default
    /// object comparer (which compares the items themselves)
    /// </summary>
    public FASortDescription()
        : this(null, null, FASortDirection.Ascending, ObjectComparer.Instance) { }

    /// <summary>
    /// Creates a sort description with the specified SortDirection and comparer
    /// </summary>
    /// <param name="direction">The direction this description should sort items</param>
    /// <param name="comparer">The custom IComparer implementation</param>
    public FASortDescription(FASortDirection direction, IComparer comparer = null)
        : this(null, null, direction, comparer) { }

    /// <summary>
    /// Creates a sort description that sorts using a custom binding expression
    /// </summary>
    /// <param name="property">The property that should be used for sorting</param>
    /// <param name="propertyName">The name of the property. If this is unset, the name is retrieved
    /// from the binding, if applicable</param>
    /// <param name="comparer">The custom IComparer implementation</param>
    /// <remarks>Note: nested properties are not supported, particularly for live shaping</remarks>
    public FASortDescription(BindingBase property, string propertyName = null, IComparer comparer = null)
        : this(property, propertyName, FASortDirection.Ascending, comparer) { }

    /// <summary>
    /// Creates a custom sort description with a specified property binding, direction, and IComparer
    /// </summary>
    /// <param name="property">The property that should be used for sorting</param>
    /// <param name="propertyName">The name of the property. If this is null, the name is retrieved
    /// from the binding, if applicable></param>
    /// <param name="direction">The direction this description should sort items</param>
    /// <param name="comparer">The custom IComparer implementation</param>
    /// <remarks>Note: nested properties are not supported, particularly for live shaping</remarks>
    public FASortDescription(BindingBase property, string propertyName, FASortDirection direction, IComparer comparer)
    {
        Property = property;
        PropertyName = propertyName;
        Direction = direction;
        Comparer = comparer ?? ObjectComparer.Instance;
    }

    /// <summary>
    /// Gets or sets the property this sort description uses to sort.
    /// </summary>
    [AssignBinding]
    public BindingBase Property { get; set; }

    /// <summary>
    /// Gets or sets the name of the property used for sorting. If the name hasn't been set,
    /// it will be set from the binding, if possible. Note that nested properties are not supported
    /// </summary>
    public string PropertyName
    {
        get
        {
            if (field == null)
            {
                if (Property is Binding b)
                {
                    field = b.Path;
                }
                else if (Property is CompiledBindingExtension cbe)
                {
                    field = cbe.Path.ToString();
                }
            }

            return field;
        }
        set;
    }

    /// <summary>
    /// Gets or sets the direction the sort description should sort
    /// </summary>
    public FASortDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets a custom IComparer implementation used to compare items for sorting
    /// </summary>
    public IComparer Comparer { get; set; }

    public static FASortDescription CreateCompiled(string propertyName,
        Func<object, object> getter,
        Type propertyType,
        FASortDirection direction)
    {
        return CreateCompiled(propertyName, getter, propertyType, direction, null);
    }

    public static FASortDescription CreateCompiled(string propertyName,
        Func<object, object> getter,
        Type propertyType,
        FASortDirection direction,
        IComparer comparer)
    {
        var x = new CompiledBindingPathBuilder();
        x = x.Property(
            new ClrPropertyInfo(propertyName, getter, null, propertyType),
            PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);
        var path = x.Build();

        var cb = new CompiledBindingExtension(path);

        return new FASortDescription(cb, propertyName, direction, comparer);
    }

    private class ObjectComparer : IComparer
    {
        private ObjectComparer() { }

        public static readonly IComparer Instance = new ObjectComparer();

        public int Compare(object x, object y)
        {
            var cx = x as IComparable;
            var cy = y as IComparable;

            return cx == cy ? 0 : cx == null ? -1 : cy == null ? +1 : cx.CompareTo(cy);
        }
    }
}
