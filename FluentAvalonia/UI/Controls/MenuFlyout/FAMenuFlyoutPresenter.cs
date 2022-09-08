using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Displays the content of a <see cref="FAMenuFlyout"/> control.
/// </summary>
public class FAMenuFlyoutPresenter : MenuBase, IStyleable
{
    public FAMenuFlyoutPresenter()
        : base(new MenuFlyoutInteractionHandler(true))
    {
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
    }

    public FAMenuFlyoutPresenter(IMenuInteractionHandler handler)
        : base(handler) { }

    Type IStyleable.StyleKey => typeof(Avalonia.Controls.MenuFlyoutPresenter);

    /// <summary>
    /// Closes the MenuFlyout.
    /// </summary>
    /// <remarks>
    /// This method should generally not be called directly and is present for the
    /// MenuInteractionHandler. Close Flyouts by calling Hide() on the Flyout object directly.
    /// </remarks>
    public override void Close()
    {
        // DefaultMenuInteractionHandler calls this
        var host = this.FindLogicalAncestorOfType<Popup>();
        if (host != null)
        {
            for (int i = 0; i < LogicalChildren.Count; i++)
            {
                if (LogicalChildren[i] is IMenuItem item)
                {
                    item.IsSubMenuOpen = false;
                }
            }

            SelectedIndex = -1;
            host.IsOpen = false;

            RaiseMenuClosed();
        }
    }

    /// <summary>
    /// This method has no functionality
    /// </summary>
    /// <exception cref="NotSupportedException" />
    public override void Open()
    {
        throw new NotSupportedException("Use MenuFlyout.ShowAt(Control) instead");
    }

    protected override IItemContainerGenerator CreateItemContainerGenerator()
    {
        return new MenuFlyoutPresenterItemContainerGenerator(this);
    }

    internal void RaiseMenuOpened()
    {
        RaiseEvent(new RoutedEventArgs(MenuOpenedEvent) { Source = this });
    }

    internal void RaiseMenuClosed()
    {
        RaiseEvent(new RoutedEventArgs(MenuClosedEvent) { Source = this });
    }

    internal bool InternalMoveSelection(NavigationDirection dir, bool wrap) =>
        MoveSelection(dir, wrap);

    protected override void OnContainersMaterialized(ItemContainerEventArgs e)
    {
        base.OnContainersMaterialized(e);

        // v2 Change: ControlThemes means we can't use styling on the MFP to apply the 
        // Icon/Toggle adjustments and we have to put them directly on the items

        int iconCount = _iconCount;
        int toggleCount = _toggleCount;
        for (int i = 0; i < e.Containers.Count; i++)
        {
            if (e.Containers[i].ContainerControl is ToggleMenuFlyoutItem tmfi)
            {
                if (tmfi.Icon != null)
                {
                    iconCount++;
                }

                toggleCount++;
            }
            else if (e.Containers[i].ContainerControl is RadioMenuFlyoutItem rmfi)
            {
                if (rmfi.Icon != null)
                {
                    iconCount++;
                }

                toggleCount++;
            }
            else if (e.Containers[i].ContainerControl is MenuFlyoutItem mfi)
            {
                if (mfi.Icon != null)
                {
                    iconCount++;
                }
            }
            else if (e.Containers[i].ContainerControl is MenuFlyoutSubItem mfsi)
            {
                if (mfsi.Icon != null)
                {
                    iconCount++;
                }
            }
        }

        // Only run the update IF something has changed
        if (_iconCount != iconCount || _toggleCount != toggleCount)
        {
            _toggleCount = toggleCount;
            _iconCount = iconCount;

            UpdateVisualState();
        }
    }

    protected override void OnContainersDematerialized(ItemContainerEventArgs e)
    {
        base.OnContainersDematerialized(e);

        // v2 Change: ControlThemes means we can't use styling on the MFP to apply the 
        // Icon/Toggle adjustments and we have to put them directly on the items

        int iconCount = _iconCount;
        int toggleCount = _toggleCount;
        for (int i = 0; i < e.Containers.Count; i++)
        {
            if (e.Containers[i].ContainerControl is ToggleMenuFlyoutItem tmfi)
            {
                if (tmfi.Icon != null)
                {
                    iconCount--;
                }

                toggleCount--;
            }
            else if (e.Containers[i].ContainerControl is RadioMenuFlyoutItem rmfi)
            {
                if (rmfi.Icon != null)
                {
                    iconCount--;
                }

                toggleCount--;
            }
            else if (e.Containers[i].ContainerControl is MenuFlyoutItem mfi)
            {
                if (mfi.Icon != null)
                {
                    iconCount--;
                }
            }
            else if (e.Containers[i].ContainerControl is MenuFlyoutSubItem mfsi)
            {
                if (mfsi.Icon != null)
                {
                    iconCount--;
                }
            }
        }

        if (_toggleCount != toggleCount || _iconCount != iconCount)
        {
            _iconCount = iconCount;
            _toggleCount = toggleCount;
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        // v2 Change: ControlThemes means we can't use styling on the MFP to apply the 
        // Icon/Toggle adjustments and we have to put them directly on the items

        const string iconClass = ":icons";
        const string toggleClass = ":toggle";
        bool icon = _iconCount > 0;
        bool toggle = _toggleCount > 0;
        foreach (var item in ItemContainerGenerator.Containers)
        {
            ((IPseudoClasses)item.ContainerControl.Classes).Set(iconClass, icon);
            ((IPseudoClasses)item.ContainerControl.Classes).Set(toggleClass, toggle);
        }
    }

    private int _iconCount = 0;
    private int _toggleCount = 0;
}
