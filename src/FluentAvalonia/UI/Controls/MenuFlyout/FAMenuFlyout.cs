using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a flyout that displays a menu of commands.
/// </summary>
public class FAMenuFlyout : PopupFlyoutBase
{
    public FAMenuFlyout()
    {
        var al = new AvaloniaList<object>();
        al.CollectionChanged += ItemsCollectionChanged;
        Items = al;
    }

    /// <summary>
    /// Defines the <see cref="Items"/> property
    /// </summary>
    public static readonly DirectProperty<FAMenuFlyout, IEnumerable> ItemsProperty =
        ItemsControl.ItemsProperty.AddOwner<FAMenuFlyout>(x => x.Items,
            (x, v) => x.Items = v);

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<FAMenuFlyout>();

    /// <summary>
    /// Defines the <see cref="ItemContainerTheme"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<ControlTheme>();

    /// <summary>
    /// Gets or sets the items of the MenuFlyout
    /// </summary>
    [Content]
    public IEnumerable Items
    {
        get => _items;
        set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    /// <summary>
    /// Gets or sets the template used for the items
    /// </summary>
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="ControlTheme"/> to apply for the items
    /// </summary>
    public ControlTheme ItemContainerTheme
    {
        get => GetValue(ItemContainerThemeProperty);
        set => SetValue(ItemContainerThemeProperty, value);
    }

    internal AvaloniaList<MenuFlyoutItemBase> ItemsInternal => _itemsInternal;

    /// <summary>
    /// Sets the Classes used for styling the MenuFlyoutPresenter. This property
    /// takes the place of WinUI's MenuFlyoutPresenterStyle
    /// </summary>
    public Classes FlyoutPresenterClasses => _classes ??= new Classes();

    protected override Control CreatePresenter()
    {
        return new FAMenuFlyoutPresenter
        {
            Items = _itemsInternal,
            [!ItemContainerThemeProperty] = this[!ItemContainerThemeProperty]
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsProperty)
        {
            var (oldV, newV) = change.GetOldAndNewValue<IEnumerable>();

            if (oldV is INotifyCollectionChanged inccOld)
            {
                inccOld.CollectionChanged -= ItemsCollectionChanged;
            }

            if (_itemsInternal != null)
            {
                _itemsInternal?.Clear();
            }

            if (newV is INotifyCollectionChanged inccNew)
            {
                inccNew.CollectionChanged += ItemsCollectionChanged;
            }

            // If the flyout is open and we receive a new Items list, generate now
            // as we have the presenter reference. Otherwise, it will occur next
            // time the flyout is opened
            if (IsOpen && newV != null)
            {
                GenerateItems(Popup.Child as FAMenuFlyoutPresenter);
            }
        }
        else if (change.Property == ItemTemplateProperty)
        {
            if (_itemsInternal != null)
            {
                _itemsInternal.Clear();

                if (IsOpen)
                {
                    GenerateItems(Popup.Child as FAMenuFlyoutPresenter);
                }                
            }
        }
    }

    protected override void OnOpened()
    {
        var presenter = Popup.Child as FAMenuFlyoutPresenter;
        if (_classes != null)
        {
            SetPresenterClasses(presenter, FlyoutPresenterClasses);
        }

        if (_itemsInternal == null || _itemsInternal.Count == 0)
        {
            GenerateItems(presenter);
        }

        base.OnOpened();

        presenter?.RaiseMenuOpened();
    }

    private static void SetPresenterClasses(Control presenter, Classes classes)
    {
        //Remove any classes no longer in use, ignoring pseudoclasses
        for (int i = presenter.Classes.Count - 1; i >= 0; i--)
        {
            if (!classes.Contains(presenter.Classes[i]) &&
                !presenter.Classes[i].Contains(':'))
            {
                presenter.Classes.RemoveAt(i);
            }
        }

        //Add new classes
        presenter.Classes.AddRange(classes);
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (!IsOpen)
        {
            // If the flyout isn't open we'll just trigger a refresh when the flyout next opens
            // We can't add now b/c we don't have way to resolve item template (possibly)
            _itemsInternal?.Clear();
            return;
        }

        var presenter = Popup.Child as FAMenuFlyoutPresenter;
        var template = ItemTemplate;
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add(args.NewItems, args.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                Remove(args.OldStartingIndex, args.OldItems.Count);
                break;

            case NotifyCollectionChangedAction.Reset:
                _itemsInternal.Clear();
                if (args.NewItems != null)
                {
                    Add(args.NewItems, args.NewStartingIndex);
                }                
                break;

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                Remove(args.OldStartingIndex, args.OldItems.Count);
                Add(args.NewItems, args.NewStartingIndex);
                break;
        }

        void Add(IList items, int startIndex)
        {
            for (int i = 0, idx = startIndex; i < items.Count; i++, idx++)
            {
                var item = items[i];
                _itemsInternal.Insert(idx, CreateContainer(item, presenter.FindDataTemplate(item, template)));
            }
        }

        void Remove(int index, int count)
        {
            _itemsInternal.RemoveRange(index, count);
        }
    }

    private void GenerateItems(FAMenuFlyoutPresenter presenter)
    {
        if (_items == null)
            return;

        _itemsInternal.Clear();

        var first = _items.ElementAt(0);
        // If the first item is a MenuFlyoutItemBase, assume all are (added via xaml or code)
        if (first is MenuFlyoutItemBase)
        {
            _itemsInternal.AddRange(_items.Cast<MenuFlyoutItemBase>());
            return;
        }

        var itemTemplate = ItemTemplate;
        foreach(var item in _items)
        {
            var template = presenter.FindDataTemplate(item, itemTemplate);
            _itemsInternal.Add(CreateContainer(item, template));
        }
    }

    internal static MenuFlyoutItemBase CreateContainer(object item, IDataTemplate template)
    {
        if (item == null)
            return null;

        if (template is MenuFlyoutSubItemTemplate mfsit)
        {
            var mfsi = new MenuFlyoutSubItem
            {
                DataContext = item
            };

            if (mfsit.SubItems != null)
            {
                mfsi.Bind(MenuFlyoutSubItem.ItemsProperty, mfsit.SubItems);
            }

            if (mfsit.HeaderText != null)
            {
                mfsi.Bind(MenuFlyoutItem.TextProperty, mfsit.HeaderText);
            }

            if (mfsit.Icon != null)
            {
                mfsi[!MenuFlyoutItem.IconSourceProperty] = mfsit.Icon; 
            }

            return mfsi;
        }
        else
        {
            // Other templates will do whatever
            var builtContent = template.Build(item);

            if (builtContent is MenuFlyoutItemBase mfibFromTemplate)
            {
                mfibFromTemplate.DataContext = item;
                return mfibFromTemplate;
            }
        }

        // If we reach here, just create a normal MenuFlyoutItem b/c we don't have enough
        // information on how to construct it.
        return new MenuFlyoutItem
        {
            DataContext = item,
            Text = item.ToString()
        };
    }

    private Classes _classes;
    private IEnumerable _items;
    private readonly AvaloniaList<MenuFlyoutItemBase> _itemsInternal = new AvaloniaList<MenuFlyoutItemBase>();
}
