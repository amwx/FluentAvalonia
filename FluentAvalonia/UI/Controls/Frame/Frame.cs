using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.Core.Attributes;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls
{
    public class Frame : ContentControl
    {
        public Frame()
        {
            var back = new AvaloniaList<PageStackEntry>();
            var forw = new AvaloniaList<PageStackEntry>();

            back.CollectionChanged += OnBackStackChanged;
            forw.CollectionChanged += OnFowardwardStackChanged;

            BackStack = back;
            ForwardStack = forw;
        }


        public static readonly DirectProperty<Frame, Type> SourcePageTypeProperty =
            AvaloniaProperty.RegisterDirect<Frame, Type>(nameof(SourcePageType),
                x => x.SourcePageType, (x,v) => x.SourcePageType = v);

        public static readonly DirectProperty<Frame, int> CacheSizeProperty =
            AvaloniaProperty.RegisterDirect<Frame, int>(nameof(CacheSize),
                x => x.CacheSize, (x, v) => x.CacheSize = v);

        public static readonly DirectProperty<Frame, int> BackStackDepthProperty =
            AvaloniaProperty.RegisterDirect<Frame, int>(nameof(BackStackDepth),
                x => x.BackStackDepth);

        public static readonly DirectProperty<Frame, bool> CanGoBackProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoBack),
                x => x.CanGoBack);

        public static readonly DirectProperty<Frame, bool> CanGoForwardProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoForward),
                x => x.CanGoForward);

        public static readonly DirectProperty<Frame, Type> CurrentSourcePageTypeProperty =
            AvaloniaProperty.RegisterDirect<Frame, Type>(nameof(CurrentSourcePageType),
                x => x.CurrentSourcePageType);

        public static readonly DirectProperty<Frame, IList<PageStackEntry>> BackStackProperty =
            AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(BackStack),
                x => x.BackStack);

        public static readonly DirectProperty<Frame, IList<PageStackEntry>> ForwardStackProperty =
            AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(ForwardStack),
                x => x.ForwardStack);

        public static readonly DirectProperty<Frame, bool> IsNavigationStackEnabledProperty =
            AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(IsNavigationStackEnabled),
                x => x.IsNavigationStackEnabled, (x, v) => x.IsNavigationStackEnabled = v);

        public Type SourcePageType
        {
            get => _sourcePageType;
            set => SetAndRaise(SourcePageTypeProperty, ref _sourcePageType, value);
        }

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

        public int BackStackDepth
        {
            get => _backStack.Count;
        }

        public bool CanGoBack
        {
            get => _backStack.Count > 0;
        }

        public bool CanGoForward
        {
            get => _forwardStack.Count > 0;
        }

        public Type CurrentSourcePageType => Content?.GetType();

        public IList<PageStackEntry> BackStack
        {
            get => _backStack;
            private set => SetAndRaise(BackStackProperty, ref _backStack, value);
        }

        public IList<PageStackEntry> ForwardStack
        {
            get => _forwardStack;
            private set => SetAndRaise(ForwardStackProperty, ref _forwardStack, value);
        }

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

        public event NavigatedEventHandler Navigated;
        public event NavigatingCancelEventHandler Navigating;
        public event NavigationFailedEventHandler NavigationFailed;
        public event NavigationStoppedEventHandler NavigationStopped;


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ContentProperty)
            {
                if (change.NewValue.GetValueOrDefault() == null)
                {
                    CurrentEntry = null;
                }
            }
            else if (change.Property == SourcePageTypeProperty)
            {
                if (!_isNavigating)
                {
                    if (change.NewValue.GetValueOrDefault() is null)
                        throw new InvalidOperationException("SourcePageType cannot be null. Use Content instead.");

                    Navigate(change.NewValue.GetValueOrDefault<Type>());
                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _presenter = e.NameScope.Find<ContentPresenter>("ContentPresenter");
        }


        public void GoBack() => GoBack(null);

        public void GoForward() 
        {
            if (CanGoForward)
            {
                NavigateCore(_forwardStack[_forwardStack.Count - 1], NavigationMode.Forward);
            }
        }

        public bool Navigate(Type sourcePageType, object parameter) => Navigate(sourcePageType, parameter, null);

        //TODO
        public string GetNavigationState() => throw new NotImplementedException();

        public void SetNavigationState(string navState) { }

        public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            return NavigateCore(new PageStackEntry(sourcePageType, parameter,
                infoOverride), NavigationMode.New);
        }

        public void GoBack(NavigationTransitionInfo infoOverride)
        {
            if (CanGoBack)
            {
                var entry = _backStack[_backStack.Count - 1];
                if (infoOverride != null)
                {
                    entry.NavigationTransitionInfo = infoOverride;
                }
                else
                {
                    entry.NavigationTransitionInfo = CurrentEntry?.NavigationTransitionInfo ?? null;
                }

                NavigateCore(entry, NavigationMode.Back);
            }
        }

        //TODO
        public void SetNavigationState(string navState, bool suppressNavigate) 
        {
            throw new NotImplementedException();
        }

        //TODO
        public bool NavigateToType(Type sourcePageType, object parameter, FrameNavigationOptions navOptions)
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type sourcePageType) => Navigate(sourcePageType, null, null);

        private bool NavigateCore(PageStackEntry entry, NavigationMode mode)
        {
            try
            {
                _isNavigating = true;

                var ea = new NavigatingCancelEventArgs(mode,
                    entry.NavigationTransitionInfo,
                    entry.Parameter,
                    entry.SourcePageType);

                Navigating?.Invoke(this, ea);

                if (ea.Cancel)
                {
                    OnNavigationStopped(entry, mode);
                    return false;
                }

                //Unlike WinUI/UWP, we don't have page & allow navigating from anything IControl,
                //so we don't have Page.OnNavigatingFrom();
                //CurrentEntry?.Instance?.OnNavigatingFrom(ea);

                //Navigate to new page
                var prevEntry = CurrentEntry;
                CurrentEntry = entry;

                if (mode == NavigationMode.New)
                {
                    //Check if we already have an instance of the page in the cache
                    CurrentEntry.Instance = CheckCacheAndGetPage(entry.SourcePageType);
                }

                if (CurrentEntry.Instance == null)
                {
                    var page = CreatePageAndCacheIfNecessary(entry.SourcePageType);
                    if (page == null)
                        return false;

                    CurrentEntry.Instance = page;
                }

                SetContentAndAnimate(entry);

                if (IsNavigationStackEnabled)
                {
                    switch (mode)
                    {
                        case NavigationMode.New:
                            ForwardStack.Clear();
                            if (prevEntry != null)
                            {
                                BackStack.Add(prevEntry);
                            }
                            break;

                        case NavigationMode.Back:
                            ForwardStack.Add(prevEntry);
                            BackStack.Remove(CurrentEntry);
                            break;

                        case NavigationMode.Forward:
                            BackStack.Add(prevEntry);
                            ForwardStack.Remove(CurrentEntry);
                            break;

                        case NavigationMode.Refresh:
                            break;
                    }
                }

                var navEA = new NavigationEventArgs(
                    CurrentEntry.Instance,
                    mode, entry.NavigationTransitionInfo,
                    entry.Parameter,
                    entry.SourcePageType);

                SourcePageType = entry.SourcePageType;
                //CurrentSourcePageType = entry.SourcePageType;

                Navigated?.Invoke(this, navEA);

                //Need to find compatible method for this
                //VisualTreeHelper.CloseAllPopups();

                return true;

            }
            catch (Exception ex)
            {
                NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(ex, entry.SourcePageType));

                //I don't really want to throw an exception and break things. Just return false
                return false;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private void OnNavigationStopped(PageStackEntry entry, NavigationMode mode)
        {
            NavigationStopped?.Invoke(this, new NavigationEventArgs(entry.Instance,
                mode, entry.NavigationTransitionInfo, entry.Parameter, entry.SourcePageType));
        }

        private void OnFowardwardStackChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int oldCount = (_forwardStack.Count - (e.NewItems?.Count ?? 0) + (e.OldItems?.Count ?? 0));
            
            bool oldForward = oldCount > 0;
            bool newForward = _forwardStack.Count > 0;
            RaisePropertyChanged(new AvaloniaPropertyChangedEventArgs<bool>(this, 
                CanGoForwardProperty, oldForward, newForward, Avalonia.Data.BindingPriority.LocalValue));            
        }

        private void OnBackStackChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int oldCount = (_backStack.Count - (e.NewItems?.Count ?? 0) + (e.OldItems?.Count ?? 0));

            bool oldBack = oldCount > 0;
            bool newBack = _backStack.Count > 0;
            RaisePropertyChanged(new AvaloniaPropertyChangedEventArgs<bool>(this,
                CanGoBackProperty, oldBack, newBack, Avalonia.Data.BindingPriority.LocalValue));
            RaisePropertyChanged(new AvaloniaPropertyChangedEventArgs<int>(this,
                BackStackDepthProperty, oldCount, _backStack.Count, Avalonia.Data.BindingPriority.LocalValue));
        }

        private IControl CreatePageAndCacheIfNecessary(Type srcPageType)
        {
            if (CacheSize == 0)
            {
                return Activator.CreateInstance(srcPageType) as IControl;
            }

            for (int i = 0; i < _cache.Count; i++)
            {
                if (_cache[i].GetType() == srcPageType)
                {
                    throw new Exception($"An object of type {srcPageType} has already been added to the Navigation Stack");
                }
            }

            var newPage = Activator.CreateInstance(srcPageType) as IControl;
            _cache.Add(newPage);
            if (_cache.Count > CacheSize)
            {
                _cache.RemoveAt(0);
            }

            return newPage;
        }

        private IControl CheckCacheAndGetPage(Type srcPageType)
        {
            if (CacheSize == 0)
                return null;

            for (int i = _cache.Count-1; i >= 0; i--)
            {
                if (_cache[i].GetType() == srcPageType)
                {
                    var item = _cache[i];
                    _cache.RemoveAt(i);
                    return item;
                }
            }

            return null;
        }

        private void SetContentAndAnimate(PageStackEntry entry)
        {
            if (entry == null)
                return;

            Content = entry.Instance;

            if (_presenter != null)
            {
                //Default to entrance transition
                entry.NavigationTransitionInfo = entry.NavigationTransitionInfo ?? new EntranceNavigationTransitionInfo();
				_presenter.Opacity = 0;
				// Very busy pages will delay loading b/c layout & render has to occur first
				// Posting this helps a little bit, but not much
				// Not really sure how to get the transition to occur while the page is loading
				// so speed is comparable to WinUI...this may be an Avalonia limitation???
				Dispatcher.UIThread.Post(() =>
				{
					entry.NavigationTransitionInfo.RunAnimation(_presenter);
				}, DispatcherPriority.Loaded);
            }
        }

        private ContentPresenter _presenter;
        private List<IControl> _cache = new List<IControl>(10);
        bool _isNavigating = false;
        private Type _sourcePageType;
        private int _cacheSize = 10;
        private IList<PageStackEntry> _backStack;
        private IList<PageStackEntry> _forwardStack;
        private bool _isNavigationStackEnabled = true;
    }   
}
