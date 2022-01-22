using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Navigation;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
    public partial class Frame : ContentControl
    {
        /// <summary>
        /// Defines the <see cref="SourcePageType"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, Type> SourcePageTypeProperty =
            AvaloniaProperty.RegisterDirect<Frame, Type>(nameof(SourcePageType),
                x => x.SourcePageType, (x,v) => x.SourcePageType = v);

        /// <summary>
        /// Defines the <see cref="CacheSize"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, int> CacheSizeProperty =
            AvaloniaProperty.RegisterDirect<Frame, int>(nameof(CacheSize),
                x => x.CacheSize, (x, v) => x.CacheSize = v);

        /// <summary>
        /// Defines the <see cref="BackStackDepth"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, int> BackStackDepthProperty =
            AvaloniaProperty.RegisterDirect<Frame, int>(nameof(BackStackDepth),
                x => x.BackStackDepth);

        /// <summary>
        /// Defines the <see cref="CanGoBack"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, bool> CanGoBackProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoBack),
                x => x.CanGoBack);

        /// <summary>
        /// Defines the <see cref="CanGoForward"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, bool> CanGoForwardProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoForward),
                x => x.CanGoForward);

        /// <summary>
        /// Defines the <see cref="CurrentSourcePageType"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, Type> CurrentSourcePageTypeProperty =
            AvaloniaProperty.RegisterDirect<Frame, Type>(nameof(CurrentSourcePageType),
                x => x.CurrentSourcePageType);

        /// <summary>
        /// Defines the <see cref="BackStack"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, IList<PageStackEntry>> BackStackProperty =
            AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(BackStack),
                x => x.BackStack);

        /// <summary>
        /// Defines the <see cref="ForwardStack"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, IList<PageStackEntry>> ForwardStackProperty =
            AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(ForwardStack),
                x => x.ForwardStack);

        /// <summary>
        /// Defines the <see cref="IsNavigationStackEnabled"/> property
        /// </summary>
        public static readonly DirectProperty<Frame, bool> IsNavigationStackEnabledProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(IsNavigationStackEnabled),
                x => x.IsNavigationStackEnabled, (x, v) => x.IsNavigationStackEnabled = v);

        /// <summary>
        /// Gets or sets a type reference of the current content, or the content that should be navigated to.
        /// </summary>
        public Type SourcePageType
        {
            get => _sourcePageType;
            set => SetAndRaise(SourcePageTypeProperty, ref _sourcePageType, value);
        }

        /// <summary>
        /// Gets or sets the number of pages in the navigation history that can be cached for the frame.
        /// </summary>
        public int CacheSize
        {
            get => _cacheSize;
            set 
            {
                if (value < 0)
                    value = 0;//Ensure we never get a negative cachesize

                SetAndRaise(CacheSizeProperty, ref _cacheSize, value);
            }
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
        public bool CanGoBack
        {
            get => _backStack.Count > 0;
        }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        public bool CanGoForward
        {
            get => _forwardStack.Count > 0;
        }

        /// <summary>
        /// Gets a type reference for the content that is currently displayed.
        /// </summary>
        public Type CurrentSourcePageType => Content?.GetType();

        /// <summary>
        /// Gets a collection of <see cref="PageStackEntry"/> instances representing the 
        /// backward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> BackStack
        {
            get => _backStack;
            private set => SetAndRaise(BackStackProperty, ref _backStack, value);
        }

        /// <summary>
        /// Gets a collection of <see cref="PageStackEntry"/> instances representing the 
        /// forward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> ForwardStack
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
            get => _isNavigationStackEnabled;
            set
            {
                if (SetAndRaise(IsNavigationStackEnabledProperty, ref _isNavigationStackEnabled, value))
                {
                    if (!value)
                    {
                        _backStack.Clear();
                        _forwardStack.Clear();                     
                    }
                }
            }
        }

        internal PageStackEntry CurrentEntry { get; set; }

        /// <summary>
        /// Occurs when the content that is being navigated to has been found and is available 
        /// from the Content property, although it may not have completed loading.
        /// </summary>
        public event NavigatedEventHandler Navigated;

        /// <summary>
        /// Occurs when a new navigation is requested.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating;

        /// <summary>
        /// Occurs when an error is raised while navigating to the requested content.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed;

        /// <summary>
        /// Occurs when a new navigation is requested while a current navigation is in progress.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped;


        private Type _sourcePageType;
        private int _cacheSize = 10;
        private IList<PageStackEntry> _backStack;
        private IList<PageStackEntry> _forwardStack;
        private bool _isNavigationStackEnabled = true;
    }   
}
