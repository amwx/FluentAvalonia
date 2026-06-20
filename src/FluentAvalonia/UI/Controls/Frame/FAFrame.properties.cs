using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Navigation;

namespace FluentAvalonia.UI.Controls;

public partial class FAFrame : ContentControl
{
    /// <summary>
    /// Defines the <see cref="SourcePageType"/> property
    /// </summary>
    /// <remarks>
    /// When concerned about trimming/aot, do not set the <see cref="SourcePageType"/> using the property!
    /// Use <see cref="Navigate(Type)"/> instead.
    /// </remarks>
    public static readonly StyledProperty<Type> SourcePageTypeProperty =
        AvaloniaProperty.Register<FAFrame, Type>(nameof(SourcePageType));

    /// <summary>
    /// Defines the <see cref="CacheSize"/> property
    /// </summary>
    public static readonly StyledProperty<int> CacheSizeProperty =
        AvaloniaProperty.Register<FAFrame, int>(nameof(CacheSize),
            defaultValue: 10, 
            coerce: (x, v) => v >= 0 ? v : 0);

    /// <summary>
    /// Defines the <see cref="BackStackDepth"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, int> BackStackDepthProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, int>(nameof(BackStackDepth),
            x => x.BackStackDepth);

    /// <summary>
    /// Defines the <see cref="CanGoBack"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, bool> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, bool>(nameof(CanGoBack),
            x => x.CanGoBack);

    /// <summary>
    /// Defines the <see cref="CanGoForward"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, bool> CanGoForwardProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, bool>(nameof(CanGoForward),
            x => x.CanGoForward);

    /// <summary>
    /// Defines the <see cref="CurrentSourcePageType"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, Type> CurrentSourcePageTypeProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, Type>(nameof(CurrentSourcePageType),
            x => x.CurrentSourcePageType);

    /// <summary>
    /// Defines the <see cref="BackStack"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, IList<FAPageStackEntry>> BackStackProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, IList<FAPageStackEntry>>(nameof(BackStack),
            x => x.BackStack);

    /// <summary>
    /// Defines the <see cref="ForwardStack"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, IList<FAPageStackEntry>> ForwardStackProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, IList<FAPageStackEntry>>(nameof(ForwardStack),
            x => x.ForwardStack);

    /// <summary>
    /// Defines the <see cref="IsNavigationStackEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsNavigationStackEnabledProperty =
        AvaloniaProperty.Register<FAFrame, bool>(nameof(IsNavigationStackEnabled),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="NavigationPageFactory"/> property
    /// </summary>
    public static readonly DirectProperty<FAFrame, IFANavigationPageFactory> NavigationPageFactoryProperty =
        AvaloniaProperty.RegisterDirect<FAFrame, IFANavigationPageFactory>(nameof(NavigationPageFactory),
            x => x.NavigationPageFactory, (x, v) => x.NavigationPageFactory = v);

    /// <summary>
    /// Gets or sets a type reference of the current content, or the content that should be navigated to.
    /// </summary>
    /// <remarks>
    /// Do not use this method with trimming/aot! Use <see cref="Navigate(System.Type)"/> instead
    /// </remarks>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type SourcePageType
    {
        // We are suppressing the IL2073 warning here.
        // We do that because we don't own getting the value from a SourcePageTypeProperty.
        // Therefore, this API is unsafe with NativeAOT/Trimming enabled (see the remarks in the xml docs)
#pragma warning disable IL2073
        get => GetValue(SourcePageTypeProperty);
#pragma warning restore IL2073
        set => SetValue(SourcePageTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of pages in the navigation history that can be cached for the frame.
    /// </summary>
    public int CacheSize
    {
        get => GetValue(CacheSizeProperty);
        set => SetValue(CacheSizeProperty, value);
    }

    /// <summary>
    /// Gets the number of entries in the navigation back stack.
    /// </summary>
    public int BackStackDepth
    {
        get => _backStack.Count;
    }

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in back navigation history.
    /// </summary>
    public bool CanGoBack => _backStack.Count > 0;

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in forward navigation history.
    /// </summary>
    public bool CanGoForward=> _forwardStack.Count > 0;

    /// <summary>
    /// Gets a type reference for the content that is currently displayed.
    /// </summary>
    public Type CurrentSourcePageType => Content?.GetType();

    /// <summary>
    /// Gets a collection of <see cref="FAPageStackEntry"/> instances representing the 
    /// backward navigation history of the Frame.
    /// </summary>
    public IList<FAPageStackEntry> BackStack
    {
        get => _backStack;
        private set => SetAndRaise(BackStackProperty, ref _backStack, value);
    }

    /// <summary>
    /// Gets a collection of <see cref="FAPageStackEntry"/> instances representing the 
    /// forward navigation history of the Frame.
    /// </summary>
    public IList<FAPageStackEntry> ForwardStack
    {
        get => _forwardStack;
        private set => SetAndRaise(ForwardStackProperty, ref _forwardStack, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether navigation is recorded in the Frame's 
    /// <see cref="ForwardStack"/> or <see cref="BackStack"/>.
    /// </summary>
    public bool IsNavigationStackEnabled
    {
        get => GetValue(IsNavigationStackEnabledProperty); 
        set => SetValue(IsNavigationStackEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the user specified factory that should be use for resolving pages
    /// when types are not controls or from object instances directly
    /// </summary>
    public IFANavigationPageFactory NavigationPageFactory
    {
        get => _pageFactory;
        set => SetAndRaise(NavigationPageFactoryProperty, ref _pageFactory, value);
    }

    internal FAPageStackEntry CurrentEntry { get; set; }

    /// <summary>
    /// Occurs when the content that is being navigated to has been found and is available 
    /// from the Content property, although it may not have completed loading.
    /// </summary>
    public event FANavigatedEventHandler Navigated;

    /// <summary>
    /// Occurs when a new navigation is requested.
    /// </summary>
    public event FANavigatingCancelEventHandler Navigating;

    /// <summary>
    /// Occurs when an error is raised while navigating to the requested content.
    /// </summary>
    public event FANavigationFailedEventHandler NavigationFailed;

    /// <summary>
    /// Occurs when a new navigation is requested while a current navigation is in progress.
    /// </summary>
    public event FANavigationStoppedEventHandler NavigationStopped;

    /// <summary>
    /// Indicates to a page that it is being navigated away from. Takes the place of 
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatingFrom() method
    /// </summary>
    public static readonly RoutedEvent<FANavigatingCancelEventArgs> NavigatingFromEvent =
        RoutedEvent.Register<Control, FANavigatingCancelEventArgs>("NavigatingFrom",
            RoutingStrategies.Direct);

    /// <summary>
    /// Indiates to a page that it has been navigated away from. Takes the place of
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatedFrom() method
    /// </summary>
    public static readonly RoutedEvent<FluentAvalonia.UI.Navigation.FANavigationEventArgs> NavigatedFromEvent =
        RoutedEvent.Register<Control, FluentAvalonia.UI.Navigation.FANavigationEventArgs>("NavigatedFrom",
            RoutingStrategies.Direct);

    /// <summary>
    /// Indiates to a page that it is being navigated to. Takes the place of
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatedTo() method
    /// </summary>
    public static readonly RoutedEvent<FluentAvalonia.UI.Navigation.FANavigationEventArgs> NavigatedToEvent =
        RoutedEvent.Register<Control, FluentAvalonia.UI.Navigation.FANavigationEventArgs>("NavigatedTo",
            RoutingStrategies.Direct);

    private IList<FAPageStackEntry> _backStack;
    private IList<FAPageStackEntry> _forwardStack;
    private IFANavigationPageFactory _pageFactory;
}
