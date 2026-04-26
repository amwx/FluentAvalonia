using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Controls.Metadata;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents an item in a <see cref="FAMenuFlyout"/> that a user can change 
/// between two states, checked or unchecked.
/// </summary>
[PseudoClasses(SharedPseudoclasses.s_pcChecked)]
public class ToggleMenuFlyoutItem : MenuFlyoutItem
{
    /// <summary>
    /// Defines the <see cref="IsChecked"/> Property
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<ToggleMenuFlyoutItem, bool>(nameof(IsChecked),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets whether the ToggleMenuFlyoutItem is checked.
    /// </summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ToggleMenuFlyoutItem);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsCheckedProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcChecked, change.GetNewValue<bool>());
        }
    }

    protected override void OnClick()
    {
        base.OnClick();
        IsChecked = !IsChecked;
    }
}
