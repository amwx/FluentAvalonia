using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a data-driven collection control that incorporates a flexible layout system,
/// custom views, and virtualization, with no default UI or interaction policies.
/// </summary>
public partial class FAItemsRepeater : Panel
{
    /// <summary>
    /// Defines the <see cref="VerticalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> VerticalCacheLengthProperty =
        AvaloniaProperty.Register<FAItemsRepeater, double>(nameof(VerticalCacheLength), defaultValue: 2.0);

    /// <summary>
    /// Defines the <see cref="HorizontalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> HorizontalCacheLengthProperty =
        AvaloniaProperty.Register<FAItemsRepeater, double>(nameof(HorizontalCacheLength), defaultValue: 2.0);

    /// <summary>
    /// Defines the <see cref="Layout"/> property
    /// </summary>
    public static readonly StyledProperty<FALayout> LayoutProperty =
        AvaloniaProperty.Register<FAItemsRepeater, FALayout>(nameof(Layout));

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<object> ItemsSourceProperty =
        AvaloniaProperty.Register<FAItemsRepeater, object>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="VerticalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<FAItemsRepeater>();

    /// <summary>
    /// Defines the <see cref="ItemTransitionProvider"/> property
    /// </summary>
    public static readonly StyledProperty<FAItemCollectionTransitionProvider> ItemTransitionProviderProperty =
        AvaloniaProperty.Register<FAItemsRepeater, FAItemCollectionTransitionProvider>(nameof(ItemTransitionProvider));

    /// <summary>
    /// Gets or sets a value that indicates the size of the buffer used to realize items when panning or scrolling vertically.
    /// </summary>
    public double VerticalCacheLength
    {
        get => GetValue(VerticalCacheLengthProperty);
        set => SetValue(VerticalCacheLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the size of the buffer used to realize items when panning or scrolling vertically.
    /// </summary>
    public double HorizontalCacheLength
    {
        get => GetValue(HorizontalCacheLengthProperty);
        set => SetValue(HorizontalCacheLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout used to size and position elements in the ItemsRepeater.
    /// </summary>
    public FALayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets an object source used to generate the content of the ItemsRepeater.
    /// </summary>
    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display each item.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="FAItemCollectionTransitionProvider"/> for the ItemsRepeater
    /// </summary>
    public FAItemCollectionTransitionProvider ItemTransitionProvider
    {
        get => GetValue(ItemTransitionProviderProperty);
        set => SetValue(ItemTransitionProviderProperty, value);
    }

    /// <summary>
    /// Gets a standardized view of the supported interactions between a given ItemsSource object and the ItemsRepeater control and its associated components.
    /// </summary>
    /// <remarks>
    /// Note the return type is <see cref="FAItemsSourceView"/> and not the ItemsSourceView in Avalonia
    /// </remarks>
    public FAItemsSourceView ItemsSourceView => _itemsSourceView;

    internal Control MadeAnchor => _viewportManager.MadeAnchor;

    internal object LayoutState
    {
        get => _layoutState;
        set => _layoutState = value;
    }

    internal TransitionManager TransitionManager => _transitionManager;

    internal Rect VisibleWindow => _viewportManager.GetLayoutVisibleWindow();

    internal Rect RealizationWindow => _viewportManager.GetLayoutRealizationWindow();

    internal Control SuggestedAnchor => _viewportManager.SuggestedAnchor;

    internal Point LayoutOrigin
    {
        get => _layoutOrigin;
        set => _layoutOrigin = value;
    }

    internal IFAElementFactory ItemTemplateShim => _itemTemplateWrapper;

    internal ViewManager ViewManager => _viewManager;

    private bool IsProcessingCollectionChange => _processingItemsSourceChange != null;

    internal bool ShouldPhase => ContainerContentChanging != null;

    /// <summary>
    /// Occurs each time an element is prepared for use.
    /// </summary>
    public event TypedEventHandler<FAItemsRepeater, FAItemsRepeaterElementPreparedEventArgs> ElementPrepared;

    /// <summary>
    /// Occurs each time an element is cleared and made available to be re-used.
    /// </summary>
    public event TypedEventHandler<FAItemsRepeater, FAItemsRepeaterElementClearingEventArgs> ElementClearing;

    /// <summary>
    /// Occurs for each realized UIElement when the index for the item it represents has changed.
    /// </summary>
    public event TypedEventHandler<FAItemsRepeater, FAItemsRepeaterElementIndexChangedEventArgs> ElementIndexChanged;

    /// <summary>
    /// Occurs when container content is changing, used for Phased rendering
    /// </summary>
    public event TypedEventHandler<FAItemsRepeater, FAContainerContentChangingEventArgs> ContainerContentChanging;

    internal static readonly AttachedProperty<VirtualizationInfo> VirtualizationInfoProperty =
        AvaloniaProperty.RegisterAttached<FAItemsRepeater, Control, VirtualizationInfo>("VirtualizationInfo");

    internal static VirtualizationInfo GetVirtualizationInfo(Control c)
    {
        var result = c.GetValue(VirtualizationInfoProperty);

        if (result == null)
        {
            result = CreateAndInitializeVirtualizationInfo(c);
        }

        return result;
    }

    internal static VirtualizationInfo TryGetVirtualizationInfo(Control c) =>
        GetVirtualizationInfo(c);

    internal static VirtualizationInfo CreateAndInitializeVirtualizationInfo(Control element)
    {
        var result = new VirtualizationInfo();
        element.SetValue(VirtualizationInfoProperty, result);
        return result;
    }

    internal void RaiseContainerContentChanging(FAContainerContentChangingEventArgs args)
    {
        ContainerContentChanging?.Invoke(this, args);
    }
}
