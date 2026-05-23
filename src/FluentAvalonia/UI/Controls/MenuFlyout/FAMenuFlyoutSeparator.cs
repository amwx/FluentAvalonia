namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a horizontal line that separates items in a <see cref="FAMenuFlyout"/>
/// </summary>
public class FAMenuFlyoutSeparator : FAMenuFlyoutItemBase
{
    static FAMenuFlyoutSeparator()
    {
        FocusableProperty.OverrideDefaultValue<FAMenuFlyoutSeparator>(false);
    }
}
