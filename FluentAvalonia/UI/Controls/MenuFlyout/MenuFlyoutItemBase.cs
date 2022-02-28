using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutItemBase : TemplatedControl
	{
		static MenuFlyoutItemBase()
		{
			FocusableProperty.OverrideDefaultValue<MenuFlyoutItemBase>(true);
		}

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            var point = e.GetCurrentPoint(null);
            RaiseEvent(new PointerEventArgs(MenuItem.PointerEnterItemEvent, this, e.Pointer, VisualRoot, point.Position,
                e.Timestamp, point.Properties, e.KeyModifiers));
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);
            var point = e.GetCurrentPoint(null);
            RaiseEvent(new PointerEventArgs(MenuItem.PointerLeaveItemEvent, this, e.Pointer, VisualRoot, point.Position,
                e.Timestamp, point.Properties, e.KeyModifiers));
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            RaiseEvent(new PointerEventArgs(MenuItem.PointerLeaveItemEvent, this, e.Pointer, VisualRoot, new Point(),
                0, null, KeyModifiers.None));
        }
    }
}
