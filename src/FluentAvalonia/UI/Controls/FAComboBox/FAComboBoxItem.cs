using Avalonia.Input;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls.Internal;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the container for an item in a <see cref="FAComboBox"/> control.
/// </summary>
public class FAComboBoxItem : FASelectorItem
{
    static FAComboBoxItem()
    {
        FocusableProperty.OverrideDefaultValue<FAComboBoxItem>(true);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.NavigationMethod == NavigationMethod.Directional || e.NavigationMethod == NavigationMethod.Tab)
        {
            var parent = (Parent as FAComboBox) ?? this.FindAncestorOfType<FAComboBox>();
            parent?.ItemFocused(this);
        }

    }
}
