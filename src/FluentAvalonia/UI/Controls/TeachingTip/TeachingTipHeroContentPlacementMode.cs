namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate the preferred location of the <see cref="TeachingTip.HeroContent"/> 
/// within a <see cref="TeachingTip"/>.
/// </summary>
public enum TeachingTipHeroContentPlacementMode
{
    /// <summary>
    /// The header of the teaching tip. The hero content might be moved to the footer to avoid 
    /// intersecting with the tail of the targeted teaching tip.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The header of the teaching tip.
    /// </summary>
    Top = 1,

    /// <summary>
    /// The footer of the teaching tip.
    /// </summary>
    Bottom = 2
}
