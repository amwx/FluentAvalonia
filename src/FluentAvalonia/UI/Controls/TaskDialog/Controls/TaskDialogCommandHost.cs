using Avalonia;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents a command button in a TaskDialog
/// </summary>
/// <remarks>
/// This type should not be used directly and is generated automatically
/// by a TaskDialog
/// </remarks>
public class TaskDialogCommandHost : TaskDialogButtonHost
{
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<TaskDialogCommandHost, string>(nameof(Description));

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
