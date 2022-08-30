using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants (flags) that describe the state of a progress 
/// <see cref="TaskDialog"/>. Note that these are just visual indicators,
/// and actual state behavior is up to you
/// </summary>
[Flags]
public enum TaskDialogProgressState
{
    /// <summary>
    /// Progress bar is in a normal state
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Progress bar is shown in an error state
    /// </summary>
    Error = 1,

    /// <summary>
    /// Progress bar is shown in a suspended state
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Progress bar is shown as indeterminate
    /// </summary>
    Indeterminate = 10,
}
