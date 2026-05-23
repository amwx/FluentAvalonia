using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents a button in a TaskDialog
/// </summary>
/// <remarks>
/// This type should not be used directly and is generated automatically
/// by a TaskDialog
/// </remarks>
public class FATaskDialogButtonHost : Button
{
    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        FASettingsExpander.IconSourceProperty.AddOwner<FATaskDialogButtonHost>();

    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();

        if (DataContext is FATaskDialogButton tdb)
        {
            tdb.RaiseClick();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, change.NewValue != null);
        }
    }
}
