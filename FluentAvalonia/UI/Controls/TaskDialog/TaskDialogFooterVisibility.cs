namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Defines constants that specify how the footer area is shown
    /// in a <see cref="TaskDialog"/>
    /// </summary>
    public enum TaskDialogFooterVisibility
    {
        /// <summary>
        /// The footer is never shown
        /// </summary>
        Never,

        /// <summary>
        /// The footer is hidden by default, but can be expanded open
        /// </summary>
        Auto,

        /// <summary>
        /// The footer is always visible
        /// </summary>
        Always // Footer is always visible and the footer title is hidden
    }
}
