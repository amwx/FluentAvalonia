using Avalonia;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents the base class for all items in a <see cref="FATaskDialog"/>
/// </summary>
public abstract class FATaskDialogControl : AvaloniaObject
{
    public FATaskDialogControl() { }

    /// <summary>
    /// Creates a new TaskDialog control with the specified text and dialog result
    /// </summary>
    /// <param name="text">The text the button/command should display</param>
    /// <param name="result">The dialog result the button should return if invoked. Can 
    /// be any of <see cref="FATaskDialogStandardResult"/> or any custom name.
    /// </param>
    public FATaskDialogControl(string text, object result)
    {
        _text = text;
        _dialogResult = result;
    }

    /// <summary>
    /// Defines the <see cref="Text"/> property
    /// </summary>
    public static readonly DirectProperty<FATaskDialogControl, string> TextProperty =
        AvaloniaProperty.RegisterDirect<FATaskDialogControl, string>(nameof(Text),
            x => x.Text, (x, v) => x.Text = v);

    /// <summary>
    /// Defines the <see cref="DialogResult"/> property
    /// </summary>
    public static readonly DirectProperty<FATaskDialogControl, object> DialogResultProperty =
        AvaloniaProperty.RegisterDirect<FATaskDialogControl, object>(nameof(DialogResult),
            x => x.DialogResult, (x, v) => x.DialogResult = v);

    /// <summary>
    /// Defines the <see cref="IsEnabled"/> property
    /// </summary>
    public static readonly DirectProperty<FATaskDialogControl, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<FATaskDialogControl, bool>(nameof(IsEnabled),
            x => x.IsEnabled, (x, v) => x.IsEnabled = v);

    /// <summary>
    /// Defines the <see cref="IsDefault"/> property
    /// </summary>
    public static readonly DirectProperty<FATaskDialogControl, bool> IsDefaultProperty =
        AvaloniaProperty.RegisterDirect<FATaskDialogControl, bool>(nameof(IsDefault),
            x => x.IsDefault, (x, v) => x.IsDefault = v);

    /// <summary>
    /// Gets or sets the Text associated with the TaskDialog control
    /// </summary>
    public string Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    /// <summary>
    /// Gets or sets the dialog result associated with the control
    /// </summary>
    /// <remarks>
    /// Can be any of <see cref="FATaskDialogStandardResult"/> or any custom name. NOTE:
    /// that 'null' is not a valid result and will be converted to <see cref="FATaskDialogStandardResult.None"/>
    /// </remarks>
    public object DialogResult
    {
        get => _dialogResult;
        set => SetAndRaise(DialogResultProperty, ref _dialogResult, value);
    }

    /// <summary>
    /// Gets or sets whether the control is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }

    /// <summary>
    /// Gets or sets whether the control is the default button/command.
    /// </summary>
    /// <remarks>
    /// This can only be set on one button/command in a TaskDialog or an error will be thrown.
    /// The desired button will be invoked upon 'Enter' key press and receives first focus
    /// </remarks>
    public bool IsDefault
    {
        get => _isDefault;
        set => SetAndRaise(IsDefaultProperty, ref _isDefault, value);
    }


    private string _text;
    private object _dialogResult = FATaskDialogStandardResult.None;
    private bool _isEnabled = true;
    private bool _isDefault;
}
