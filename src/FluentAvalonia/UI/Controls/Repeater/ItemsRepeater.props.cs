using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a data-driven collection control that incorporates a flexible layout system,
/// custom views, and virtualization, with no default UI or interaction policies.
/// </summary>
public partial class ItemsRepeater : Panel
{
    /// <summary>
    /// Defines the <see cref="VerticalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> VerticalCacheLengthProperty =
        AvaloniaProperty.Register<ItemsRepeater, double>(nameof(VerticalCacheLength), defaultValue: 2.0);

    /// <summary>
    /// Defines the <see cref="HorizontalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<double> HorizontalCacheLengthProperty =
        AvaloniaProperty.Register<ItemsRepeater, double>(nameof(HorizontalCacheLength), defaultValue: 2.0);

    /// <summary>
    /// Defines the <see cref="Layout"/> property
    /// </summary>
    public static readonly StyledProperty<Layout> LayoutProperty =
        AvaloniaProperty.Register<ItemsRepeater, Layout>(nameof(Layout), defaultValue: new StackLayout());

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property
    /// </summary>
    public static readonly StyledProperty<object> ItemsSourceProperty =
        AvaloniaProperty.Register<ItemsRepeater, object>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="VerticalCacheLength"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<ItemsRepeater>();

    /// <summary>
    /// Defines the <see cref="ItemTransitionProvider"/> property
    /// </summary>
    public static readonly StyledProperty<ItemCollectionTransitionProvider> ItemTransitionProviderProperty =
        AvaloniaProperty.Register<ItemsRepeater, ItemCollectionTransitionProvider>(nameof(ItemTransitionProvider));

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
    public Layout Layout
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
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ItemCollectionTransitionProvider"/> for the ItemsRepeater
    /// </summary>
    public ItemCollectionTransitionProvider ItemTransitionProvider
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

    internal IElementFactory ItemTemplateShim => _itemTemplateWrapper;

    internal ViewManager ViewManager => _viewManager;

    private bool IsProcessingCollectionChange => _processingItemsSourceChange != null;

    internal bool ShouldPhase => ContainerContentChanging != null;

    /// <summary>
    /// Occurs each time an element is prepared for use.
    /// </summary>
    public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> ElementPrepared;

    /// <summary>
    /// Occurs each time an element is cleared and made available to be re-used.
    /// </summary>
    public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> ElementClearing;

    /// <summary>
    /// Occurs for each realized UIElement when the index for the item it represents has changed.
    /// </summary>
    public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementIndexChangedEventArgs> ElementIndexChanged;

    /// <summary>
    /// Occurs when container content is changing, used for Phased rendering
    /// </summary>
    public event TypedEventHandler<ItemsRepeater, ContainerContentChangingEventArgs> ContainerContentChanging;

    internal static readonly AttachedProperty<VirtualizationInfo> VirtualizationInfoProperty =
        AvaloniaProperty.RegisterAttached<ItemsRepeater, Control, VirtualizationInfo>("VirtualizationInfo");

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

    internal void RaiseContainerContentChanging(ContainerContentChangingEventArgs args)
    {
        ContainerContentChanging?.Invoke(this, args);
    }
}
