namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify whether a <see cref="TeachingTip"/>'s Tail is visible or collapsed.
/// </summary>
public enum TeachingTipTailVisibility
{
    /// <summary>
    /// The teaching tip's tail is collapsed when non-targeted and visible when the targeted.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The teaching tip's tail is visible.
    /// </summary>
    Visible = 1,

    /// <summary>
    /// The teaching tip's tail is collapsed.
    /// </summary>
    Collapsed = 2
}
