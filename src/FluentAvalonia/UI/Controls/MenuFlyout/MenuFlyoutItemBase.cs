using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls;

public class MenuFlyoutItemBase : TemplatedControl
{
    static MenuFlyoutItemBase()
    {
        FocusableProperty.OverrideDefaultValue<MenuFlyoutItemBase>(true);
    }

    internal bool IsContainerFromTemplate { get; set; }

    internal FAMenuFlyoutPresenter InternalParent { get; set; }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        InternalParent.PointerEnteredItem(this);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        InternalParent.PointerExitedItem(this);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        InternalParent.PointerExitedItem(this);
    }
}

