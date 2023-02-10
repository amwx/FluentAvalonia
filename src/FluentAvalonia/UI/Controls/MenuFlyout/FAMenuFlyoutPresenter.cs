using Avalonia.Controls;
using Avalonia.Controls.Metadata;
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
[PseudoClasses(s_pcIcons, s_pcToggle)]
public class FAMenuFlyoutPresenter : MenuBase, IStyleable
{
    public FAMenuFlyoutPresenter()
        : base(new MenuFlyoutInteractionHandler(true))
    {
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
    }

    public FAMenuFlyoutPresenter(IMenuInteractionHandler handler)
        : base(handler) { }

    Type IStyleable.StyleKey => typeof(MenuFlyoutPresenter);

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

    protected override bool IsItemItsOwnContainerOverride(Control item) => true;

    protected override Control CreateContainerForItemOverride() =>
        new MenuFlyoutItem();

    protected override void PrepareContainerForItemOverride(Control element, object item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);

        var iconCount = _iconCount;
        var toggleCount = _toggleCount;
        if (element is ToggleMenuFlyoutItem tmfi)
        {
            if (tmfi.IconSource != null)
            {
                iconCount++;
            }

            toggleCount++;
        }
        else if (element is RadioMenuFlyoutItem rmfi)
        {
            if (rmfi.IconSource != null)
            {
                iconCount++;
            }

            toggleCount++;
        }
        else if (element is MenuFlyoutItem mfi)
        {
            if (mfi.IconSource != null)
            {
                iconCount++;
            }
        }
        else if (element is MenuFlyoutSubItem mfsi)
        {
            if (mfsi.IconSource != null)
            {
                iconCount++;
            }
        }

        if (iconCount != _iconCount || _toggleCount != toggleCount)
        {
            _iconCount = iconCount;
            _toggleCount = toggleCount;
            UpdateVisualState();
            // This container isn't realized yet, so we need to apply the classes here
            ((IPseudoClasses)element.Classes).Set(s_pcIcons, iconCount != 0);
            ((IPseudoClasses)element.Classes).Set(s_pcToggle, toggleCount != 0);
        }
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        var iconCount = _iconCount;
        var toggleCount = _toggleCount;

        if (element is ToggleMenuFlyoutItem tmfi)
        {
            if (tmfi.IconSource != null)
            {
                iconCount--;
            }

            toggleCount--;
        }
        else if (element is RadioMenuFlyoutItem rmfi)
        {
            if (rmfi.IconSource != null)
            {
                iconCount--;
            }

            toggleCount--;
        }
        else if (element is MenuFlyoutItem mfi)
        {
            if (mfi.IconSource != null)
            {
                iconCount--;
            }
        }
        else if (element is MenuFlyoutSubItem mfsi)
        {
            if (mfsi.IconSource != null)
            {
                iconCount--;
            }
        }

        if (iconCount != _iconCount || _toggleCount != toggleCount)
        {
            _iconCount = iconCount;
            _toggleCount = toggleCount;
            UpdateVisualState();
        }
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
        
    private void UpdateVisualState()
    {
        // v2 Change: ControlThemes means we can't use styling on the MFP to apply the 
        // Icon/Toggle adjustments and we have to put them directly on the items

        bool icon = _iconCount > 0;
        bool toggle = _toggleCount > 0;
        foreach (var item in GetRealizedContainers())
        {
            ((IPseudoClasses)item.Classes).Set(s_pcIcons, icon);
            ((IPseudoClasses)item.Classes).Set(s_pcToggle, toggle);
        }
    }

    private int _iconCount = 0;
    private int _toggleCount = 0;

    private const string s_pcIcons = ":icons";
    private const string s_pcToggle = ":toggle";
}
