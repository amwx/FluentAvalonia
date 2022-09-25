namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate the preferred location of the <see cref="TeachingTip"/> teaching tip.
/// </summary>
public enum TeachingTipPlacementMode
{
    /// <summary>
    /// Along the bottom side of the xaml root when non-targeted and above the target element when targeted.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Along the top side of the xaml root when non-targeted and above the target element when targeted.
    /// </summary>
    Top = 1,

    /// <summary>
    /// Along the bottom side of the xaml root when non-targeted and below the target element when targeted.
    /// </summary>
    Bottom = 2,

    /// <summary>
    /// Along the left side of the xaml root when non-targeted and left of the target element when targeted.
    /// </summary>
    Left = 3,

    /// <summary>
    /// Along the right side of the xaml root when non-targeted and right of the target element when targeted.
    /// </summary>
    Right = 4,

    /// <summary>
    /// The top right corner of the xaml root when non-targeted and above the target element expanding rightward when targeted.
    /// </summary>
    TopRight = 5,

    /// <summary>
    /// The top left corner of the xaml root when non-targeted and above the target element expanding leftward when targeted.
    /// </summary>
    TopLeft = 6,

    /// <summary>
    /// The bottom right corner of the xaml root when non-targeted and below the target element expanding rightward when targeted.
    /// </summary>
    BottomRight = 7,

    /// <summary>
    /// The bottom left corner of the xaml root when non-targeted and below the target element expanding leftward when targeted.
    /// </summary>
    BottomLeft = 8,

    /// <summary>
    /// The top left corner of the xaml root when non-targeted and left of the target element expanding upward when targeted.
    /// </summary>
    LeftTop = 9,

    /// <summary>
    /// The bottom left corner of the xaml root when non-targeted and left of the target element expanding downward when targeted.
    /// </summary>
    LeftBottom = 10,

    /// <summary>
    /// The top right corner of the xaml root when non-targeted and right of the target element expanding upward when targeted.
    /// </summary>
    RightTop = 11,

    /// <summary>
    /// The bottom right corner of the xaml root when non-targeted and right of the target element expanding downward when targeted.
    /// </summary>
    RightBottom = 12,

    /// <summary>
    /// The center of the xaml root when non-targeted and pointing at the center of the target element when targeted.
    /// </summary>
    Center = 13
}
