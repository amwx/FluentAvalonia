using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a command item in a <see cref="FATaskDialog"/>
/// </summary>
public class FATaskDialogCommand : FATaskDialogButton
{
    /// <summary>
    /// Defines the <see cref="Description"/> property
    /// </summary>
    public static readonly DirectProperty<FATaskDialogCommand, string> DescriptionProperty =
        AvaloniaProperty.RegisterDirect<FATaskDialogCommand, string>(nameof(Description),
            x => x.Description, (x, v) => x.Description = v);

    /// <summary>
    /// Gets or sets whether invoking this command should also close the dialog
    /// </summary>
    public bool ClosesOnInvoked { get; set; } = true;

    /// <summary>
    /// Gets or sets the description of the TaskDialogCommand
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetAndRaise(DescriptionProperty, ref _description, value);
    }

    private string _description;
}
