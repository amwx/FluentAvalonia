namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines values that specify the input validation behavior of a NumberBox when invalid input is entered.
/// </summary>
public enum NumberBoxValidationMode
{
    /// <summary>
    /// Invalid input is replaced by NumberBox.PlaceholderText text.
    /// </summary>
    InvalidInputOverwritten,

    /// <summary>
    /// Input validation is disabled
    /// </summary>
    Disabled
}

/// <summary>
/// Defines values that specify how the spin buttons used to increment or decrement 
/// the Value of a NumberBox are displayed.
/// </summary>
public enum NumberBoxSpinButtonPlacementMode
{
    /// <summary>
    /// The spin buttons are not displayed.
    /// </summary>
    Hidden,

    /// <summary>
    /// The spin buttons have two visual states, depending on focus. By default, the 
    /// spin buttons are displayed in a compact, vertical orientation. When the 
    /// Numberbox gets focus, the spin buttons expand.
    /// </summary>
    Compact,

    /// <summary>
    /// The spin buttons are displayed in an expanded, horizontal orientation.
    /// </summary>
    Inline
}


