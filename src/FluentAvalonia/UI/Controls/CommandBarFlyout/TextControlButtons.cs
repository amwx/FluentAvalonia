namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants to define which commands should be available
/// in a <see cref="TextCommandBarFlyout"/>
/// </summary>
public enum TextControlButtons
{
    None = 0x0000,
    Cut = 0x0001,
    Copy = 0x0002,
    Paste = 0x0004,
    Bold = 0x0008,
    Italic = 0x0010,
    Underline = 0x0020,
    Undo = 0x0040,
    Redo = 0x0080,
    SelectAll = 0x0100
}
