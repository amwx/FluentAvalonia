using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace FluentAvalonia.UI.Controls;

public class MenuFlyoutItemBase : TemplatedControl
{
    static MenuFlyoutItemBase()
    {
        FocusableProperty.OverrideDefaultValue<MenuFlyoutItemBase>(true);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
        //      this is to just change the event name and source and re-raise it. This isn't ideal
        e.RoutedEvent = MenuItem.PointerEnteredItemEvent;
        e.Source = this;
        RaiseEvent(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
        //      this is to just change the event name and source and re-raise it. This isn't ideal
        e.RoutedEvent = MenuItem.PointerExitedItemEvent;
        e.Source = this;
        RaiseEvent(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        // Internal Bug: Clicking on a MenuFlyoutSubItem is triggering a PointerCaptureLost event being raised
        // which causes flickering of the submenu - disabling this
        // Side-effect is for touch/pen devices won't trigger exited event if device is lost - but that should
        // only be a minimal impact - the submenu would just stay open until cancelled by user
        //RaiseEvent(new PointerEventArgs(MenuItem.PointerExitedItemEvent, this, e.Pointer, VisualRoot, new Point(),
        //    0, default, KeyModifiers.None));
    }
}

