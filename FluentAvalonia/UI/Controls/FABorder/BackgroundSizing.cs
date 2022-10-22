namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify how far an element's background extends in relation to the element's border.
/// </summary>
public enum BackgroundSizing
{
    /// <summary>
    /// The element's background extends to the inner edge of the border, but does not extend under the border.
    /// </summary>
    InnerBorderEdge,

    /// <summary>
    /// The element's background extends under the border to its outer edge, and is visible if the border is transparent.
    /// </summary>
    OuterBorderEdge
}
