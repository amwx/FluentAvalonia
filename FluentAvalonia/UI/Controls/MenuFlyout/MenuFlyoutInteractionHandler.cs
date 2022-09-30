using Avalonia.Controls.Platform;
using Avalonia.Controls;
using Avalonia.Input.Raw;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.ComponentModel;
using Avalonia.Platform;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides the default keyboard and pointer interaction for menus.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[Browsable(false)]
internal class MenuFlyoutInteractionHandler : IMenuInteractionHandler
{
    private IDisposable _inputManagerSubscription;
    private IRenderRoot _root;

    public MenuFlyoutInteractionHandler(bool isContextMenu)
        : this(Avalonia.Input.InputManager.Instance, DefaultDelayRun)
    {
    }

    public MenuFlyoutInteractionHandler(
        IInputManager inputManager,
        Action<Action, TimeSpan> delayRun)
    {
        delayRun = delayRun ?? throw new ArgumentNullException(nameof(delayRun));

        InputManager = inputManager;
        DelayRun = delayRun;
    }

    void IMenuInteractionHandler.Attach(IMenu menu) => Attach((FAMenuFlyoutPresenter)menu);

    public virtual void Attach(FAMenuFlyoutPresenter menu)
    {
        if (Menu != null)
        {
            throw new NotSupportedException("DefaultMenuInteractionHandler is already attached.");
        }

        Menu = menu;
        Menu.GotFocus += GotFocus;
        Menu.LostFocus += LostFocus;
        Menu.KeyDown += KeyDown;
        Menu.PointerPressed += PointerPressed;
        Menu.PointerReleased += PointerReleased;
        Menu.AddHandler(AccessKeyHandler.AccessKeyPressedEvent, AccessKeyPressed);
        Menu.AddHandler(MenuBase.MenuOpenedEvent, MenuOpened);
        Menu.AddHandler(MenuItem.PointerEnteredItemEvent, PointerEnter);
        Menu.AddHandler(MenuItem.PointerExitedItemEvent, PointerLeave);

        _root = Menu.GetVisualRoot();

        if (_root is InputElement inputRoot)
        {
            inputRoot.AddHandler(InputElement.PointerPressedEvent, RootPointerPressed, RoutingStrategies.Tunnel);
        }

        if (_root is WindowBase window)
        {
            window.Deactivated += WindowDeactivated;
        }

        if (_root is TopLevel tl && tl.PlatformImpl != null)
            tl.PlatformImpl.LostFocus += TopLevelLostPlatformFocus;

        _inputManagerSubscription = InputManager?.Process.Subscribe(RawInput);
    }

    public virtual void Detach(IMenu menu)
    {
        if (Menu != menu)
        {
            throw new NotSupportedException("DefaultMenuInteractionHandler is not attached to the menu.");
        }

        Menu.GotFocus -= GotFocus;
        Menu.LostFocus -= LostFocus;
        Menu.KeyDown -= KeyDown;
        Menu.PointerPressed -= PointerPressed;
        Menu.PointerReleased -= PointerReleased;
        Menu.RemoveHandler(AccessKeyHandler.AccessKeyPressedEvent, AccessKeyPressed);
        Menu.RemoveHandler(MenuBase.MenuOpenedEvent, MenuOpened);
        Menu.RemoveHandler(MenuItem.PointerEnteredItemEvent, PointerEnter);
        Menu.RemoveHandler(MenuItem.PointerExitedItemEvent, PointerLeave);

        if (_root is InputElement inputRoot)
        {
            inputRoot.RemoveHandler(InputElement.PointerPressedEvent, RootPointerPressed);
        }

        if (_root is WindowBase root)
        {
            root.Deactivated -= WindowDeactivated;
        }

        if (_root is TopLevel tl && tl.PlatformImpl != null)
            tl.PlatformImpl.LostFocus -= TopLevelLostPlatformFocus;

        _inputManagerSubscription?.Dispose();

        Menu = null;
        _root = null;
    }

    protected Action<Action, TimeSpan> DelayRun { get; }

    protected IInputManager InputManager { get; }

    protected FAMenuFlyoutPresenter Menu { get; private set; }

    protected static TimeSpan MenuShowDelay { get; } = TimeSpan.FromMilliseconds(400);

    protected internal virtual void GotFocus(object sender, GotFocusEventArgs e)
    {
        //var item = GetMenuItem(e.Source as IControl);

        //if (item?.Parent != null)
        //{
        //	item.SelectedItem = item;
        //}
    }

    protected internal virtual void LostFocus(object sender, RoutedEventArgs e)
    {
        //var item = GetMenuItem(e.Source as IControl);

        //if (item != null)
        //{
        //	item.SelectedItem = null;
        //}
    }

    protected internal virtual void KeyDown(object sender, KeyEventArgs e)
    {
        KeyDown(GetMenuItem(e.Source as IControl), e);
    }

    protected internal virtual void KeyDown(IMenuItem item, KeyEventArgs e)
    {
        if (item == null)
            return;

        switch (e.Key)
        {
            case Key.Down:
                {
                    var current = FocusManager.Instance.Current;
                    if (current is MenuFlyoutItemBase mfib && mfib.Parent == Menu)
                    {
                        var index = Menu.ItemContainerGenerator.IndexFromContainer(mfib);

                        if (index == -1) // something's wrong
                            return;

                        while (true)
                        {
                            index += 1;
                            if (index >= Menu.ItemCount)
                                index = 0;

                            var cont = Menu.ItemContainerGenerator.ContainerFromIndex(index);

                            if (cont != null && cont.Focusable && cont.IsEffectivelyEnabled)
                            {
                                FocusManager.Instance.Focus(cont, NavigationMethod.Directional);
                                break;
                            }
                            else if (cont == item)
                            {
                                // Failsafe to prevent infinite loop, if nothings focusable for some reason
                                break;
                            }
                        }
                    }

                    e.Handled = true;
                }

                break;

            case Key.Up:
                {
                    var current = FocusManager.Instance.Current;
                    if (current is MenuFlyoutItemBase mfib && mfib.Parent == Menu)
                    {
                        var index = Menu.ItemContainerGenerator.IndexFromContainer(mfib);

                        if (index == -1) // something's wrong
                            return;

                        while (true)
                        {
                            index -= 1;
                            if (index < 0)
                                index = Menu.ItemCount - 1;

                            var cont = Menu.ItemContainerGenerator.ContainerFromIndex(index);

                            if (cont != null && cont.Focusable && cont.IsEffectivelyEnabled)
                            {
                                FocusManager.Instance.Focus(cont, NavigationMethod.Directional);
                                break;
                            }
                            else if (cont == item)
                            {
                                // Failsafe to prevent infinite loop, if nothings focusable for some reason
                                break;
                            }
                        }
                    }

                    e.Handled = true;
                }

                break;

            case Key.Right:
                {
                    if (item.HasSubMenu)
                    {
                        item.Open();
                        e.Handled = true;
                    }
                }
                break;

            case Key.Left:
                {
                    if (item.Parent is MenuFlyoutSubItem mfsi)
                    {
                        Menu.Close();

                        FocusManager.Instance.Focus(item.Parent, NavigationMethod.Directional);
                        e.Handled = true;
                    }
                }
                break;

            case Key.Enter:
                {
                    var current = FocusManager.Instance.Current;
                    if (current is MenuFlyoutItemBase mfib && mfib.Parent == Menu)
                    {
                        if (mfib.Focusable && mfib.IsEffectivelyEnabled)
                        {
                            if (mfib is MenuFlyoutSubItem mfsi)
                            {
                                Open(mfsi, false);
                            }
                            else
                            {
                                Click((IMenuItem)mfib);
                            }
                            e.Handled = true;
                        }
                    }
                }
                break;

            case Key.Escape:
                {
                    Menu.Close();
                    e.Handled = true;
                }
                break;
        }
    }

    protected internal virtual void AccessKeyPressed(object sender, RoutedEventArgs e)
    {
        var item = GetMenuItem(e.Source as IControl);

        if (item == null)
        {
            return;
        }

        if (item.HasSubMenu)
        {
            Open(item, true);
        }
        else
        {
            Click(item);
        }

        e.Handled = true;
    }

    protected internal virtual void PointerEnter(object sender, PointerEventArgs e)
    {
        var item = GetMenuItem(e.Source as IControl);

        if (item?.Parent == null)
        {
            return;
        }

        Menu.SelectedItem = item;

        if (item.HasSubMenu)
        {
            OpenWithDelay(item);
        }
        else
        {
            foreach (var sibling in item.Parent.SubItems)
            {
                if (sibling.IsSubMenuOpen)
                {
                    CloseWithDelay(sibling);
                }
            }
        }
    }

    protected internal virtual void PointerLeave(object sender, PointerEventArgs e)
    {
        var item = GetMenuItem(e.Source as IControl);

        if (item?.Parent == null)
        {
            return;
        }

        if (item.Parent.SelectedItem == item)
        {
            if (!item.HasSubMenu)
            {
                item.Parent.SelectedItem = null;
            }
            else if (!item.IsPointerOverSubMenu)
            {
                DelayRun(() =>
                {
                    if (!item.IsPointerOverSubMenu)
                    {
                        item.IsSubMenuOpen = false;
                    }
                }, MenuShowDelay);
            }
        }
    }

    protected internal virtual void PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var item = GetMenuItem(e.Source as IControl);
        var visual = (IVisual)sender;

        if (e.GetCurrentPoint(visual).Properties.IsLeftButtonPressed && item?.HasSubMenu == true)
        {
            if (item.IsSubMenuOpen)
            {
                if (item.IsTopLevel)
                {
                    CloseMenu(item);
                }
            }
            else
            {
                if (item.IsTopLevel && item.Parent is IMainMenu mainMenu)
                {
                    mainMenu.Open();
                }

                Open(item, false);
            }

            e.Handled = true;
        }
    }

    protected internal virtual void PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        var item = GetMenuItem(e.Source as IControl);

        if (e.InitialPressMouseButton == MouseButton.Left && item?.HasSubMenu == false)
        {
            Click(item);
            e.Handled = true;
        }
    }

    protected internal virtual void MenuOpened(object sender, RoutedEventArgs e)
    {
        if (e.Source == Menu)
        {
            if (Menu.Presenter?.Panel is Panel p)
            {
                for (int i = 0; i < p.Children.Count; i++)
                {
                    if (p.Children[i].Focusable && p.Children[i].IsEffectivelyEnabled)
                    {
                        p.Children[i].Focus();
                        break;
                    }
                }
            }
        }
    }

    protected internal virtual void RawInput(RawInputEventArgs e)
    {
        var mouse = e as RawPointerEventArgs;

        if (mouse?.Type == RawPointerEventType.NonClientLeftButtonDown)
        {
            Menu?.Close();
        }
    }

    protected internal virtual void RootPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (Menu?.IsOpen == true)
        {
            var control = e.Source as ILogical;

            if (!Menu.IsLogicalAncestorOf(control))
            {
                Menu.Close();
            }
        }
    }

    protected internal virtual void WindowDeactivated(object sender, EventArgs e)
    {
        Menu?.Close();
    }

    private void TopLevelLostPlatformFocus()
    {
        Menu?.Close();
    }

    protected void Click(IMenuItem item)
    {
        item.RaiseClick();
        // TODO: uncomment when StaysOpenOnClick is in main package
        //if (!item.StaysOpenOnClick)
        //{
        CloseMenu(item);
        //}
    }

    protected void CloseMenu(IMenuItem item)
    {
        var current = (IMenuElement)item;

        // We change this behavior here b/c in cascading menus, Avalonia runs it MenuItem->MenuItem
        // whereas with the MenuFlyoutPresenter, it runs MenuFlyoutSubItem->MenuFlyoutPresenter->Items...
        // And MenuFlyoutPresenter is an IMenu and causes the old loop to stop preventing any higher level
        // menus from closing. So we iterate the entire Logical tree and explicitly close any IMenu instances
        // Not a fan of the linq, but GetLogicalAncestors returns IEnumerable *cries in I wish everything were lists*
        foreach (var itm in current.GetLogicalAncestors())
        {
            if (itm is IMenu mnu)
            {
                mnu.Close();
            }
        }
    }

    protected void CloseWithDelay(IMenuItem item)
    {
        void Execute()
        {
            if (item.Parent?.SelectedItem != item)
            {
                item.Close();
            }
        }

        DelayRun(Execute, MenuShowDelay);
    }

    protected void Open(IMenuItem item, bool selectFirst)
    {
        item.Open();
    }

    protected void OpenWithDelay(IMenuItem item)
    {
        void Execute()
        {
            if (item.Parent?.SelectedItem == item)
            {
                Open(item, false);
            }
        }

        DelayRun(Execute, MenuShowDelay);
    }

    protected void SelectItemAndAncestors(IMenuItem item)
    {
        var current = item;

        while (current?.Parent != null)
        {
            current.Parent.SelectedItem = current;
            current = current.Parent as IMenuItem;
        }
    }

    protected static IMenuItem GetMenuItem(IControl item)
    {
        while (true)
        {
            if (item == null)
                return null;
            if (item is IMenuItem menuItem)
                return menuItem;
            item = item.Parent;
        }
    }

    private static void DefaultDelayRun(Action action, TimeSpan timeSpan)
    {
        DispatcherTimer.RunOnce(action, timeSpan);
    }
}
