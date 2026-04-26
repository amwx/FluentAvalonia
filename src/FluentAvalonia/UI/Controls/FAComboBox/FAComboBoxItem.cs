using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the container for an item in a <see cref="FAComboBox"/> control.
/// </summary>
public class FAComboBoxItem : ListBoxItem
{
    static FAComboBoxItem()
    {
        FocusableProperty.OverrideDefaultValue<FAComboBoxItem>(true);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.NavigationMethod == NavigationMethod.Directional || e.NavigationMethod == NavigationMethod.Tab)
        {
            var parent = (Parent as FAComboBox) ?? this.FindAncestorOfType<FAComboBox>();
            if (parent != null)
            {
                parent.ItemFocused(this);
            }
        }

    }
}
