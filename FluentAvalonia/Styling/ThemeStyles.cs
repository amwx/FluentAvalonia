using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
//using DynamicData.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FluentAvalonia.Styling
{
    public class ThemeStyles : AvaloniaObject, IAvaloniaList<IStyle>, IStyle, IResourceProvider
    {
        public ThemeStyles()
        {
            _themeDictionaries = new AvaloniaDictionary<object, IResourceDictionary>();
            _themeDictionaries.CollectionChanged += OnThemeDictionariesChanged;
            _styles = new List<IStyle>();
        }

        public ThemeStyles(IResourceHost owner)
            :this()
        {
            Owner = owner;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler OwnerChanged;

        public IStyle this[int index]
        {
            get => _styles[index];
            set
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException();

                _styles[index] = value;
            }
        }

        public IReadOnlyList<IStyle> Children => _styles;

        public IResourceDictionary? ThemeResources
        {
            get
            {
                if (CurrentTheme == null)
                    return null;

                return _themeDictionaries[CurrentTheme];
            }
        }

        public IResourceDictionary CommonResources
        {
            get => _commonResources ?? (CommonResources = new ResourceDictionary());
            set
            {
                value = value ?? throw new ArgumentNullException(nameof(CommonResources));

                if (Owner is object)
                {
                    _commonResources?.RemoveOwner(Owner);
                }

                _commonResources = value;

                if (Owner is object)
                {
                    _commonResources?.AddOwner(Owner);
                }
            }
        }

        public int Count => _styles.Count;
        bool ICollection<IStyle>.IsReadOnly => false;
        bool IResourceNode.HasResources
        {
            get
            {
                if (_themeDictionaries.Count == 0)
                    return false;
                
                foreach(KeyValuePair<object,IResourceDictionary> kvp in _themeDictionaries)
                {
                    if (kvp.Value.Count > 0)
                        return true;
                }

                for (int i = 0; i < Count; i++)
                {
                    if (this[i] is IResourceProvider p && p.HasResources)
                        return true;
                }

                return false;
            }
        }

        public IResourceHost Owner
        {
            get => _host;
            private set
            {
                if (_host != value)
                {
                    _host = value;
                    OwnerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public AvaloniaDictionary<object, IResourceDictionary> ThemeDictionaries => _themeDictionaries;

        public object CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
                }
            }
        }

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost host)
        {
            _cache ??= new Dictionary<Type, List<IStyle>>();

            if (_cache.TryGetValue(target.StyleKey, out var cached))
            {
                if (cached is object)
                {
                    for (int i = 0; i < cached.Count; i++)
                    {
                        cached[i].TryAttach(target, host);
                    }

                    return SelectorMatchResult.AlwaysThisType;
                }
                else
                {
                    return SelectorMatchResult.NeverThisType;
                }
            }
            else
            {
                List<IStyle> matches = null;

                for (int i = 0; i < Count; i++)
                {
                    if (this[i].TryAttach(target, host) != SelectorMatchResult.NeverThisType)
                    {
                        matches ??= new List<IStyle>();
                        matches.Add(this[i]);
                    }
                }

                _cache.Add(target.StyleKey, matches);

                return matches is null ? SelectorMatchResult.NeverThisType : SelectorMatchResult.AlwaysThisType;
            }
        }

        public bool TryGetResource(object key, out object? value)
        {
            if (CurrentTheme == null)
            {
                value = null;
                return false;
            }

            if (_themeDictionaries.ContainsKey(CurrentTheme))
            {
                if (_themeDictionaries[CurrentTheme].TryGetResource(key, out value))
                {
                    return true;
                }
            }

            if (CommonResources.TryGetResource(key, out value))
            {
                return true;
            }

            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i] is IResourceProvider p && p.TryGetResource(key, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        void IResourceProvider.AddOwner(IResourceHost owner)
        {
            owner = owner ?? throw new ArgumentNullException(nameof(owner));

            if (Owner != null)
            {
                throw new InvalidOperationException("Already have an owner");
            }

            Owner = owner;

            foreach(KeyValuePair<object, IResourceDictionary> kvp in _themeDictionaries)
            {
                kvp.Value.AddOwner(owner);
            }

            CommonResources.AddOwner(owner);

            for (int i = 0; i < Count; i++)
            {
                if (this[i] is IResourceProvider p)
                {
                    p.AddOwner(owner);
                }
            }
        }

        void IResourceProvider.RemoveOwner(IResourceHost owner)
        {
            owner = owner ?? throw new ArgumentNullException(nameof(owner));

            if (Owner == owner)
            {
                Owner = null;

                foreach (KeyValuePair<object, IResourceDictionary> kvp in _themeDictionaries)
                {
                    kvp.Value.RemoveOwner(owner);
                }

                CommonResources.RemoveOwner(owner);

                for (int i = 0; i < Count; i++)
                {
                    if (this[i] is IResourceProvider p)
                    {
                        p.RemoveOwner(owner);
                    }
                }

            }
        }


        

        private void OnThemeDictionariesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is IResourceDictionary d)
                        {
                            d.AddOwner(_host);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i <e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is IResourceDictionary d)
                        {
                            d.RemoveOwner(_host);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is IResourceDictionary d)
                        {
                            d.RemoveOwner(_host);
                        }
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.OldItems[i] is IResourceDictionary d)
                        {
                            d.AddOwner(_host);
                        }
                    }
                    break;
            }
        }

        public void Add(IStyle item)
        {
            _styles.Add(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { item }, _styles.Count - 1);
        }

        public void AddRange(IEnumerable<IStyle> items)
        {
            InsertRange(_styles.Count, items);
        }

        public void Clear()
        {
            _styles.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, _styles, 0);
        }

        public bool Contains(IStyle item)
        {
            return _styles.Contains(item);
        }

        public void CopyTo(IStyle[] array, int index)
        {
            _styles.CopyTo(array, index);
        }

        public int IndexOf(IStyle item)
        {
            return _styles.IndexOf(item);
        }

        public void Insert(int index, IStyle item)
        {
            _styles.Insert(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { item }, index);
        }

        public void InsertRange(int index, IEnumerable<IStyle> items)
        {
            _styles.InsertRange(index, items);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToList(), index);
        }

        public void Move(int oldIndex, int newIndex)
        {
            var item = this[oldIndex];
            _styles.RemoveAt(oldIndex);
            _styles.Insert(newIndex, item);

            CollectionChanged?.Invoke(this, 
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        public void MoveRange(int oldIndex, int count, int newIndex)
        {
            var items = _styles.GetRange(oldIndex, count);
            var index = newIndex;
            _styles.RemoveRange(oldIndex, count);

            if (newIndex > oldIndex)
            {
                index -= count;
            }

            _styles.InsertRange(index, items);

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
        }

        public bool Remove(IStyle item)
        {
            int index = _styles.IndexOf(item);
            if (index != -1)
            {
                _styles.RemoveAt(index);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { item }, index);
                return true;
            }
            return false;
        }

        public void RemoveAll(IEnumerable<IStyle> items)
        {
            foreach(var i in items)
            {
                Remove(i);
            }
        }

        public void RemoveAt(int index)
        {
            IStyle item = _styles[index];
            _styles.RemoveAt(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { item }, index);
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                var list = _styles.GetRange(index, count);
                _styles.RemoveRange(index, count);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, list, index);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int index)
        {
            CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(action, items, index));
        }

        public IEnumerator<IStyle> GetEnumerator()
        {
            return _styles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _styles.GetEnumerator();
        }

        private object _currentTheme;
        private List<IStyle> _styles;
        private IResourceHost _host;
        private AvaloniaDictionary<object, IResourceDictionary> _themeDictionaries;
        private Dictionary<Type, List<IStyle>> _cache;
        private IResourceDictionary _commonResources;
    }
}
