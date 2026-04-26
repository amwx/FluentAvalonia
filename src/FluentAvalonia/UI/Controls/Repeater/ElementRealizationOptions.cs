namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify whether to suppress automatic recycling of the retrieved element or force creation of a new element.
/// This enumeration supports a bitwise combination of its member values.
/// </summary>
[Flags]
public enum ElementRealizationOptions
{
    /// <summary>
    /// No option is specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Creation of a new element is forced.
    /// </summary>
    ForceCreate = 1,

    /// <summary>
    /// The element is ignored by the automatic recycling logic.
    /// </summary>
    SuppressAutoRecycle = 2
}
