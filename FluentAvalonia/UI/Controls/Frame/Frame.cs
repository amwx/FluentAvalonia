using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Displays <see cref="UserControl"/> instances (Pages in WinUI), supports navigation to new pages, 
    /// and maintains a navigation history to support forward and backward navigation.
    /// </summary>
    public partial class Frame : ContentControl
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

        /// <summary>
        /// Navigates to the most recent item in back navigation history, if a Frame manages its own navigation history.
        /// </summary>
        public void GoBack() => GoBack(null);

        /// <summary>
        /// Navigates to the most recent item in back navigation history, if a Frame manages its own navigation history, 
        /// and specifies the animated transition to use.
        /// </summary>
        /// <param name="infoOverride">Info about the animated transition to use.</param>
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

        /// <summary>
        /// Navigates to the most recent item in forward navigation history, if a Frame manages its own navigation history.
        /// </summary>
        public void GoForward() 
        {
            if (CanGoForward)
            {
                NavigateCore(_forwardStack[_forwardStack.Count - 1], NavigationMode.Forward);
            }
        }

        /// <summary>
        /// Causes the Frame to load content represented by the specified Page.
        /// </summary>
        /// <param name="sourcePageType">he page to navigate to, specified as a type reference to its partial class type.</param>
        /// <returns>false if a <see cref="NavigationFailed"/> event handler has set Handled to true; otherwise, true. </returns>
        public bool Navigate(Type sourcePageType) => Navigate(sourcePageType, null, null);


        /// <summary>
        /// Causes the Frame to load content represented by the specified Page, also passing a parameter to be 
        /// interpreted by the target of the navigation.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to its 
        /// partial class type.</param>
        /// <param name="parameter">The navigation parameter to pass to the target page; 
        /// must have a basic type (string, char, numeric, or GUID) to support parameter serialization
        /// using GetNavigationState.</param>
        /// <returns>false if a <see cref="NavigationFailed"/> event handler has set Handled to true; 
        /// otherwise, true.</returns>
        public bool Navigate(Type sourcePageType, object parameter) => Navigate(sourcePageType, parameter, null);

        /// <summary>
        /// Causes the Frame to load content represented by the specified Page -derived data type, 
        /// also passing a parameter to be interpreted by the target of the navigation, and a value 
        /// indicating the animated transition to use.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to 
        /// its partial class type. </param>
        /// <param name="parameter">The navigation parameter to pass to the target page; must have a 
        /// basic type (string, char, numeric, or GUID) to support parameter serialization using 
        /// GetNavigationState.</param>
        /// <param name="infoOverride">Info about the animated transition.</param>
        /// <returns>false if a <see cref="NavigationFailed"/> event handler has set Handled to true; 
        /// otherwise, true.</returns>
        public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            return NavigateCore(new PageStackEntry(sourcePageType, parameter,
                infoOverride), NavigationMode.New);
        }

        /// <summary>
        /// TODO: Implement this method
        /// Causes the Frame to load content represented by the specified Page, also passing a parameter to be 
        /// interpreted by the target of the navigation.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to its partial class type.</param>
        /// <param name="parameter">The navigation parameter to pass to the target page; must have a basic type 
        /// (string, char, numeric, or GUID) to support parameter serialization using GetNavigationState.</param>
        /// <param name="navOptions">Options for the navigation, including whether it is recorded in the navigation stack 
        /// and what transition animation is used.</param>
        /// <returns>false if a <see cref="NavigationFailed"/> event handler has set Handled to true; otherwise, true.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool NavigateToType(Type sourcePageType, object parameter, FrameNavigationOptions navOptions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: implement this method
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetNavigationState() => throw new NotImplementedException();

        /// <summary>
        /// TODO: Implement this method
        /// </summary>
        /// <param name="navState"></param>
        public void SetNavigationState(string navState) { }

        /// <summary>
        /// TODO: Implement this method
        /// </summary>
        /// <param name="navState"></param>
        /// <param name="suppressNavigate"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetNavigationState(string navState, bool suppressNavigate) 
        {
            throw new NotImplementedException();
        }

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
                
                if (mode == NavigationMode.New)
                {
                    //Check if we already have an instance of the page in the cache
                    entry.Instance = CheckCacheAndGetPage(entry.SourcePageType);
                }

                if (entry.Instance == null)
                {
                    var page = CreatePageAndCacheIfNecessary(entry.SourcePageType);
                    if (page == null)
                    {
                        throw new ArgumentException($"The type {entry.SourcePageType} is not a valid page type.");
                    }

                    entry.Instance = page;
                }

                CurrentEntry = entry;
                SetContentAndAnimate(entry);

                if (IsNavigationStackEnabled)
                {
                    switch (mode)
                    {
                        case NavigationMode.New:
                            ForwardStack.Clear();
                            if (prevEntry != null)
                            {
                                if (BackStack.Count == CacheSize)
                                {
                                    if (BackStack.Count > 0)
                                    {
                                        BackStack.RemoveAt(0);
                                    }    
                                }

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
    }   
}
