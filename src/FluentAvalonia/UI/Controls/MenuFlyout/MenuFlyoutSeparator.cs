namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a horizontal line that separates items in a <see cref="FAMenuFlyout"/>
/// </summary>
public class MenuFlyoutSeparator : MenuFlyoutItemBase
{
    static MenuFlyoutSeparator()
    {
        FocusableProperty.OverrideDefaultValue<MenuFlyoutSeparator>(false);
    }
}
