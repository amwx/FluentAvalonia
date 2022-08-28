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
        var point = e.GetCurrentPoint(null);
        RaiseEvent(new PointerEventArgs(MenuItem.PointerEnteredItemEvent, this, e.Pointer, VisualRoot, point.Position,
            e.Timestamp, point.Properties, e.KeyModifiers));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        var point = e.GetCurrentPoint(null);
        RaiseEvent(new PointerEventArgs(MenuItem.PointerExitedItemEvent, this, e.Pointer, VisualRoot, point.Position,
            e.Timestamp, point.Properties, e.KeyModifiers));
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        RaiseEvent(new PointerEventArgs(MenuItem.PointerExitedItemEvent, this, e.Pointer, VisualRoot, new Point(),
            0, default, KeyModifiers.None));
    }
}

