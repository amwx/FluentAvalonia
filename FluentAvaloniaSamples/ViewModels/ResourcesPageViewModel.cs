using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.Styling;

namespace FluentAvaloniaSamples.ViewModels
{
    public class ResourcesPageViewModel : ViewModelBase
    {
        public ResourcesPageViewModel()
        {
        }


        public DataGridCollectionView ResourceView
        {
            get
            {
                if (_colView == null)
                {
                    lock (this)
                    {
                        LoadResources();
                        Dispatcher.UIThread.Post(() => RaisePropertyChanged(nameof(ResourceView)), DispatcherPriority.Background);
                    }
                }

                return _colView;
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (RaiseAndSetIfChanged(ref _searchText, value))
                {
                    // I don't want to regenerate the entire list again, but for some reason Filter isn't resetting the DataGrid
                    // even though all the events are called
                    lock (this)
                    {
                        LoadResources();
                        Dispatcher.UIThread.Post(() => RaisePropertyChanged(nameof(ResourceView)), DispatcherPriority.Background);
                    }
                }
            }
        }

        private async void LoadResources()
        {
            var l = await LoadResourcesForTheme();
            _colView = new DataGridCollectionView(new AvaloniaList<ResourceItemBase>(l))
            {
                Filter = !string.IsNullOrEmpty(_searchText) ? FilterView : null
            };
        }

        private bool FilterView(object arg)
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                return true;
            }

            if (arg is ResourceItemBase rib)
            {
                var res = rib.ResourceKey.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
                return res;
            }

            return false;
        }

        private async Task<List<ResourceItemBase>> LoadResourcesForTheme()
        {
            static void AddResource(IList<ResourceItemBase> l, KeyValuePair<object,object> kvp)
            {
                if (kvp.Value.GetType().IsAssignableTo(typeof(IBrush)))
                {
                    l.Add(new BrushResourceItem { Brush = (IBrush)kvp.Value, ResourceKey = kvp.Key.ToString() });
                }
                else if (kvp.Value.GetType() == typeof(Color))
                {
                    l.Add(new ColorResourceItem { Color = (Color)kvp.Value, ResourceKey = kvp.Key.ToString() });
                }
                else if (kvp.Value.GetType() == typeof(FontFamily))
                {
                    l.Add(new FontFamilyResourceItem { FontFamily = (FontFamily)kvp.Value, ResourceKey = kvp.Key.ToString() });
                }
                else
                {
                    l.Add(new PrimitiveResourceItem { Value = kvp.Value, ResourceKey = kvp.Key.ToString() });
                }
            }

            return await Task.Run(() =>
            {                
                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                var themeResourcesDictionary =
                    thm.GetType().GetField("_themeResources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(thm) as ResourceDictionary;

                var l = new List<ResourceItemBase>();

                // Retreive the hidden System resources from the main resource dictionary
                foreach (var kvp in themeResourcesDictionary)
                {
                    AddResource(l, kvp);
                }

                // Now retreive the base resources in the first merged dictionary
                foreach (var kvp in (themeResourcesDictionary.MergedDictionaries[0] as ResourceDictionary))
                {
                    AddResource(l, kvp);
                }

                // And finally retreive the theme resources in the second merged dictionary
                foreach (var kvp in (themeResourcesDictionary.MergedDictionaries[1] as ResourceDictionary))
                {
                    AddResource(l, kvp);
                }

                return l;
            });
        }


        private DataGridCollectionView _colView;
        private string _currentTheme;
        private string _searchText;
    }
}
