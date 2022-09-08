namespace FluentAvalonia.UI.Controls;

public class MenuFlyoutSeparator : MenuFlyoutItemBase
{
    static MenuFlyoutSeparator()
    {
        FocusableProperty.OverrideDefaultValue<MenuFlyoutSeparator>(false);
    }
}
