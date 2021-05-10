using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
    public class MenuFlyout : FlyoutBase//, IMenu
    {
        public MenuFlyout()
        {
            _items = new AvaloniaList<MenuFlyoutItemBase>();
            
        }

        public static readonly DirectProperty<MenuFlyout, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<MenuFlyout, IEnumerable>("Items",
                x => x.Items, (x, v) => x.Items = v);

        [Content]
        public IEnumerable Items
        {
            get => _items;
            set => SetAndRaise(ItemsProperty, ref _items, value);
        }

        protected override Control CreatePresenter()
        {
            return new MenuFlyoutPresenter
            {
                [!ItemsControl.ItemsProperty] = this[!ItemsProperty]
            };
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if(change.Property == ItemsProperty)
            {
                //OnItemsChanged(change.OldValue.GetValueOrDefault<IEnumerable>(), change.NewValue.GetValueOrDefault<IEnumerable>());
            }
        }

        //protected virtual void OnItemsChanged(IEnumerable oldItems, IEnumerable newItems)
        //{
        //    if (oldItems is INotifyCollectionChanged oldNC)
        //        oldNC.CollectionChanged -= OnItemsCollectionChanged;

        //    if (newItems is INotifyCollectionChanged newNC)
        //        newNC.CollectionChanged += OnItemsCollectionChanged;

        //    if (newItems != null)
        //    {
        //        _itemsDirty = true;
        //        UpdateItemsVisualState();
        //    }               
        //}

        //protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        //{
        //    _itemsDirty = true;
        //    UpdateItemsVisualState();
        //}


        protected override void OnOpened()
        {
            base.OnOpened();
            //Popup.Child?.Focus();
            FocusManager.Instance.Focus(Popup.Child, NavigationMethod.Unspecified, KeyModifiers.None);
        }

        private IEnumerable _items;
    }

    public interface IMenuPresenter
    {
        IMenuPresenter ParentPresenter { get; }

        void Close();
    }

    public class MenuFlyoutPresenter : ItemsControl, IStyleable, IMenuPresenter
    {
        public MenuFlyoutPresenter()
        {

        }
        
        public IMenuPresenter ParentPresenter
        {
            get
            {
                if(_parentPresenter != null && _parentPresenter.TryGetTarget(out IMenuPresenter target))
                {
                    return target;
                }
                return null;
            }
        }

        public IMenuFlyoutSubItem SubMenuOwner { get; set; }

        public bool IsOpen
        {
            get
            {
                if(Parent is Popup p)
                {
                    return p.IsOpen;
                }
                else
                {
                    var prnt = this.FindLogicalAncestorOfType<Popup>();
                    if (prnt != null)
                        return prnt.IsOpen;
                }

                return false;
            }
            set
            {
                if(Parent is Popup p)
                {
                    p.IsOpen = value;
                }
                else
                {
                    var prnt = this.FindLogicalAncestorOfType<Popup>();
                    if (prnt != null)
                        prnt.IsOpen = value;
                }
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);

            if(e.NewValue != null)
            {
                ResetItemStates();
            }
            else
            {
                _hasChecks = false;
                _hasIcons = false;
            }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            ResetItemStates();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    CycleFocus(true, NavigationMethod.Directional);
                    e.Handled = true;
                    break;

                case Key.Up:
                    CycleFocus(false, NavigationMethod.Directional);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    //Don't close entire tree, just this menu
                    IsOpen = false;
                    FocusManager.Instance.Focus(SubMenuOwner as IInputElement, NavigationMethod.Directional);
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);

            if(_focusIndex == -1) //The menu was just opened, focus first item
            {
                CycleFocus(true, e.NavigationMethod);
            }
            else
            {
                //Child element got focus, keep _focusIndex in sync
                var focus = FocusManager.Instance?.Current;

                if(focus != null)
                {
                    var index = ItemContainerGenerator.IndexFromContainer(focus as IControl);
                    if (index != -1) 
                    {
                        _focusIndex = index;
                    }
                }
            }
            
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            //Close all menus (if cascading)
            CancelCloseItem(); //Just in case

            if(_openItem != null)
            {
                _openItem.Close();
                _openItem = null;
            }

            IsOpen = false;
            ParentPresenter?.Close();

            _focusIndex = -1;
        }

        public void Open()
        {
            //if (IsOpen)
            //    return;

            //IsOpen = true;

            //RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs
            //{
            //    RoutedEvent = MenuOpenedEvent,
            //    Source = this
            //});
        }

        internal void CloseLightly()
        {            
            if (!IsOpen)
                return;

            //Close all menus (if cascading)
            CancelCloseItem(); //Just in case

            if (_openItem != null)
            {
                _openItem.Close();
                _openItem = null;
            }

            IsOpen = false;
            _focusIndex = -1;
        }

        internal void SetParentPresenter(IMenuPresenter parent) 
        {
            _parentPresenter = new WeakReference<IMenuPresenter>(parent);
        }

        internal void ShowSubItem(IMenuFlyoutSubItem item)
        {
            if(_openItem == null)
            {
                RequestOpenItem(item);
            }
            else
            {
                RequestCloseItem();
                _openItem = item;
            }
        }

        internal void ShowSubItemImmediate(IMenuFlyoutSubItem item)
        {
            _openItem = item;
            item?.Open();
        }

        internal void CancelOpenItem()
        {
            _subMenuTimer?.Stop();
            _openItem = null;
        }

        internal void CancelCloseItem()
        {
            _subMenuTimer?.Stop();
        }

        protected internal bool HasItemWithIcon
        {
            get
            {
                if (_itemsDirty)
                    ResetItemStates();

                return _hasIcons;
            }
        }

        protected internal bool HasItemWithCheck
        {
            get
            {
                if (_itemsDirty)
                    ResetItemStates();

                return _hasChecks;
            }
        }

        private void ResetItemStates()
        {
            bool ico = false;
            bool check = false;

            //Currently not going to support binding items to this
            //That introduces more problems that I don't wanna deal
            //with right now (dealing with ItemContainerGenerator)
            //Also going to force anything of IList, it's just easier
            if(Items is IList<MenuFlyoutItemBase> l)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    if (l[i] is IToggleMenuFlyoutItem tmfi)
                    {
                        check = true;
                        if (ico == false && tmfi.Icon != null)
                            ico = true;
                    }
                    else if (l[i] is IMenuFlyoutItem mfi)
                    {
                        if (ico == false && mfi.Icon != null)
                            ico = true;
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Items must a IList<MenuFlyoutItemBase>");
            }      

            _hasChecks = check;
            _hasIcons = ico;
            Debug.WriteLine($"Item States C {_hasChecks} >> I {_hasIcons}");

            _itemsDirty = false;
        }

        private void RequestOpenItem(IMenuFlyoutSubItem item)
        {
            if (item == null && _openItem == null || item == _openItem)
                return;
            
            if (_subMenuTimer == null)
            {
                _subMenuTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Background, OnSubMenuTimerTick);
            }

            if (_oldItem == null && _subMenuTimer.IsEnabled)
                CancelOpenItem();

            if (_oldItem == item)
                CancelCloseItem();

            if(_openItem != null)
            {
                RequestCloseItem();
            }

            _openItem = item;
            _subMenuTimer.Start();
        }

        private void RequestCloseItem()
        {
            if (_subMenuTimer != null && _subMenuTimer.IsEnabled)
            {
                return;
            }
                
            _oldItem = _openItem;

            if (_subMenuTimer == null)
            {
                _subMenuTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Background, OnSubMenuTimerTick);
            }

            _subMenuTimer.Start();
        }

        private void OnSubMenuTimerTick(object sender, EventArgs e)
        {
            if (_oldItem != null)
            {
                _oldItem.Close();
                _oldItem = null;

                _openItem?.Open();
            }
            else
            { 
                _openItem?.Open(); 
            }
            _subMenuTimer.Stop();//This only executes one at a time
        }

        private void CycleFocus(bool focusDown, NavigationMethod method)
        {
            if (ItemCount == 0)
                return;
            int nextIndex = _focusIndex + (focusDown ? 1 : -1);
            if (nextIndex < 0)
                nextIndex = ItemCount - 1;
            if (nextIndex == ItemCount)
                nextIndex = 0;

            IControl container = ItemContainerGenerator.ContainerFromIndex(nextIndex);
            if (container != null && container.Focusable) //Separators aren't focusable
            {
                FocusManager.Instance.Focus(container, method, KeyModifiers.None);
            }
            else if (container != null)
            {
                if (focusDown)
                {
                    while(container != null && !container.Focusable)
                    {
                        nextIndex++;
                        if (nextIndex == ItemCount)
                            nextIndex = 0;
                        container = ItemContainerGenerator.ContainerFromIndex(nextIndex);
                    }
                }
                else
                {
                    while (container != null && !container.Focusable)
                    {
                        nextIndex--;
                        if (nextIndex == 0)
                            nextIndex = ItemCount - 1;
                        container = ItemContainerGenerator.ContainerFromIndex(nextIndex);
                    }
                }

                FocusManager.Instance.Focus(container, method, KeyModifiers.None);
            }
            _focusIndex = nextIndex;
        }

        IMenuFlyoutSubItem _openItem;
        IMenuFlyoutSubItem _oldItem;

        private int _focusIndex = -1;
        bool _itemsDirty = true;
        private bool _hasIcons;
        private bool _hasChecks;
        private WeakReference<IMenuPresenter> _parentPresenter;
        private DispatcherTimer _subMenuTimer;
    }

    public class MenuFlyoutItemBase : TemplatedControl//, ISelectable
    {
        static MenuFlyoutItemBase()
        {
            PressedMixin.Attach<MenuFlyoutItemBase>();
        }

        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            AvaloniaProperty.Register<MenuFlyoutItemBase, HorizontalAlignment>("HorizontalContentAlignment");

        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            AvaloniaProperty.Register<MenuFlyoutItemBase, VerticalAlignment>("VerticalContentAlignment");

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }


        public MenuFlyoutPresenter ParentPresenter
        {
            get
            {
                if (_parentPresenter != null && _parentPresenter.TryGetTarget(out MenuFlyoutPresenter mfp))
                    return mfp;

                return null;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var p = e.Parent is MenuFlyoutPresenter mfp ? mfp : this.FindAncestorOfType<MenuFlyoutPresenter>();
            _parentPresenter = new WeakReference<MenuFlyoutPresenter>(p);
        }

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            if (Focusable)
                Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if(ParentPresenter != null && ParentPresenter.SubMenuOwner != null)
                    {
                        //Only close our current Menu
                        ParentPresenter.CloseLightly();
                        FocusManager.Instance.Focus(ParentPresenter.SubMenuOwner as IInputElement, NavigationMethod.Directional);
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        private WeakReference<MenuFlyoutPresenter> _parentPresenter;
    }

    public interface IMenuFlyoutItem 
    {
        string Text { get; set; }
        IconElement Icon { get; set; }

        //void Invoke();
    }

    public interface IToggleMenuFlyoutItem : IMenuFlyoutItem
    {
        bool IsChecked { get; set; }
    }

    public interface IMenuFlyoutSubItem : IMenuFlyoutItem
    {
        bool IsSubMenuOpen { get; }

        void Open();
        void Close();
    }

    public class MenuFlyoutItem : MenuFlyoutItemBase, IMenuFlyoutItem, ISelectable
    {
        static MenuFlyoutItem()
        {
            FocusableProperty.OverrideDefaultValue<MenuFlyoutItem>(true);
        }

        public static readonly DirectProperty<MenuFlyoutItem, ICommand> CommandProperty =
            Button.CommandProperty.AddOwner<MenuFlyoutItem>(x => x.Command,
                (x, v) => x.Command = v);

        public static readonly StyledProperty<object> CommandParameterProperty =
            Button.CommandParameterProperty.AddOwner<MenuFlyoutItem>();

        public static readonly StyledProperty<IconElement> IconProperty =
            AvaloniaProperty.Register<MenuFlyoutItem, IconElement>("Icon");

        public static readonly StyledProperty<KeyGesture> InputGestureProperty =
            AvaloniaProperty.Register<MenuFlyoutItem, KeyGesture>("InputGesture");

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<MenuFlyoutItem, bool>("IsSelected");

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MenuFlyoutItem, string>("Text");


        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<MenuFlyoutItem, RoutedEventArgs>("Click", RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
                
        public event EventHandler<RoutedEventArgs> Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    _isEnterDown = true;
                    PseudoClasses.Set(":pressed", true);
                    e.Handled = true;
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if(e.Key == Key.Enter && _isEnterDown)
            {
                _isEnterDown = false;
                e.Handled = true;
                Invoke();
            }
            PseudoClasses.Set(":pressed", false);
            base.OnKeyUp(e);
        }

        public ICommand Command
        {
            get => _command;
            set => SetAndRaise(CommandProperty, ref _command, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public IconElement Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public KeyGesture InputGesture
        {
            get => GetValue(InputGestureProperty);
            set => SetValue(InputGestureProperty, value);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            
            UpdateVisualState();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if(change.Property == InputGestureProperty)
            {
                PseudoClasses.Set(":gesture", change.NewValue.GetValueOrDefault() != null);
            }
        }

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            if (e.Handled)
                return;
                        
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            ParentPresenter?.ShowSubItem(null);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {            
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isPressed = true;
                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {            
            if(_isPressed && e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                Invoke();
                e.Handled = true;
            }
            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            //Pointer leave doesn't fire if the pointer is currently pressed
            //So we need to handle it here
            if (_isPressed && !(new Rect(Bounds.Size).Contains(e.GetCurrentPoint(this).Position)))
            {
                _isPressed = false;
            }            
        }

        internal virtual void Invoke()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));

            if(Command != null)
            {
                if (Command.CanExecute(CommandParameter))
                    Command.Execute(CommandParameter);
            }

            ParentPresenter?.Close();
        }

        private void UpdateVisualState()
        {
            var ico = ParentPresenter?.HasItemWithIcon ?? false;
            var ch = ParentPresenter?.HasItemWithCheck ?? false;

            PseudoClasses.Set(":iconplaceholder", ico && !ch);
            PseudoClasses.Set(":checkplaceholder", !ico && ch);
            PseudoClasses.Set(":checkandiconplaceholder", ico && ch);
        }

        private bool _isEnterDown;
        private bool _isPressed;
        private ICommand _command;
    }

    public class ToggleMenuFlyoutItem : MenuFlyoutItem, IToggleMenuFlyoutItem
    {

        public static readonly StyledProperty<bool> IsCheckedProperty =
            AvaloniaProperty.Register<ToggleMenuFlyoutItem, bool>("IsChecked");

        public bool IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if(change.Property == IsCheckedProperty)
            {
                var newVal = change.NewValue.GetValueOrDefault<bool>();
                PseudoClasses.Set(":checked", newVal);
            }
        }

        internal override void Invoke()
        {
            IsChecked = !IsChecked;
            base.Invoke();
        }

    }


    public class MenuFlyoutSubItem : MenuFlyoutItemBase, IMenuFlyoutSubItem
    {
        public MenuFlyoutSubItem()
        {
            PseudoClasses.Add(":empty");
            _items = new AvaloniaList<MenuFlyoutItemBase>();
            (_items as INotifyCollectionChanged).CollectionChanged += OnItemsChanged;
        }

        static MenuFlyoutSubItem()
        {
            FocusableProperty.OverrideDefaultValue<MenuFlyoutSubItem>(true);
        }

        public static readonly StyledProperty<IconElement> IconProperty =
            AvaloniaProperty.Register<MenuFlyoutSubItem, IconElement>("Icon");

        public static readonly DirectProperty<MenuFlyoutSubItem, IList<MenuFlyoutItemBase>> ItemsProperty =
            AvaloniaProperty.RegisterDirect<MenuFlyoutSubItem, IList<MenuFlyoutItemBase>>("Items",
                x => x.Items);

        public static readonly StyledProperty<string> TextProperty =
           AvaloniaProperty.Register<MenuFlyoutSubItem, string>("Text");
        
        public IconElement Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        [Content]
        public IList<MenuFlyoutItemBase> Items
        {
            get => _items;
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }


        public bool IsSubMenuOpen => _isSubMenuOpen;


        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            UpdateVisualState();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            //_parentPresenter = null;
        }

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            ParentPresenter?.ShowSubItem(this);
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            _lastNavMethod = e.NavigationMethod;            
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Open();
                    e.Handled = true;
                    break;

                case Key.Right:
                    ParentPresenter?.ShowSubItemImmediate(this);
                    break;
            }
            base.OnKeyDown(e);
        }



        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PseudoClasses.Set(":empty", _items == null || (_items != null && _items.Count == 0));
        }

        private void UpdateVisualState()
        {
            var ico = ParentPresenter?.HasItemWithIcon ?? false;
            var ch = ParentPresenter?.HasItemWithCheck ?? false;

            PseudoClasses.Set(":iconplaceholder", ico && !ch);
            PseudoClasses.Set(":checkplaceholder", !ico && ch);
            PseudoClasses.Set(":checkandiconplaceholder", ico && ch);
        }

        private void CreatePopup()
        {
            if(_subMenuPopup != null)
            {
                _subMenuPopup.Opened -= OnSubMenuOpened;
                _subMenuPopup.Closed -= OnSubMenuClosed;
            }

            _subMenuPopup = new Popup
            {
                WindowManagerAddShadowHint = false,
                IsLightDismissEnabled = true,
                PlacementTarget = this,
                PlacementMode = PlacementMode.Right
            };

            _subMenuPopup.Opened += OnSubMenuOpened;
            _subMenuPopup.Closed += OnSubMenuClosed;

            _subPresenter = new MenuFlyoutPresenter();
            _subPresenter.SetParentPresenter(ParentPresenter);
            _subPresenter.SubMenuOwner = this;

            ((ISetLogicalParent)_subMenuPopup).SetParent(this);

            //Popup won't have a shadow, there were some issues where mouseevents would stop
            //working after embedding the subpresenter in a Border to apply the BoxShadow
            //Not sure what's going on...
            //Possible solution would be to use a Flyout here instead of popup, but not sure
            //how much that would complicate things... TODO
            _subMenuPopup.Child = _subPresenter;
            _subPresenter[!ItemsControl.ItemsProperty] = this[!ItemsProperty];
        }


        public void Open()
        {
            if (_subMenuPopup == null || ParentPresenter != _subPresenter.ParentPresenter)
            {
                CreatePopup();
            }

            _subMenuPopup.IsOpen = true;
        }

        public void Close()
        {
            _subPresenter?.CloseLightly();
        }


        private void OnSubMenuOpened(object sender, EventArgs e)
        {
            _isSubMenuOpen = true;
            PseudoClasses.Set(":open", true);
            FocusManager.Instance.Focus(_subPresenter, _lastNavMethod);
        }

        private void OnSubMenuClosed(object sender, EventArgs e)
        {
            _isSubMenuOpen = false;
            PseudoClasses.Set(":open", false);
        }

        private NavigationMethod _lastNavMethod;
        //private bool _isEnterDown;
        private bool _isSubMenuOpen = false;
        private Popup _subMenuPopup;
        private MenuFlyoutPresenter _subPresenter;        
        private IList<MenuFlyoutItemBase> _items;
    }

    public class MenuFlyoutSeparator : MenuFlyoutItemBase
    {
       
    }
}
