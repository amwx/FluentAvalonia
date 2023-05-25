using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;

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
    public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<FAMenuFlyout>();

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
    /// Defines the <see cref="FlyoutPresenterTheme"/> property
    /// </summary>
    public static readonly StyledProperty<ControlTheme> FlyoutPresenterThemeProperty =
        AvaloniaProperty.Register<FAMenuFlyout, ControlTheme>(nameof(FlyoutPresenterTheme));

    /// <summary>
    /// Gets the items of the MenuFlyoutSubItem
    /// </summary>
    /// <remarks>
    /// NOTE: Unlike normal ItemsControls, when ItemsSource is set, this property will
    /// not act as a view over the ItemsSource
    /// </remarks>
    [Content]
    public IList Items { get; private set; }

    /// <summary>
    /// Gets or sets the items of the MenuFlyout
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
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

    /// <summary>
    /// Sets the Classes used for styling the MenuFlyoutPresenter. This property
    /// takes the place of WinUI's MenuFlyoutPresenterStyle
    /// </summary>
    public Classes FlyoutPresenterClasses => _classes ??= new Classes();

    /// <summary>
    /// Gets or sets the ControlTheme for the flyout presenter
    /// </summary>
    public ControlTheme FlyoutPresenterTheme
    {
        get => GetValue(FlyoutPresenterThemeProperty);
        set => SetValue(FlyoutPresenterThemeProperty, value);
    }

    internal void Close()
    {
        Hide();
    }

    protected override Control CreatePresenter()
    {
        _presenter = new FAMenuFlyoutPresenter
        {
            ItemsSource = ItemsSource ?? Items,
            [!ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
            [!ItemTemplateProperty] = this[!ItemTemplateProperty],
            InternalParent = this
        };

        return _presenter;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (Items.Count > 0)
            {
                throw new InvalidOperationException("Items collection must be empty before using ItemsSource.");
            }

            var newV = change.GetNewValue<IEnumerable>();

            if (_presenter != null)
            {
                _presenter.ItemsSource = newV ?? Items;
            }
        }
    }

    protected override void OnOpened()
    {
        if (_classes != null)
        {
            SetPresenterClasses(_presenter, FlyoutPresenterClasses);
        }

        var theme = FlyoutPresenterTheme;
        if (theme != null)
        {
            _presenter.Theme = theme;
        }

        base.OnOpened();

        _presenter.MenuOpened();
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
        if (ItemsSource != null)
        {
            throw new InvalidOperationException("Cannot edit Items when ItemsSource is set.");
        }
        //if (!IsOpen)
        //{
        //    // If the flyout isn't open we'll just trigger a refresh when the flyout next opens
        //    // We can't add now b/c we don't have way to resolve item template (possibly)
        //    _itemsInternal?.Clear();
        //    return;
        //}

        //var presenter = Popup.Child as FAMenuFlyoutPresenter;
        //var template = ItemTemplate;
        //switch (args.Action)
        //{
        //    case NotifyCollectionChangedAction.Add:
        //        Add(args.NewItems, args.NewStartingIndex);
        //        break;

        //    case NotifyCollectionChangedAction.Remove:
        //        Remove(args.OldStartingIndex, args.OldItems.Count);
        //        break;

        //    case NotifyCollectionChangedAction.Reset:
        //        _itemsInternal.Clear();
        //        if (args.NewItems != null)
        //        {
        //            Add(args.NewItems, args.NewStartingIndex);
        //        }                
        //        break;

        //    case NotifyCollectionChangedAction.Replace:
        //    case NotifyCollectionChangedAction.Move:
        //        Remove(args.OldStartingIndex, args.OldItems.Count);
        //        Add(args.NewItems, args.NewStartingIndex);
        //        break;
        //}

        //void Add(IList items, int startIndex)
        //{
        //    for (int i = 0, idx = startIndex; i < items.Count; i++, idx++)
        //    {
        //        var item = items[i];
        //        _itemsInternal.Insert(idx, CreateContainer(item, presenter.FindDataTemplate(item, template)));
        //    }
        //}

        //void Remove(int index, int count)
        //{
        //    _itemsInternal.RemoveRange(index, count);
        //}
    }

   
    private FAMenuFlyoutPresenter _presenter;
    private Classes _classes;
}
