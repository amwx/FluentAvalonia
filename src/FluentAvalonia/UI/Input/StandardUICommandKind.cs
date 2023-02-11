namespace FluentAvalonia.UI.Input;

/// <summary>
/// Specifies the set of platform commands (with pre-defined properties such as icon, 
/// keyboard accelerator, and description) that can be used with a StandardUICommand.
/// </summary>
public enum StandardUICommandKind
{
    /// <summary>
    /// No command. Default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies the cut command.
    /// </summary>
    Cut = 1,

    /// <summary>
    /// Specifies the copy command.
    /// </summary>
    Copy = 2,

    /// <summary>
    /// Specifies the paste command.
    /// </summary>
    Paste = 3,

    /// <summary>
    /// Specifies the select all command.
    /// </summary>
    SelectAll = 4,

    /// <summary>
    /// Specifies the delete command.
    /// </summary>
    Delete = 5,

    /// <summary>
    /// Specifies the share command.
    /// </summary>
    Share = 6,

    /// <summary>
    /// Specifies the save command.
    /// </summary>
    Save = 7,

    /// <summary>
    /// Specifies the open command.
    /// </summary>
    Open = 8,

    /// <summary>
    /// Specifies the close command.
    /// </summary>
    Close = 9,

    /// <summary>
    /// Specifies the pause command.
    /// </summary>
    Pause = 10,

    /// <summary>
    /// Specifies the play command.
    /// </summary>
    Play = 11,

    /// <summary>
    /// Specifies the stop command.
    /// </summary>
    Stop = 12,

    /// <summary>
    /// Specifies the forward command.
    /// </summary>
    Forward = 13,

    /// <summary>
    /// Specifies the backward command.
    /// </summary>
    Backward = 14,

    /// <summary>
    /// Specifies the undo command.
    /// </summary>
    Undo = 15,

    /// <summary>
    /// Specifies the redo command.
    /// </summary>
    Redo = 16
}
